using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace SSH
{
    // Class for managing the keys 
    public class TrustedKeyMng
    {

        public enum HostAcceptOptions {
            None,
            AutoAccept,
            ErrorOnUntrusted
        }

        /// <summary>
        /// Returns the proper path to the trusted hosts file for the platform.
        /// </summary>
        protected static string FilePath
        {
            get
            {
                var platform = System.Environment.OSVersion.Platform;
                var trustedHostFile = "trusted_hosts.json";
                string path = "";

                if (platform == PlatformID.Win32NT)
                {
                    path = Environment.GetEnvironmentVariable("USERPROFILE");
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

        public static string GetTrustedHostFile()
        {
            return FilePath;
        }

        public static bool InitializeTrustedHostFile(PSHostUserInterface PSHostUI)
        {
            var hostkeys = new List<TrustedHost>(){};
            hostkeys.Add(new TrustedHost("Server1","a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0"));
            try {
                string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
                File.WriteAllText(FilePath, jsonkeys);
                return true;
            } catch (Exception e) {
                PSHostUI.WriteVerbose(e.ToString());
                return false;
            }

        }

        /// <summary>
        /// Returns the trusted host and key pairs stored on the system.
        /// </summary>
        /// <returns></returns>
        public static List<TrustedHost> GetKeys()
        {
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            List<TrustedHost> currentHostkeys = new List<TrustedHost>();
            currentHostkeys.AddRange(JsonConvert.DeserializeObject<List<TrustedHost>>(json));
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
        public static bool SetKey(string host, string fingerprint, PSHostUserInterface PSHostUI)
        {

            PSHostUI.WriteVerbose("Using: " + host + " - " + fingerprint);

            bool keySet = false;
            var hostkeys = new List<TrustedHost>(){};
            PSHostUI.WriteVerbose("Host Key Count - " + hostkeys.Count);

            var json = File.ReadAllText(FilePath);
            PSHostUI.WriteVerbose("File content: " + json.ToString());

            var currentHostkeys = JsonConvert.DeserializeObject<List<TrustedHost>>(json);
            PSHostUI.WriteVerbose("Trusted Host Key Count - " + currentHostkeys.Count);

            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            TrustedHost hostMatch;
            if ( (hostMatch = hostkeys.AsQueryable().SingleOrDefault(x => x.Host == host)) != null )
            {
                if ( !(hostMatch.Fingerprint.Contains(fingerprint)) )
                {
                    hostMatch.Fingerprint.Add(fingerprint);
                    keySet = true;
                }
                
            }
            else
            {
                hostkeys.Add(new TrustedHost(host, fingerprint));
                keySet = true;
            }
            

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return keySet;
        }

        /// <summary>
        /// Remove a host from the stored trusted host on the system.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool RemoveHost(string host)
        {
            bool hostRemoved = false;
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<List<TrustedHost>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            
            if ( hostkeys.RemoveAll(x => x.Host == host) > 0 )
            {
                hostRemoved = true;
            }

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return hostRemoved;
        }

        /// <summary>
        /// Remove a key for a host from the stored trusted host on the system.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static bool RemoveHostKey(string host, string fingerprint)
        {
            bool keyRemoved = false;
            var hostkeys = new List<TrustedHost>();
            var json = File.ReadAllText(FilePath);
            var currentHostkeys = JsonConvert.DeserializeObject<List<TrustedHost>>(json);
            if (currentHostkeys != null)
            {
                hostkeys = currentHostkeys;
            }

            TrustedHost hostMatch;
            if ( (hostMatch = hostkeys.AsQueryable().SingleOrDefault(x => x.Host == host)) != null )
            {
                if (hostMatch.Fingerprint.RemoveAll(x => x == fingerprint) > 0 )
                {
                    keyRemoved = true;
                }
                
            }

            string jsonkeys = JsonConvert.SerializeObject(hostkeys, Formatting.Indented);
            File.WriteAllText(FilePath, jsonkeys);
            return keyRemoved;
        }

        /// <summary>
        /// Checks if host and fingerprint are in the stored Trusted Host file, and returns true.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when host is known but key is not. Possible MITM attack!</exception>
        /// <exception cref="OperationCanceledException">Thrown when -ErrorOnUntrusted param is used.</exception>
        public static bool HostTrusted(string host, string fingerprint, TrustedKeyMng.HostAcceptOptions policy, PSHostUserInterface PSHostUI )
        {   
            bool trusted = false;
            List<TrustedHost> sshHostKeys = TrustedKeyMng.GetKeys();

            // Check if trusted host file contains host/fingerprint.
            TrustedHost hostMatch;
            if ( (hostMatch = sshHostKeys.AsQueryable().SingleOrDefault(x => x.Host == host)) != null )
            {
                if (hostMatch.Fingerprint.Contains(fingerprint))
                {
                    trusted = true;
                }
                else if (policy == HostAcceptOptions.AutoAccept)
                {
                    SetKey(host, fingerprint, PSHostUI);
                }
                else
                {
                    
                    string message = "Remote Host identification has changed. Possible MITM attack. If you understand the reason for this, you can add the host key with New-SSHTrustedHost.";
                    
                    PSHostUI.WriteWarning(message);
                    
                    throw new KeyNotFoundException();
                }
            }
            else if (policy == HostAcceptOptions.ErrorOnUntrusted)
            {
                StringBuilder message = new StringBuilder("Host not trusted. Use one of the following options:" + Environment.NewLine);

                message.Append("- Use New-SSHTrustedHost" + Environment.NewLine);
                message.Append("- Remove -ErrorOnUntrusted parameter" + Environment.NewLine);

                PSHostUI.WriteWarning(message.ToString());

                throw new OperationCanceledException(message.ToString());
            }
            // Otherwise, prompt user to accept the host/fingerprint.
            else if ( AcceptKey(host, fingerprint, PSHostUI) )
            {
                TrustedKeyMng.SetKey(host, fingerprint, PSHostUI);
                trusted = true;
            }

            return trusted;
        }

        /// <summary>
        /// Prompts user to accept a new fingerprint for a host.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static bool AcceptKey(string host, string fingerprint, PSHostUserInterface PSHostUI)
        {

            return PSHostUI.PromptYesNo(string.Format("Host \"{0}\" has key ({1}), accept new key?", host, fingerprint));
        }

        public sealed class PSHostUserInterface
        {
            private readonly ICommandRuntime _commandRuntime;
            public bool _yesToAll = false;
            public bool _noToAll = false;

            public PSHostUserInterface(ICommandRuntime commandRuntime)
            {
                if (commandRuntime == null)
                {
                    throw new ArgumentNullException("commandRuntime","Please pass Powershell command runtime and try again.");
                }

                _commandRuntime = commandRuntime;
            }

            public bool PromptYesNo(string prompt) => _commandRuntime.ShouldContinue(prompt, "Posh-SSH", ref _yesToAll, ref _noToAll);

            public void WriteWarning(string message) => _commandRuntime.WriteWarning(message);

            public void WriteVerbose(string message) => _commandRuntime.WriteVerbose(message);
        }
    }
}
