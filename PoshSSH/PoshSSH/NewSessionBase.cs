using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace SSH
{
    public abstract class NewSessionBase : PSCmdlet
    {
        /// <summary>
        /// Desired Protocol. Should be SSH or SFTP
        /// </summary>
        internal abstract string Protocol { get; }

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
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "KeyString")]
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
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "KeyString")]
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
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
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
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
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
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        public Int32 ProxyPort
        {
            get { return _proxyport; }
            set { _proxyport = value; }
        }


        // Proxy Credentials
        private PSCredential _proxycredential;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
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
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return _proxytype; }
            set { _proxytype = value; }
        }

        //SSH Key File
        private string _keyfile = null;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public string KeyFile
        {
            get { return _keyfile; }
            set { _keyfile = value; }
        }

        //SSH Key Content
        private string[] _keystring = new string[] { };

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        public string[] KeyString
        {
            get { return _keystring; }
            set { _keystring = value; }
        }

        // ConnectionTimeout Parameter
        private int _connectiontimeout = 10;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }

        // OperationTimeout Parameter
        private int _operationtimeout = 5;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int OperationTimeout
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }

        // KeepAliveInterval Parameter
        private int _keepaliveinterval = 10;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int KeepAliveInterval
        {
            get { return _keepaliveinterval; }
            set { _keepaliveinterval = value; }
        }


        // Auto Accept key fingerprint
        private bool _acceptkey;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }

        // Do not check server fingerprint.
        private bool _force;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        // Automatically error if key is not trusted.
        private bool _errorOnUntrusted;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter ErrorOnUntrusted
        {
            get { return _errorOnUntrusted; }
            set { _errorOnUntrusted = value; }
        }
        // Variable to hold the host/fingerprint information
        private Dictionary<string, string> _sshHostKeys;

        protected override void BeginProcessing()
        {
            // no need to validate keys if the force parameter is selected.
            if ( !_force)
            {
                // Collect host/fingerprint information from the registry.
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
                            _proxycredential,
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
                            _proxycredential);
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
                            _proxycredential);
                        break;

                    default:
                        break;
                }

                //Ceate instance of SSH Client with connection info
                BaseClient client;
                if (Protocol == "SSH")
                    client = new SshClient(connectInfo);
                else
                    client = new SftpClient(connectInfo);


                // Handle host key
                if (_force)
                {
                    WriteWarning("Host key is not being verified since Force switch is used.");
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
                            e.CanTrust = _sshHostKeys[computer1] == fingerPrint;
                            if (e.CanTrust && MyInvocation.BoundParameters.ContainsKey("Verbose"))
                                Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerprint for host " + computer1);
                        }
                        else
                        {
                            if (_errorOnUntrusted)
                            {
                                e.CanTrust = false;
                            }
                            else
                            {
                                if (!_acceptkey)
                                {
                                    var choices = new Collection<ChoiceDescription>
                                    {
                                        new ChoiceDescription("Y"),
                                        new ChoiceDescription("N")
                                    };
                                    e.CanTrust = 0 == Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);
                                }
                                else
                                    e.CanTrust = true;
                                if (e.CanTrust)
                                {
                                    var keymng = new TrustedKeyMng();
                                    keymng.SetKey(computer1, fingerPrint);
                                }
                                    
                            }
                        }
                    };
                }
                try
                {
                    // Set the connection timeout
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_connectiontimeout);

                    // Set Keepalive for connections
                    client.KeepAliveInterval = TimeSpan.FromSeconds(_keepaliveinterval);

                    // Connect to host using Connection info
                    client.Connect();

                    if (Protocol == "SSH")
                        WriteObject(SshModHelper.AddToSshSessionCollection(client as SshClient, SessionState), true);
                    else
                        WriteObject(SshModHelper.AddToSftpSessionCollection(client as SftpClient, SessionState), true);
                }
                catch (SshConnectionException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (SshOperationTimeoutException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.OperationTimeout, client);
                    WriteError(erec);
                }
                catch (SshAuthenticationException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (Exception e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                    WriteError(erec);
                }

                // Renci.SshNet.Common.SshOperationTimeoutException when host is not alive or connection times out.
                // Renci.SshNet.Common.SshConnectionException when fingerprint mismatched
                // Renci.SshNet.Common.SshAuthenticationException Bad password
            }

        } // End process record
    }
}
