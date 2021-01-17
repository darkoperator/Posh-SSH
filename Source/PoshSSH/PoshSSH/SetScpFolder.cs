using Renci.SshNet;
using Renci.SshNet.Common;
using SSH.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace SSH
{
    [Cmdlet(VerbsCommon.Set, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class SetScpFolder : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SCP;
            }
        }

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2)]
        public String LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        private String _localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 3)]
        public String RemoteFolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }
        private String _remotefolder = "";

        // Supress progress bar.
        private bool _noProgress = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter NoProgress
        {
            get { return _noProgress; }
            set { _noProgress = value; }
        }

        protected override void ProcessRecord()
        {
            foreach (var computer in ComputerName)
            {
                var client = CreateConnection(computer) as ScpClient;
                try
                {
                    if (client != default && client.IsConnected)
                    {
                        var _progresspreference = (ActionPreference)this.SessionState.PSVariable.GetValue("ProgressPreference");
                        if (_noProgress == false)
                        {
                            var counter = 0;
                            // Print progess of download.

                            client.Uploading += delegate (object sender, ScpUploadEventArgs e)
                            {
                                if (e.Size != 0)
                                {
                                    counter++;

                                    if (counter > 900)
                                    {
                                        var percent = Convert.ToInt32((e.Uploaded * 100) / e.Size);

                                        if (percent == 100)
                                        {
                                            return;
                                        }

                                        var progressRecord = new ProgressRecord(1,
                                            "Uploading " + e.Filename,
                                            String.Format("{0} Bytes Uploaded of {1}",
                                            e.Uploaded, e.Size))
                                        { PercentComplete = percent };

                                        Host.UI.WriteProgress(1, progressRecord);
                                        counter = 0;
                                    }
                                }
                            };
                        }
                        WriteVerbose("Connection successful");

                        client.BufferSize = 1024;

                        // Resolve the path even if a relative one is given.
                        ProviderInfo provider;
                        var pathinfo = GetResolvedProviderPathFromPSPath(_localfolder, out provider);
                        var localfullPath = pathinfo[0];

                        //var localfullPath = Path.GetFullPath(_localfolder);
                        if (Directory.Exists(localfullPath))
                        {
                            try
                            {
                                WriteVerbose("Uploading " + _remotefolder);
                                var dirinfo = new DirectoryInfo(@localfullPath);
                                client.Upload(dirinfo, _remotefolder);
                            }
                            catch (Exception e)
                            {
                                ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                                WriteError(erec);
                            }

                        }
                        else
                        {
                            var ex = new DirectoryNotFoundException("Directory " + localfullPath + " was not found.");
                            WriteError(new ErrorRecord(ex,
                                                       "Directory " + localfullPath + " was not found.",
                                                       ErrorCategory.InvalidArgument,
                                                       localfullPath));
                        }

                        client.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.OperationStopped, client);
                    WriteError(erec);
                }
            }

        } // End process record

    } //end of the class for the Set-SCPFile
}
