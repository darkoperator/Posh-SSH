using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHMemoryStore")]
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
