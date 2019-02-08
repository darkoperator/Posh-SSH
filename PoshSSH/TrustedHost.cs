using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SSH
{
    public class TrustedHost
    {
        public string Host { get; set; }
        public List<string> Fingerprint { get; set; }

        public TrustedHost (string host, string Fingerprint) {
            this.Host = host;

            this.Fingerprint = new List<string>();
            this.Fingerprint.Add(Fingerprint);
        }
    }

    [JsonConverter(typeof(TrustedHostConverter))]
    public class TrustedHost2
    {
        public string Host { get; set; }
        
        public List<string> Fingerprint { get; set; }
        
        public TrustedHost2 () { this.Fingerprint = new List<string>(); }

        public TrustedHost2 (string host, string Fingerprint) {
            this.Host = host;

            this.Fingerprint = new List<string>();
            this.Fingerprint.Add(Fingerprint);
        }
    }

    public class TrustedHostList2
    {
        public List<TrustedHost2> TrustedHosts;

        public TrustedHostList2 () {
            this.TrustedHosts = new List<TrustedHost2>();
        }

        public TrustedHostList2 (TrustedHost2 trustedHosts) {

            this.TrustedHosts = new List<TrustedHost2>();
            this.TrustedHosts.Add(trustedHosts);
        }
    }

    public class TrustedHostConverter : JsonConverter {

        public override bool CanConvert(System.Type objectType){
            return (objectType == typeof(TrustedHost2));
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer) {
            
            JObject jo = JObject.Load(reader);

            TrustedHost2 trustedHost = new TrustedHost2();

            trustedHost.Host = (string)jo["Host"];
            trustedHost.Fingerprint = new List<string>();

            JArray ja = (JArray)jo["Fingerprint"];

            foreach (string fingerprint in ja) {
                trustedHost.Fingerprint.Add(fingerprint);
            }

            return trustedHost;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){}
    }
}
