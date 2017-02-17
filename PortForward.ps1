##############################################################################################
# SSH Port Forwarding


<#
.Synopsis
   Redirects traffic from a local port to a remote host and port via a SSH Session.
.DESCRIPTION
   Redirects TCP traffic from a local port to a remote host and port via a SSH Session.
.EXAMPLE
   Forward traffic from 0.0.0.0:8081 to 10.10.10.1:80 thru a SSH Session

    PS C:\> New-SSHLocalPortForward -Index 0 -LocalAdress 0.0.0.0 -LocalPort 8081 -RemoteAddress 10.10.10.1 -RemotePort 80 -Verbose
    VERBOSE: Finding session with Index 0
    VERBOSE: 0
    VERBOSE: Adding Forward Port Configuration to session 0
    VERBOSE: Starting the Port Forward.
    VERBOSE: Forwarding has been started.

    PS C:\> Invoke-WebRequest -Uri http://localhost:8081


    StatusCode        : 200
    StatusDescription : OK
    Content           : 
                        <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
                                "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
                        <html>
                            <head>
                                <script type="text/javascript" src="/javascript/scri...
    RawContent        : HTTP/1.1 200 OK
                        Expires: Tue, 16 Apr 2013 03:43:18 GMT,Thu, 19 Nov 1981 08:52:00 GMT
                        Cache-Control: max-age=180000,no-store, no-cache, must-revalidate, post-check=0, pre-check=0
                        Set-Cookie: PHPSESS...
    Forms             : {iform}
    Headers           : {[Expires, Tue, 16 Apr 2013 03:43:18 GMT,Thu, 19 Nov 1981 08:52:00 GMT], [Cache-Control, max-age=180000,no-store, no-cache, 
                        must-revalidate, post-check=0, pre-check=0], [Set-Cookie, PHPSESSID=d53d3dc62ffac241112bcfd16af36bb8; path=/], [Pragma, no-cache]...}
    Images            : {}
    InputFields       : {@{innerHTML=; innerText=; outerHTML=<INPUT onchange=clearError(); onclick=clearError(); tabIndex=1 id=usernamefld class="formfld user" 
                        name=usernamefld>; outerText=; tagName=INPUT; onchange=clearError();; onclick=clearError();; tabIndex=1; id=usernamefld; class=formfld 
                        user; name=usernamefld}, @{innerHTML=; innerText=; outerHTML=<INPUT onchange=clearError(); onclick=clearError(); tabIndex=2 
                        id=passwordfld class="formfld pwd" type=password value="" name=passwordfld>; outerText=; tagName=INPUT; onchange=clearError();; 
                        onclick=clearError();; tabIndex=2; id=passwordfld; class=formfld pwd; type=password; value=; name=passwordfld}, @{innerHTML=; 
                        innerText=; outerHTML=<INPUT tabIndex=3 class=formbtn type=submit value=Login name=login>; outerText=; tagName=INPUT; tabIndex=3; 
                        class=formbtn; type=submit; value=Login; name=login}}
    Links             : {}
    ParsedHtml        : mshtml.HTMLDocumentClass
    RawContentLength  : 5932
#>

function New-SSHLocalPortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(
        [Parameter(Mandatory=$true,
            Position=1)]
        [String]
        $BoundHost,

        [Parameter(Mandatory=$true,
            Position=2)]
        [Int32]
        $BoundPort,

        [Parameter(Mandatory=$true,
            Position=3)]
        [String]
        $RemoteAddress,

        [Parameter(Mandatory=$true,
            Position=4)]
        [Int32]
        $RemotePort,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Session",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias("Session")]
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Index",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId 
    )

    Begin
    {
        $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                $sess = Get-SSHSession -Index $SessionId
                if ($sess)
                {
                    $ToProcess = $sess
                }
                else
                {
                    Write-Error -Message "Session specified with Index $($SessionId) was not found"
                    return
                }
            }
        }
        
    }
    Process
    {
        $ports = $ToProcess.Session.ForwardedPorts
        foreach($p in $ports)
        {
            if (($p.BoundPort -eq $BoundPort) -and ($p.BoundHost -eq $BoundHost))
            {
                Write-Error -Message "A forward port already exists for port $($BoundPort) with address $($LocalAdress)"
                return
            }
        }
        # Initialize the ForwardPort Object
        $SSHFWP = New-Object Renci.SshNet.ForwardedPortLocal($BoundHost, $BoundPort, $RemoteAddress, $RemotePort)
        
        # Add the forward port object to the session
        Write-Verbose -message "Adding Forward Port Configuration to session $($ToProcess.Index)"
        $ToProcess.session.AddForwardedPort($SSHFWP)
        Write-Verbose -message "Starting the Port Forward."
        $SSHFWP.start()
        Write-Verbose -message "Forwarding has been started."
    
    }
    End{}


}


