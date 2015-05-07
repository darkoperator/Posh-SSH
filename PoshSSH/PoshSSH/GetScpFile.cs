using Renci.SshNet;
using Renci.SshNet.Common;
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
    public class GetScpFile : PSCmdlet
    {
        // Hosts to conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return _computername; }
            set { _computername = value; }
        }
        private string[] _computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return _credential; }
            set { _credential = value; }
        }
        private PSCredential _credential;

        // Port for SSH
        private Int32 _port = 22;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }
        }


        //Proxy Server to use
        private String _proxyserver = "";

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return _proxyserver; }
            set { _proxyserver = value; }
        }

        // Proxy Port
        private Int32 _proxyport = 8080;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return _proxyport; }
            set { _proxyport = value; }
        }


        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return _proxycredential; }
            set { _proxycredential = value; }
        }
        private PSCredential _proxycredential;

        // Proxy Type
        private string _proxytype = "HTTP";

        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return _proxytype; }
            set { _proxytype = value; }
        }

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return _keyfile; }
            set { _keyfile = value; }
        }
        private String _keyfile = "";

        //Local File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
        }
        private String _localfile = "";

        //Remote File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]

        public String RemoteFile
        {
            get { return _remotefile; }
            set { _remotefile = value; }
        }
        private String _remotefile = "";

        // OperationTimeout Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int OperationTimeout
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }
        private int _operationtimeout = 5;

        // ConnectionTimeout Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }
        private int _connectiontimeout = 5;

        // Auto Accept key fingerprint
        private bool _acceptkey;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }

        // Variable to hold the host/fingerprint information
        private Dictionary<string, string> _sshHostKeys;

        protected override void BeginProcessing()
        {
            // Collect host/fingerprint information from the registry.
            base.BeginProcessing();
            var keymng = new TrustedKeyMng();
            _sshHostKeys = keymng.GetKeys();
        }

        protected override void ProcessRecord()
        {
            foreach (var computer in _computername)
            {
                ConnectionInfo connectInfo;
                if (_keyfile.Equals(""))
                {
                    WriteVerbose("Using SSH Username and Password authentication for connection.");
                    var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.UserName);
                    connectInfo = ConnectionInfoGenerator.GetCredConnectionInfo(computer,
                        _port,
                        _credential,
                        _proxyserver,
                        _proxytype,
                        _proxyport,
                        _proxycredential,
                        kIconnectInfo);

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };

                }
                else
                {
                    WriteVerbose("Using SSH Key authentication for connection.");
                    connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                        _port,
                        _keyfile,
                        _credential,
                        _proxyserver,
                        _proxytype,
                        _proxyport,
                        _proxycredential);
                }

                //Ceate instance of SSH Client with connection info
                var client = new ScpClient(connectInfo);


                // Handle host key
                var computer1 = computer;
                client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                {
                    var sb = new StringBuilder();
                    foreach (var b in e.FingerPrint)
                    {
                        sb.AppendFormat("{0:x}:", b);
                    }
                    var fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                    if (_sshHostKeys.ContainsKey(computer1))
                    {
                        if (_sshHostKeys[computer1] == fingerPrint)
                        {
                            if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                            {
                                Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerprint for host " + computer);
                            }

                            e.CanTrust = true;
                        }
                        else
                        {
                            throw new System.Security.SecurityException("SSH fingerprint mismatch for host " + computer1);
                        }
                    }
                    else
                    {
                        int choice;
                        if (_acceptkey)
                        {
                            choice = 0;
                        }
                        else
                        {
                            var choices = new Collection<ChoiceDescription>
                                {
                                    new ChoiceDescription("Y"),
                                    new ChoiceDescription("N")
                                };

                            choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);
                        }
                        if (choice == 0)
                        {
                            var keymng = new TrustedKeyMng();
                            keymng.SetKey(computer1, fingerPrint);
                            e.CanTrust = true;
                        }
                        else
                        {
                            e.CanTrust = false;
                        }
                    }
                };
                // Set the connection timeout
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_connectiontimeout);

                // Connect to host using Connection info
                client.Connect();

                var counter = 0;
                // Print progess of download.
                client.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                {
                    if (e.Size != 0)
                    {
                        counter ++;
                        if (counter > 900)
                        {
                            var percent = Convert.ToInt32((e.Downloaded*100)/e.Size);
                            if (percent == 100)
                            {
                                return;
                            }

                            var progressRecord = new ProgressRecord(1, 
                                "Downloading " + e.Filename, 
                                String.Format("{0} Bytes Downloaded of {1}", 
                                e.Downloaded, e.Size)) {PercentComplete = percent};

                            Host.UI.WriteProgress(1, progressRecord);
                            counter = 0;
                        }
                    }
                };
                WriteVerbose("Connection successful");
                var localfullPath = Path.GetFullPath(_localfile);

                WriteVerbose("Downloading " + _remotefile);
                var fil = new FileInfo(@localfullPath);

                // Download the file
                client.Download(_remotefile, fil);

                client.Disconnect();
            }

        } // End process record

    } //end of the class for the Get-SCPFile
    ////###################################################

}
