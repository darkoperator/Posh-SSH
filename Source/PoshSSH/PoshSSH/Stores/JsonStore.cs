using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SSH.Stores
{
    public class ConfigFileStruct
    {
        public Dictionary<string, KnownHostValue> Keys { get; set; } = new Dictionary<string, KnownHostValue>();
    }
    public class JsonStore : MemoryStore
    {
        private readonly string FileName;

        public JsonStore(string fileName)
        {
            FileName = fileName;
        }
       
        public void LoadFromDisk()
        {
            if (File.Exists(FileName))
            {
                var jsonString = File.ReadAllText(FileName);
                var keys = JsonConvert.DeserializeObject<ConfigFileStruct>(jsonString).Keys;
                HostKeys = new ConcurrentDictionary<string, KnownHostValue>(keys);
            }
        }

        private void WriteToDisk()
        {
            var jsonString = JsonConvert.SerializeObject(new ConfigFileStruct()
                {
                    Keys = HostKeys.ToDictionary(x => x.Key, x => x.Value)
                },
                Formatting.Indented
            );
            var d = Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            if (d.Exists)
            {
                File.WriteAllText(FileName, jsonString);
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
