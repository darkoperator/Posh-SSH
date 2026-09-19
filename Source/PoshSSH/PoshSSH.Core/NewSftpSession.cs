using System.Management.Automation;


namespace SSH
{
    /// <summary>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SFTPSession", DefaultParameterSetName = "NoKey")]
    public class NewSftpSession : NewSessionBase
    {
        internal override PoshSessionType Protocol
        {
            get
            {
                return PoshSessionType.SFTP;
            }
        }
    } //end of the class for the New-SFTPSession
    //###################################################
}
