using System.Collections.Generic;

namespace SSH.Stores
{
    public interface IStore
    {
        bool SetKey(string host, string fingerprint);
        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <returns>returns fingerprint if found or default if not</returns>
        string GetKey(string host);
    }
}
