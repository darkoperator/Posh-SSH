using System.IO;
using System;
using System.Management.Automation;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using SSH.Stores;

namespace SSH
{
    [Cmdlet(VerbsCommon.Get, "SSHHostKey")]
    public class GetSSHHostKey : PSCmdlet
    {
        /// <summary>
        /// Hosts to conect to 
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "FQDN or IP Address of host to establish a SSH connection.")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName { get; set; }

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

        protected override void ProcessRecord()
        {
            foreach (var computer in ComputerName)
            {
                var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod("x");
                var fakeCredential = new PSCredential("x", new System.Security.SecureString());
                var connectInfo = ConnectionInfoGenerator.GetCredConnectionInfo(computer,
                           Port,
                           fakeCredential,
                           ProxyServer,
                           ProxyType,
                           ProxyPort,
                           ProxyCredential,
                           kIconnectInfo);
                var client = new SshClient(connectInfo);

                KnownHostRecord record = null;
                ErrorRecord erec = null;
                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    var sb = new StringBuilder();
                    foreach (var b in e.FingerPrint)
                    {
                        sb.AppendFormat("{0:x}:", b);
                    }
                    var fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);
                    record = new KnownHostRecord()
                    {
                        HostName = computer,
                        HostKeyName = e.HostKeyName,
                        Fingerprint = fingerPrint,
                    };
                    e.CanTrust = false;
                };
                try
                {
                    // I just want to get host key
                    client.Connect();
                }
                catch (Exception ex)
                {
                    erec = new ErrorRecord(ex, null, ErrorCategory.ConnectionError, computer);
                }
                finally
                {
                    client.Dispose();
                    if (record != null)
                    {
                        WriteObject(record, true);
                    }
                    else if (erec != null)
                    {
                        WriteError(erec);
                    }
                }
            }
        } // End process record
    }

}