function New-SSHRemotePortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(
    [Parameter(Mandatory=$true)]
    [String]$LocalAdress = '127.0.0.1',

    [Parameter(Mandatory=$true)]
    [Int32]$LocalPort,

    [Parameter(Mandatory=$true)]
    [String]$RemoteAddress,

    [Parameter(Mandatory=$true)]
    [Int32]$RemotePort,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
    [Alias("Session")]
    [SSH.SSHSession]$SSHSession,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Index",
        ValueFromPipeline=$true)]
    [Int32]$Index = $null
    )

    Begin
    {
        # Initialize the ForwardPort Object
        $SSHFWP = New-Object Renci.SshNet.ForwardedPortRemote($LocalAdress, $LocalPort, $RemoteAddress, $RemotePort)
    }
    Process
    {
        if ($PSCmdlet.ParameterSetName -eq 'Index')
        {
            Write-Verbose "Finding session with Index $Index"
            foreach($session in $Global:SshSessions)
            {
                Write-Verbose $session.index
                if ($session.index -eq $Index)
                {
                    # Add the forward port object to the session
                    Write-Verbose "Adding Forward Port Configuration to session $Index"
                    $session.session.AddForwardedPort($SSHFWP)
                    Write-Verbose "Starting the Port Forward."
                    $SSHFWP.start()
                    Write-Verbose "Forwarding has been started."
                }
            }
        }
        elseif ($PSCmdlet.ParameterSetName -eq 'Session')
        {
            if ($SSHSession -in $Global:SshSessions)
            {
                # Add the forward port object to the session
                Write-Verbose "Adding Forward Port Configuration to session $($SSHSession.index)"
                $SSHSession.session.AddForwardedPort($SSHFWP)
                Write-Verbose "Starting the Port Forward."
                $SSHFWP.start()
                Write-Verbose "Forwarding has been started."
            }
            else
            {
                Write-Error "The Session does not appear in the list of created sessions."
            }
        }
    
    }
    End{}
}

<#
.Synopsis
   Establishes a Dynamic Port Forward thru a stablished SSH Session.
.DESCRIPTION
   Dynamic port forwarding is a transparent mechanism available for applications, which 
   support the SOCKS4 or SOCKS5 client protoco. In windows for best results the local address
   to bind to should be the IP of the network interface. 
.EXAMPLE
    New-SSHDynamicPortForward -LocalAdress 192.168.28.131 -LocalPort 8081 -Index 0 -Verbose
    VERBOSE: Finding session with Index 0
    VERBOSE: 0
    VERBOSE: Adding Forward Port Configuration to session 0
    VERBOSE: Starting the Port Forward.
    VERBOSE: Forwarding has been started.
#>

function New-SSHDynamicPortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(
        [Parameter(Mandatory=$true,
            Position=1)]
        [String]
        $BoundHost = 'localhost',

        [Parameter(Mandatory=$true,
            Position=2)]
        [Int32]
        $BoundPort,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Session",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias("Session")]
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Index",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId
    )

     Begin
    {
        $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                $sess = Get-SSHSession -Index $SessionId
                if ($sess)
                {
                    $ToProcess = $sess
                }
                else
                {
                    Write-Error -Message "Session specified with Index $($SessionId) was not found"
                    return
                }
            }
        }  
    }
    Process
    {
        $ports = $ToProcess.Session.ForwardedPorts
        foreach($p in $ports)
        {
            if ($p.BoundHost -eq $BoundHost -and $p.BoundPort -eq $BoundPort)
            {
                throw "A forward port already exists for port $($BoundPort) with address $($BoundHost)"
            }
        }

         # Initialize the ForwardPort Object
        $SSHFWP = New-Object Renci.SshNet.ForwardedPortDynamic($BoundHost, $BoundPort)

        # Add the forward port object to the session
        Write-Verbose -message "Adding Forward Port Configuration to session $($ToProcess.Index)"
        $ToProcess.session.AddForwardedPort($SSHFWP)
        $ToProcess.session.KeepAliveInterval = New-TimeSpan -Seconds 30
        $ToProcess.session.ConnectionInfo.Timeout = New-TimeSpan -Seconds 20
        $ToProcess.session.SendKeepAlive()
        
        [System.Threading.Thread]::Sleep(500)
        Write-Verbose -message "Starting the Port Forward."
        $SSHFWP.start()
        Write-Verbose -message "Forwarding has been started."
               
    }
    End{}
}


