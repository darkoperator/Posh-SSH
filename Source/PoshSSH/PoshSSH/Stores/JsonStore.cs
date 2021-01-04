using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SSH.Stores
{
    public class ConfigFileStruct
    {
        public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
    }
    public class JsonStore : IStore
    {
        public String FileName;

        public JsonStore(String fileName)
        {
            FileName = fileName;
        }

        private ConfigFileStruct _settings = new ConfigFileStruct();
        public IDictionary<string, string> GetKeys()
        {
            LoadFromDisk();
            return _settings.Keys;
        }

        public bool SetKey(string host, string fingerprint)
        {
            _settings.Keys.Remove(host);
            _settings.Keys.Add(host, fingerprint);
            WriteToDisk();
            return true;
        }

        public void LoadFromDisk()
        {
            if (File.Exists(FileName))
            {
                var jsonString = File.ReadAllText(FileName);
                _settings = JsonSerializer.Deserialize<ConfigFileStruct>(jsonString);
            }
        }

        private void WriteToDisk()
        {
            var jsonString = JsonSerializer
                .Serialize<ConfigFileStruct>(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            File.WriteAllText(FileName, jsonString);
        }
    }
}
