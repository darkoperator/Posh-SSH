using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;

namespace SSH
{

    [Cmdlet(VerbsCommon.New, "SSHSession", DefaultParameterSetName = "NoKey")]
    public class New_SSHSession : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SSH Client with connection info
                            var Client = new SshClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteObject(SSHModHelper.AddToSSHSessionCollection(Client, this.SessionState), true);

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeac computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SSH Client with connection info
                            var Client = new SshClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteObject(SSHModHelper.AddToSSHSessionCollection(Client, this.SessionState), true);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SSH Client with connection info
                                var Client = new SshClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteObject(SSHModHelper.AddToSSHSessionCollection(Client, this.SessionState), true);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    WriteVerbose("Using SSH Key authentication for connection.");
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SSH Client with connection info
                                var Client = new SshClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteObject(SSHModHelper.AddToSSHSessionCollection(Client, this.SessionState), true);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }

        } // End process record
    } //end of the class for the New-SSHSession
    //###################################################


    [Cmdlet(VerbsCommon.New, "SFTPSession", DefaultParameterSetName = "NoKey")]
    public class New_SFTPSession : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SFTP Client with connection info
                            var Client = new SftpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteObject(SSHModHelper.AddToSFTPSessionCollection(Client, this.SessionState), true);

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeac computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SFTP Client with connection info
                            var Client = new SftpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteObject(SSHModHelper.AddToSFTPSessionCollection(Client, this.SessionState), true);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SFTP Client with connection info
                                var Client = new SftpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteObject(SSHModHelper.AddToSFTPSessionCollection(Client, this.SessionState), true);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SFTP Client with connection info
                                var Client = new SftpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteObject(SSHModHelper.AddToSFTPSessionCollection(Client, this.SessionState), true);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                }

            } // End process record
        }

    } //end of the class for the New-SFTPSession
    //###################################################


    [Cmdlet(VerbsCommon.Set, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class Set_SCPFile : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        //Local File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            ParameterSetName = "Key")]
        public String LocalFile
        {
            get { return localfile; }
            set { localfile = value; }
        }
        private String localfile = "";

        //Remote File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            ParameterSetName = "Key")]
        public String RemoteFile
        {
            get { return remotefile; }
            set { remotefile = value; }
        }
        private String remotefile = "";
        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();

                            var localfullPath = Path.GetFullPath(localfile);
                            if (File.Exists(localfullPath))
                            {
                                FileInfo fil = new FileInfo(@localfullPath);
                                Client.Upload_2(fil, remotefile);
                                Client.Disconnect();
                            }

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteVerbose("Connection succesfull");
                            var localfullPath = Path.GetFullPath(localfile);
                            if (File.Exists(localfullPath))
                            {
                                WriteVerbose("Uploading " + localfullPath);
                                FileInfo fil = new FileInfo(@localfullPath);
                                Client.Upload_2(fil, remotefile);

                                Client.Disconnect();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");
                                var localfullPath = Path.GetFullPath(localfile);
                                if (File.Exists(localfullPath))
                                {
                                    FileInfo fil = new FileInfo(@localfullPath);
                                    Client.Upload(fil, remotefile);
                                    Client.Disconnect();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");
                                var localfullPath = Path.GetFullPath(localfile);
                                if (File.Exists(localfullPath))
                                {
                                    FileInfo fil = new FileInfo(@localfullPath);
                                    Client.Upload_2(fil, remotefile);
                                    Client.Disconnect();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                }

            } // End process record
        }

    } //end of the class for the Set-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Get, "SCPFile", DefaultParameterSetName = "NoKey")]
    public class Get_SCPFile : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        //Local File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFile
        {
            get { return localfile; }
            set { localfile = value; }
        }
        private String localfile = "";

        //Remote File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFile
        {
            get { return remotefile; }
            set { remotefile = value; }
        }
        private String remotefile = "";
        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();

                            var localfullPath = Path.GetFullPath(localfile);
                            //if (File.Exists(localfullPath))
                            //{
                            WriteVerbose("Downloading " + remotefile);
                            FileInfo fil = new FileInfo(@localfullPath);
                            Client.Download(remotefile, fil);
                            Client.Disconnect();
                            //}

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteVerbose("Connection succesfull");
                            var localfullPath = Path.GetFullPath(localfile);
                            WriteVerbose("Downloading " + remotefile);
                            FileInfo fil = new FileInfo(@localfullPath);
                            Client.Download(remotefile, fil);
                            Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");
                                var localfullPath = Path.GetFullPath(localfile);
                                WriteVerbose("Downloading " + remotefile);
                                FileInfo fil = new FileInfo(@localfullPath);
                                Client.Download(remotefile, fil);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");
                                var localfullPath = Path.GetFullPath(localfile);
                                WriteVerbose("Downloading " + remotefile);
                                FileInfo fil = new FileInfo(@localfullPath);
                                Client.Download(remotefile, fil);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                }

            } // End process record
        }

    } //end of the class for the Get-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Get, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class Get_SCPFolder : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFolder
        {
            get { return localfolder; }
            set { localfolder = value; }
        }
        private String localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFolder
        {
            get { return remotefolder; }
            set { remotefolder = value; }
        }
        private String remotefolder = "";

        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();

                            var localfullPath = Path.GetFullPath(localfolder);
                            WriteVerbose("Downloading " + remotefolder);
                            DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                            Client.Download(remotefolder, dirinfo);
                            Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteVerbose("Connection succesfull");

                            var localfullPath = Path.GetFullPath(localfolder);
                            WriteVerbose("Downloading " + remotefolder);
                            DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                            Client.Download(remotefolder, dirinfo);
                            Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");

                                var localfullPath = Path.GetFullPath(localfolder);
                                WriteVerbose("Downloading " + remotefolder);
                                DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                                Client.Download(remotefolder, dirinfo);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");

                                var localfullPath = Path.GetFullPath(localfolder);
                                WriteVerbose("Downloading " + remotefolder);
                                DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                                Client.Download(remotefolder, dirinfo);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                }

            } // End process record
        }

    } //end of the class for the Get-SCPFile
    ////###################################################


    [Cmdlet(VerbsCommon.Set, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class Set_SCPFolder : PSCmdlet
    {
        // Hosts tp conect to
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Key")]
        [Alias("HostName", "Computer", "IPAddress", "Host")]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            ParameterSetName = "Key")]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return keyfile; }
            set { keyfile = value; }
        }
        private String keyfile = "";

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFolder
        {
            get { return localfolder; }
            set { localfolder = value; }
        }
        private String localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFolder
        {
            get { return remotefolder; }
            set { remotefolder = value; }
        }
        private String remotefolder = "";

        protected override void ProcessRecord()
        {
            if (keyfile.Equals(""))
            {
                //###########################################
                //### Connect using Username and Password ###
                //###########################################

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);
                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new ConnectionInfo(computer,
                            port,
                            credential.GetNetworkCredential().UserName,
                            ptype,
                            proxyserver,
                            proxyport,
                            proxycredential.GetNetworkCredential().UserName,
                            proxycredential.GetNetworkCredential().Password,
                            KIconnectInfo,
                            PassconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();

                            var localfullPath = Path.GetFullPath(localfolder);
                            WriteVerbose("Uploading " + remotefolder);
                            DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                            Client.Upload(dirinfo, remotefolder);
                            Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
                else
                {
                    WriteVerbose("Using Username and Password authentication for connection.");
                    // Connection info for Keyboard Interactive
                    var KIconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.GetNetworkCredential().UserName);
                    var PassconnectInfo = new PasswordAuthenticationMethod(credential.GetNetworkCredential().UserName, credential.GetNetworkCredential().Password);

                    foreach (var computer in computername)
                    {
                        WriteVerbose("Connecting to " + computer + " with user " + credential.GetNetworkCredential().UserName);
                        var connectInfo = new Renci.SshNet.ConnectionInfo(computer, credential.GetNetworkCredential().UserName,
                                    PassconnectInfo,
                                    KIconnectInfo);

                        // Event Handler for interactive Authentication
                        KIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                        {
                            foreach (var prompt in e.Prompts)
                            {
                                if (prompt.Request.Contains("Password"))
                                    prompt.Response = credential.GetNetworkCredential().Password;
                            }
                        };
                        try
                        {
                            //Ceate instance of SCP Client with connection info
                            var Client = new ScpClient(connectInfo);

                            // Connect to  host using Connection info
                            Client.Connect();
                            WriteVerbose("Connection succesfull");

                            var localfullPath = Path.GetFullPath(localfolder);
                            WriteVerbose("Uploading " + remotefolder);
                            DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                            Client.Upload(dirinfo, remotefolder);
                            Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } // End foroeach computer
                }
            }
            else
            {
                //##########################
                //### Connect using Keys ###
                //##########################

                WriteVerbose("Using SSH Key authentication for connection.");
                var fullPath = Path.GetFullPath(keyfile);

                if (proxyserver != "")
                {
                    // Set the proper proxy type
                    var ptype = Renci.SshNet.ProxyTypes.Http;
                    WriteVerbose("A Proxy Server has been specified");
                    switch (proxytype)
                    {
                        case "HTTP":
                            ptype = Renci.SshNet.ProxyTypes.Http;
                            break;
                        case "Socks4":
                            ptype = Renci.SshNet.ProxyTypes.Socks4;
                            break;
                        case "Socks5":
                            ptype = Renci.SshNet.ProxyTypes.Socks5;
                            break;
                    }

                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);

                                if (proxycredential.UserName == "")
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        sshkey);
                                }
                                else
                                {
                                    connectionInfo = new PrivateKeyConnectionInfo(computer,
                                        credential.GetNetworkCredential().UserName,
                                        ptype,
                                        proxyserver,
                                        proxyport,
                                        proxycredential.GetNetworkCredential().UserName,
                                        proxycredential.GetNetworkCredential().Password,
                                        sshkey);
                                }
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");

                                var localfullPath = Path.GetFullPath(localfolder);
                                WriteVerbose("Uploading " + remotefolder);
                                DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                                Client.Upload(dirinfo, remotefolder);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        foreach (var computer in computername)
                        {
                            PrivateKeyConnectionInfo connectionInfo;
                            if (credential.GetNetworkCredential().Password == "")
                            {
                                WriteVerbose("Using key with no passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath));
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            else
                            {
                                WriteVerbose("Using key with passphrase.");
                                var sshkey = new PrivateKeyFile(File.OpenRead(@fullPath), credential.GetNetworkCredential().Password);
                                connectionInfo = new PrivateKeyConnectionInfo(computer, credential.GetNetworkCredential().UserName, sshkey);
                            }
                            try
                            {
                                //Ceate instance of SCP Client with connection info
                                var Client = new ScpClient(connectionInfo);

                                // Connect to  host using Connection info
                                Client.Connect();
                                WriteVerbose("Connection succesfull");

                                var localfullPath = Path.GetFullPath(localfolder);
                                WriteVerbose("Uploading " + remotefolder);
                                DirectoryInfo dirinfo = new DirectoryInfo(@localfullPath);
                                Client.Upload(dirinfo, remotefolder);
                                Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                }

            } // End process record
        }

    } //end of the class for the Set-SCPFile
    ////###################################################


    // Class for creating PS Custom Objects
    public class SSHModHelper
    {
        // Create Custom Object from Hashtable
        public static PSObject CreateCustom(Hashtable properties)
        {
            PSObject obj = new PSObject();
            foreach (DictionaryEntry NoteProperty in properties)
            {
                obj.Properties.Add(new PSNoteProperty(NoteProperty.Key.ToString(), NoteProperty.Value));
            }
            return obj;
        }

        public static SSHSession AddToSSHSessionCollection(Renci.SshNet.SshClient sshclient, SessionState pssession)
        {
            //Set initial variables
            SSHSession obj = new SSHSession();
            List<SSHSession> SSHSessions = new List<SSHSession>();
            Int32 Index = 0;

            // Retrive existing sessions from the globla variable.
            var sessionvar = pssession.PSVariable.GetValue("Global:SshSessions") as List<SSHSession>;

            // If sessions exist  we set the proper index number for it.
            if (sessionvar != null)
            {
                SSHSessions.AddRange(sessionvar);
                Index = SSHSessions.Count;
            }

            // Create the object that will be saved
            obj.Index = Index;
            obj.Host = sshclient.ConnectionInfo.Host;
            obj.Session = sshclient;
            SSHSessions.Add(obj);

            // Set the Global Variable for the sessions.
            pssession.PSVariable.Set((new PSVariable("Global:SshSessions", SSHSessions, ScopedItemOptions.AllScope)));

            return obj;
        }

        public static SFTPSession AddToSFTPSessionCollection(Renci.SshNet.SftpClient sftpclient, SessionState pssession)
        {
            //Set initial variables
            SFTPSession obj = new SFTPSession();
            List<SFTPSession> SFTPSessions = new List<SFTPSession>();
            Int32 Index = 0;

            // Retrive existing sessions from the globla variable.
            var sessionvar = pssession.PSVariable.GetValue("Global:SFTPSessions") as List<SFTPSession>;

            // If sessions exist  we set the proper index number for it.
            if (sessionvar != null)
            {
                SFTPSessions.AddRange(sessionvar);
                Index = SFTPSessions.Count;
            }

            // Create the object that will be saved
            obj.Index = Index;
            obj.Host = sftpclient.ConnectionInfo.Host;
            obj.Session = sftpclient;
            SFTPSessions.Add(obj);

            // Set the Global Variable for the sessions.
            pssession.PSVariable.Set((new PSVariable("Global:SFTPSessions", SFTPSessions, ScopedItemOptions.AllScope)));
            return obj;
        }
    }

    // Object for SSH Sessions
    public class SSHSession
    {
        public Int32 Index;
        public string Host;
        public Renci.SshNet.SshClient Session;
        public bool Connected
        {
            get { return this.Session.IsConnected; }
        }

        // Method for Connecing
        public void Connect()
        {
            this.Session.Connect();
        }

        // Method for disconecting session
        public void Disconnect()
        {
            this.Session.Disconnect();
        }
    }

    // Object for SSTP Sessions
    public class SFTPSession
    {
        public Int32 Index;
        public string Host;
        public Renci.SshNet.SftpClient Session;
        public bool Connected
        {
            get { return this.Session.IsConnected; }
            set { }
        }
        public void Disconnect()
        {
            this.Session.Disconnect();
        }

        // Method for Connecing
        public void Connect()
        {
            this.Session.Connect();
        }
    }
} 
//###################################################
