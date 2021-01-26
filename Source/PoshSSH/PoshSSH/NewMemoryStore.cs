using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHMemoryKnownHost")]
    public class NewMemoryKnownHost : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var store = new Stores.MemoryStore();

            WriteObject(store);
        }
    }
}
