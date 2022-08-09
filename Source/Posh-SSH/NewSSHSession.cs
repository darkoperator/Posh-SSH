using System.Management.Automation;

namespace SSH
{
    /// <summary>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SSHSession", DefaultParameterSetName = "NoKey")]
    public class NewSshSession : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SSH;
            }
        }
    } //end of the class for the New-SSHSession
    //###################################################
}
