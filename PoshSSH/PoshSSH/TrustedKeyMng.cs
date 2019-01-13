using System.Collections.Generic;
using Microsoft.Win32;
using System;
using Newtonsoft.Json;
using System.IO;

namespace SSH
{
    // Class for managing the keys 
    public class TrustedKeyMng
    {

        public Dictionary<string, string> GetKeys()
        {
            var platform = System.Environment.OSVersion.Platform;
            var hostkeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // check if the platform is windows we stay with the registry.
            if (platform == PlatformID.Win32NT)
            {
                var homeFolder = Environment.GetEnvironmentVariable("HOMEPATH");
                var keyStore = $"{homeFolder}\\.poshssh\\keystore.json";
                if (File.Exists(keyStore))
                {
                    var json = File.ReadAllText(keyStore);
                    var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (currentHostkeys != null)
                    {
                        hostkeys = currentHostkeys;
                    }
                }
                else
                {
                    var keyStoreHome = $"{homeFolder}\\.poshssh";
                    Directory.CreateDirectory(keyStoreHome);
                    File.CreateText(keyStore);
                }
            }
            else if (platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                var homeFolder = Environment.GetEnvironmentVariable("HOME");
                var keyStore = $"{homeFolder}/.poshssh/keystore.json";
                if (File.Exists(keyStore))
                {
                    var json = File.ReadAllText(keyStore);
                    var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (currentHostkeys != null)
                    {
                        hostkeys = currentHostkeys;
                    }
                }
                else
                {
                    var keyStoreHome = $"{homeFolder}/.poshssh/";
                    Directory.CreateDirectory(keyStoreHome);
                    File.CreateText(keyStore);
                }
            }
            return hostkeys;
        }

        public bool SetKey(string host, string fingerprint)
        {


            var platform = System.Environment.OSVersion.Platform;
            var hostkeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // check if the platform is windows we stay with the registry.
            if (platform == PlatformID.Win32NT)
            {
                var homeFolder = Environment.GetEnvironmentVariable("HOMEPATH");
                var keyStore = $"{homeFolder}\\.poshssh\\keystore.json";
                if (File.Exists(keyStore))
                {
                    var json = File.ReadAllText(keyStore);
                    var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (currentHostkeys != null)
                    {
                        hostkeys = currentHostkeys;
                    }
                    hostkeys.Add(host, fingerprint);

                    string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
                    File.WriteAllText(keyStore, jsonkeys);
                    return true;
                }
                else
                {
                    var keyStoreHome = $"{homeFolder}\\.poshssh";
                    Directory.CreateDirectory(keyStoreHome);
                    File.CreateText(keyStore);
                    return true;
                }
            }
            else if (platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                var homeFolder = Environment.GetEnvironmentVariable("HOME");
                var keyStore = $"{homeFolder}/.poshssh/keystore.json";
                if (File.Exists(keyStore))
                {
                    var json = File.ReadAllText(keyStore);
                    var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (currentHostkeys != null)
                    {
                        hostkeys = currentHostkeys;
                    }
                    hostkeys.Add(host, fingerprint);
                    string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
                    File.WriteAllText(keyStore, jsonkeys);
                    return true;
                }
                else
                {
                    var keyStoreHome = $"{homeFolder}/.poshssh/";
                    Directory.CreateDirectory(keyStoreHome);
                    File.CreateText(keyStore);
                    return true;
                }
            }
            return true;

        }
    }
}
