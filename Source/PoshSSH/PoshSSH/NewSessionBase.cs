using Renci.SshNet;
using Renci.SshNet.Common;
using SSH.Stores;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.IO;

namespace SSH
{
    public abstract class NewSessionBase : PSCmdlet
    {
        /// <summary>
        /// Desired Protocol. Should be SSH or SFTP
        /// </summary>
        internal abstract PoshSessionType Protocol { get; }

        /// <summary>
        /// Hosts to conect to 
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = "FQDN or IP Address of host to establish a SSH connection.")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName { get; set; }

        /// <summary>
        /// Credentials for Connection
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "SSH Credentials to use for connecting to a server. If a key file is used the password field is used for the Key pass phrase.")]
        [Credential()]
        public PSCredential Credential { get; set; }        

        /// <summary>
        /// Port for SSH
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "SSH TCP Port number to use for the SSH connection.")]
        public Int32 Port { get; set; } = 22;


        /// <summary>
        /// Proxy Server to use
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Proxy server name or IP Address to use for connection.")]
        public String ProxyServer { get; set; } = "";

        /// <summary>
        /// Proxy Port 
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Port to connect to on proxy server to route connection.")]
        public Int32 ProxyPort { get; set; } = 8080;


        /// <summary>
        /// Proxy Credentials
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerShell Credential Object with the credentials for use to connect to proxy server if required.")]
        [ValidateNotNullOrEmpty]
        [System.Management.Automation.CredentialAttribute()]
        public PSCredential ProxyCredential { get; set; }


        /// <summary>
        /// Proxy Type
        /// </summary>
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Type of Proxy being used (HTTP, Socks4 or Socks5).")]
        public string ProxyType { get; set; } = "HTTP";

        /// <summary>
        /// SSH Key File
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key",
            HelpMessage = "OpenSSH format SSH private key file.")]
        public string KeyFile { get; set; } = null;

        /// <summary>
        /// SSH Key Content
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString",
            HelpMessage = "String array of the content of a OpenSSH key file.")]
        public string[] KeyString { get; set; } = new string[] { };

        /// <summary>
        /// ConnectionTimeout Parameter
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Connection timeout interval in seconds.")]
        public int ConnectionTimeout { get; set; } = 10;

        /// <summary>
        /// OperationTimeout Parameter
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Operation timeout interval in seconds.")]
        public int OperationTimeout { get; set; } = 5;

        /// <summary>
        /// KeepAliveInterval Parameter 
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Sets a timeout interval in seconds after which if no data has been received from the server, session will send a message through the encrypted channel to request a response from the server")]
        public int KeepAliveInterval { get; set; } = 10;


        /// <summary>
        /// Auto Accept key fingerprint 
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
             HelpMessage = "Auto add host key fingerprint to the list of trusted host/fingerprint pairs.")]
        public SwitchParameter AcceptKey { get; set; } = false;

        /// <summary>
        /// Do not check server fingerprint.
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Do not check the remote host fingerprint.")]
        public SwitchParameter Force { get; set; } = false;


        /// <summary>
        /// Automatically error if key is not trusted.
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Raise an exception if the fingerprint is not trusted for the host.")]
        public SwitchParameter ErrorOnUntrusted { get; set; } = false;
        
        /// <summary>
        /// Place where fingerprint can persist
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IStore Store { get; set; }

        protected override void BeginProcessing()
        {
            // no need to validate keys if the force parameter is selected.
            if (!Force)
            {
                // check is a IStore was specified.
                bool storeSpecified = MyInvocation.BoundParameters.ContainsKey("Store");

                if (storeSpecified)
                {
                    // Collect host/fingerprint information from the IStore specified.
                    base.BeginProcessing();
                }
                else
                {
                    var homeFolder = GetVariableValue("HOME").ToString();
                    var configPath = Path.Combine(homeFolder, ".poshssh", "hosts.json");
                    if (!File.Exists(configPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                    }
                    Store = new Stores.JsonStore(configPath);
                    base.BeginProcessing();
                }
            }
        }
        protected BaseClient CreateConnection(string computer) {
                ConnectionInfo connectInfo = null;
                switch (ParameterSetName)
                {
                    case "NoKey":
                        WriteVerbose("Using SSH Username and Password authentication for connection.");
                        var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(Credential.UserName);
                        connectInfo = ConnectionInfoGenerator.GetCredConnectionInfo(computer,
                            Port,
                            Credential,
                            ProxyServer,
                            ProxyType,
                            ProxyPort,
                            ProxyCredential,
                            kIconnectInfo);

                        // Event Handler for interactive Authentication
                        kIconnectInfo.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = Credential.GetNetworkCredential().Password;
                            }
                        };
                        break;

                    case "Key":
                        ProviderInfo provider;
                        var pathinfo = GetResolvedProviderPathFromPSPath(KeyFile, out provider);
                        var localfullPath = pathinfo[0];
                        connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                            Port,
                            localfullPath,
                            Credential,
                            ProxyServer,
                            ProxyType,
                            ProxyPort,
                            ProxyCredential);
                        break;

                    case "KeyString":
                        WriteVerbose("Using SSH Key authentication for connection.");
                        connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                            Port,
                            KeyString,
                            Credential,
                            ProxyServer,
                            ProxyType,
                            ProxyPort,
                            ProxyCredential);
                        break;

                    default:
                        break;
                }

                //Ceate instance of SSH Client with connection info
                BaseClient client;
                switch (Protocol) 
                {
                    case PoshSessionType.SFTP:
                        client = new SftpClient(connectInfo);
                        break;
                    case PoshSessionType.SCP:
                        client = new ScpClient(connectInfo);
                        break;
                    default:
                        client = new SshClient(connectInfo);
                        break;
                }

                // Handle host key
                if (Force)
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

                        if (Store.GetKey(computer1) != default)
                        {
                            e.CanTrust = Store.GetKey(computer1) == fingerPrint;
                            if (e.CanTrust && MyInvocation.BoundParameters.ContainsKey("Verbose"))
                                Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerprint for host " + computer1);
                        }
                        else
                        {
                            if (ErrorOnUntrusted)
                            {
                                e.CanTrust = false;
                            }
                            else
                            {
                                if (!AcceptKey)
                                {
                                    var choices = new Collection<ChoiceDescription>
                                    {
                                        new ChoiceDescription("Y"),
                                        new ChoiceDescription("N")
                                    };
                                    e.CanTrust = 0 == Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);
                                }
                                else // User specified he would accept the key so we can just add it to our list.
                                {
                                    e.CanTrust = true;
                                }
                                if (e.CanTrust)
                                {
                                    Store.SetKey(computer1, fingerPrint);
                                }

                            }
                        }
                    };
                }
                try
                {
                    // Set the connection timeout
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(ConnectionTimeout);

                    // Set Keepalive for connections
                    client.KeepAliveInterval = TimeSpan.FromSeconds(KeepAliveInterval);

                    // Connect to host using Connection info
                    client.Connect();

                    return client;
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
                return default;

                // Renci.SshNet.Common.SshOperationTimeoutException when host is not alive or connection times out.
                // Renci.SshNet.Common.SshConnectionException when fingerprint mismatched
                // Renci.SshNet.Common.SshAuthenticationException Bad password
        }

        protected override void ProcessRecord()
        {
            foreach (var computer in ComputerName)
            {
                var client = CreateConnection(computer);
                if (client != default) {
                    if (Protocol == PoshSessionType.SSH)
                        WriteObject(SshModHelper.AddToSshSessionCollection(client as SshClient, SessionState), true);
                    else
                        WriteObject(SshModHelper.AddToSftpSessionCollection(client as SftpClient, SessionState), true);
                }
            }

        } // End process record
    }
}
