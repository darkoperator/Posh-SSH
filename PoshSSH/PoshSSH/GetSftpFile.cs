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
    [Cmdlet(VerbsCommon.Get, "SFTPFile", DefaultParameterSetName = "Index")]
    public class GetSftpFile : PSCmdlet
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
        /// Session paramter that takes private SSH.SftpSession[] 
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
        /// Remote file to download.
        /// </summary>
        private String _remotefile;
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2)]
        public string RemoteFile
        {
            get { return _remotefile; }
            set { _remotefile = value; }
        }

        /// <summary>
        /// The local path where to save the file.
        /// </summary>
        private String _localpath;
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1)]
        [Alias("PSPath")]
        public String LocalPath
        {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// If the local file exists overwrite it.
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
            var pathinfo = GetResolvedProviderPathFromPSPath(_localpath, out provider);
            var localfullPath = pathinfo[0];

            if (Directory.Exists(@localfullPath))
            {
                var filename = Path.GetFileName(_remotefile);

                var localfilefullpath = localfullPath + "/" + filename;
                var fil = new FileInfo(@localfilefullpath);

                
                foreach (var sftpSession in ToProcess)
                {

                    WriteVerbose("Downloading " + filename + " to " + localfilefullpath + " from " + sftpSession.Host);

                    

                    // Check that the path we are downloading from actually exists on the target.
                    if (sftpSession.Session.Exists(_remotefile))
                    {
                        // Ensure the remote path is a directory. 
                        var attribs = sftpSession.Session.GetAttributes(_remotefile);
                        if (!attribs.IsRegularFile)
                        {
                            throw new SftpPathNotFoundException("Specified path is not a file.");
                        }

                        // Setup Action object for showing download progress.

                        var res = new Action<ulong>(rs =>
                        {
                            //if (!MyInvocation.BoundParameters.ContainsKey("Verbose")) return;
                            if (attribs.Size != 0)
                            {
                                var percent = (int)((((double)rs) / attribs.Size) * 100.0);
                                if (percent % 10 == 0)
                                {
                                    // This will prevent the progress message from being stuck on the screen.
                                    if (percent == 100)
                                    {
                                        return;
                                    }

                                    var progressRecord = new ProgressRecord(1,
                                    "Downloading " + fil.Name,
                                    String.Format("{0} Bytes Downloaded of {1}", rs, attribs.Size)) { PercentComplete = percent };

                                    Host.UI.WriteProgress(1, progressRecord);
                                    
                                }
                            }
                        });
                       
                        var present = File.Exists(localfilefullpath);
                        
                        if ((present & _overwrite) || (!present))
                        {
                            var localstream = File.Create(@localfilefullpath);
                            try
                            {
                                
                                sftpSession.Session.DownloadFile(_remotefile, localstream, res);
                                localstream.Close();

                            }
                            catch
                            {
                                localstream.Close();
                                var ex = new SftpPermissionDeniedException("Unable to download file from host.");
                                ThrowTerminatingError(new ErrorRecord(
                                    ex,
                                    "Unable to download file from host.",
                                    ErrorCategory.InvalidOperation,
                                    sftpSession));
                            }
                        }
                        else
                        {
                            var ex = new SftpPermissionDeniedException("File already present on local host.");
                            WriteError(new ErrorRecord(
                                             ex,
                                             "File already present on local host.",
                                             ErrorCategory.InvalidOperation,
                                             sftpSession));
                        }

                    }
                    else
                    {
                        var ex = new SftpPathNotFoundException(RemoteFile + " does not exist.");
                       ThrowTerminatingError(new ErrorRecord(
                                                ex,
                                                RemoteFile + " does not exist.",
                                                ErrorCategory.InvalidOperation,
                                                sftpSession));
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
    }
}
