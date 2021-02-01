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
    [Cmdlet(VerbsCommon.Set, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class SetScpFile : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SCP;
            }
        }

        //Local File
        private String _localfile = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2)]
        [Alias("FullName")]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
        }


        //Remote File
        private String _remotepath = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 3)]
        public String RemotePath
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }

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

                        // Resolve the path even if a relative one is given.
                        ProviderInfo provider;
                        var pathinfo = GetResolvedProviderPathFromPSPath(_localfile, out provider);
                        var localfullPath = pathinfo[0];

                        if (File.Exists(@localfullPath))
                        {
                            try
                            {
                                WriteVerbose("Uploading " + localfullPath);
                                var fil = new FileInfo(@localfullPath);
                                var remoteFullpath = RemotePath.TrimEnd(new[] { '/' }) + "/" + fil.Name;
                                client.Upload(fil, remoteFullpath);

                                client.Disconnect();
                            }
                            catch (Exception e)
                            {
                                ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                                WriteError(erec);
                            }
                        }
                        else
                        {
                            var ex = new FileNotFoundException("File to upload " + localfullPath + " was not found.");

                            WriteError(new ErrorRecord(ex,
                                                        "File to upload " + localfullPath + " was not found.",
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
    ////###################################################
}
