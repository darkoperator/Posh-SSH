using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Security;
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
        public static ConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            string keyfile,
            PSCredential credential,
            System.Security.SecureString passphrase,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)

        {
            string fullPath = Path.GetFullPath(keyfile);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File " + fullPath + " not found");
            var keyFileStream = File.OpenRead(@fullPath);
            return GetKeyConnectionInfo(computer, port, keyFileStream, credential, passphrase, proxyserver, proxytype, proxyport, proxycredential);
        }
        public static ConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            string[] keycontent,
            PSCredential credential,
            System.Security.SecureString passphrase,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)
        {
            var keyFileStream = new MemoryStream(System.Text.Encoding.Default.GetBytes(String.Join("\n", keycontent)));

            return GetKeyConnectionInfo(computer, port, keyFileStream, credential, passphrase, proxyserver, proxytype, proxyport, proxycredential);
        }
        private static ConnectionInfo GetKeyConnectionInfo(string computer,
            int port,
            Stream keyFileStream,
            PSCredential credential,
            System.Security.SecureString passphrase,
            string proxyserver,
            string proxytype,
            int proxyport,
            PSCredential proxycredential)
        {
            ConnectionInfo connectionInfo;
            // Create the key object.
            PrivateKeyFile sshkey;
            PSCredential keyPass = new PSCredential(credential.UserName, passphrase);

            if (keyPass.GetNetworkCredential().Password == String.Empty)
                sshkey = new PrivateKeyFile(keyFileStream);
            else
                sshkey = new PrivateKeyFile(keyFileStream, keyPass.GetNetworkCredential().Password);

            // Check if credentials in addition to passphrase where provided so as to create auth for both types.
            List<AuthenticationMethod> aMethods = new List<AuthenticationMethod>();
            if (credential.GetNetworkCredential().Password == String.Empty)
            {

                aMethods.Add(new PrivateKeyAuthenticationMethod(credential.UserName, new PrivateKeyFile[] { sshkey }));
            }
            else
            {
                aMethods.Add(new KeyboardInteractiveAuthenticationMethod(credential.UserName));
                aMethods.Add(new PasswordAuthenticationMethod(credential.UserName, credential.GetNetworkCredential().Password));
                aMethods.Add(new PrivateKeyAuthenticationMethod(credential.UserName, new PrivateKeyFile[] { sshkey }));
            }
            var methods = aMethods.ToArray();


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
                        methods);
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
                        methods);
                }
            }
            else // Handle connection with no proxy server
            {

                connectionInfo = new ConnectionInfo(computer,
                    port,
                    credential.UserName,
                    methods);

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
