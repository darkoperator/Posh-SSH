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
            PrivateKeyConnectionInfo connectionInfo;
            var fullPath = Path.GetFullPath(keyfile);
            // Check if the file actually exists.
            if (File.Exists(fullPath))
            {
                // Create the key object.
                PrivateKeyFile sshkey;
                if (credential.GetNetworkCredential().Password == "")
                {
                    sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                }
                else
                {
                    sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                }

                if (proxyserver != "")
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

                    if (proxycredential.UserName != "")
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
            } // file exists
            else
            {
                throw new FileNotFoundException("Key file " + fullPath + " was not found.");
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
            if (proxyserver != "")
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

                if (proxycredential.UserName != "")
                {
                    connectionInfo = new ConnectionInfo(computer,
                                                        port,
                                                        credential.UserName,
                                                        ptype,
                                                        proxyserver,
                                                        proxyport,
                                                        "",
                                                        "",
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
