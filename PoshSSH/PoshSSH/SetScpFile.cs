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
        // Hosts to conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0)]
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
            Position = 1)]
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
            ValueFromPipelineByPropertyName = true)]
        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }
        }

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public String ProxyServer
        {
            get { return _proxyserver; }
            set { _proxyserver = value; }
        }
        private String _proxyserver = "";

        // Proxy Port
        private Int32 _proxyport = 8080;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public Int32 ProxyPort
        {
            get { return _proxyport; }
            set { _proxyport = value; }
        }

        // Proxy Credentials
        private PSCredential _proxycredential;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
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
            ValueFromPipelineByPropertyName = true)]
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


        // OperationTimeout Parameter
        private int _operationtimeout = 5;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public int OperationTimeout
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }

        // ConnectionTimeout Parameter
        private int _connectiontimeout = 5;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }

        // Auto Accept key fingerprint
        private bool _acceptkey;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public bool AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }

        // Do not check server fingerprint.
        private bool _force = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        // Automatically error if key is not trusted.
        private bool _errorOnUntrusted = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter ErrorOnUntrusted
        {
            get { return _errorOnUntrusted; }
            set { _errorOnUntrusted = value; }
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
                var client = new ScpClient(connectInfo);


                // Handle host key
                if (_force)
                {
                    WriteWarning("Host key is not being verified since Force switch is used.");
                }
                else
                {
                    var computer1 = computer;
                    client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
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
                    // Set the connection timeout
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_connectiontimeout);

                    // Connect to host using Connection info
                    client.Connect();
                    //client.BufferSize = 1024;

                    if (_noProgress == false)
                    {
                        var counter = 0;
                        // Print progess of download.
                        client.Uploading += delegate(object sender, ScpUploadEventArgs e)
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
                                        e.Uploaded, e.Size)) { PercentComplete = percent };

                                    Host.UI.WriteProgress(1, progressRecord);
                                    counter = 0;
                                }
                            }
                        };
                    }
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
                if (client.IsConnected)
                {
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
                }
            }

        } // End process record


    } //end of the class for the Set-SCPFile
    ////###################################################
}
