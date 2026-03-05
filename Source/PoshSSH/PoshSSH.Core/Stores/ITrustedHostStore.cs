using System.Collections.Generic;

namespace SSH.Stores
{
    public class TrustedHostRecord
    {
        public string HostName { get; set; }
        public string HostKeyName { get; set; }
        public string Fingerprint { get; set; }
    }
    public class TrustedHostValue
    {
        public string HostKeyName { get; set; }
        public string Fingerprint { get; set; }
    }

    public interface ITrustedHostStore
    {
        /// <summary>
        /// Save in storage keyName and fingerprint for given host
        /// </summary>
        /// <param name="Host">HostName to update</param>
        /// <param name="HostKeyName">Hostkey type name</param>
        /// <param name="Fingerprint">Fingerprint</param>
        /// <param name="append">New key appended to collection or collection is replaced</param>
        /// <returns></returns>
        bool SetKey(string Host, string HostKeyName, string Fingerprint, bool append);

        /// <summary>
        /// Get keyName and fingerprint for given host
        /// </summary>
        /// <param name="Host">HostName to get keys</param>
        /// <returns>returns tuple from keyName and fingerprint if found or default if not</returns>
        IEnumerable<TrustedHostValue> GetKeys(string Host);

        /// <summary>
        /// Remove all host records from store
        /// </summary>
        /// <param name="Hostname"></param>
        /// <returns></returns>
        bool RemoveHost(string Hostname);
        /// <summary>
        /// Remove all host records for hosts with selected fingerprint
        /// </summary>
        /// <param name="Fingerprint"></param>
        /// <returns></returns>
        bool RemoveHostByFingerprint(string Fingerprint);
        /// <summary>
        /// Remove trusted records for hosts with selected fingerprint
        /// </summary>
        /// <param name="Fingerprint"></param>
        /// <returns></returns>
        bool RemoveHostFingerprint(string Fingerprint);

        TrustedHostRecord[] GetAllKeys();
    }
}
