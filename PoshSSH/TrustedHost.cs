using System.Collections.Generic;

namespace SSH
{
    class TrustedHost
    {
        public string Host { get; set; }
        public IList<string> FingerPrints { get; set; }
    }
}
