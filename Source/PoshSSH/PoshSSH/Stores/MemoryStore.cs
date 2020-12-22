using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SSH.Stores
{
    public class MemoryStore : IStore
    {
        private static ConcurrentDictionary<string, string> hostKeys;

        public static ConcurrentDictionary<string, string> HostKeys
        {
            get
            {
                return hostKeys ?? (hostKeys = new ConcurrentDictionary<string, string>());
            }
        }

        public IDictionary<string, string> GetKeys()
        {
            var hostKeys = HostKeys;
            return hostKeys;
        }

        public bool SetKey(string host, string fingerprint)
        {
            HostKeys.AddOrUpdate(host, fingerprint, (key, oldValue) => {
                return fingerprint;
            });
            return true;
        }
    }
}
