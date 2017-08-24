﻿using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace SSH
{
    [Cmdlet(VerbsCommon.Set, "SCPFolder", DefaultParameterSetName = "NoKey")]
    public class SetScpFolder : PSCmdlet
    {
        // Hosts to conect to
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
            get { return _computername; }
            set { _computername = value; }
        }
        private string[] _computername;

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
        [System.Management.Automation.CredentialAttribute()]
        public PSCredential Credential
        {
            get { return _credential; }
            set { _credential = value; }
        }
        private PSCredential _credential;

        // Port for SSH
        private Int32 _port = 22;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }
        }


        //Proxy Server to use
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String ProxyServer
        {
            get { return _proxyserver; }
            set { _proxyserver = value; }
        }
        private String _proxyserver = "";

        // Proxy Port
        private Int32 _proxyport = 8080;

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public Int32 ProxyPort
        {
            get { return _proxyport; }
            set { _proxyport = value; }
        }


        // Proxy Credentials
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [ValidateNotNullOrEmpty]
        [System.Management.Automation.CredentialAttribute()]
        public PSCredential ProxyCredential
        {
            get { return _proxycredential; }
            set { _proxycredential = value; }
        }
        private PSCredential _proxycredential;

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
            get { return _proxytype; }
            set { _proxytype = value; }
        }
        private string _proxytype = "HTTP";

        //SSH Key File
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        public String KeyFile
        {
            get { return _keyfile; }
            set { _keyfile = value; }
        }
        private String _keyfile = "";

        //Local Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String LocalFolder
        {
            get { return _localfolder; }
            set { _localfolder = value; }
        }
        private String _localfolder = "";

        //Remote Folder
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public String RemoteFolder
        {
            get { return _remotefolder; }
            set { _remotefolder = value; }
        }
        private String _remotefolder = "";

        // OperationTimeout Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int OperationTimeout
        {
            get { return _operationtimeout; }
            set { _operationtimeout = value; }
        }
        private int _operationtimeout = 15;

        // ConnectionTimeout Parameter
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set { _connectiontimeout = value; }
        }
        private int _connectiontimeout = 5;

        // Auto Accept key fingerprint
        private bool _acceptkey;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter AcceptKey
        {
            get { return _acceptkey; }
            set { _acceptkey = value; }
        }

        // Automatically error if key is not trusted.
        private bool _errorOnUntrusted = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter ErrorOnUntrusted
        {
            get { return _errorOnUntrusted; }
            set { _errorOnUntrusted = value; }
        }

        // Supress progress bar.
        private bool _noProgress = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter NoProgress
        {
            get { return _noProgress; }
            set { _noProgress = value; }
        }

        // Do not check server fingerprint.
        private bool _force = false;
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Key")]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "NoKey")]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }


        // Variable to hold the host/fingerprint information
        private List<TrustedKey> _sshHostKeys;

        protected override void BeginProcessing()
        {
            // Collect host/fingerprint information from the registry.
            base.BeginProcessing();
            TrustedKeyMng keymng = new TrustedKeyMng();
            _sshHostKeys = keymng.GetKeys();
        }

        

        protected override void ProcessRecord()
        {
            foreach (var computer in _computername)
            {
                ConnectionInfo connectInfo;
                if (_keyfile.Equals(""))
                {
                    WriteVerbose("Using SSH Username and Password authentication for connection.");
                    var kIconnectInfo = new KeyboardInteractiveAuthenticationMethod(_credential.UserName);
                    connectInfo = ConnectionInfoGenerator.GetCredConnectionInfo(computer,
                        _port,
                        _credential,
                        _proxyserver,
                        _proxytype,
                        _proxyport,
                        _proxycredential,
                        kIconnectInfo);

                    // Event Handler for interactive Authentication
                    kIconnectInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
                    {
                        foreach (var prompt in e.Prompts)
                        {
                            if (prompt.Request.Contains("Password"))
                                prompt.Response = _credential.GetNetworkCredential().Password;
                        }
                    };

                }
                else
                {
                    WriteVerbose("Using SSH Key authentication for connection.");
                    connectInfo = ConnectionInfoGenerator.GetKeyConnectionInfo(computer,
                        _port,
                        _keyfile,
                        _credential,
                        _proxyserver,
                        _proxytype,
                        _proxyport,
                        _proxycredential);
                }

                //Ceate instance of SSH Client with connection info
                var client = new ScpClient(connectInfo);


                // Handle host key
                if (_force)
                {
                    WriteWarning("Host key is not being verified since Force switch is used.");
                }
                else
                {
                    var computer1 = computer;
                    client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                    {

                        var sb = new StringBuilder();
                        foreach (var b in e.FingerPrint)
                        {
                            sb.AppendFormat("{0:x}:", b);
                        }
                        var fingerPrint = sb.ToString().Remove(sb.ToString().Length - 1);

                        if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                        {
                            Host.UI.WriteVerboseLine("Fingerprint for " + computer1 + ": " + fingerPrint);
                        }

                        List<TrustedKey> computerKeys = _sshHostKeys.FindAll(key => key.Host == computer1);
                        bool hostKeyFound = false;
                        e.CanTrust = false;

                        if (computerKeys.Count > 0)
                        {
                            if (computerKeys.Exists(key => key.Key == fingerPrint))
                            {
                                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                                {
                                    Host.UI.WriteVerboseLine("Fingerprint matched trusted key for host " + computer1);
                                }
                                e.CanTrust = true;
                                hostKeyFound = true;

                            }
                            else
                            {
                                if (e.CanTrust && MyInvocation.BoundParameters.ContainsKey("Verbose"))
                                    Host.UI.WriteVerboseLine("No trusted key match found for host " + computer1);
                                e.CanTrust = false;

                            }
                        }

                        if (!e.CanTrust)
                        {
                            if (_errorOnUntrusted)
                            {
                                e.CanTrust = false;
                            }
                            else
                            {
                                if (!_acceptkey)
                                {
                                    var choices = new Collection<ChoiceDescription>
                                    {
                                        new ChoiceDescription("Y"),
                                        new ChoiceDescription("N")
                                    };
                                    e.CanTrust = 0 == Host.UI.PromptForChoice("Server SSH Fingerprint", "Do you want to trust the fingerprint " + fingerPrint, choices, 1);
                                }
                                else
                                    e.CanTrust = true;
                                if (e.CanTrust && hostKeyFound == false)
                                {
                                    var keymng = new TrustedKeyMng();
                                    keymng.SetKey(computer1, fingerPrint);
                                }
                            }
                        }
                    };
                }
                try
                {
                    // Set the connection timeout
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_connectiontimeout);

                    // Connect to host using Connection info
                    client.Connect();
                }
                catch (Renci.SshNet.Common.SshConnectionException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (Renci.SshNet.Common.SshOperationTimeoutException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.OperationTimeout, client);
                    WriteError(erec);
                }
                catch (Renci.SshNet.Common.SshAuthenticationException e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.SecurityError, client);
                    WriteError(erec);
                }
                catch (Exception e)
                {
                    ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                    WriteError(erec);
                }
                if (client.IsConnected)
                {
                    client.BufferSize = 1024;

                    // Print progess of upload.

                    if (!_noProgress)
                    {
                        client.Uploading += delegate(object sender, ScpUploadEventArgs e)
                        {
                            var progressRecord = new ProgressRecord(1, "Uploading " + e.Filename, String.Format("{0} Bytes Uploaded of {1}", e.Uploaded, e.Size))
                            {
                                PercentComplete = Convert.ToInt32((e.Uploaded * 100) / e.Size)
                            };

                            Host.UI.WriteProgress(1, progressRecord);
                        };
                    }

                    // Resolve the path even if a relative one is given.
                    ProviderInfo provider;
                    var pathinfo = GetResolvedProviderPathFromPSPath(_localfolder, out provider);
                    var localfullPath = pathinfo[0];

                    //var localfullPath = Path.GetFullPath(_localfolder);
                    if (Directory.Exists(localfullPath))
                    {
                        try
                        {
                            WriteVerbose("Uploading " + _remotefolder);
                            var dirinfo = new DirectoryInfo(@localfullPath);
                            client.Upload(dirinfo, _remotefolder);
                        }
                        catch (Exception e)
                        {
                            ErrorRecord erec = new ErrorRecord(e, null, ErrorCategory.InvalidOperation, client);
                            WriteError(erec);
                        }

                    }
                    else
                    {
                        var ex = new DirectoryNotFoundException("Directory " + localfullPath + " was not found.");
                        WriteError(new ErrorRecord(ex,
                                                   "Directory " + localfullPath + " was not found.",
                                                   ErrorCategory.InvalidArgument,
                                                   localfullPath));
                    }
                    client.Disconnect();
                }
            }

        } // End process record

    } //end of the class for the Set-SCPFile
}
