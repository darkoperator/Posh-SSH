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
    [Cmdlet(VerbsCommon.Get, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class GetScpFolder : NewSessionBase
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
            ValueFromPipelineByPropertyName = true)]
        public String LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        private String _localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public String RemoteFolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }
        private String _remotefolder = "";

        // Supress progress bar.
        private bool _noProgress = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
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

                            client.Downloading += delegate (object sender, ScpDownloadEventArgs e)
                            {
                                if (e.Size != 0)
                                {
                                    counter++;
                                    if (counter > 900)
                                    {
                                        var percent = Convert.ToInt32((e.Downloaded * 100) / e.Size);
                                        if (percent == 100)
                                        {
                                            return;
                                        }

                                        var progressRecord = new ProgressRecord(1,
                                            "Downloading " + e.Filename,
                                            String.Format("{0} Bytes Downloaded of {1}",
                                            e.Downloaded, e.Size))
                                        { PercentComplete = percent };

                                        Host.UI.WriteProgress(1, progressRecord);
                                        counter = 0;
                                    }
                                }
                            };
                        }
                        WriteVerbose("Connection successful");

                        var localfullPath = Path.GetFullPath(_localfolder);
                        WriteVerbose("Downloading " + _remotefolder);
                        var dirinfo = new DirectoryInfo(@localfullPath);
                        client.Download(_remotefolder, dirinfo);
                        WriteVerbose("Finished downloading.");

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

    } //end of the class for the Get-SCPFile
    ////###################################################
}
