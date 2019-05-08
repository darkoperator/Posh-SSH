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

        // Supress progress bar.
        private bool _noProgress = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Do not show upload progress.")]
        public SwitchParameter NoProgress
        {
            get { return _noProgress; }
            set { _noProgress = value; }
        }

        /// <summary>
        /// If the local file exists overwrite it.
        /// </summary>
        [Parameter(Position = 3,
            HelpMessage = "Overrite item on remote host if it already pressent.")]
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
            //var toProcess = new SSH.SftpSession[];
            base.BeginProcessing();
            var sessionvar = SessionState.PSVariable.GetValue("Global:SftpSessions") as List<SftpSession>;
            switch (ParameterSetName)
            {
                case "Session":
                    // fix issue #37: Get-SFTPFile/Set-SFTPFile fail with 'Object reference not set to an instance of an object.'
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
            } // switch (ParameterSetName...
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
                                string dirName = new DirectoryInfo(remotepath).Name;
                                var fileFullPath = $"{@localfullPath}{System.IO.Path.DirectorySeparatorChar}{dirName}";

                                var present = Directory.Exists(fileFullPath);
                                if ((present & _overwrite) || (!present))
                                {
                                    var res = new Action<ulong>(rs =>
                                    {
                                        if (attribs.Size != 0)
                                        {
                                            var percent = (int)((((double)rs) / attribs.Size) * 100.0);
                                            if (percent % 10 == 0)
                                            {
                                                // This should prevent the progress message from being stuck on the screen.
                                                if (percent == 100)
                                                {
                                                    return;
                                                }

                                                var progressRecord = new ProgressRecord(1,
                                                "Downloading " + remotepath,
                                                String.Format("{0} Bytes Downloaded of {1}", rs, attribs.Size))
                                                { PercentComplete = percent };

                                                Host.UI.WriteProgress(1, progressRecord);
                                            }

                                        }
                                    });
                                    Directory.CreateDirectory(fileFullPath);
                                    DownloadDirectory(sftpSession.Session, remotepath, fileFullPath, _skipsymlink, res);
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
                                var fileName = new FileInfo(remotepath).Name;
                                // Setup Action object for showing download progress.
                                var res = new Action<ulong>(rs =>
                                {
                                    if (attribs.Size != 0)
                                    {
                                        var percent = (int)((((double)rs) / attribs.Size) * 100.0);
                                        if (percent % 10 == 0)
                                        {
                                            // This should prevent the progress message from being stuck on the screen.
                                            if (percent == 100)
                                            {
                                                return;
                                            }

                                            var progressRecord = new ProgressRecord(1,
                                            "Downloading: " + fileName,
                                            String.Format("{0} Bytes Downloaded of {1}", rs, attribs.Size))
                                            { PercentComplete = percent };

                                            Host.UI.WriteProgress(1, progressRecord);
                                        }

                                    }
                                });

                                var fileFullPath = $"{@localfullPath}{System.IO.Path.DirectorySeparatorChar}{fileName}";

                                var present = File.Exists(fileFullPath);

                                if ((present & _overwrite) || (!present))
                                {
                                    var localstream = File.Create(fileFullPath);
                                    try
                                    {
                                        WriteVerbose($"Downloading: {remotepath}");
                                        sftpSession.Session.DownloadFile(remotepath, localstream, res);
                                        localstream.Close();
                                        var progressRecord = new ProgressRecord(1,
                                            "Downloading: " + remotepath,
                                            String.Format("{0} Bytes Downloaded of {1}",attribs.Size, attribs.Size))
                                        { PercentComplete = 100 };

                                        WriteProgress(progressRecord);
                                    }
                                    catch
                                    {
                                        localstream.Close();
                                        var ex = new SftpPermissionDeniedException($"Unable to download {remotepath} from host.");
                                        WriteError(new ErrorRecord(
                                            ex,
                                           $"Unable to download {remotepath} from host.",
                                            ErrorCategory.InvalidOperation,
                                            sftpSession));
                                    }
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
                            else
                            {
                                 
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
                var ex = new FileNotFoundException("Local path" + localfullPath + " was not found.");

                ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                "Local path" + localfullPath + " was not found.",
                                                ErrorCategory.InvalidOperation,
                                                localfullPath));
            }
        }

        private void DownloadDirectory(SftpClient client, string source, string localPath, bool skipsymlink, Action<ulong> progress)
        {
            var files = client.ListDirectory(source);
            foreach (var file in files)
            {
                if (!file.IsDirectory && !file.IsSymbolicLink)
                {
                    DownloadFile(client, file, localPath, progress);
                }
                else if (file.IsSymbolicLink)
                {
                    var attribs = client.GetAttributes(source);
                    if (skipsymlink)
                    {
                        WriteVerbose($"Skipping symbolic link {file.FullName}");
                    }
                    else
                    {
                        if (attribs.IsDirectory)
                        {
                            var localFullPath = System.IO.Path.Combine(localPath, file.Name);
                            var dir = Directory.CreateDirectory(localFullPath);
                            DownloadDirectory(client, file.FullName, dir.FullName, skipsymlink, progress);
                        }
                        else if (attribs.IsRegularFile)
                        {
                            DownloadFile(client, file, localPath, progress);
                        }
                    }

                }
                else if (file.Name != "." && file.Name != "..")
                {
                    var localFullPath = System.IO.Path.Combine(localPath, file.Name);
                    var dir = Directory.CreateDirectory(localFullPath);
                    DownloadDirectory(client, file.FullName, dir.FullName, skipsymlink, progress);
                }
            }
        }

        private void DownloadFile(SftpClient client, SftpFile file, string localDirectory, Action<ulong> progress)
        {
            WriteVerbose($"Downloading {file.FullName}");
            var localFullPath = System.IO.Path.Combine(localDirectory, file.Name);
            using (Stream fileStream = File.OpenWrite(localFullPath))
            {
                client.DownloadFile(file.FullName, fileStream, progress);
            }
        }
    }
}
