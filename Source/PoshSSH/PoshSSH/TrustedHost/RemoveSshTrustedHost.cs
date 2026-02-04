using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Renci.SshNet;
using SSH.Stores;

namespace SSH
{
    [Cmdlet(VerbsCommon.Remove, "SSHTrustedHost",
        DefaultParameterSetName = "Host", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true
    )]
    public class RemoveSSHTrustedHost : PSCmdlet
    { 
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ParameterSetName = "Host",
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "FQDN or IP Address of host")]
        [Alias("ComputerName", "IPAddress")]
        public string[] HostName { get; set; }

        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
        ParameterSetName = "TrustedHost",
        ValueFromPipeline = true,
        HelpMessage = "TrustedHostRecord of remote host")]
        public TrustedHostRecord[] TrustedHostRecord { get; set; }

        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
        ParameterSetName = "Fingerprint",
        Position = 0,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Fingerprint (hostkey) of remote host")]
        public string[] Fingerprint { get; set; }

        [Parameter(Mandatory = false,
        ParameterSetName = "Fingerprint",
        HelpMessage = "Remove the host to which the selected fingerprint belongs")]
        public SwitchParameter WithHost { get; set; } = false;

        /// <summary>
        /// Place where fingerprint can persist
        /// </summary>
        [Parameter(Mandatory = false,
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
            if (ParameterSetName.Equals("Host"))
            {
                foreach (var host in HostName)
                {
                    if (ShouldProcess(host, "Remove host"))
                        WriteObject(TrustedHostStore.RemoveHost(host), true);
                }
            }
            else
            {
                if (ParameterSetName.Equals("TrustedHost"))
                {
                    Fingerprint = TrustedHostRecord.Select(r => r.Fingerprint ).ToArray();
                }
                if (WithHost)
                {
                    foreach (var fingerprint in Fingerprint)
                    {
                        if (ShouldProcess(fingerprint, "Remove host with fingerprint"))
                            WriteObject(TrustedHostStore.RemoveHostByFingerprint(fingerprint), true);
                    }
                }
                else {
                    foreach (var fingerprint in Fingerprint)
                    {
                        if (ShouldProcess(fingerprint, "Remove fingerprint from host"))
                            WriteObject(TrustedHostStore.RemoveHostFingerprint(fingerprint), true);
                    }
                }
            }
        } // End process record
    }
}
