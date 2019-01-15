using System.IO;
using System.Linq;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Renci.SshNet;

namespace SSH
{
    [Cmdlet(VerbsCommon.Set, "SFTPItem", DefaultParameterSetName = "Index")]
    public class SetSftpItem : PSCmdlet
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
        /// Folder on remote target to upload the file to.
        /// </summary>
        private String _remotepath;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = "Remote path where to upload the item to")]
        public string Destination
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }

        /// <summary>
        /// The local file to be uploaded.
        /// </summary>
        private String[] _localItem;
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "Local path to item to upload")]
        [Alias("PSPath")]
        public String[] Path
        {
            get { return _localItem; }
            set { _localItem = value; }
        }

        /// <summary>
        /// If a file on the target should be overwritten or not.
        /// </summary>
        [Parameter(Position = 3,
            HelpMessage = "Overrite item on remote host if it already pressent.")]
        public SwitchParameter Force
        {
            get { return _overwrite; }
            set { _overwrite = value; }
        }
        private bool _overwrite;

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
            foreach (var sftpSession in ToProcess)
            {
                // check if the file specified actually exists.
                // Resolve the path even if a relative one is given.
                foreach (var localitem in _localItem)
                {
                    ProviderInfo provider;
                    var pathinfo = GetResolvedProviderPathFromPSPath(localitem, out provider);
                    var localfullPath = pathinfo[0];
                    var filePresent = File.Exists(@localfullPath);
                    var dirPresent = Directory.Exists(@localfullPath);

                    if (filePresent || dirPresent)
                    {
                        if (filePresent)
                        {
                            var fil = new FileInfo(@localfullPath);
                            var remoteFullpath = _remotepath.TrimEnd(new[] { '/' }) + "/" + fil.Name;
                            WriteVerbose("Uploading " + localfullPath + " to " + _remotepath);

                            // Setup Action object for showing download progress.

                            var res = new Action<ulong>(rs =>
                            {
                                //if (!MyInvocation.BoundParameters.ContainsKey("Verbose")) return;
                                if (fil.Length > 1240000)
                                {
                                    var percent = (int)((((double)rs) / fil.Length) * 100.0);
                                    if (percent % 10 == 0)
                                    {
                                        // This will prevent the progress message from being stuck on the screen.
                                        if (percent == 90 || percent > 90)
                                        {
                                            return;
                                        }

                                        var progressRecord = new ProgressRecord(1,
                                        "Uploading " + fil.Name,
                                        String.Format("{0} Bytes Uploaded of {1}", rs, fil.Length))
                                        { PercentComplete = percent };

                                        Host.UI.WriteProgress(1, progressRecord);
                                    }
                                }
                            });

                            // Check that the path we are uploading to actually exists on the target.
                            if (sftpSession.Session.Exists(_remotepath))
                            {
                                // Ensure the remote path is a directory. 
                                var attribs = sftpSession.Session.GetAttributes(_remotepath);
                                if (!attribs.IsDirectory)
                                {
                                    throw new SftpPathNotFoundException("Specified path is not a directory");
                                }
                                // Check if the file already exists on the target system.
                                var present = sftpSession.Session.Exists(remoteFullpath);
                                if ((present & _overwrite) || (!present))
                                {
                                    var localstream = File.OpenRead(localfullPath);
                                    try
                                    {
                                        sftpSession.Session.UploadFile(localstream, remoteFullpath, res);
                                        localstream.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        localstream.Close();
                                        WriteError(new ErrorRecord(
                                                     ex,
                                                     "Error while Uploading",
                                                     ErrorCategory.InvalidOperation,
                                                     sftpSession));

                                    }
                                }
                                else
                                {
                                    var ex = new SftpPermissionDeniedException("File already exists on remote host.");
                                    WriteError(new ErrorRecord(
                                                     ex,
                                                     "File already exists on remote host",
                                                     ErrorCategory.InvalidOperation,
                                                     sftpSession));
                                }

                            }
                            else
                            {
                                var ex = new SftpPathNotFoundException(_remotepath + " does not exist.");
                                WriteError(new ErrorRecord(
                                            ex,
                                            _remotepath + " does not exist.",
                                            ErrorCategory.InvalidOperation,
                                            sftpSession));
                            }

                        }
                        else
                        {
                            var dirName = new DirectoryInfo(@localfullPath).Name;
                            var remoteFullpath = _remotepath.TrimEnd(new[] { '/' }) + "/" + dirName;
                            
                            WriteVerbose("Uploading " + localfullPath + " to " + _remotepath);
                            if (!sftpSession.Session.Exists(remoteFullpath))
                            {
                                sftpSession.Session.CreateDirectory(remoteFullpath);
                            }
                            else
                            {
                                if (!_overwrite)
                                {
                                    var ex = new SftpPermissionDeniedException("Folder already exists on remote host.");
                                    ThrowTerminatingError(new ErrorRecord(
                                        ex,
                                        "Folder already exists on remote host",
                                        ErrorCategory.InvalidOperation,
                                        sftpSession));
                                }
                            }

                            try
                            {
                                UploadDirectory(sftpSession.Session, localfullPath, remoteFullpath);
                            }
                            catch (Exception ex)
                            {
                                WriteError(new ErrorRecord(
                                             ex,
                                             "Error while Uploading",
                                             ErrorCategory.InvalidOperation,
                                             sftpSession));

                            }
                        }
                    }
                    else
                    {
                        var ex = new FileNotFoundException("Item to upload " + localfullPath + " was not found.");

                        ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                "Item to upload " + localfullPath + " was not found.",
                                                ErrorCategory.InvalidOperation,
                                                localfullPath));

                    } // check if item exists.
                } // foreach local item
            } // sftp session.
        } // Process Record.
        void UploadDirectory(SftpClient client, string localPath, string remotePath)
        {

            IEnumerable<FileSystemInfo> infos = new DirectoryInfo(localPath).EnumerateFileSystemInfos();
            foreach (FileSystemInfo info in infos)
            {
                if (info.Attributes.HasFlag(FileAttributes.Directory))
                {
                    string subPath = remotePath + "/" + info.Name;
                    WriteVerbose("Uploading to " + subPath);
                    if (!client.Exists(subPath))
                    {
                        client.CreateDirectory(subPath);
                    }
                    UploadDirectory(client, info.FullName, remotePath + "/" + info.Name);
                }
                else
                {
                    using (Stream fileStream = new FileStream(info.FullName, FileMode.Open))
                    {
                        var fil = new FileInfo(info.FullName);
                        WriteVerbose("Uploading file: " + remotePath + "/" + info.Name);
                        var res = new Action<ulong>(rs =>
                        {

                            if (fil.Length > 1240000)
                            {
                                var percent = (int)((((double)rs) / fil.Length) * 100.0);
                                if (percent % 10 == 0)
                                {
                                    // This will prevent the progress message from being stuck on the screen.
                                    if (percent == 90 || percent > 90)
                                    {
                                        return;
                                    }

                                    var progressRecord = new ProgressRecord(1,
                                            "Uploading " + fil.Name,
                                            $"{rs} Bytes Uploaded of {fil.Length}")
                                    { PercentComplete = percent };

                                    Host.UI.WriteProgress(1, progressRecord);
                                }
                            }
                        });
                        client.UploadFile(fileStream, remotePath + "/" + info.Name, res);

                        // Clean any stray progress bar.
                        var progressRecordEnd = new ProgressRecord(1,
                                "Uploading ",
                                "end");

                        Host.UI.WriteProgress(1, progressRecordEnd);
                    }
                }
            }
        } // upload directory.
    }
}
