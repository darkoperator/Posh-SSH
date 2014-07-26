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
    [Cmdlet(VerbsCommon.Set, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class SetScpFile : PSCmdlet
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
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            ParameterSetName = "Key")]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
        }
        private String _localfile = "";

        //Remote File
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            ParameterSetName = "Key")]
        public String RemoteFile
        {
            get { return _remotefile; }
            set { _remotefile = value; }
        }
        private String _remotefile = "";

        // OperationTimeout Parameter
        private int _operationtimeout = 5;

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
                foreach (var computer in _computername)
                {
                    #region AuthUserPass
                    //###########################################
                    //### Connect using Username and Password ###
                    //###########################################

                    ConnectionInfo connectInfo;
                    KeyboardInteractiveAuthenticationMethod kIconnectInfo;
                    if (_proxyserver != "")
                    {
                        #region Proxy
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

                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
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

                        #endregion
                    } // Proxy Server
                    else
                    {
                        #region No Proxy
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive
                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer, 
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            passconnectInfo,
                            kIconnectInfo);

                        #endregion
                    }// No Proxy

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };
                    //Ceate instance of SCP Client with connection info
                    var client = new ScpClient(connectInfo);

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
                            var choices = new Collection<ChoiceDescription>
                            {
                                new ChoiceDescription("Y"),
                                new ChoiceDescription("N")
                            };

                            int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                    // Set the Operation Timeout
                    client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                    // Connect to  host using Connection info
                    client.Connect();

                    // Print progess of download.
                    client.Uploading += delegate(object sender, ScpUploadEventArgs e)
                    {
                        var progressRecord = new ProgressRecord(1, "Uploading " + e.Filename, String.Format("{0} Bytes Uploaded of {1}", e.Uploaded, e.Size));
                        if (e.Size != 0)
                        {
                            progressRecord.PercentComplete = Convert.ToInt32((e.Uploaded * 100) / e.Size);

                            Host.UI.WriteProgress(1, progressRecord);
                        }
                    };

                    WriteVerbose("Connection succesfull");
                    var localfullPath = Path.GetFullPath(_localfile);
                    if (File.Exists(localfullPath))
                    {
                        WriteVerbose("Uploading " + localfullPath);
                        var fil = new FileInfo(@localfullPath);
                        client.Upload(fil, _remotefile);

                        client.Disconnect();
                    }
                    else
                    {
                        throw new FileNotFoundException("File to upload " + localfullPath + " was not found.");
                    }
                } //end foreach computer
                    #endregion
            } //Use/Password Auth
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
                                connectionInfo = new PrivateKeyConnectionInfo(computer, _port, _credential.GetNetworkCredential().UserName, sshkey);
                            }
                        }

                        //Ceate instance of SCP Client with connection info
                        var client = new ScpClient(connectionInfo);

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
                                var choices = new Collection<ChoiceDescription>
                                {
                                    new ChoiceDescription("Y"),
                                    new ChoiceDescription("N")
                                };

                                int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                        // Set the Operation Timeout
                        client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                        // Connect to  host using Connection info
                        client.Connect();
                        WriteVerbose("Connection succesfull");

                        // Print progess of download.
                        client.Uploading += delegate(object sender, ScpUploadEventArgs e)
                        {
                            var progressRecord = new ProgressRecord(1, "Uploading " + e.Filename, String.Format("{0} Bytes Uploaded of {1}", e.Uploaded, e.Size));

                            if (e.Size != 0)
                            {
                                progressRecord.PercentComplete = Convert.ToInt32((e.Uploaded * 100) / e.Size);

                                Host.UI.WriteProgress(1, progressRecord);
                            }
                        };
                        string localfullPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localfile);
                        if (File.Exists(localfullPath))
                        {
                            WriteVerbose("Uploading " + localfullPath);
                            var fil = new FileInfo(@localfullPath);
                            client.Upload(fil, _remotefile);

                            client.Disconnect();
                        }
                        else
                        {
                            throw new FileNotFoundException("File to upload " + localfullPath + " was not found.");
                        }
                    }

                }// file exist
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }
            }

        } // End process record


    } //end of the class for the Set-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Get, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class GetScpFile : PSCmdlet
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
                foreach (var computer in _computername)
                {
                    #region AuthUserPass
                    //###########################################
                    //### Connect using Username and Password ###
                    //###########################################

                    ConnectionInfo connectInfo;
                    KeyboardInteractiveAuthenticationMethod kIconnectInfo;
                    if (_proxyserver != "")
                    {
                        #region Proxy
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

                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
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

                        #endregion
                    } // Proxy Server
                    else
                    {
                        #region No Proxy
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive
                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            passconnectInfo,
                            kIconnectInfo);

                        #endregion
                    }// No Proxy

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };
                    //Ceate instance of SCP Client with connection info
                    var client = new ScpClient(connectInfo);

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
                            var choices = new Collection<ChoiceDescription>
                            {
                                new ChoiceDescription("Y"),
                                new ChoiceDescription("N")
                            };

                            int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                    // Set the Operation Timeout
                    client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                    // Connect to  host using Connection info
                    client.Connect();
                    client.BufferSize = 1024;

                    // Print progess of download.
                    client.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                    {
                        var progressRecord = new ProgressRecord(1, "Downloading " + e.Filename, String.Format("{0} Bytes Downloaded of {1}", e.Downloaded, e.Size));

                        if (e.Size != 0)
                        {
                            progressRecord.PercentComplete = Convert.ToInt32((e.Downloaded * 100) / e.Size);

                            Host.UI.WriteProgress(1, progressRecord);
                        }
                    };
                    WriteVerbose("Connection succesfull");
                    var localfullPath = Path.GetFullPath(_localfile);

                    WriteVerbose("Downloading " + _remotefile);
                    var fil = new FileInfo(@localfullPath);

                    // Download the file
                    client.Download(_remotefile, fil);

                    client.Disconnect();
                } //end foreach computer
                    #endregion
            } //Use/Password Auth
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
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
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

                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                        }

                        //Ceate instance of SCP Client with connection info
                        var client = new ScpClient(connectionInfo);

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
                                var choices = new Collection<ChoiceDescription>
                                {
                                    new ChoiceDescription("Y"),
                                    new ChoiceDescription("N")
                                };

                                int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                        // Set the Operation Timeout
                        client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                        // Connect to  host using Connection info
                        client.Connect();

                        client.BufferSize = 1024;

                        // Print progess of download.
                        client.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                        {
                            var progressRecord = new ProgressRecord(1, "Downloading " + e.Filename, String.Format("{0} Bytes Downloaded of {1}", e.Downloaded, e.Size));

                            if (e.Size != 0)
                            {
                                progressRecord.PercentComplete = Convert.ToInt32((e.Downloaded * 100) / e.Size);

                                Host.UI.WriteProgress(1, progressRecord);
                            }
                        };

                        WriteVerbose("Connection succesfull");
                        var localfullPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localfile);

                        WriteVerbose("Downloading " + _remotefile);
                        var fil = new FileInfo(@localfullPath);

                        // Download the file
                        client.Download(_remotefile, fil);

                        client.Disconnect();
                    }

                }// file exist
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }
            }

        } // End process record

    } //end of the class for the Get-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Get, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class GetScpFolder : PSCmdlet
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

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        private String _localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }
        private String _remotefolder = "";

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
                foreach (var computer in _computername)
                {
                    #region AuthUserPass
                    //###########################################
                    //### Connect using Username and Password ###
                    //###########################################

                    ConnectionInfo connectInfo;
                    KeyboardInteractiveAuthenticationMethod kIconnectInfo;
                    if (_proxyserver != "")
                    {
                        #region Proxy
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

                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
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

                        #endregion
                    } // Proxy Server
                    else
                    {
                        #region No Proxy
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive
                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer, 
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            passconnectInfo,
                            kIconnectInfo);

                        #endregion
                    }// No Proxy

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };
                    //Ceate instance of SCP Client with connection info
                    var client = new ScpClient(connectInfo);

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
                            var choices = new Collection<ChoiceDescription>
                            {
                                new ChoiceDescription("Y"),
                                new ChoiceDescription("N")
                            };

                            int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                    // Set the Operation Timeout
                    client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                    // Connect to  host using Connection info
                    client.Connect();
                    client.BufferSize = 1024;

                    // Print progess of download.
                    client.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                    {
                        var progressRecord = new ProgressRecord(1, "Downloading " + e.Filename, String.Format("{0} Bytes Downloaded of {1}", e.Downloaded, e.Size));

                        if (e.Size != 0)
                        {
                            progressRecord.PercentComplete = Convert.ToInt32((e.Downloaded * 100) / e.Size);

                            Host.UI.WriteProgress(1, progressRecord);
                        }
                    };

                    var localfullPath = Path.GetFullPath(_localfolder);
                    WriteVerbose("Downloading " + _remotefolder);
                    var dirinfo = new DirectoryInfo(@localfullPath);
                    client.Download(_remotefolder, dirinfo);
                    client.Disconnect();
                } //end foreach computer
                    #endregion
            } //Use/Password Auth
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
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,  
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
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

                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                        }

                        //Ceate instance of SCP Client with connection info
                        var client = new ScpClient(connectionInfo);

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
                                var choices = new Collection<ChoiceDescription>
                                {
                                    new ChoiceDescription("Y"),
                                    new ChoiceDescription("N")
                                };

                                int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                        // Set the Operation Timeout
                        client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                        // Connect to  host using Connection info
                        client.Connect();

                        client.BufferSize = 1024;

                        // Print progess of download.
                        client.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                        {
                            var progressRecord = new ProgressRecord(1, "Downloading " + e.Filename, String.Format("{0} Bytes Downloaded of {1}", e.Downloaded, e.Size));

                            if (e.Size != 0)
                            {
                                progressRecord.PercentComplete = Convert.ToInt32((e.Downloaded * 100) / e.Size);

                                Host.UI.WriteProgress(1, progressRecord);
                            }
                        };

                        var localfullPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localfolder);
                        WriteVerbose("Downloading " + _remotefolder);
                        var dirinfo = new DirectoryInfo(@localfullPath);
                        client.Download(_remotefolder, dirinfo);
                        client.Disconnect();
                    }

                }// file exist
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }
            }

        } // End process record

    } //end of the class for the Get-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Set, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class SetScpFolder : PSCmdlet
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

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        private String _localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }
        private String _remotefolder = "";

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
        private int _operationtimeout = 15;

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
                foreach (var computer in _computername)
                {
                    #region AuthUserPass
                    //###########################################
                    //### Connect using Username and Password ###
                    //###########################################

                    ConnectionInfo connectInfo;
                    KeyboardInteractiveAuthenticationMethod kIconnectInfo;
                    if (_proxyserver != "")
                    {
                        #region Proxy
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

                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
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

                        #endregion
                    } // Proxy Server
                    else
                    {
                        #region No Proxy
                        WriteVerbose("Using Username and Password authentication for connection.");
                        // Connection info for Keyboard Interactive
                        kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.GetNetworkCredential().UserName);
                        var passconnectInfo = new PasswordAuthenticationMethod(_credential.GetNetworkCredential().UserName, _credential.GetNetworkCredential().Password);

                        WriteVerbose("Connecting to " + computer + " with user " + _credential.GetNetworkCredential().UserName);
                        connectInfo = new ConnectionInfo(computer,
                            _port,
                            _credential.GetNetworkCredential().UserName,
                            passconnectInfo,
                            kIconnectInfo);

                        #endregion
                    }// No Proxy

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };
                    //Ceate instance of SCP Client with connection info
                    var client = new ScpClient(connectInfo);

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
                            var choices = new Collection<ChoiceDescription>
                            {
                                new ChoiceDescription("Y"),
                                new ChoiceDescription("N")
                            };

                            int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                    // Set the Operation Timeout
                    client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                    // Connect to  host using Connection info
                    client.Connect();
                    client.BufferSize = 1024;

                    // Print progess of upload.
                    client.Uploading += delegate(object sender, ScpUploadEventArgs e)
                    {
                        var progressRecord = new ProgressRecord(1, "Uploading " + Path.GetDirectoryName(_remotefolder), String.Format("{0} Bytes Uploaded of {1}", e.Uploaded, e.Size))
                        {
                            PercentComplete = Convert.ToInt32((e.Uploaded*100)/e.Size)
                        };

                        Host.UI.WriteProgress(1, progressRecord);
                    };

                    var localfullPath = Path.GetFullPath(_localfolder);
                    if (Directory.Exists(localfullPath))
                    {

                        WriteVerbose("Uploading " + _remotefolder);
                        var dirinfo = new DirectoryInfo(@localfullPath);
                        client.Upload(dirinfo, _remotefolder);

                    }
                    else
                    {
                        throw new DirectoryNotFoundException("Directory " + localfullPath + " was not found.");
                    }
                    client.Disconnect();
                } //end foreach computer
                    #endregion
            } //Use/Password Auth
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
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
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

                            if (_credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port, 
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), _credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, 
                                    _port,
                                    _credential.GetNetworkCredential().UserName, 
                                    sshkey);
                            }
                        }

                        //Ceate instance of SCP Client with connection info
                        var client = new ScpClient(connectionInfo);

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
                                var choices = new Collection<ChoiceDescription>
                                {
                                    new ChoiceDescription("Y"),
                                    new ChoiceDescription("N")
                                };

                                int choice = Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);

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

                        // Set the Operation Timeout
                        client.OperationTimeout = TimeSpan.FromSeconds(_operationtimeout);

                        // Connect to  host using Connection info
                        client.Connect();

                        client.BufferSize = 1024;

                        // Print progess of upload.
                        client.Uploading += delegate(object sender, ScpUploadEventArgs e)
                        {
                            var progressRecord = new ProgressRecord(1, "Uploading " + Path.GetDirectoryName(_remotefolder), String.Format("{0} Bytes Uploaded of {1}", e.Uploaded, e.Size))
                            {
                                PercentComplete = Convert.ToInt32((e.Uploaded*100)/e.Size)
                            };

                            Host.UI.WriteProgress(1, progressRecord);
                        };

                        var localfullPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(_localfolder);
                        if (Directory.Exists(localfullPath))
                        {

                            WriteVerbose("Uploading " + _remotefolder);
                            var dirinfo = new DirectoryInfo(@localfullPath);
                            client.Upload(dirinfo, _remotefolder);
                            
                        }
                        else
                        {
                            throw new DirectoryNotFoundException("Directory " + localfullPath + " was not found.");
                        }
                        client.Disconnect();
                    }

                }// file exist
                else
                {
                    throw new FileNotFoundException("Key file " + fullPath + " was not found.");
                }
            }

        } // End process record

    } //end of the class for the Set-SCPFile
} 
//###################################################
