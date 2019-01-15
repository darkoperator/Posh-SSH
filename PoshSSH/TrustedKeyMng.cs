using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;

namespace SSH
{
    // Class for managing the keys 
    public class TrustedKeyMng
    {
        /// <summary>
        /// Returns the proper path to the trusted hosts file for the platform.
        /// </summary>
        protected string FilePath
        {
            get
            {
                var platform = System.Environment.OSVersion.Platform;
                var trustedHostFile = "trusted_hosts.json";
                string path = "";

                if (platform == PlatformID.Win32NT)
                {
                    path = Environment.GetEnvironmentVariable("HOMEPATH");
                }
                else if (platform == PlatformID.Unix || platform == PlatformID.MacOSX)
                {
                    path = Environment.GetEnvironmentVariable("HOME");
                }
                
                var trustedHostPath = Path.Combine(path, ".poshssh", trustedHostFile);
                if (!File.Exists(trustedHostPath))
                {
                    Directory.CreateDirectory(Path.Combine(path, ".poshssh"));
                    var jfile = File.CreateText(trustedHostPath);
                    jfile.Close();
                }
                return trustedHostPath;
            }
        }

        /// <summary>
        /// Returns the trusted host and key pairs stored on the system.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetKeys()
        {
            var platform = System.Environment.OSVersion.Platform;
            var hostkeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }
            return hostkeys;
        }

        /// <summary>
        /// Add to the stored trusted host fingerprint pairs stored on the system.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public bool SetKey(string host, string fingerprint)
        {
            var hostkeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }
            hostkeys.Add(host, fingerprint);

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return true;
        }
    }
}
