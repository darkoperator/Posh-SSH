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
    [Cmdlet(VerbsCommon.New, "SFTPSession", DefaultParameterSetName = "NoKey")]
    public class NewSftpSession : NewSessionBase
    {
        internal override string Protocol
        {
            get
            {
                return "SFTP";
            }
        }
    } //end of the class for the New-SFTPSession
    //###################################################
}
