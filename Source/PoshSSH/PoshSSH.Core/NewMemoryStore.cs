using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHMemoryTrustedHostStore")]
    [Alias("New-SSHMemoryKnownHost")]
    public class NewMemoryStore : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var store = new Stores.MemoryTrustedHostStore();

            WriteObject(store);
        }
    }
}
