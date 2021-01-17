using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SSH.Stores
{
    public class MemoryStore : IStore
    {
        protected static ConcurrentDictionary<string, string> hostKeys;

        public static ConcurrentDictionary<string, string> HostKeys
        {
            get
            {
                return hostKeys ?? (hostKeys = new ConcurrentDictionary<string, string>());
            }
            protected set
            {
                hostKeys = value;
            }
        }
        protected virtual void OnGetKeys() { }
        protected virtual bool OnKeyUpdated() => true;

        public IDictionary<string, string> GetKeys()
        {
            OnGetKeys();
            return new Dictionary<string, string>(HostKeys);
        }

        public bool SetKey(string host, string fingerprint)
        {
            HostKeys.AddOrUpdate(host, fingerprint, (key, oldValue) => {
                return fingerprint;
            });
            return OnKeyUpdated();
        }
        /// <summary>
        /// If IStore is updated this can be the implementation
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public string GetKey(string host)
        {
            var found = HostKeys.TryGetValue(host, out string fingerprint);
            return found?fingerprint: default;
        }
    }
}
