using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SSH.Stores
{
    public class OpenSSHTrustedHostStore : MemoryTrustedHostStore
    {
        private class HashedKeysStruct
        {
            public byte[] Salt { get; set; }
            public string HostHash { get; set; }
            public string HostKeyName { get; set; }
            public string Fingerprint { get; set; }
        }

        private class WildcardKeysStruct
        {
            public WildcardPattern Pattern { get; set; }
            public string HostKeyName { get; set; }
            public string Fingerprint { get; set; }
        }

        private readonly string FileName;
        private readonly ConcurrentBag<HashedKeysStruct> hashedKeys;
        private readonly ConcurrentBag<WildcardKeysStruct> wildcardKeys;

        public OpenSSHTrustedHostStore(string fileName)
        {
            FileName = fileName;
            hashedKeys = new ConcurrentBag<HashedKeysStruct>();
            wildcardKeys = new ConcurrentBag<WildcardKeysStruct>();
        }

        private void LoadFromDisk()
        {
            if (!File.Exists(FileName))
                return;
            foreach (var line in File.ReadAllLines(FileName)) {
                // skip emty lines or comments
                // skip @cert-authority and @revoked because we do not validate
                if (line.Length < 1 || line[0] == '#' || line[0] == '@') { continue; }

                var hostparts = line.Split(' ');
                // Skip invalid lines
                if (hostparts.Length < 3 || hostparts[0].Length < 1) { continue; }
                var (hostName, hostKeyName, pubKey) = (hostparts[0], hostparts[1], hostparts[2]);

                string fingerprint;
                using (var cipher = SHA256.Create())
                {
                    var pubkey = Convert.FromBase64String(pubKey);
                    var fp_as_bytes = cipher.ComputeHash(pubkey);
                    fingerprint = Convert.ToBase64String(fp_as_bytes).Replace("=", "");
                }

                // hashed hostname, can be only one on line
                if (hostName[0] == '|')
                {
                    var hashparts = hostName.Split('|');
                    // skip invalid or unsupported lines
                    if (hashparts.Length < 4 || hashparts[1] != "1") { continue; }
                    hashedKeys.Add(
                        new HashedKeysStruct()
                        {
                            Salt = Convert.FromBase64String(hashparts[2]),
                            HostHash = hashparts[3],
                            HostKeyName = hostKeyName,
                            Fingerprint = fingerprint,
                        }
                    );
                }
                else
                {
                    foreach (var host in hostName.Split(','))
                    {
                        var (tmpHost, tmpFingerprint) = (host, fingerprint);
                        Match m = Regex.Match(host, @"^\[(.*)\]:(\d+)$");
                        if (host[0] == '!') // Host connection denied
                        {
                            tmpHost = host.Substring(1); // clean '!'
                            tmpFingerprint = '!' + fingerprint; // make fingerprint for this host invalid
                        }
                        // [host]:port
                        else if (m.Success)
                        {
                            tmpHost = m.Groups[1].Value + ':' + m.Groups[2].Value;
                            this.SetKey(tmpHost, hostKeyName, tmpFingerprint, true);
                        }
                        // wildcard pattern
                        else if (WildcardPattern.ContainsWildcardCharacters(host))
                        {
                            wildcardKeys.Add(
                                new WildcardKeysStruct()
                                {
                                    Pattern = new WildcardPattern(tmpHost),
                                    HostKeyName = hostKeyName,
                                    Fingerprint = tmpFingerprint,
                                }
                            );
                        }
                        // simple host
                        else
                        {
                            this.SetKey(tmpHost, hostKeyName, tmpFingerprint, true);
                        }
                    }
                }
            }
        }

        protected override void OnGetKeys()
        {
            LoadFromDisk();
        }
        public override bool SetKey(string Host, string HostKeyName, string Fingerprint, bool append)
        {
            base.SetKey(Host, HostKeyName, Fingerprint, append);
            // It is read-only collection
            return false;
        }

        public override IEnumerable<TrustedHostValue> GetKeys(string Host)
        {
            if (HostKeys.TryGetValue(Host, out var keyData))
            {
                foreach (var kv in keyData)
                    yield return new TrustedHostValue { Fingerprint = kv.Key, HostKeyName = kv.Value };
            }

            var hostbytes = Encoding.ASCII.GetBytes(Host);

            foreach (var hashedKey in hashedKeys)
            {
                using (HMACSHA1 hmac = new HMACSHA1(hashedKey.Salt))
                {
                    var hostHash = Convert.ToBase64String(hmac.ComputeHash(hostbytes));
                    if (hostHash.Equals(hashedKey.HostHash))
                    {
                        yield return new TrustedHostValue() {
                            HostKeyName = hashedKey.HostKeyName,
                            Fingerprint = hashedKey.Fingerprint
                        };
                    }
                }
            }
            foreach (var wildcardKey in wildcardKeys)
            {
                if (wildcardKey.Pattern.IsMatch(Host))
                {
                    yield return new TrustedHostValue()
                    {
                        HostKeyName = wildcardKey.HostKeyName,
                        Fingerprint = wildcardKey.Fingerprint,
                    };
                }
            }
        }

        public override bool RemoveHost(string Host)
        {
            return false;
        }
        public override bool RemoveHostByFingerprint(string Fingerprint)
        {
            return false;
        }
        public override bool RemoveHostFingerprint(string Fingerprint)
        {
            return false;
        }

        public override TrustedHostRecord[] GetAllKeys()
        {
            var keys = new List<TrustedHostRecord>(base.GetAllKeys());

            keys.AddRange(hashedKeys.Select(v => new TrustedHostRecord()
            {
                HostName = v.HostHash,
                HostKeyName = v.HostKeyName,
                Fingerprint = v.Fingerprint,
            }
            ));
            keys.AddRange(wildcardKeys.Select(v => new TrustedHostRecord()
            {
                HostName = v.Pattern.ToString(),
                HostKeyName = v.HostKeyName,
                Fingerprint = v.Fingerprint,
            }
            ));
            return keys.ToArray();
        }
    }
}
