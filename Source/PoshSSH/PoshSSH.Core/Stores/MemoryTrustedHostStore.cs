using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SSH.Stores
{
    public class MemoryTrustedHostStore : ITrustedHostStore
    {
        protected ConcurrentDictionary<string, ConcurrentDictionary<string, string>> hostKeys;
        protected ConcurrentDictionary<string, ConcurrentDictionary<string, string>> HostKeys
        {
            get
            {
                if (hostKeys == default)
                {
                    hostKeys = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
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

        public virtual bool SetKey(string Host, string HostKeyName, string Fingerprint, bool append)
        {
            var newKeyCollection = new ConcurrentDictionary<string, string>();
            newKeyCollection.TryAdd(Fingerprint, HostKeyName);
            bool skip_update = false;
            if (append) {
                HostKeys.AddOrUpdate(Host, newKeyCollection, (key, oldKeyCollection) =>
                {
                    oldKeyCollection.AddOrUpdate(Fingerprint, HostKeyName, (key, oldHostKeyName) =>
                    {
                        skip_update = HostKeyName.Equals(oldHostKeyName);
                        return HostKeyName;
                    });
                    return oldKeyCollection;
                });
            }
            else
            {
                HostKeys.AddOrUpdate(Host, newKeyCollection, (key, oldKeyCollection) => {
                    skip_update = oldKeyCollection.Count == 1 && oldKeyCollection.ContainsKey(Fingerprint);
                    return newKeyCollection;
                });
            }
            if (skip_update)
                return true;
            else
                return OnKeyUpdated();
        }
        public virtual IEnumerable<TrustedHostValue> GetKeys(string Host)
        {
            var found = HostKeys.TryGetValue(Host, out var hostKeyCollection);
            if (found == false) yield break;
            else
            {
                foreach (var kv in hostKeyCollection)
                {
                    yield return new TrustedHostValue() { Fingerprint = kv.Key, HostKeyName = kv.Value };
                }
            }
        }

        public virtual bool RemoveHost(string Host)
        {
            return HostKeys.TryRemove(Host, out _) && OnKeyUpdated();
        }
        public virtual bool RemoveHostByFingerprint(string Fingerprint)
        {
            bool removedAny = false;
            foreach (var hostRecord in HostKeys.ToList())
            {
                var inner = hostRecord.Value;
                if (inner == null) continue;

                if (inner.ContainsKey(Fingerprint)) {
                    HostKeys.TryRemove(hostRecord.Key, out _);
                    removedAny = true;
                }
            }
            return removedAny && OnKeyUpdated();
        }
        public virtual bool RemoveHostFingerprint(string Fingerprint)
        {
            bool removedAny = false;
            foreach (var hostRecord in HostKeys.ToList())
            {
                var inner = hostRecord.Value;
                if (inner == null) continue;

                if (inner.TryRemove(Fingerprint, out _))
                    removedAny = true;

                if (inner.IsEmpty)
                    HostKeys.TryRemove(hostRecord.Key, out _);
            }
            return removedAny && OnKeyUpdated();
            // the same, but low readability:
            // var hostRecords = HostKeys.Where(kv => kv.Value.ContainsKey(Fingerprint)).ToList();
            // return hostRecords.Any() && hostRecords
            //         .Select(kv => kv.Value.TryRemove(Fingerprint, out _))
            //         .Aggregate(false, (acc, cur) => acc || cur) && OnKeyUpdated();
        }

        public virtual TrustedHostRecord[] GetAllKeys()
        {
            return HostKeys
                .SelectMany(hostkv => hostkv.Value.Select(fpkv => new TrustedHostRecord
                {
                    HostName = hostkv.Key,
                    HostKeyName = fpkv.Value,
                    Fingerprint = fpkv.Key,
                }))
                .ToArray();
        }
    }
}
