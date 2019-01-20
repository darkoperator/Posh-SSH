using System.Management.Automation;


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