<#
.Synopsis
   Get a list of forwarded TCP Ports for a SSH Session
.DESCRIPTION
   Get a list of forwarded TCP Ports for a SSH Session
.EXAMPLE
   Get list of configured forwarded ports

    PS C:\> Get-SSHPortForward -Index 0


    BoundHost : 0.0.0.0
    BoundPort : 8081
    Host      : 10.10.10.1
    Port      : 80
    IsStarted : True
#>

function Get-SSHPortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(
        [Parameter(Mandatory=$true,
            ParameterSetName = "Session",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias("Session")]
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Index",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId
    )

     Begin
    {
        $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                $sess = Get-SSHSession -Index $SessionId
                if ($sess)
                {
                    $ToProcess = $sess
                }
                else
                {
                    Write-Error -Message "Session specified with Index $($SessionId) was not found"
                    return
                }
            }
        }  
    }
    Process
    {
        
        $ToProcess.Session.ForwardedPorts
              
    }
    End{}
}


<#
.Synopsis
   Stops a configured port forward configured for a SSH Session
.DESCRIPTION
   Stops a configured port forward configured for  a SSH Session given the session and port number
.EXAMPLE
   Stop a currently working port forward thru a SSH Session

   C:\Users\Carlos> Get-SSHPortForward -Index 0


    BoundHost : 192.168.1.158
    BoundPort : 8081
    Host      : 10.10.10.1
    Port      : 80
    IsStarted : True



    C:\Users\Carlos> Stop-SSHPortForward -Index 0 -BoundPort 8081


    BoundHost : 192.168.1.158
    BoundPort : 8081
    Host      : 10.10.10.1
    Port      : 80
    IsStarted : False
#>

function Stop-SSHPortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(

        [Parameter(Mandatory=$true,
            ParameterSetName = "Session",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias("Session")]
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Index",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId,

        [Parameter(Mandatory=$true,
            Position=2)]
        [Int32]
        $BoundPort,

        [Parameter(Mandatory=$true,
            Position=1)]
        [string]
        $BoundHost
    )

     Begin
    {
         $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                $sess = Get-SSHSession -Index $SessionId
                if ($sess)
                {
                    $ToProcess = $sess
                }
                else
                {
                    Write-Error -Message "Session specified with Index $($SessionId) was not found"
                    return
                }
            }
        } 
    }
    Process
    {
        $ports = $ToProcess.Session.ForwardedPorts
        foreach($p in $ports)
        {
            if ($p.BoundPort -eq $BoundPort -and $p.BoundHost -eq $BoundHost)
            {
                $p.Stop()
                $p
            }
        }
    }
    End{}
}#>


<#
.Synopsis
   Start a configured port forward configured for a SSH Session
.DESCRIPTION
   Stops a configured port forward configured for a SSH Session given the session and port number
.EXAMPLE
   Stop a currently working port forward thru a SSH Session

   C:\Users\Carlos> Get-SSHPortForward -Index 0


    BoundHost : 192.168.1.158
    BoundPort : 8081
    Host      : 10.10.10.1
    Port      : 80
    IsStarted : False



    C:\Users\Carlos> Start-SSHPortForward -Index 0 -BoundPort 8081


    BoundHost : 192.168.1.158
    BoundPort : 8081
    Host      : 10.10.10.1
    Port      : 80
    IsStarted : True
#>
function Start-SSHPortForward
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    param(

        [Parameter(Mandatory=$true,
            ParameterSetName = "Session",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias("Session")]
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
            ParameterSetName = "Index",
            ValueFromPipeline=$true,
            Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId,

        [Parameter(Mandatory=$true,
            Position=2)]
        [Int32]
        $BoundPort,

        [Parameter(Mandatory=$true,
            Position=1)]
        [string]
        $BoundHost
    )

     Begin
    {
         $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                $sess = Get-SSHSession -Index $SessionId
                if ($sess)
                {
                    $ToProcess = $sess
                }
                else
                {
                    Write-Error -Message "Session specified with Index $($SessionId) was not found"
                    return
                }
            }
        } 
    }
    Process
    {

        $ports = $ToProcess.Session.ForwardedPorts
        foreach($p in $ports)
        {
            if ($p.BoundPort -eq $BoundPort -and $p.BoundHost -eq $BoundHost)
            {
                $p.Start()
                $p
            }
        }
               
    }
    End{}
}