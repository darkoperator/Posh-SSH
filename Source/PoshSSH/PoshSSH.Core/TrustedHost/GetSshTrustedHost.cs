using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Renci.SshNet;
using SSH.Stores;

namespace SSH
{
    [Cmdlet(VerbsCommon.Get, "SSHTrustedHost")]
    public class GetSSHTrustedHost : PSCmdlet
    { 
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = false,
            Position = 0,
            HelpMessage = "FQDN or IP Address of host")]
        [Alias("ComputerName", "IPAddress")]
        public string[] HostName { get; set; }

        /// <summary>
        /// Place where fingerprint can persist
        /// </summary>
        [Parameter(Mandatory = false,
            ValueFromPipeline = true,
            HelpMessage = "Known Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.")]
        [ValidateNotNullOrEmpty]
        public ITrustedHostStore TrustedHostStore { get; set; }

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
            if (MyInvocation.BoundParameters.ContainsKey(nameof(HostName)))
            {
                foreach (var host in HostName)
                {
                    foreach (var r in TrustedHostStore.GetKeys(host).Select(k => new TrustedHostRecord()
                    {
                        HostName = host,
                        HostKeyName = k.HostKeyName,
                        Fingerprint = k.Fingerprint
                    }))
                    {
                        WriteObject(r, true);
                    }                    
                }
            }
            else
            {
                foreach (var r in TrustedHostStore.GetAllKeys())
                {
                    WriteObject(r, true);
                }
            }
        } // End process record
    }
}