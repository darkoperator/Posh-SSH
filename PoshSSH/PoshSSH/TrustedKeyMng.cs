using System.Collections.Generic;
using Microsoft.Win32;
using System;

namespace SSH
{
    // Class for managing the keys 
    public class TrustedKeyMng
    {
        public List<TrustedKey> GetKeys()
        {
            List<TrustedKey> hostkeys = new List<TrustedKey>();
            RegistryKey poshSoftKey = Registry.CurrentUser.OpenSubKey(@"Software\PoshSSH", true);
            if (poshSoftKey != null)
            {
                var hosts = poshSoftKey.GetValueNames();
                foreach (var host in hosts)
                {
                    TrustedKey[] keys = Array.ConvertAll((string[])poshSoftKey.GetValue(host), element => new TrustedKey(host, element));

                    hostkeys.AddRange(keys);

                }
            }
            else
            {
                using (var softKey = Registry.CurrentUser.OpenSubKey(@"Software", true))
                {
                    if (softKey != null) softKey.CreateSubKey("PoshSSH");
                }
            }
            return hostkeys;
        }

        public bool SetKey(string host, string fingerprint)
        {
            RegistryKey softKey = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);

            if (softKey != null) {
                RegistryKey poshSoftKey = softKey.OpenSubKey("PoshSSH", RegistryKeyPermissionCheck.ReadWriteSubTree);
                if ( poshSoftKey == null ) {
                    poshSoftKey = softKey.CreateSubKey("PoshSSH", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }

                List<string> hostkeys = new List<string>();
                if (Array.Exists(poshSoftKey.GetValueNames(), value => value == host)) {
                    hostkeys.AddRange((string[])poshSoftKey.GetValue(host));
                    hostkeys.Add(fingerprint);
                } else {
                    hostkeys.Add(fingerprint);
                }

                poshSoftKey.SetValue(host, ((string[])hostkeys.ToArray()), RegistryValueKind.MultiString);
                return true;
            }
            return false;
        }
    }

    public class TrustedKey
    {
        public string Host { get; set; }
        public string Key { get; set; }

        public TrustedKey (string Host, string Key)
        {
            this.Host = Host;
            this.Key = Key;
        }
    }
}
