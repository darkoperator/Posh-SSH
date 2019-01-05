using System;
using System.IO;
using System.Management.Automation;
using Renci.SshNet;

namespace SSH
{
    class ConnectionInfoGenerator
    {
        /// <summary>
        /// Generate a ConnectionInfoObject using a SSH Key.
        /// </summary>
        /// <param name="computer"></param>
        /// <param name="port"></param>
        /// <param name="keyfile"></param>
        /// <param name="credential"></param>
        /// <param name="proxyserver"></param>
        /// <param name="proxytype"></param>
        /// <param name="proxyport"></param>
        /// <param name="proxycredential"></param>
        /// <returns></returns>
        public static PrivateKeyConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            string keyfile,
            PSCredential credential,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)

        {
            string fullPath = Path.GetFullPath(keyfile);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File " + fullPath + " not found");
            var keyFileStream = File.OpenRead(@fullPath);
            return GetKeyConnectionInfo(computer, port, keyFileStream, credential, proxyserver, proxytype, proxyport, proxycredential);
        }
        public static PrivateKeyConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            string[] keycontent,
            PSCredential credential,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)
        {
            var keyFileStream = new MemoryStream(System.Text.Encoding.Default.GetBytes(String.Join("\n", keycontent)));

            return GetKeyConnectionInfo(computer, port, keyFileStream, credential, proxyserver, proxytype, proxyport, proxycredential);
        }
        private static PrivateKeyConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            Stream keyFileStream,
            PSCredential credential,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)
        {
            PrivateKeyConnectionInfo connectionInfo;
            // Create the key object.
            PrivateKeyFile sshkey;
            if (credential.GetNetworkCredential().Password == String.Empty)
                sshkey = new PrivateKeyFile(keyFileStream);
            else
                sshkey = new PrivateKeyFile(keyFileStream, credential.GetNetworkCredential().Password);

            if (proxyserver != String.Empty)
            {
                // Set the proper proxy type
                var ptype = ProxyTypes.Http;
                switch (proxytype)
                {
                    case "HTTP":
                        ptype = ProxyTypes.Http;
                        break;
                    case "Socks4":
                        ptype = ProxyTypes.Socks4;
                        break;
                    case "Socks5":
                        ptype = ProxyTypes.Socks5;
                        break;
                }

                if (proxycredential.UserName != String.Empty)
                {
                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                        port,
                        credential.UserName,
                        ptype,
                        proxyserver,
                        proxyport,
                        sshkey);
                }
                else
                {

                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                        port,
                        credential.UserName,
                        ptype,
                        proxyserver,
                        proxyport,
                        proxycredential.UserName,
                        proxycredential.GetNetworkCredential().Password,
                        sshkey);
                }
            }
            else // Handle connection with no proxy server
            {

                connectionInfo = new PrivateKeyConnectionInfo(computer,
                    port,
                    credential.UserName,
                    sshkey);

            }
            return connectionInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="computer"></param>
        /// <param name="port"></param>
        /// <param name="credential"></param>
        /// <param name="proxyserver"></param>
        /// <param name="proxytype"></param>
        /// <param name="proxyport"></param>
        /// <param name="proxycredential"></param>
        /// <param name="kIconnectInfo"></param>
        /// <returns></returns>
        public static ConnectionInfo GetCredConnectionInfo(string computer,
            int port,
            PSCredential credential,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential,
            KeyboardInteractiveAuthenticationMethod kIconnectInfo)
        {
            ConnectionInfo connectionInfo;
            var passconnectInfo = new PasswordAuthenticationMethod(credential.UserName,
                                                                   credential.GetNetworkCredential().Password);
            if (proxyserver != String.Empty)
            {
                // Set the proper proxy type
                var ptype = ProxyTypes.Http;
                switch (proxytype)
                {
                    case "HTTP":
                        ptype = ProxyTypes.Http;
                        break;
                    case "Socks4":
                        ptype = ProxyTypes.Socks4;
                        break;
                    case "Socks5":
                        ptype = ProxyTypes.Socks5;
                        break;
                }

                if (proxycredential == null)
                {
                    connectionInfo = new ConnectionInfo(computer,
                                                        port,
                                                        credential.UserName,
                                                        ptype,
                                                        proxyserver,
                                                        proxyport,
                                                        String.Empty,
                                                        String.Empty,
                                                        kIconnectInfo,
                                                        passconnectInfo);
                }
                else
                {

                    connectionInfo = new ConnectionInfo(computer,
                                                        port,
                                                        credential.UserName,
                                                        ptype,
                                                        proxyserver,
                                                        proxyport,
                                                        proxycredential.UserName,
                                                        proxycredential.GetNetworkCredential().Password,
                                                        kIconnectInfo,
                                                        passconnectInfo);
                }
            }
            else // Handle connection with no proxy server
            {

                connectionInfo = new ConnectionInfo(computer,
                                                    port,
                                                    credential.UserName,
                                                    passconnectInfo,
                                                    kIconnectInfo);

            }
            return connectionInfo;
        }

    }
}
