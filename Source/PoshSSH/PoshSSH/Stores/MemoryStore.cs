using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SSH.Stores
{
    public class MemoryStore : IStore
    {
        protected bool loaded;
        protected ConcurrentDictionary<string, string> hostKeys;
        protected ConcurrentDictionary<string, string> HostKeys
        {
            get
            {
                return hostKeys ?? (hostKeys = new ConcurrentDictionary<string, string>());
            }
            set
            {
                hostKeys = value;
            }
        }
        protected virtual void OnGetKeys() { }
        protected virtual bool OnKeyUpdated() => true;

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
            if (!loaded) {
                OnGetKeys();
                loaded = true;
            }
            var found = HostKeys.TryGetValue(host, out string fingerprint);
            return found?fingerprint: default;
        }
    }
}
