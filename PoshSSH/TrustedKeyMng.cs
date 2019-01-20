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
        public static Dictionary<string, List<string>> GetKeys()
        {
            var platform = System.Environment.OSVersion.Platform;
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<TrustedHost>>(json);
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
        public static bool SetKey(string host, string fingerprint)
        {
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<TrustedHost>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            if (hostkeys.ContainsKey(host))
            {
                hostkeys[host].Add(fingerprint);
            }
            else {
                hostkeys.Add(host, fingerprint);
            }
            

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return true;
        }

        /// <summary>
        /// Remove a host from the stored trusted host on the system.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool RemoveHost(string host)
        {
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<TrustedHost>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            bool result = false;
            if (hostkeys.ContainsKey(host))
            {
                hostkeys.Remove(host);
                result = true;
            }

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return result;
        }

        /// <summary>
        /// Remove a key for a host from the stored trusted host on the system.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static bool RemoveHostKey(string host, string fingerprint)
        {
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<TrustedHost>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            bool result = false;
            if (hostkeys.ContainsKey(host))
            {
                if (hostkeys[host].Contains(fingerprint))
                {
                    hostkeys[host].Remove(fingerprint);
                    result = true;
                }
                
            }

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return result;
        }

        /// <summary>
        /// Checks if host and fingerprint are in the stored Trusted Host file, and returns true.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static bool HostTrusted(string host, string fingerprint, bool verbose)
        {
            _sshHostKeys = TrustedKeyMng.GetKeys();

            bool trusted = false;

            if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
            {
                Host.UI.WriteVerboseLine("Fingerprint for " + computer1 + ": " + fingerPrint);
            }

            // Check if trusted host file contains host/fingerprint.
            if (_sshHostKeys.ContainsKey(computer1))
            {
                if (_sshHostKeys[computer1].Contains(fingerPrint))
                {
                    if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                    {
                        Host.UI.WriteVerboseLine("Fingerprint matched trusted fingerprint for host " + computer1);
                    }
                    trusted = true;

                }
                else
                {
                    
                    //Host.UI.WriteWarningLine("WARNING: REMOTE HOST IDENTIFICATION HAS CHANGED!")
                }
            }
            // Otherwise, prompt user to accept the host/fingerprint.
            else if ( AcceptKey(host, fingerprint) )
            {
                TrustedKeyMng.SetKey(host, fingerPrint);
                trusted = true;
            }
        }

        /// <summary>
        /// Prompts user to accept a new fingerprint for a host.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static bool AcceptKey(string host, string fingerprint, bool verbose)
        {
            var choices = new Collection<ChoiceDescription>
            {
                new ChoiceDescription("Y"),
                new ChoiceDescription("N")
            };

            choice = Host.UI.PromptForChoice("Server SSH Fingerprint - " + host, "Do you want to trust the fingerprint " + fingerPrint, choices, 1);
        }
    }
}
