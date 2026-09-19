using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Renci.SshNet;
using SSH.Stores;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHTrustedHost")]
    public class NewSSHTrustedHost : SetSSHTrustedHost
    { 
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _appendMode = false;
        }
    }
}
