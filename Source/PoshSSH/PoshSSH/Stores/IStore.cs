using System.Collections.Generic;

namespace SSH.Stores
{
    public interface IStore
    {
        IDictionary<string, string> GetKeys();

        bool SetKey(string host, string fingerprint);

    }
}
