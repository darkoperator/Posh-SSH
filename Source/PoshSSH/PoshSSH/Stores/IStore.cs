using System;

namespace SSH.Stores
{
    public interface IStore
    {
        /// <summary>
        /// Save in storage keyName and fingerprint for given host
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="HostKeyName"></param>
        /// <param name="Fingerprint"></param>
        /// <returns></returns>
        bool SetKey(string Host, string HostKeyName, string Fingerprint);
        /// <summary>
        /// Get keyName and fingerprint for given host
        /// </summary>
        /// <param name="Host"></param>
        /// <returns>returns tuple from keyName and fingerprint if found or default if not</returns>
        Tuple<string, string> GetKey(string Host);
    }
}
