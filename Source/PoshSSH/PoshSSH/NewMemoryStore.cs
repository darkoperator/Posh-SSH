using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHMemoryKnownHost")]
    public class NewMemoryStore : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var store = new Stores.MemoryStore();

            WriteObject(store);
        }
    }
}
