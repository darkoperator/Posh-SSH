using System.IO;
using System.Linq;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SSH
{
    [Cmdlet(VerbsCommon.Get, "SFTPItem", DefaultParameterSetName = "Index")]
    public class GetSftpItem : PSCmdlet
    {
        /// <summary>
        /// Parameter for Index of the SFTPSession.
        /// </summary>
        private Int32[] _index;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Index",
            HelpMessage = "Session Id of an existing SFTPSession.")]
        public Int32[] SessionId
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// Session parameter that takes private SSH.SftpSession[] 
        /// </summary>
        private SftpSession[] _session;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Session",
            HelpMessage = "Existing SFTPSession object.")]
        public SftpSession[] SFTPSession
        {
            get { return _session; }
            set { _session = value; }
        }

        /// <summary>
        /// Remote file to download.
        /// </summary>
        private String[] _remotepath;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "Remote path of item to download.")]
        public string[] Path
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }

        /// <summary>
        /// The local path where to save the file.
        /// </summary>
        private String _localpath;
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "Local path where to download item to.")]
        [Alias("PSPath")]
        public String Destination
        {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// If the local file exists overwrite it.
        /// </summary>
        [Parameter(Position = 3,
            HelpMessage = "Overwrite item on remote host if it already present.")]
        public SwitchParameter Force
        {
            get { return _overwrite; }
            set { _overwrite = value; }
        }
        private bool _overwrite;

        /// <summary>
        /// Skip Symbolic Links when downloading a folder.
        /// </summary>
        [Parameter(Position = 4,
            HelpMessage = "Do not follow symboliclinks if present in a directory.")]
        public SwitchParameter SkipSymLink
        {
            get { return _skipsymlink; }
            set { _skipsymlink = value; }
        }
        private bool _skipsymlink;

        private List<SftpSession> ToProcess { get; set; }

        protected override void BeginProcessing()
        {
            // Collect the sessions we will upload to.
            var toProcess = new List<SftpSession>();
            base.BeginProcessing();
            var sessionvar = SessionState.PSVariable.GetValue("Global:SftpSessions") as List<SftpSession>;
            switch (ParameterSetName)
            {
                case "Session":
                    toProcess.AddRange(_session);
                    ToProcess = toProcess;
                    break;
                case "Index":
                    if (sessionvar != null)
                    {
                        foreach (var sess in sessionvar)
                        {
                            if (_index.Contains(sess.SessionId))
                            {
                                toProcess.Add(sess);
                            }
                        }
                        ToProcess = toProcess;
                    }
                    break;
                default:
                    throw new ArgumentException("Bad ParameterSet Name");
            }
        }

        protected override void ProcessRecord()
        {
            // check if the path specified actually exists.
            // Resolve the path even if a relative one is given for PowerShell.
            ProviderInfo provider;
            var pathinfo = GetResolvedProviderPathFromPSPath(_localpath, out provider);
            var localfullPath = pathinfo[0];

            if (Directory.Exists(@localfullPath))
            {
                foreach (var sftpSession in ToProcess)
                {
                    foreach (string remotepath in _remotepath)
                    {
                        // Check that the path we are downloading from actually exists on the target.
                        if (sftpSession.Session.Exists(remotepath))
                        {
                            // Check if the remote path is a file or a directory to perform proper action.
                            var attribs = sftpSession.Session.GetAttributes(remotepath);

                            if (attribs.IsDirectory)
                            {
                                string dirName = System.IO.Path.GetFileName(remotepath.TrimEnd('/'));
                                var fileFullPath = System.IO.Path.Combine(@localfullPath, dirName);

                                var present = Directory.Exists(fileFullPath);
                                if (!present || _overwrite)
                                {
                                    Directory.CreateDirectory(fileFullPath);
                                    DownloadDirectory(sftpSession.Session, remotepath, fileFullPath, _skipsymlink, remotepath);
                                }
                                else
                                {
                                    var ex = new SftpPermissionDeniedException($"Item {remotepath} already present on local host.");
                                    WriteError(new ErrorRecord(
                                                     ex,
                                                     $"Item {remotepath} already present on local host.",
                                                     ErrorCategory.InvalidOperation,
                                                     sftpSession));
                                }
                            }
                            else if (attribs.IsRegularFile)
                            {
                                var fileName = System.IO.Path.GetFileName(remotepath);
                                var fileFullPath = System.IO.Path.Combine(@localfullPath, fileName);

                                var present = File.Exists(fileFullPath);

                                if (!present || _overwrite)
                                {
                                    DownloadFile(sftpSession.Session, sftpSession.Session.Get(remotepath), @localfullPath);
                                }
                                else
                                {
                                    var ex = new SftpPermissionDeniedException($"Item {remotepath} already present on local host.");
                                    WriteError(new ErrorRecord(
                                                     ex,
                                                     $"Item {remotepath} already present on local host.",
                                                     ErrorCategory.InvalidOperation,
                                                     sftpSession));
                                }
                            }
                        }
                        else
                        {
                            var ex = new SftpPathNotFoundException(remotepath + " does not exist.");

                            WriteError(new ErrorRecord(
                                                     ex,
                                                     remotepath + " does not exist.",
                                                     ErrorCategory.InvalidOperation,
                                                     sftpSession));
                        }
                    }
                }
            }
            else
            {
                var ex = new FileNotFoundException("Local path " + localfullPath + " was not found.");

                ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                "Local path " + localfullPath + " was not found.",
                                                ErrorCategory.InvalidOperation,
                                                localfullPath));
            }
        }

        private void DownloadDirectory(SftpClient client, string source, string localPath, bool skipsymlink, string basePath)
        {
            try
            {
                var files = client.ListDirectory(source);
                foreach (var file in files)
                {
                    WriteVerbose($"Processing {file.FullName}");
                    string relativePath = file.FullName.Substring(basePath.Length).TrimStart('/');
                    string localFilePath = System.IO.Path.Combine(localPath, relativePath);

                    try
                    {
                        if (ShouldDownloadFile(file))
                        {
                            DownloadFile(client, file, System.IO.Path.GetDirectoryName(localFilePath));
                        }
                        else if (file.IsSymbolicLink)
                        {
                            var attribs = client.GetAttributes(file.FullName);
                            if (skipsymlink)
                            {
                                WriteVerbose($"Skipping symbolic link {file.FullName}");
                            }
                            else
                            {
                                if (attribs.IsDirectory)
                                {
                                    Directory.CreateDirectory(localFilePath);
                                    DownloadDirectory(client, file.FullName, localPath, skipsymlink, basePath);
                                }
                                else if (ShouldDownloadFile(file))
                                {
                                    DownloadFile(client, file, System.IO.Path.GetDirectoryName(localFilePath));
                                }
                            }
                        }
                        else if (file.IsDirectory && file.Name != "." && file.Name != "..")
                        {
                            Directory.CreateDirectory(localFilePath);
                            DownloadDirectory(client, file.FullName, localPath, skipsymlink, basePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Error processing {file.FullName}: {ex.Message}");
                        // Continue with the next file
                    }
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Error accessing directory {source}: {ex.Message}");
                // The directory download will stop here, but the overall process will continue
            }
        }

        private bool ShouldDownloadFile(ISftpFile file)
        {
            if (file.IsRegularFile &&
                !file.IsSocket &&
                //!file.IsSymbolicLink &&
                !file.IsBlockDevice &&
                //!file.IsCharacterDevice &&
                !file.IsNamedPipe)
            {
                return true;
            }
            else
            {
                string fileType = GetFileType(file);
                WriteVerbose($"Skipping file {file.FullName}. File type: {fileType}");
                return false;
            }
        }

        private string GetFileType(ISftpFile file)
        {
            if (file.IsSocket) return "Socket";
            if (file.IsSymbolicLink) return "Symbolic Link";
            if (file.IsBlockDevice) return "Block Device";
            if (file.IsCharacterDevice) return "Character Device";
            if (file.IsNamedPipe) return "Named Pipe";
            if (file.IsDirectory) return "Directory";
            if (file.IsRegularFile) return "Regular File";
            return "Unknown";
        }

        private void DownloadFile(SftpClient client, ISftpFile file, string localDirectory)
        {
            if (ShouldDownloadFile(file))
            {
                WriteVerbose($"Downloading {file.FullName}");
                try
                {
                    Directory.CreateDirectory(localDirectory); // Ensure the directory exists
                    var localFullPath = System.IO.Path.Combine(localDirectory, file.Name);
                    // Setup Action object for showing download progress.
                    var progressHelper = new OperationProgressHelper(this, "Download", file.Name, file.Length, 1);
                    using (Stream fileStream = File.Create(localFullPath))
                    {
                        client.DownloadFile(file.FullName, fileStream, progressHelper.Callback);
                        progressHelper.Complete();
                    }
                }
                catch (Exception ex)
                {
                    WriteWarning($"Error downloading {file.FullName}: {ex.Message}");
                    // The individual file download failed, but we'll continue with the next file
                }
            }
        }
    }
}