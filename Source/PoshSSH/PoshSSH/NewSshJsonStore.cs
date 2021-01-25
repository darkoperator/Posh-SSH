using System.IO;
using System;
using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHJsonStore")]
    public class SSHJsonStore : PSCmdlet
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
            if (string.IsNullOrEmpty(_localfile)) {
                var homeFolder = GetVariableValue("HOME").ToString();
                _localfile = Path.Combine(homeFolder, ".poshssh", "hosts.json");
            }
            var store = new Stores.JsonStore(_localfile);

            WriteObject(store);
        }
    }
}