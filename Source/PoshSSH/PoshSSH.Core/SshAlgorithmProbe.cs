using Renci.SshNet.Messages.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SSH
{
    /// <summary>
    /// What a SSH server advertised in its SSH_MSG_KEXINIT.
    /// </summary>
    internal class KexInitProbeResult
    {
        public string ServerVersion { get; set; }
        public KeyExchangeInitMessage Message { get; set; }
    }

    /// <summary>
    /// Reads the algorithms a SSH server advertises without authenticating to it.
    ///
    /// The server sends SSH_MSG_KEXINIT unencrypted as the first binary packet after the
    /// version exchange, so the whole probe completes before any key exchange happens and
    /// no credentials are required. Nothing is written to the trusted host store.
    /// </summary>
    internal static class SshAlgorithmProbe
    {
        // RFC 4253 requires implementations to accept packets of at least 35000 bytes.
        private const int MaxPacketLength = 35000;

        // RFC 4253 section 4.2 limits the identification string to 255 bytes.
        private const int MaxBannerLineLength = 255;

        // The RFC allows arbitrary lines before the identification string. Cap them so a
        // misbehaving endpoint cannot stream preamble at us forever.
        private const int MaxBannerLines = 100;

        private const byte SshMsgKexInit = 20;

        public static KexInitProbeResult Probe(string host, int port, int timeoutSeconds)
        {
            var timeoutMs = (timeoutSeconds > 0 ? timeoutSeconds : 10) * 1000;

            using (var client = new TcpClient())
            {
                var pending = client.BeginConnect(host, port, null, null);
                if (!pending.AsyncWaitHandle.WaitOne(timeoutMs))
                {
                    throw new IOException("Timed out connecting to " + host + ":" + port + ".");
                }
                client.EndConnect(pending);

                using (var stream = client.GetStream())
                {
                    stream.ReadTimeout = timeoutMs;
                    stream.WriteTimeout = timeoutMs;

                    var identification = Encoding.ASCII.GetBytes("SSH-2.0-PoshSSH_" + ModuleVersion() + "\r\n");
                    stream.Write(identification, 0, identification.Length);
                    stream.Flush();

                    return new KexInitProbeResult
                    {
                        ServerVersion = ReadIdentification(stream),
                        Message = ReadKeyExchangeInit(stream)
                    };
                }
            }
        }

        /// <summary>
        /// Algorithms both sides support, in client preference order.
        ///
        /// RFC 4253 section 7.1 picks the first algorithm on the client's list that also
        /// appears on the server's, so client order determines what actually gets negotiated
        /// and the first element of the result is the algorithm that would be chosen.
        /// </summary>
        public static string[] Intersect(IEnumerable<string> clientSupported, IEnumerable<string> serverOffered)
        {
            var offered = new HashSet<string>(serverOffered ?? new string[0], StringComparer.Ordinal);
            return (clientSupported ?? new string[0]).Where(a => offered.Contains(a)).ToArray();
        }

        private static string ReadIdentification(NetworkStream stream)
        {
            for (var line = 0; line < MaxBannerLines; line++)
            {
                var text = ReadLine(stream);
                if (text.StartsWith("SSH-", StringComparison.Ordinal))
                {
                    return text;
                }
            }
            throw new IOException("No SSH identification string was received from the server.");
        }

        private static string ReadLine(NetworkStream stream)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var b = stream.ReadByte();
                if (b < 0)
                {
                    throw new IOException("The connection was closed during the SSH version exchange.");
                }
                if (b == '\n')
                {
                    return sb.ToString();
                }
                if (b != '\r')
                {
                    sb.Append((char)b);
                }
                if (sb.Length > MaxBannerLineLength)
                {
                    throw new IOException("The server sent an identification line longer than " + MaxBannerLineLength + " bytes.");
                }
            }
        }

        private static KeyExchangeInitMessage ReadKeyExchangeInit(NetworkStream stream)
        {
            var header = ReadExactly(stream, 4);
            var packetLength = ((long)header[0] << 24) | ((long)header[1] << 16) | ((long)header[2] << 8) | header[3];
            if (packetLength < 2 || packetLength > MaxPacketLength)
            {
                throw new IOException("The server sent an implausible packet length of " + packetLength + " bytes.");
            }

            var packet = ReadExactly(stream, (int)packetLength);
            int paddingLength = packet[0];
            var payloadLength = (int)packetLength - paddingLength - 1;
            if (payloadLength < 1)
            {
                throw new IOException("The server sent a malformed packet (padding length " + paddingLength + " for a " + packetLength + " byte packet).");
            }

            // packet[0] is the padding length, so the payload starts at packet[1] and the
            // first payload byte is the message number.
            if (packet[1] != SshMsgKexInit)
            {
                throw new IOException("Expected SSH_MSG_KEXINIT (" + SshMsgKexInit + ") but the server sent message number " + packet[1] + ".");
            }

            var message = new KeyExchangeInitMessage();
            try
            {
                // Load wants the payload with the message number byte removed. Passing the whole
                // payload parses without error but yields no algorithms, so this offset matters.
                message.Load(packet, 2, payloadLength - 1);
            }
            catch (Exception e)
            {
                // A truncated or malformed name-list surfaces here as an argument or index
                // exception from the library, which says nothing useful on its own.
                throw new IOException("The server sent a SSH_MSG_KEXINIT packet that could not be parsed (" + e.Message + ").", e);
            }
            return message;
        }

        private static byte[] ReadExactly(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read <= 0)
                {
                    throw new IOException("The connection was closed after " + offset + " of " + count + " expected bytes.");
                }
                offset += read;
            }
            return buffer;
        }

        private static string ModuleVersion()
        {
            var version = typeof(SshAlgorithmProbe).Assembly.GetName().Version;
            return version == null ? "4.0.0" : version.ToString(3);
        }
    }
}
