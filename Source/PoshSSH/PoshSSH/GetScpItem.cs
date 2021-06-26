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

    [Cmdlet(VerbsCommon.Get, "SCPItem", DefaultParameterSetName = "NoKey")]
    public class GetScpItem : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SCP;
            }
        }

        //Local Path
        private String _localpath = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the location where to download the item to.")]
        public String Destination
        {
            get { return _localpath; }
            set { _localpath = value; }
        }


        //Remote Path
        private String _remotepath = "";
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to item on the remote host to download.")]
        public String Path
        {
            get { return _remotepath; }
            set { _remotepath = value; }
        }

        // Path Type
        private String _pathtype = "";
        [ValidateSet("File", "Directory", IgnoreCase = true)]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "What type of Item you are getting from the remote host via SCP.")]
        public string PathType
        {
            get { return _pathtype; }
            set { _pathtype = value; }
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

        private string _pathTransformation = "none";
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = false,
            HelpMessage = "Remote Path transormation to use.")]
        [ValidateSet("ShellQuote", "None", "DoubleQuote", IgnoreCase = true)]
        public string PathTransformation
        {
            get { return _pathTransformation; }
            set { _pathTransformation = value; }
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
                        switch (PathTransformation.ToLower())
                        {
                            case "shellquote":
                                client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                                break;
                            case "none":
                                client.RemotePathTransformation = RemotePathTransformation.None;
                                break;
                            case "doublequote":
                                client.RemotePathTransformation = RemotePathTransformation.DoubleQuote;
                                break;
                        }
                        WriteVerbose("Connection successful");

                        WriteVerbose("Downloading " + _remotepath);
                        // Get file/directory name for use when downloading the file. 
                        var localname = "";
                        var destinationpath = "";
                        var localfullPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localpath);

                        if (String.IsNullOrEmpty(_newname))
                        {
                            localname = new DirectoryInfo(@_remotepath).Name;
                        }
                        else
                        {
                            localname = _newname;
                        }
                        destinationpath = (localfullPath.TrimEnd('/', '\\')) + System.IO.Path.DirectorySeparatorChar + localname;

                        string curName = "";
                        var progressHelper = new OperationProgressHelper(this, "Download", curName, 0, 1);
                        if (progressHelper.IsProgressVisible)
                        {
                            client.Downloading += delegate (object sender, ScpDownloadEventArgs e)
                            {
                                if (e.Filename != curName)
                                {
                                    progressHelper.Complete();
                                    curName = e.Filename;
                                    progressHelper = new OperationProgressHelper(this, "Download", curName, e.Size, 1);
                                }
                                progressHelper.Callback?.Invoke((ulong)e.Downloaded);
                            };
                        }
                        if (String.Equals(_pathtype, "File", StringComparison.OrdinalIgnoreCase))
                        {
                            WriteVerbose("File name " + localname);
                            WriteVerbose("Item type selected: File");

                            WriteVerbose("Saving as " + destinationpath);

                            var fil = new FileInfo(@destinationpath);

                            if (fil.Exists && !this.MyInvocation.BoundParameters.ContainsKey("Force"))
                            {
                                var e = new IOException("File " + localname + " already exists.");
                                ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                                WriteError(erec);
                            }
                            else
                            {
                                if (fil.Exists)
                                {
                                    WriteWarning("Overwritting " + destinationpath);
                                    File.Delete(destinationpath);
                                }
                                // Download the file
                                client.Download(_remotepath, fil);
                                progressHelper.Complete();
                            }
                        }
                        else
                        {
                            WriteVerbose("Item type selected: Directory");
                            Directory.CreateDirectory(destinationpath);

                            WriteVerbose("Destination: " + destinationpath);

                            var dirinfo = new DirectoryInfo(@destinationpath);
                            client.Download(_remotepath, dirinfo);
                            progressHelper.Complete();

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
    }
}
