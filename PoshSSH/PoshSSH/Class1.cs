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

    [Cmdlet(VerbsCommon.New, "SSHSession")]
    public class New_SSHSession : PSCmdlet
    {
        // Hosts tp conect to
        [Parameter(Position = 0,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] ComputerName
        {
            get { return computername; }
            set { computername = value; }
        }
        private string[] computername;

        // Credentials for Connection
        [Parameter(Position = 1,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSCredential Credential
        {
            get { return credential; }
            set { credential = value; }
        }
        private PSCredential credential;

        // Port for SSH
        [Parameter()]
        public Int32 Port
        {
            get { return port; }
            set { port = value; }
        }
        private Int32 port = 22;

        //Proxy Server to use
        [Parameter(ParameterSetName = "Proxy")]
        public String ProxyServer
        {
            get { return proxyserver; }
            set { proxyserver = value; }
        }
        private String proxyserver = "";

        // Proxy Port
        [Parameter(ParameterSetName = "Proxy")]
        public Int32 ProxyPort
        {
            get { return proxyport; }
            set { proxyport = value; }
        }
        private Int32 proxyport = 8080;

        // Proxy Credentials
        [Parameter(ParameterSetName = "Proxy")]
        [ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get { return proxycredential; }
            set { proxycredential = value; }
        }
        private PSCredential proxycredential;

        // Proxy Type
        [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
        [Parameter(ParameterSetName = "Proxy")]
        public string ProxyType
        {
            get { return proxytype; }
            set { proxytype = value; }
        }
        private string proxytype = "HTTP";

        //SSH Key File
        [Parameter()]
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
                        WriteVerbose("Connecting to " + computer + " with user" + credential.GetNetworkCredential().UserName);
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
                        WriteVerbose("Connecting to " + computer + " with user" + credential.GetNetworkCredential().UserName);
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
    }

} //end of the class for the New-SSHSession
//###################################################

[Cmdlet(VerbsCommon.New, "SFTPSession")]
public class New_SFTPSession : PSCmdlet
{
    // Hosts tp conect to
    [Parameter(Position = 0,
        Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string[] ComputerName
    {
        get { return computername; }
        set { computername = value; }
    }
    private string[] computername;

    // Credentials for Connection
    [Parameter(Position = 1,
        Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public PSCredential Credential
    {
        get { return credential; }
        set { credential = value; }
    }
    private PSCredential credential;

    // Port for SSH
    [Parameter()]
    public Int32 Port
    {
        get { return port; }
        set { port = value; }
    }
    private Int32 port = 22;

    //Proxy Server to use
    [Parameter(ParameterSetName = "Proxy")]
    public String ProxyServer
    {
        get { return proxyserver; }
        set { proxyserver = value; }
    }
    private String proxyserver = "";

    // Proxy Port
    [Parameter(ParameterSetName = "Proxy")]
    public Int32 ProxyPort
    {
        get { return proxyport; }
        set { proxyport = value; }
    }
    private Int32 proxyport = 8080;

    // Proxy Credentials
    [Parameter(ParameterSetName = "Proxy")]
    [ValidateNotNullOrEmpty]
    public PSCredential ProxyCredential
    {
        get { return proxycredential; }
        set { proxycredential = value; }
    }
    private PSCredential proxycredential;

    // Proxy Type
    [ValidateSet("HTTP", "Socks4", "Socks5", IgnoreCase = true)]
    [Parameter(ParameterSetName = "Proxy")]
    public string ProxyType
    {
        get { return proxytype; }
        set { proxytype = value; }
    }
    private string proxytype = "HTTP";

    //SSH Key File
    [Parameter()]
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
                    WriteVerbose("Connecting to " + computer + " with user" + credential.GetNetworkCredential().UserName);
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
                    WriteVerbose("Connecting to " + computer + " with user" + credential.GetNetworkCredential().UserName);
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

    public static PSObject AddToSSHSessionCollection(Renci.SshNet.SshClient sshclient, SessionState pssession)
    {
        //Set initial variables
        PSObject obj = new PSObject();
        List<PSObject> SSHSessions = new List<PSObject>();
        Int32 Index = 0;

        // Retrive existing sessions from the globla variable.
        var sessionvar = pssession.PSVariable.GetValue("Global:SshSessions") as List<PSObject>;

        // If sessions exist  we set the proper index number for it.
        if (sessionvar != null)
        {
            SSHSessions.AddRange(sessionvar);
            Index = SSHSessions.Count;
        }

        // Create the object that will be saved
        obj.Properties.Add(new PSNoteProperty("Index", Index));
        obj.Properties.Add(new PSNoteProperty("Session", sshclient));
        obj.Properties.Add(new PSNoteProperty("Host", sshclient.ConnectionInfo.Host));
        SSHSessions.Add(obj);

        // Set the Global Variable for the sessions.
        pssession.PSVariable.Set((new PSVariable("Global:SshSessions", SSHSessions, ScopedItemOptions.AllScope)));

        return obj;
    }

    public static PSObject AddToSFTPSessionCollection(Renci.SshNet.SftpClient sftpclient, SessionState pssession)
    {
        //Set initial variables
        PSObject obj = new PSObject();
        List<PSObject> SFTPSessions = new List<PSObject>();
        Int32 Index = 0;

        // Retrive existing sessions from the globla variable.
        var sessionvar = pssession.PSVariable.GetValue("Global:SFTPSessions") as List<PSObject>;

        // If sessions exist  we set the proper index number for it.
        if (sessionvar != null)
        {
            SFTPSessions.AddRange(sessionvar);
            Index = SFTPSessions.Count;
        }

        // Create the object that will be saved
        obj.Properties.Add(new PSNoteProperty("Index", Index));
        obj.Properties.Add(new PSNoteProperty("Session", sftpclient));
        obj.Properties.Add(new PSNoteProperty("Host", sftpclient.ConnectionInfo.Host));
        SFTPSessions.Add(obj);

        // Set the Global Variable for the sessions.
        pssession.PSVariable.Set((new PSVariable("Global:SFTPSessions", SFTPSessions, ScopedItemOptions.AllScope)));
        return obj;
    }
}
