using System;

namespace SSH
{
    /// <summary>
    /// Comparison of the algorithms a SSH server offers for one negotiation category
    /// against the algorithms the bundled SSH.NET library supports.
    /// </summary>
    public class AlgorithmComparison
    {
        /// <summary>
        /// Host that was probed. Null when only the local library was inspected.
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// Port that was probed. Null when only the local library was inspected.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// SSH identification string sent by the server, for example SSH-2.0-OpenSSH_9.6.
        /// Null when only the local library was inspected.
        /// </summary>
        public string ServerVersion { get; set; }

        /// <summary>
        /// Version of the Renci.SshNet assembly the module is running against.
        /// </summary>
        public string LibraryVersion { get; set; }

        /// <summary>
        /// Negotiation category: KeyExchange, HostKey, Encryption, Mac or Compression.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Direction the comparison applies to. Both when the server offers the same list
        /// in each direction, otherwise ClientToServer or ServerToClient.
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Algorithms the server advertised. Empty when only the local library was inspected.
        /// </summary>
        public string[] ServerOffered { get; set; }

        /// <summary>
        /// Algorithms the bundled library supports, in client preference order.
        /// </summary>
        public string[] ClientSupported { get; set; }

        /// <summary>
        /// Algorithms both sides support, in client preference order. RFC 4253 section 7.1
        /// selects the first entry, so the first element is the algorithm that would be
        /// negotiated. Empty when only the local library was inspected.
        /// </summary>
        public string[] Common { get; set; }

        /// <summary>
        /// Whether the two sides have any algorithm in common. Null when only the local
        /// library was inspected, since there is no server to compare against.
        /// </summary>
        public bool? HasCommon { get; set; }
    }
}
