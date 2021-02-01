using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SSH.Stores
{
    public class MemoryStore : IStore
    {
        protected ConcurrentDictionary<string, KnownHostValue> hostKeys;
        protected ConcurrentDictionary<string, KnownHostValue> HostKeys
        {
            get
            {
                if (hostKeys == default)
                {
                    hostKeys = new ConcurrentDictionary<string, KnownHostValue>();
                    OnGetKeys();
                }
                return hostKeys;
            }
            set
            {
                hostKeys = value;
            }
        }
        protected virtual void OnGetKeys() { }
        protected virtual bool OnKeyUpdated() => true;

        public virtual bool SetKey(string Host, string HostKeyName, string Fingerprint)
        {
            var hostData = new KnownHostValue() {
                HostKeyName = HostKeyName,
                Fingerprint = Fingerprint,
            };
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
        public virtual KnownHostValue GetKey(string Host)
        {
            var found = HostKeys.TryGetValue(Host, out var hostData);
            return found?hostData: default;
        }

        public virtual bool RemoveByHost(string Host)
        {
            return (HostKeys.TryRemove(Host, out var value)) ? OnKeyUpdated() : false;
        }

        public virtual bool RemoveByFingerprint(string Fingerprint)
        {
            var hostRecord = HostKeys.Where(kv => kv.Value.Fingerprint.Equals(Fingerprint));
            return (hostRecord.Any()) ? RemoveByHost(hostRecord.First().Key) : false;
        }

        public virtual KnownHostRecord[] GetAllKeys()
        {
            return HostKeys.Select(kv => new KnownHostRecord()
            {
                HostName = kv.Key,
                HostKeyName = kv.Value.HostKeyName,
                Fingerprint = kv.Value.Fingerprint
            }
            ).ToArray();
        }
    }
}
