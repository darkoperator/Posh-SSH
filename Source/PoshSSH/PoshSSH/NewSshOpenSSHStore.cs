using System.IO;
using System.Linq;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHOpenSSHStore")]
    public class SshOpenSSHStore : PSCmdlet
    { 
        /// <summary>
        /// The local file to be uploaded.
        /// </summary>
        private String _localfile;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 1)]
        [Alias("PSPath")]
        public String LocalFile
        {
            get { return _localfile; }
            set { _localfile = value; }
        }

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(_localfile))
            {
                var homeFolder = GetVariableValue("HOME").ToString();
                _localfile = Path.Combine(homeFolder, ".ssh", "known_hosts");
            }
            var store = new Stores.OpenSSHStore(_localfile);

            WriteObject(store);
        }
    }
}