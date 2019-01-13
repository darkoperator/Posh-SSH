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

    [Cmdlet(VerbsCommon.Get, "SCPItem", DefaultParameterSetName = "NoKey")]
    public class GetScpItem : PSCmdlet
    {
        // Hosts to conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = "FQDN or IP Address of host to establish a SSH connection.")]
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
            HelpMessage = "SSH Credentials to use for connecting to a server. If a key file is used the password field is used for the Key pass phrase.")]
        [System.Management.Automation.CredentialAttribute()]
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
            HelpMessage = "SSH TCP Port number to use for the SSH connection.")]
        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }
        }

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Proxy server name or IP Address to use for connection.")]
        public String ProxyServer
        {
            get { return _proxyserver; }
            set { _proxyserver = value; }
        }
        private String _proxyserver = "";

        // Proxy Port
        private Int32 _proxyport = 8080;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Port to connect to on proxy server to route connection.")]
        public Int32 ProxyPort
        {
            get { return _proxyport; }
            set { _proxyport = value; }
        }

        // Proxy Credentials
        private PSCredential _proxycredential;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerShell Credential Object with the credentials for use to connect to proxy server if required.")]
        [ValidateNotNullOrEmpty]
        [System.Management.Automation.CredentialAttribute()]
        public PSCredential ProxyCredential
        {
            get { return _proxycredential; }
            set { _proxycredential = value; }
        }

        // Proxy Type
        private string _proxytype = "HTTP";
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Type of Proxy being used (HTTP, Socks4 or Socks5).")]
        public string ProxyType
        {
            get { return _proxytype; }
            set { _proxytype = value; }
        }

        //SSH Key File
        private String _keyfile = "";
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key",
            HelpMessage = "OpenSSH format SSH private key file.")]
        public String KeyFile
        {
            get { return _keyfile; }
            set { _keyfile = value; }
        }

        //SSH Key Content
        private string[] _keystring = new string[] { };

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString",
            HelpMessage = "String array of the content of a OpenSSH key file.")]
        public string[] KeyString
        {
            get { return _keystring; }
            set { _keystring = value; }
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

        // OperationTimeout Parameter
        private int _operationtimeout = 5;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Timeout for execution of an operation.")]
        public int OperationTimeout
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }

        // ConnectionTimeout Parameter
        private int _connectiontimeout = 5;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Connection timeout interval.")]
        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }

        // Auto Accept key fingerprint
        private bool _acceptkey;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Auto add host key fingerprint to the list of trusted host/gingerprint pairs.")]
        public bool AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }

        // Do not check server fingerprint.
        private bool _force = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Do not check the remote host fingerprint.")]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        // Automatically error if key is not trusted.
        private bool _errorOnUntrusted = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Raise an exception if the fingerprint is not trusted for the host.")]
        public SwitchParameter ErrorOnUntrusted
        {
            get { return _errorOnUntrusted; }
            set { _errorOnUntrusted = value; }
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

        // Variable to hold the host/fingerprint information
        private Dictionary<string, string> _sshHostKeys;

        

        protected override void BeginProcessing()
        {
            // Collect host/fingerprint information from the registry if connection is not forced.
            if (!_force)
            {
                base.BeginProcessing();
                var keymng = new TrustedKeyMng();
                _sshHostKeys = keymng.GetKeys();
            }
        }

        protected override void ProcessRecord()
        {
            foreach (var computer in _computername)
            {
                ConnectionInfo connectInfo = null;
                switch (ParameterSetName)
                {
                    case "NoKey":
                        WriteVerbose("Using SSH Username and Password authentication for connection.");
                        var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.UserName);
                        connectInfo = ConnectionInfoGenerator.GetCredConnectionInfo(computer,
                            _port,
                            _credential,
                            _proxyserver,
                            _proxytype,
                            _proxyport,
                            ProxyCredential,
                            kIconnectInfo);

                        // Event Handler for interactive Authentication
                        kIconnectInfo.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = _credential.GetNetworkCredential().Password;
                            }
                        };
                        break;

                    case "Key":
                        ProviderInfo provider;
                        var pathinfo = GetResolvedProviderPathFromPSPath(_keyfile, out provider);
                        var localfullPath = pathinfo[0];
                        connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                            _port,
                            localfullPath,
                            _credential,
                            _proxyserver,
                            _proxytype,
                            _proxyport,
                            ProxyCredential);
                        break;

                    case "KeyString":
                        WriteVerbose("Using SSH Key authentication for connection.");
                        connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                            _port,
                            _keystring,
                            _credential,
                            _proxyserver,
                            _proxytype,
                            _proxyport,
                            ProxyCredential);
                        break;

                    default:
                        break;
                }

                //Ceate instance of SSH Client with connection info
                var client = new ScpClient(connectInfo);
                // Set the connection timeout
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_connectiontimeout);

                // Handle host key
                if (_force)
                {
                    WriteWarning("Host key for " + computer + " is not being verified since Force switch is used.");
                }
                else
                {
                    var computer1 = computer;
                    client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                    {

                        var sb = new StringBuilder();
                        foreach (var b in e.FingerPrint)
                        {
                            sb.AppendFormat("{0:x}:", b);
                        }
                        var fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                        if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                        {
                            Host.UI.WriteVerboseLine("Fingerprint for " + computer1 + ": " + fingerPrint);
                        }

                        if (_sshHostKeys.ContainsKey(computer1))
                        {
                            if (_sshHostKeys[computer1] == fingerPrint)
                            {
                                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                                {
                                    Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerprint for host " + computer1);
                                }
                                e.CanTrust = true;

                            }
                            else
                            {
                                e.CanTrust = false;

                            }
                        }
                        else
                        {
                            if (_errorOnUntrusted)
                            {
                                e.CanTrust = false;
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
                        }
                    };
                }
                try
                {
                    // Connect to host using Connection info
                    client.Connect();

                }
                catch (Renci.SshNet.Common.SshConnectionException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (Renci.SshNet.Common.SshOperationTimeoutException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.OperationTimeout, client);
                    WriteError(erec);
                }
                catch (Renci.SshNet.Common.SshAuthenticationException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (Exception e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                    WriteError(erec);
                }

                try
                {
                    if (client.IsConnected)
                    {
                        if (String.Equals(_pathtype, "File", StringComparison.OrdinalIgnoreCase))
                        {
                            WriteVerbose("Item type selected: File");
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

                            // Get file name for use when downloading the file. 
                            var filename = "";
                            var destinationpath = "";
                            var localfullPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localpath);

                            if (String.IsNullOrEmpty(_newname))
                            {
                                filename = new DirectoryInfo(@_remotepath).Name;
                            }
                            else
                            {
                                filename = _newname;

                            }
                            destinationpath = (localfullPath.TrimEnd('/', '\\')) + System.IO.Path.DirectorySeparatorChar + filename;
                            WriteVerbose("Downloading " + _remotepath);
                            WriteVerbose("Saving as " + destinationpath);
                            var fil = new FileInfo(@destinationpath);

                            // Download the file
                            client.Download(_remotepath, fil);

                            client.Disconnect();
                        }
                        else
                        {
                            WriteVerbose("Item type selected: Directory");

                            var counter = 0;
                            // Print progess of download.
                            if (!_noProgress)
                            {
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
                                                    e.Downloaded,
                                                    e.Size))
                                            { PercentComplete = percent };

                                            Host.UI.WriteProgress(1, progressRecord);
                                            counter = 0;
                                        }
                                    }
                                };
                            }

                            try
                            {
                                // Get directory name for use when downloading the file. 
                                var dirname = "";
                                var destinationpath = "";
                                var localfullPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localpath);

                                if (String.IsNullOrEmpty(_newname))
                                {
                                    dirname = new DirectoryInfo(@_remotepath).Name;
                                }
                                else
                                {
                                    dirname = _newname;

                                }
                                destinationpath = (localfullPath.TrimEnd('/', '\\')) + System.IO.Path.DirectorySeparatorChar + dirname;

                                Directory.CreateDirectory(destinationpath);
                                WriteVerbose("Downloading: " + _remotepath);
                                WriteVerbose("Destination: " + destinationpath);
                                var dirinfo = new DirectoryInfo(@destinationpath);
                                client.Download(_remotepath, dirinfo);
                                WriteVerbose("Finished downloading.");
                            }
                            catch (Exception e)
                            {
                                ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                                WriteError(erec);
                            }
                            client.Disconnect();
                        }
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
