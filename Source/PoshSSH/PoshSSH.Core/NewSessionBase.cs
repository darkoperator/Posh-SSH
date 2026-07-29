using Renci.SshNet;
using Renci.SshNet.Common;
using SSH.Stores;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.IO;
using System.Linq;

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
        /// Credentials for Connection
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key",
            HelpMessage = "Passphrase for the SSH Key.")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "KeyString",
            HelpMessage = "Passphrase for the SSH Key.")]
        public System.Security.SecureString Passphrase { get; set; }

        /// <summary>
        /// Place where fingerprint can persist
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false,
             HelpMessage = "Connection encoding")]
        [ValidateNotNullOrEmpty]
        public Encoding Encoding { get; set; }

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
        public int OperationTimeout { get; set; } = 0;

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
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false,
             HelpMessage = "Known Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.")]
        [ValidateNotNullOrEmpty]
        [Alias("KnownHostStore")]
        public ITrustedHostStore TrustedHostStore { get; set; }

        protected override void BeginProcessing()
        {
            // no need to validate keys if the force parameter is selected.
            if (!Force)
            {
                // check is a ITrustedHostStore was specified.
                bool storeSpecified = MyInvocation.BoundParameters.ContainsKey(nameof(TrustedHostStore));

                if (storeSpecified)
                {
                    // Collect host/fingerprint information from the ITrustedHostStore specified.
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
                    TrustedHostStore = new Stores.JsonTrustedHostStore(configPath);
                    base.BeginProcessing();
                }
            }
        }
        protected BaseClient CreateConnection(string computer)
        {
            var isVerboseEnabled = MyInvocation.BoundParameters.ContainsKey("Verbose");
            if (!isVerboseEnabled)
            {
                switch (this.SessionState.PSVariable.GetValue("VerbosePreference"))
                {
                    case ActionPreference apVp:
                        isVerboseEnabled = (apVp != ActionPreference.SilentlyContinue);
                        break;

                    case string strVp:
                        isVerboseEnabled = Enum.TryParse<ActionPreference>(strVp, true, out ActionPreference vp) &&
                         (vp != ActionPreference.SilentlyContinue);
                        break;

                    default:
                        break;
                }
            }

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
                            if (prompt.Request.Contains(":"))
                                prompt.Response = Credential.GetNetworkCredential().Password;
                        }
                    };
                    break;

                case "Key":
                    WriteVerbose("Using SSH Key authentication for connection (file).");
                    ProviderInfo provider;
                    var pathinfo = GetResolvedProviderPathFromPSPath(KeyFile, out provider);
                    var localfullPath = pathinfo[0];
                    connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                        Port,
                        localfullPath,
                        Credential,
                        Passphrase,
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
                        Passphrase,
                        ProxyServer,
                        ProxyType,
                        ProxyPort,
                        ProxyCredential);
                    break;

                default:
                    break;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Encoding)))
                connectInfo.Encoding = Encoding;

            //Create instance of SSH Client with connection info
            BaseClient client;
            switch (Protocol)
            {
                case PoshSessionType.SFTP:
                    client = new SftpClient(connectInfo);
                    if (OperationTimeout > 0)
                        (client as SftpClient).OperationTimeout = new TimeSpan(0, 0, OperationTimeout);
                    break;
                case PoshSessionType.SCP:
                    client = new ScpClient(connectInfo);
                    if (OperationTimeout > 0)
                        (client as ScpClient).OperationTimeout = new TimeSpan(0, 0, OperationTimeout);
                    break;
                default:
                    client = new SshClient(connectInfo);
                    break;
            }

            // Captured by the HostKeyReceived handler so a rejected key can be explained after
            // the connection attempt throws. Without these the caller only sees "Host key could
            // not be verified", which does not say what was presented or what was expected.
            SSH.Stores.TrustedHostValue[] rejectedAgainstKeys = null;
            string rejectedHostKeyName = null;
            string rejectedFingerprint = null;
            var hostKeyWasRejected = false;

            // Handle host key
            if (Force)
            {
                WriteWarning("Host key for " + computer + " is not being verified since the Force switch was used.");
            }
            else
            {
                var computer1 = computer;
                if (Port != 22)
                {
                    computer1 = computer1 + ':' + Port.ToString();
                }
                var savedHostKeys = TrustedHostStore.GetKeys(computer1).ToArray();
                // filter out unsupported hostkeynames
                if (savedHostKeys.Length > 0)
                {
                    var hostKeyTypes = savedHostKeys.Select(hk => hk.HostKeyName).ToArray();
                    if (hostKeyTypes.Length > 0 && !hostKeyTypes.Contains("")) {
                        foreach (var keyName in connectInfo.HostKeyAlgorithms.Keys.ToArray())
                        {
                            if (!hostKeyTypes.Contains(keyName))
                            {
                                connectInfo.HostKeyAlgorithms.Remove(keyName);
                            }
                        }
                    }
                }
                computer1 = computer;
                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    var sb = new StringBuilder();
                    foreach (var b in e.FingerPrint)
                    {
                        sb.AppendFormat("{0:x}:", b);
                    }
                    var legacyFingerprint = sb.ToString().Remove(sb.ToString().Length - 1);

                    rejectedHostKeyName = e.HostKeyName;
                    rejectedFingerprint = e.FingerPrintSHA256;
                    rejectedAgainstKeys = savedHostKeys;

                    if (isVerboseEnabled)
                    {
                        Host.UI.WriteVerboseLine(e.HostKeyName + " Fingerprint for " + computer1 + ": " + e.FingerPrintSHA256);
                    }

                    if (savedHostKeys.Length > 0)
                    {
                        var hostKeyFound = savedHostKeys.Where(hk =>
                                string.IsNullOrEmpty(hk.HostKeyName) || (hk.HostKeyName == e.HostKeyName) &&
                                (hk.Fingerprint == e.FingerPrintSHA256 || hk.Fingerprint == legacyFingerprint)
                        );
                        e.CanTrust = hostKeyFound.Any();
                        //e.CanTrust = savedHostKey.Fingerprint == fingerprintMD5 && (savedHostKey.HostKeyType == e.HostKeyName || savedHostKey.HostKeyType == string.Empty);

                        if (isVerboseEnabled)
                        {
                            if (e.CanTrust) {
                                Host.UI.WriteVerboseLine("Fingerprint matched trusted " + hostKeyFound.FirstOrDefault()?.HostKeyName + " fingerprint for host " + computer1);
                            }
                            else
                            {
                                Host.UI.WriteVerboseLine("Fingerprint not matched trusted " + string.Join(", ", savedHostKeys.Select(h => h.HostKeyName)) + " fingerprints for host " + computer1);
                            }
                        }
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
                                e.CanTrust = 0 == Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + e.FingerPrintSHA256, choices, 1);
                            }
                            else // User specified he would accept the key so we can just add it to our list.
                            {
                                e.CanTrust = true;
                            }
                            if (e.CanTrust)
                            {
                                if (Port != 22)
                                {
                                    computer1 = computer1 + ':' + Port.ToString();
                                }
                                bool keySaved = TrustedHostStore.SetKey(computer1, e.HostKeyName, e.FingerPrintSHA256, false);
                                if (isVerboseEnabled) {
                                    Host.UI.WriteVerboseLine(
                                        string.Format("Host key for {0} ({1}) {2} to store",
                                            computer1,
                                            e.FingerPrintSHA256,
                                            (keySaved) ? "saved" : "not saved"
                                        )
                                    );
                                }
                            }
                        }
                    }

                    hostKeyWasRejected = !e.CanTrust;
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
                // A failed negotiation or a rejected host key say only that something did not
                // match, which is not enough to act on. Attach the detail the caller needs.
                // ErrorDetails overrides the displayed text without altering the exception, so
                // anything catching or matching on the original error is unaffected.
                var details = NegotiationFailureDetails(e, computer, connectInfo);
                if (details == null && hostKeyWasRejected)
                {
                    details = HostKeyRejectedDetails(e, computer, rejectedHostKeyName, rejectedFingerprint, rejectedAgainstKeys);
                }
                if (details != null)
                {
                    erec.ErrorDetails = details;
                }
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

        /// <summary>
        /// Builds guidance for an algorithm negotiation failure, or null if the exception is
        /// something else. SSH.NET reports these as "&lt;side&gt; &lt;category&gt; algorithm not found".
        /// </summary>
        private ErrorDetails NegotiationFailureDetails(Exception e, string computer, ConnectionInfo connectInfo)
        {
            var message = e.Message ?? "";
            if (message.IndexOf("algorithm not found", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }

            string category = null;
            string[] supported = null;
            if (message.IndexOf("encryption", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                category = "encryption";
                supported = connectInfo.Encryptions.Keys.ToArray();
            }
            else if (message.IndexOf("key exchange", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                category = "key exchange";
                supported = connectInfo.KeyExchangeAlgorithms.Keys.ToArray();
            }
            else if (message.IndexOf("host key", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                category = "host key";
                supported = connectInfo.HostKeyAlgorithms.Keys.ToArray();
            }
            else if (message.IndexOf("mac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     message.IndexOf("message authentication", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                category = "MAC";
                supported = connectInfo.HmacAlgorithms.Keys.ToArray();
            }
            else if (message.IndexOf("compression", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                category = "compression";
                supported = connectInfo.CompressionAlgorithms.Keys.ToArray();
            }

            var libraryVersion = typeof(ConnectionInfo).Assembly.GetName().Version;
            var text = new StringBuilder(message);
            text.Append(Environment.NewLine);
            text.AppendFormat("The client and the server share no {0} algorithm.", category ?? "negotiated");
            text.Append(Environment.NewLine);
            text.AppendFormat("Renci.SshNet {0} in use by this module supports", libraryVersion);
            if (category != null && supported != null)
            {
                text.AppendFormat(" ({0}): {1}", category, string.Join(", ", supported));
            }
            else
            {
                text.Append(" a limited set of algorithms.");
            }

            var probe = string.Format("Run 'Get-SSHAlgorithm -ComputerName {0}{1}' to see what the server offers and where the overlap is missing.",
                computer,
                (Port != 22) ? " -Port " + Port : "");

            return new ErrorDetails(text.ToString()) { RecommendedAction = probe };
        }

        /// <summary>
        /// Builds guidance for a host key the trusted host store would not accept. The bare
        /// error does not say what the server presented, what was expected, or that AcceptKey
        /// is deliberately ignored once a key is already recorded for the host.
        /// </summary>
        private ErrorDetails HostKeyRejectedDetails(Exception e, string computer, string hostKeyName,
            string fingerprint, SSH.Stores.TrustedHostValue[] storedKeys)
        {
            var host = (Port != 22) ? computer + ":" + Port : computer;
            var text = new StringBuilder(e.Message ?? "Host key could not be verified.");
            text.Append(Environment.NewLine);
            text.AppendFormat("{0} presented a {1} host key with fingerprint {2}.", host, hostKeyName, fingerprint);
            text.Append(Environment.NewLine);

            if (storedKeys != null && storedKeys.Length > 0)
            {
                text.AppendFormat("The trusted host store already holds {0} key(s) for this host, none of which match:",
                    storedKeys.Length);
                foreach (var key in storedKeys)
                {
                    text.Append(Environment.NewLine);
                    text.AppendFormat("  {0} {1}",
                        string.IsNullOrEmpty(key.HostKeyName) ? "(any)" : key.HostKeyName,
                        key.Fingerprint);
                }
                text.Append(Environment.NewLine);
                text.Append("AcceptKey does not override a recorded key. Either the host key changed, or the stored entry is stale.");

                return new ErrorDetails(text.ToString())
                {
                    RecommendedAction = string.Format(
                        "Verify the fingerprint out of band. If the change is expected, run 'Remove-SSHTrustedHost -HostName {0} -Confirm:$false' and connect again with -AcceptKey.",
                        host)
                };
            }

            text.Append("No key is recorded for this host, so the key was not trusted.");
            return new ErrorDetails(text.ToString())
            {
                RecommendedAction = "Connect with -AcceptKey to record this fingerprint, or add it with Add-SSHTrustedHost."
            };
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
