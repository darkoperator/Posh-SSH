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
    [Cmdlet(VerbsCommon.New, "SSHSession", DefaultParameterSetName = "NoKey")]
    public class NewSshSession : PSCmdlet
    {
        // Hosts tp conect to
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
        private String _keyfile = "";

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return _keyfile; }
            set { _keyfile = value; }
        }

        // ConnectionTimeOut Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int ConnectionTimeOut
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }
        private int _connectiontimeout = 10;

        // KeepAliveInterval Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int KeepAliveInterval
        {
            get { return _keepaliveinterval; }
            set { _keepaliveinterval = value; }
        }
        private int _keepaliveinterval = 10;

        // Auto Accept key fingerprint
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public bool AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }
        private bool _acceptkey;

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
            if (_keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################
                var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                foreach (var computer in _computername)
                {
                    ConnectionInfo connectInfo;
                    if (_proxyserver != "")
                    {
                        // Set the proper proxy type
                        var ptype = ProxyTypes.Http;
                        WriteVerbose("A Proxy Server has been specified");
                        switch (_proxytype)
                        {
                            case "HTTP":
                                ptype = ProxyTypes.Http;
                                break;
                            case "Socks4":
                                ptype = ProxyTypes.Socks4;
                                break;
                            case "Socks5":
                                ptype = ProxyTypes.Socks5;
                                break;
                        }

                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            ptype,
                            _proxyserver,
                            _proxyport,
                            _proxycredential.GetNetworkCredential().UserName,
                            _proxycredential.GetNetworkCredential().Password,
                            kIconnectInfo,
                            passconnectInfo);



                    }
                    else
                    {
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive

                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);


                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                                    _port,
                                    _credential.GetNetworkCredential().UserName,
                                    passconnectInfo,
                                    kIconnectInfo);


                        //} // End foroeach computer
                    }

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };


                    //Ceate instance of SSH Client with connection info
                    var client = new SshClient(connectInfo);

                    // Handle host key
                    string computer1 = computer;
                    client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                    {
                        var sb = new StringBuilder();
                        foreach (var b in e.FingerPrint)
                        {
                            sb.AppendFormat("{0:x}:", b);
                        }
                        string fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);
                        //this.Host.UI.WriteVerboseLine("Key algorithm of " + Client.ConnectionInfo.CurrentHostKeyAlgorithm);
                        //this.Host.UI.WriteVerboseLine("Key exchange alhorithm " + Client.ConnectionInfo.CurrentKeyExchangeAlgorithm);
                        //this.Host.UI.WriteVerboseLine("Host key fingerprint: " + FingerPrint);
                        if (_sshHostKeys.ContainsKey(computer1))
                        {
                            if (_sshHostKeys[computer1] == fingerPrint)
                            {
                                //this.Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerpring for host " + computer);
                                e.CanTrust = true;
                            }
                            else
                            {
                                throw new System.Security.SecurityException("SSH fingerprint mistmatch for host " + computer1);
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
                                //this.Host.UI.WriteVerboseLine("Saving fingerprint " + FingerPrint + " for host " + computer);
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

                    // Set Keepalive for connections
                    client.KeepAliveInterval = TimeSpan.FromSeconds(_keepaliveinterval);

                    // Connect to  host using Connection info
                    client.Connect();
                    WriteObject(SshModHelper.AddToSshSessionCollection(client, SessionState), true);
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(_keyfile);
                if (File.Exists(fullPath))
                {
                    foreach (var computer in _computername)
                    {
                        PrivateKeyConnectionInfo connectionInfo;
                        if (_proxyserver != "")
                        {
                            // Set the proper proxy type
                            var ptype = ProxyTypes.Http;
                            WriteVerbose("A Proxy Server has been specified");
                            switch (_proxytype)
                            {
                                case "HTTP":
                                    ptype = ProxyTypes.Http;
                                    break;
                                case "Socks4":
                                    ptype = ProxyTypes.Socks4;
                                    break;
                                case "Socks5":
                                    ptype = ProxyTypes.Socks5;
                                    break;
                            }

                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _port, _credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);

                                if (_proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        _port,
                                        _credential.GetNetworkCredential().UserName,
                                        ptype,
                                        _proxyserver,
                                        _proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        _port,
                                        _credential.GetNetworkCredential().UserName,
                                        ptype,
                                        _proxyserver,
                                        _proxyport,
                                        _proxycredential.GetNetworkCredential().UserName,
                                        _proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                        }
                        else
                        {
                            WriteVerbose("Using SSH Key authentication for connection.");
                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _credential.GetNetworkCredential().UserName, sshkey);
                            }


                        }
                        //Ceate instance of SSH Client with connection info
                        var client = new SshClient(connectionInfo);

                        // Handle host key
                        string computer1 = computer;
                        client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                        {
                            var sb = new StringBuilder();
                            foreach (var b in e.FingerPrint)
                            {
                                sb.AppendFormat("{0:x}:", b);
                            }
                            string fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                            if (_sshHostKeys.ContainsKey(computer1))
                            {
                                if (_sshHostKeys[computer1] == fingerPrint)
                                {
                                    //this.Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerpring for host " + computer);
                                    e.CanTrust = true;
                                }
                                else
                                {
                                    throw new System.Security.SecurityException("SSH fingerprint mistmatch for host " + computer1);
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
                                    //this.Host.UI.WriteVerboseLine("Saving fingerprint " + FingerPrint + " for host " + computer);
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

                        // Set Keepalive for connections
                        client.KeepAliveInterval = TimeSpan.FromSeconds(_keepaliveinterval);

                        // Connect to  host using Connection info
                        client.Connect();
                        WriteObject(SshModHelper.AddToSshSessionCollection(client, SessionState), true);
                    } // for each computer
                } // file exists
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }
            }

        } // End process record
    } //end of the class for the New-SSHSession
    //###################################################


    [Cmdlet(VerbsCommon.New, "SFTPSession", DefaultParameterSetName = "NoKey")]
    public class NewSftpSession : PSCmdlet
    {
        // Hosts tp conect to
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
        private Int32 _port = 22;

        //Proxy Server to use
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
        private String _proxyserver = "";

        // Proxy Port
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
        private Int32 _proxyport = 8080;

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
        private string _proxytype = "HTTP";

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

        // OperationTimeout Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int OperationTimeOut
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }
        private int _operationtimeout = 5;

        // ConnectionTimeOut Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int ConnectionTimeOut
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }
        private int _connectiontimeout = 5;

        // KeepAliveInterval Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int KeepAliveInterval
        {
            get { return _keepaliveinterval; }
            set { _keepaliveinterval = value; }
        }
        private int _keepaliveinterval = 10;

        // Auto Accept key fingerprint
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public bool AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }
        private bool _acceptkey;

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
            if (_keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################
                var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                foreach (var computer in _computername)
                {
                    ConnectionInfo connectInfo;
                    if (_proxyserver != "")
                    {
                        // Set the proper proxy type
                        var ptype = ProxyTypes.Http;
                        WriteVerbose("A Proxy Server has been specified");
                        switch (_proxytype)
                        {
                            case "HTTP":
                                ptype = ProxyTypes.Http;
                                break;
                            case "Socks4":
                                ptype = ProxyTypes.Socks4;
                                break;
                            case "Socks5":
                                ptype = ProxyTypes.Socks5;
                                break;
                        }

                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            ptype,
                            _proxyserver,
                            _proxyport,
                            _proxycredential.GetNetworkCredential().UserName,
                            _proxycredential.GetNetworkCredential().Password,
                            kIconnectInfo,
                            passconnectInfo);



                    }
                    else
                    {
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive

                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);


                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            passconnectInfo,
                            kIconnectInfo);
                    }

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };


                    //Ceate instance of SFTP Client with connection info
                    var client = new SftpClient(connectInfo);

                    // Handle host key
                    string computer1 = computer;
                    client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                    {
                        var sb = new StringBuilder();
                        foreach (var b in e.FingerPrint)
                        {
                            sb.AppendFormat("{0:x}:", b);
                        }
                        string fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                        if (_sshHostKeys.ContainsKey(computer1))
                        {
                            if (_sshHostKeys[computer1] == fingerPrint)
                            {
                                e.CanTrust = true;
                            }
                            else
                            {
                                throw new System.Security.SecurityException("SSH fingerprint mistmatch for host " + computer1);
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

                    // Set Keepalive for connections
                    client.KeepAliveInterval = TimeSpan.FromSeconds(_keepaliveinterval);

                    // Connect to  host using Connection info
                    client.Connect();
                    WriteObject(SshModHelper.AddToSftpSessionCollection(client, SessionState), true);
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(_keyfile);
                if (File.Exists(fullPath))
                {
                    foreach (var computer in _computername)
                    {
                        PrivateKeyConnectionInfo connectionInfo;
                        if (_proxyserver != "")
                        {
                            // Set the proper proxy type
                            var ptype = ProxyTypes.Http;
                            WriteVerbose("A Proxy Server has been specified");
                            switch (_proxytype)
                            {
                                case "HTTP":
                                    ptype = ProxyTypes.Http;
                                    break;
                                case "Socks4":
                                    ptype = ProxyTypes.Socks4;
                                    break;
                                case "Socks5":
                                    ptype = ProxyTypes.Socks5;
                                    break;
                            }

                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);

                                if (_proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        _port,
                                        _credential.GetNetworkCredential().UserName,
                                        ptype,
                                        _proxyserver,
                                        _proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        _port,
                                        _credential.GetNetworkCredential().UserName,
                                        ptype,
                                        _proxyserver,
                                        _proxyport,
                                        _proxycredential.GetNetworkCredential().UserName,
                                        _proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                        }
                        else
                        {
                            WriteVerbose("Using SSH Key authentication for connection.");
                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _credential.GetNetworkCredential().UserName, sshkey);
                            }


                        }
                        //Ceate instance of SSH Client with connection info
                        var client = new SftpClient(connectionInfo);

                        // Handle host key
                        string computer1 = computer;
                        client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                        {
                            var sb = new StringBuilder();
                            foreach (var b in e.FingerPrint)
                            {
                                sb.AppendFormat("{0:x}:", b);
                            }
                            string fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                            if (_sshHostKeys.ContainsKey(computer1))
                            {
                                if (_sshHostKeys[computer1] == fingerPrint)
                                {
                                    //this.Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerpring for host " + computer);
                                    e.CanTrust = true;
                                }
                                else
                                {
                                    throw new System.Security.SecurityException("SSH fingerprint mistmatch for host " + computer1);
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

                        // Connect to  host using Connection info
                        client.Connect();
                        WriteObject(SshModHelper.AddToSftpSessionCollection(client, SessionState), true);
                    } // for each computer
                } // file exists
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }

            } // End process record
        }

    } //end of the class for the New-SFTPSession
    //###################################################

    // Object for SSH Sessions
    public class SshSession
    {
        public Int32 Index;
        public string Host;
        public SshClient Session;
        public bool Connected
        {
            get { return Session.IsConnected; }
        }

        // Method for Connecing
        public void Connect()
        {
            Session.Connect();
        }

        // Method for disconecting session
        public void Disconnect()
        {
            Session.Disconnect();
        }
    }

    // Object for SSTP Sessions
    public class SftpSession
    {
        public Int32 Index;
        public string Host;
        public SftpClient Session;
        public bool Connected
        {
            get { return Session.IsConnected; }
        }
        public void Disconnect()
        {
            Session.Disconnect();
        }

        // Method for Connecing
        public void Connect()
        {
            Session.Connect();
        }
    }
}
