using System.IO;
using System.Linq;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Renci.SshNet;


namespace SSH
{

    [Cmdlet(VerbsCommon.Set, "SFTPFolder", DefaultParameterSetName = "Index")]
    public class SetSftpFolder : PSCmdlet
    {
        /// <summary>
        /// Parameter for Index of the SFTPSession.
        /// </summary>
        private int[] _index;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Index")]
        public int[] SessionId
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
            ParameterSetName = "Session")]
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
            Position = 2)]
        public string RemotePath
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }

        /// <summary>
        /// The local file to be uploaded.
        /// </summary>
        private String[] _localfolder;
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1)]
        [Alias("PSPath")]
        public String[] LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }

        /// <summary>
        /// If a file on the target should be overwritten or not.
        /// </summary>
        [Parameter(Position = 3)]
        public SwitchParameter Overwrite
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
                // check if the folder specified actually exists.
                // Resolve the path even if a relative one is given.
                foreach (var localfolder in _localfolder)
                {
                    ProviderInfo provider;
                    var pathinfo = GetResolvedProviderPathFromPSPath(localfolder, out provider);
                    var localfullPath = pathinfo[0];

                    
                    if (Directory.Exists(@localfullPath))
                    {
                        
                        WriteVerbose("Uploading " + localfullPath + " to "+ RemotePath);
                        if (!sftpSession.Session.Exists(RemotePath))
                        {
                            sftpSession.Session.CreateDirectory(RemotePath);
                        }
                        else
                        {
                            if (!Overwrite)
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
                            UploadDirectory(sftpSession.Session, localfullPath, RemotePath);
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
                    else
                    {
                        var ex = new FileNotFoundException("Folder to upload " + localfullPath + " was not found.");

                        ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                "Folder to upload " + localfullPath + " was not found.",
                                                ErrorCategory.InvalidOperation,
                                                localfullPath));
                    } // check if folder exists.
                } // foreach local folder
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
        }
    }
}
