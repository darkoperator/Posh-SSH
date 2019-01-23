using System.Collections.Generic;

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
}
