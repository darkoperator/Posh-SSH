using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace SSH.Stores
{
    public class ConfigFileStruct
    {
        public Dictionary<string, KnownHostValue> Keys { get; set; } = new Dictionary<string, KnownHostValue>();
    }
    public class JsonStore : MemoryStore
    {
        private readonly string FileName;
        private readonly DataContractJsonSerializerSettings serializationSettings;

        public JsonStore(string fileName)
        {
            FileName = fileName;
            serializationSettings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
        }
       
        public void LoadFromDisk()
        {
            if (File.Exists(FileName))
            {
                using (var stream = File.OpenRead(FileName))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ConfigFileStruct), serializationSettings);
                    var keys = (ConfigFileStruct)serializer.ReadObject(stream);
                    if (Equals(keys, null)) throw new Exception();
                    HostKeys = new ConcurrentDictionary<string, KnownHostValue>(keys.Keys);
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
                        var serializer = new DataContractJsonSerializer(typeof(ConfigFileStruct), serializationSettings);
                        serializer.WriteObject(writer,
                            new ConfigFileStruct()
                            {
                                Keys = HostKeys.ToDictionary(x => x.Key, x => x.Value)
                            }
                        );
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
