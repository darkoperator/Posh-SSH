using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System;

namespace SSH
{
    // Class for managing the keys 
    public class TrustedKeyMng
    {
        private const string keyFilename = "PoshSSH";

        public Dictionary<string, string> GetKeys()
        {
            var hostkeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            // use the registry to store keys on Windows, and the $HOME path everywhere else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var poshSoftKey = Registry.CurrentUser.OpenSubKey(@"Software\PoshSSH", true);
                if (poshSoftKey != null)
                {
                    var hosts = poshSoftKey.GetValueNames();
                    foreach (var host in hosts)
                    {
                        var hostkey = poshSoftKey.GetValue(host).ToString();
                        hostkeys.Add(host, hostkey);
                    }
                }
                else
                {
                    using (var softKey = Registry.CurrentUser.OpenSubKey(@"Software", true))
                    {
                        if (softKey != null) softKey.CreateSubKey("PoshSSH");
                    }
                }
            }
            else
            {
                var keyPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                keyPath = Path.Combine(keyPath, ".ssh");
                keyPath = Path.Combine(keyPath, keyFilename);
                if (File.Exists(keyPath))
                {
                    foreach (var line in File.ReadLines(keyPath))
                    {
                        int separatorIndex = line.IndexOf("=");
                        if (separatorIndex == -1)
                        {
                            continue;
                        }
                        hostkeys.Add(line.Substring(0, separatorIndex), line.Substring(separatorIndex + 1));
                    }
                }
            }
            return hostkeys;
        }

        public bool SetKey(string host, string fingerprint)
        {
            // use the registry to store keys on Windows, and the $HOME path everywhere else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var poshSoftKey = Registry.CurrentUser.OpenSubKey(@"Software\PoshSSH", true);
                if (poshSoftKey != null)
                {
                    poshSoftKey.SetValue(host, fingerprint);
                    return true;
                }
                var softKey = Registry.CurrentUser.OpenSubKey(@"Software", true);
                if (softKey == null) return true;
                softKey.CreateSubKey("PoshSSH");
                softKey.SetValue(host, fingerprint);
            }
            else
            {
                var keyPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                keyPath = Path.Combine(keyPath, ".ssh");
                keyPath = Path.Combine(keyPath, keyFilename);
                if (!File.Exists(keyPath))
                {
                    File.Create(keyPath).Dispose();
                }
                File.AppendAllText(keyPath, host + "=" + fingerprint + Environment.NewLine);
            }
            return true;
        }
    }
}
