using System.IO;
using System.Linq;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
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
        [Parameter(Mandatory = true,
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
            var store = new Stores.JsonStore(_localfile);

            WriteObject(store);
        }
    }
}