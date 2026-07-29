using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace SSH
{
    /// <summary>
    /// Reports which key exchange, host key, cipher, MAC and compression algorithms the
    /// bundled SSH.NET library supports, and optionally compares them against what a
    /// remote host offers.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SSHAlgorithm", DefaultParameterSetName = "Local")]
    [OutputType(typeof(AlgorithmComparison))]
    public class GetSshAlgorithm : PSCmdlet
    {
        /// <summary>
        /// Hosts to probe.
        /// </summary>
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ParameterSetName = "Remote",
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = "FQDN or IP Address of the host whose offered algorithms should be read.")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName { get; set; }

        /// <summary>
        /// Port for SSH.
        /// </summary>
        [Parameter(Mandatory = false,
            ParameterSetName = "Remote",
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "SSH TCP Port number to use for the probe.")]
        public Int32 Port { get; set; } = 22;

        /// <summary>
        /// ConnectionTimeout Parameter
        /// </summary>
        [Parameter(Mandatory = false,
            ParameterSetName = "Remote",
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Connection timeout interval in seconds.")]
        public int ConnectionTimeout { get; set; } = 10;

        protected override void ProcessRecord()
        {
            // Constructing a ConnectionInfo does not touch the network, it just populates the
            // algorithm dictionaries the library was built with.
            var connectionInfo = new PasswordConnectionInfo("localhost", 22, "posh-ssh", "posh-ssh");
            var libraryVersion = LibraryVersion();

            if (ParameterSetName == "Local")
            {
                WriteVerbose("Reporting the algorithms supported by Renci.SshNet " + libraryVersion + ".");
                foreach (var comparison in LocalComparisons(connectionInfo, libraryVersion))
                {
                    WriteObject(comparison);
                }
                return;
            }

            foreach (var computer in ComputerName)
            {
                KexInitProbeResult probe;
                try
                {
                    WriteVerbose("Reading the algorithms offered by " + computer + ":" + Port + ".");
                    probe = SshAlgorithmProbe.Probe(computer, Port, ConnectionTimeout);
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "SshAlgorithmProbeFailed", ErrorCategory.ConnectionError, computer));
                    continue;
                }

                WriteVerbose("Server identified itself as " + probe.ServerVersion + ".");
                foreach (var comparison in RemoteComparisons(computer, probe, connectionInfo, libraryVersion))
                {
                    WriteObject(comparison);
                }
            }
        }

        private static string LibraryVersion()
        {
            var version = typeof(ConnectionInfo).Assembly.GetName().Version;
            return version == null ? "unknown" : version.ToString();
        }

        private IEnumerable<AlgorithmComparison> LocalComparisons(ConnectionInfo info, string libraryVersion)
        {
            foreach (var category in Categories(info))
            {
                yield return new AlgorithmComparison
                {
                    LibraryVersion = libraryVersion,
                    Category = category.Key,
                    Direction = "Both",
                    ServerOffered = new string[0],
                    ClientSupported = category.Value,
                    Common = new string[0],
                    HasCommon = null
                };
            }
        }

        private IEnumerable<AlgorithmComparison> RemoteComparisons(string computer, KexInitProbeResult probe, ConnectionInfo info, string libraryVersion)
        {
            var message = probe.Message;
            var results = new List<AlgorithmComparison>();

            // Key exchange and host key are negotiated once, not per direction.
            results.Add(Build(computer, probe, libraryVersion, "KeyExchange", "Both",
                ClientList(info, "KeyExchange"), message.KeyExchangeAlgorithms));
            results.Add(Build(computer, probe, libraryVersion, "HostKey", "Both",
                ClientList(info, "HostKey"), message.ServerHostKeyAlgorithms));

            AddDirectional(results, computer, probe, libraryVersion, "Encryption", ClientList(info, "Encryption"),
                message.EncryptionAlgorithmsClientToServer, message.EncryptionAlgorithmsServerToClient);
            AddDirectional(results, computer, probe, libraryVersion, "Mac", ClientList(info, "Mac"),
                message.MacAlgorithmsClientToServer, message.MacAlgorithmsServerToClient);
            AddDirectional(results, computer, probe, libraryVersion, "Compression", ClientList(info, "Compression"),
                message.CompressionAlgorithmsClientToServer, message.CompressionAlgorithmsServerToClient);

            return results;
        }

        /// <summary>
        /// SSH negotiates ciphers, MACs and compression separately per direction, but the
        /// library's own support is not directional. Servers almost always offer the same
        /// list both ways, so collapse to a single row unless they genuinely differ.
        /// </summary>
        private void AddDirectional(List<AlgorithmComparison> results, string computer, KexInitProbeResult probe,
            string libraryVersion, string category, string[] clientSupported,
            string[] clientToServer, string[] serverToClient)
        {
            if (SameList(clientToServer, serverToClient))
            {
                results.Add(Build(computer, probe, libraryVersion, category, "Both", clientSupported, clientToServer));
            }
            else
            {
                results.Add(Build(computer, probe, libraryVersion, category, "ClientToServer", clientSupported, clientToServer));
                results.Add(Build(computer, probe, libraryVersion, category, "ServerToClient", clientSupported, serverToClient));
            }
        }

        private static bool SameList(string[] left, string[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            return left.SequenceEqual(right, StringComparer.Ordinal);
        }

        private AlgorithmComparison Build(string computer, KexInitProbeResult probe, string libraryVersion,
            string category, string direction, string[] clientSupported, string[] serverOffered)
        {
            var offered = serverOffered ?? new string[0];
            var common = SshAlgorithmProbe.Intersect(clientSupported, offered);

            return new AlgorithmComparison
            {
                ComputerName = computer,
                Port = Port,
                ServerVersion = probe.ServerVersion,
                LibraryVersion = libraryVersion,
                Category = category,
                Direction = direction,
                ServerOffered = offered,
                ClientSupported = clientSupported,
                Common = common,
                HasCommon = common.Length > 0
            };
        }

        private static IEnumerable<KeyValuePair<string, string[]>> Categories(ConnectionInfo info)
        {
            yield return new KeyValuePair<string, string[]>("KeyExchange", ClientList(info, "KeyExchange"));
            yield return new KeyValuePair<string, string[]>("HostKey", ClientList(info, "HostKey"));
            yield return new KeyValuePair<string, string[]>("Encryption", ClientList(info, "Encryption"));
            yield return new KeyValuePair<string, string[]>("Mac", ClientList(info, "Mac"));
            yield return new KeyValuePair<string, string[]>("Compression", ClientList(info, "Compression"));
        }

        private static string[] ClientList(ConnectionInfo info, string category)
        {
            switch (category)
            {
                case "KeyExchange":
                    return info.KeyExchangeAlgorithms.Keys.ToArray();
                case "HostKey":
                    return info.HostKeyAlgorithms.Keys.ToArray();
                case "Encryption":
                    return info.Encryptions.Keys.ToArray();
                case "Mac":
                    return info.HmacAlgorithms.Keys.ToArray();
                case "Compression":
                    return info.CompressionAlgorithms.Keys.ToArray();
                default:
                    return new string[0];
            }
        }
    } // end of the class for Get-SSHAlgorithm
}
