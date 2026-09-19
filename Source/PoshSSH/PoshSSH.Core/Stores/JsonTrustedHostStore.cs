using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Renci.SshNet.Common;

namespace SSH.Stores
{
    public class JsonTrustedHostStore : MemoryTrustedHostStore
    {
        public class TrustedHostConfigFileStruct
        {
            public int Version { get; set; } = 0;
            public Dictionary<string, List<TrustedHostValue>> HostKeys { get; set; } = new Dictionary<string, List<TrustedHostValue>>();
        }

        public class LegacyConfigFileStruct
        {
            public Dictionary<string, TrustedHostValue> Keys { get; set; } = new Dictionary<string, TrustedHostValue>();
        }

        private readonly string FileName;
        private readonly DataContractJsonSerializerSettings serializationSettings;

        public JsonTrustedHostStore(string fileName)
        {
            FileName = fileName;
            serializationSettings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
        }
       
        private void LegacyLoadFromDisk()
        {
            // Try to read legacy format
            using (var stream = File.OpenRead(FileName))
            {
                var legacySerializer = new DataContractJsonSerializer(typeof(LegacyConfigFileStruct), serializationSettings);
                var legacyKeyData = (LegacyConfigFileStruct)legacySerializer.ReadObject(stream);
                if (legacyKeyData == null)
                {
                    throw new Exception(string.Format("Invalid keydata"));
                }
                HostKeys.Clear();
                foreach (var hostkv in legacyKeyData.Keys)
                {
                    var hostData = new ConcurrentDictionary<string, string>
                    {
                        [hostkv.Value.Fingerprint] = hostkv.Value.HostKeyName
                    };
                    HostKeys.TryAdd(hostkv.Key, hostData);
                }
            }
        }
        private void LoadFromDisk()
        {
            if (File.Exists(FileName))
            {
                using (var stream = File.OpenRead(FileName))
                {
                    var serializer = new DataContractJsonSerializer(typeof(TrustedHostConfigFileStruct), serializationSettings);
                    try {
                        var keyData = (TrustedHostConfigFileStruct)serializer.ReadObject(stream);
                        if (Equals(keyData, null) || keyData.Version != 1)
                        {
                            LegacyLoadFromDisk();
                        }
                        else
                        {
                            HostKeys.Clear();
                            foreach (var hostkv in keyData.HostKeys)
                            {
                                var hostData = new ConcurrentDictionary<string, string>(
                                    hostkv.Value.Select(v => new KeyValuePair<string, string>(v.Fingerprint, v.HostKeyName))
                                );
                                HostKeys.TryAdd(hostkv.Key, hostData);
                            }
                        }
                    }
                    catch (SerializationException ex) when (ex.Message.StartsWith("Expecting element 'root'"))
                    {
                        throw new SerializationException("Invalid config structure");
                    }
                }
            }
        }

        private void WriteToDisk()
        {
            var d = Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            if (d.Exists)
            {
                using (var stream = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        stream, System.Text.Encoding.UTF8, true, true, "  "))
                    {
                        var config = new TrustedHostConfigFileStruct()
                        {
                            Version = 1,
                            HostKeys = new Dictionary<string, List<TrustedHostValue>>(),
                        };
                        foreach (var hostkv in HostKeys)
                        {
                            var keylist = hostkv.Value.Select(fpkv => new TrustedHostValue() { Fingerprint = fpkv.Key, HostKeyName = fpkv.Value }).ToList();
                            if (keylist.Count > 0)
                            {
                                config.HostKeys.Add(
                                    hostkv.Key,
                                    keylist
                                );
                            }
                        }
                        var serializer = new DataContractJsonSerializer(typeof(TrustedHostConfigFileStruct), serializationSettings);
                        serializer.WriteObject(writer, config);
                        writer.Flush();
                    }
                }
            }
        }

        protected override void OnGetKeys()
        {
            LoadFromDisk();
        }

        protected override bool OnKeyUpdated()
        {
            WriteToDisk();
            return true;
        }
    }
}
