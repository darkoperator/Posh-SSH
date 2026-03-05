using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Renci.SshNet;
using SSH.Stores;

namespace SSH
{
    public class SetSSHTrustedHost : PSCmdlet
    { 
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "FQDN or IP Address of host")]
        [Alias("ComputerName", "IPAddress")]
        public string HostName { get; set; }

        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Fingerprint of hostkey for remote host")]
        public string Fingerprint { get; set; }

        [ValidateNotNullOrEmpty]
        [ValidateSet(
                    "ssh-ed25519",
                    "ecdsa-sha2-nistp256",
                    "ecdsa-sha2-nistp384",
                    "ecdsa-sha2-nistp521",
                    "rsa-sha2-512",
                    "rsa-sha2-256",
                    "ssh-rsa",
                    "ssh-dss"
        )]
        [Parameter(Mandatory = false,
        Position = 2,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "HostKeyName (cipher name) of hostkey for remote host")]
        [Alias("KeyCipherName")]
        public string HostKeyName { get; set; }

        /// <summary>
        /// Place where fingerprint can persist
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Known Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.")]
        [ValidateNotNullOrEmpty]
        [Alias("KnownHostStore")]
        public ITrustedHostStore TrustedHostStore { get; set; }

        protected bool _appendMode = false;

        protected override void BeginProcessing()
        {
            // check is a ITrustedHostStore was specified.
            if (MyInvocation.BoundParameters.ContainsKey(nameof(TrustedHostStore)))
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
        protected override void ProcessRecord()
        {
            WriteObject(TrustedHostStore.SetKey(HostName, HostKeyName, Fingerprint, _appendMode), true);
        }
    }
}
