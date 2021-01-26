using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SSH.Stores
{
    public class MemoryStore : IStore
    {
        protected bool loaded;
        protected ConcurrentDictionary<string, Tuple<string, string>> hostKeys;
        protected ConcurrentDictionary<string, Tuple<string, string>> HostKeys
        {
            get
            {
                return hostKeys ?? (hostKeys = new ConcurrentDictionary<string, Tuple<string, string>>());
            }
            set
            {
                hostKeys = value;
            }
        }
        protected virtual void OnGetKeys() { }
        protected virtual bool OnKeyUpdated() => true;

        public bool SetKey(string Host, string HostKeyName, string Fingerprint)
        {
            var hostData = new Tuple<string, string>(HostKeyName, Fingerprint);
            HostKeys.AddOrUpdate(Host, hostData, (key, oldValue) => {
                return hostData;
            });
            return OnKeyUpdated();
        }
        /// <summary>
        /// If IStore is updated this can be the implementation
        /// </summary>
        /// <param name="Host"></param>
        /// <returns></returns>
        public Tuple<string, string> GetKey(string Host)
        {
            if (!loaded) {
                OnGetKeys();
                loaded = true;
            }
            var found = HostKeys.TryGetValue(Host, out Tuple<string, string> hostData);
            return found?hostData: default;
        }

        public Boolean RemoveKey(string Host)
        {
            if (!loaded)
            {
                OnGetKeys();
                loaded = true;
            }
            var found = HostKeys.TryRemove(Host, out Tuple<string, string> hostData);
            return found;
        }
    }
}
