using System.Management.Automation;

namespace SSH
{
    [Cmdlet(VerbsCommon.New, "SSHRegistryStore")]
    public class NewRegistryStore : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var store = new Stores.RegistryStore();

            WriteObject(store);
        }
    }
}
