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
    [Cmdlet(VerbsCommon.Set, "SCPItem", DefaultParameterSetName = "NoKey")]
    public class SetScpItem : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SCP;
            }
        }

        //Local Item
        private String _localpath = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = "Path of the item to upload.")]
        [Alias("FullName")]
        public String Path
        {
            get { return _localpath; }
            set { _localpath = value; }
        }


        //Remote Item
        private String _remotepath = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            HelpMessage = "Path on the remote system where to copy the Item.")]
        public String Destination
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }


        // New name for the item at the destination.
        private String _newname = "";
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "New name for the item on the destination path.")]
        public String NewName
        {
            get { return _newname; }
            set { _newname = value; }
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

                        ProviderInfo provider;
                        var pathinfo = GetResolvedProviderPathFromPSPath(_localpath, out provider);
                        var localfullPath = pathinfo[0];
                        var fil = new FileInfo(@localfullPath);
                        var dirinfo = new DirectoryInfo(@localfullPath);
                        var remoteFullpath = "";
                        var localname = "";

                        if (fil.Exists || dirinfo.Exists)
                        {
                            WriteVerbose("Uploading: " + localfullPath);

                            if (fil.Exists)
                            {
                                localname = fil.Name;
                            }
                            else {
                                localname = dirinfo.Name;
                            }
                            // Set the proper name for the file on the target.
                            if (String.IsNullOrEmpty(_newname))
                            {
                                remoteFullpath = Destination.TrimEnd(new[] { '/' }) + "/" + localname;
                            }
                            else
                            {
                                remoteFullpath = Destination.TrimEnd(new[] { '/' }) + "/" + _newname;
                            }

                            WriteVerbose("Destination: " + remoteFullpath);
                            if (fil.Exists)
                            {
                                client.Upload(fil, remoteFullpath);
                            }
                            else
                            {
                                client.Upload(dirinfo, remoteFullpath);
                            }

                            client.Disconnect();
                        }
                        else
                        {
                            var ex = new FileNotFoundException("Item to upload " + localfullPath + " was not found.");

                            ThrowTerminatingError(new ErrorRecord(
                                                    ex,
                                                    "Item to upload " + localfullPath + " was not found.",
                                                    ErrorCategory.InvalidOperation,
                                                    localfullPath));

                        } // check if file exists.

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
