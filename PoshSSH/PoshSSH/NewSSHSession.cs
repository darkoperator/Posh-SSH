using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace SSH
{
    /// <summary>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SSHSession", DefaultParameterSetName = "NoKey")]
    public class NewSshSession : NewSessionBase
    {
        internal override string Protocol
        {
            get
            {
                return "SSH";
            }
        }
    } //end of the class for the New-SSHSession
    //###################################################
}
