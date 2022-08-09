using System;

namespace SSH
{
    public class SSHRegistryKeyStore : Stores.MemoryStore
    {
        protected override void OnGetKeys()
        {
            // FIXME: Implement this
            /*
            $p = Get-ItemProperty HKCU:\SOFTWARE\PoshSSH
              $HostKeys = $this.HostKeys
              $p | Get-Member -MemberType NoteProperty |
              Where-Object { $_.Name -notin 'PSPath', 'PSParentPath', 'PSChildName', 'PSDrive', 'PSProvider' } |
              ForEach-Object {
                 $name = $_.Name
                 $hostData = [SSH.Stores.KnownHostValue]@{ HostKeyName='ssh-rsa'; Fingerprint=$p.$name }
                 $HostKeys.AddOrUpdate($name, $hostData, { return $hostData } )
              }
            */
            return;
        }

        public override bool SetKey(string Host, string HostKeyName, string Fingerprint) => false;

        public override bool RemoveByHost(string Host) => false;

        public override bool RemoveByFingerprint(string Fingerprint) => false;
    }
}
