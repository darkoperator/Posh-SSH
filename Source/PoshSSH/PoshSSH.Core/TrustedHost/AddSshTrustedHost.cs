using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Renci.SshNet;
using SSH.Stores;

namespace SSH
{
    [Cmdlet(VerbsCommon.Add, "SSHTrustedHost")]
    public class AddSSHTrustedHost : SetSSHTrustedHost
    { 
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _appendMode = true;
        }
    }
}
