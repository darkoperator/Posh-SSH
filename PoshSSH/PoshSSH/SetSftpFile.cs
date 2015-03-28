using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;


namespace SSH
{
    [Cmdlet(VerbsCommon.Set, "SFTPFile", DefaultParameterSetName = "Index")]
    public class SetSftpFile : PSCmdlet
    {
        /// <summary>
        /// Parameter for Index of the SFTPSession.
        /// </summary>
        private Int32[] _index;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Index")]
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
        private String _localfile;
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1)]
        [Alias("PSPath")]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
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
            // check if the file specified actually exists.
            // Resolve the path even if a relative one is given.
            ProviderInfo provider;
            var pathinfo = GetResolvedProviderPathFromPSPath(_localfile, out provider);
            var localfullPath = pathinfo[0];

            if (File.Exists(@localfullPath))
            {
                WriteVerbose("Uploading " + localfullPath);
                var fil = new FileInfo(@localfullPath);
                foreach (var sftpSession in ToProcess)
                {
                    var remoteFullpath = RemotePath.TrimEnd(new[] { '/' }) + "/" + fil.Name;
                    WriteVerbose("Uploading to " + remoteFullpath + " on " + sftpSession.Host);

                    // Setup Action object for showing download progress.

                    var res = new Action<ulong>(rs =>
                    {
                        //if (!MyInvocation.BoundParameters.ContainsKey("Verbose")) return;
                        if (fil.Length != 0)
                        {
                            var percent = (int)((((double)rs) / fil.Length) * 100.0);
                            if (percent % 10 == 0)
                            {
                                // This will prevent the progress message from being stuck on the screen.
                                if (percent == 100)
                                {
                                    return;
                                }

                                var progressRecord = new ProgressRecord(1,
                                "Uploading " + fil.Name,
                                String.Format("{0} Bytes Uploaded of {1}", rs, fil.Length)) { PercentComplete = percent };

                                Host.UI.WriteProgress(1, progressRecord);
                                //Host.UI.WriteVerboseLine(percent.ToString(CultureInfo.InvariantCulture) + "% Completed.");
                            }
                        }
                    });

                    // Check that the path we are uploading to actually exists on the target.
                    if (sftpSession.Session.Exists(RemotePath))
                    {
                        // Ensure the remote path is a directory. 
                        var attribs = sftpSession.Session.GetAttributes(RemotePath);
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
                        var ex = new SftpPathNotFoundException(RemotePath + " does not exist.");
                       ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                RemotePath + " does not exist.",
                                                ErrorCategory.InvalidOperation,
                                                sftpSession));
                    }
                }
            }
            else
            {
                var ex = new FileNotFoundException("File to upload " + localfullPath + " was not found.");

                ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                "File to upload " + localfullPath + " was not found.",
                                                ErrorCategory.InvalidOperation,
                                                localfullPath));
            }
        }
    }
}
