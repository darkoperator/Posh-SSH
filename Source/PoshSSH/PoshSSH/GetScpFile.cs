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
    [Cmdlet(VerbsCommon.Get, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class GetScpFile : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SCP;
            }
        }

        //Local File<DirectedGraph xmlns="http://schemas.microsoft.com/vs/2009/dgml">
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
        }
        private String _localfile = "";

        //Remote File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public String RemoteFile
        {
            get { return _remotefile; }
            set { _remotefile = value; }
        }
        private String _remotefile = "";

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

                        var localfullPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localfile);

                        WriteVerbose("Downloading " + _remotefile);
                        WriteVerbose("Saving as " + localfullPath);
                        var fil = new FileInfo(@localfullPath);

                        // Download the file
                        client.Download(_remotefile, fil);

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
