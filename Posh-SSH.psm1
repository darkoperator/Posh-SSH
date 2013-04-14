##############################################################################################
# SSH Function

<#
.Synopsis
   Get current SSH Session that are available for interaction.
.DESCRIPTION
   Get current SSH Session that are available for interaction.

.EXAMPLE
    Get list of current sessions available.

    PS C:\> Get-SSHSession

    Index Host                                                                                Connected                                                                   
    ----- ----                                                                                ---------                                                                   
      0   192.168.1.163                                                                         True                                                                      
      1   192.168.1.154                                                                         False                                                                     
      2   192.168.1.155                                                                         True                                                                      
      3   192.168.1.234                                                                         True  

.PARAMETER Index
    Index number of Session to retrive.

.NOTES
    AUTHOR: Carlos Perez carlos_perez@darkoprator.com
.LINK
    http://sshnet.codeplex.com/
    http://www.darkoperator.com/
#>
function Get-SSHSession 
{
    [CmdletBinding()]
    param( 
        [Parameter(Mandatory=$false)]
        [Int32] $Index
    )

    Begin{}
    Process
    {
        if ($Index)
        {
            foreach($i in $Index)
            {
                foreach($session in $SshSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session
                    }
                }
            }
        }
        else
        {
            # Can not reference SShSessions directly so as to be able
            # to remove the sessions when Remove-SSHSession is used
            $return_sessions = @()
            foreach($s in $SshSessions){$return_sessions += $s}
            $return_sessions
        }
    }
    End{}
}


<#
.Synopsis
   Removes and Closes an existing SSH Session.
.DESCRIPTION
    Removes and Closes an existing SSH Session. The session can be a SSH Session object
    or they can be specified by Index.

.EXAMPLE
    Remove a SSH Session specified by Index

    PS C:\> Remove-SSHSession -Index 0
    True

.PARAMETER Index
    Index number of Session to close and remove.

.PARAMETER Session
    SSH Session to close and remove.

.NOTES
    AUTHOR: Carlos Perez carlos_perez@darkoprator.com
.LINK
    http://sshnet.codeplex.com/
    http://www.darkoperator.com/
#>
function Remove-SSHSession
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$false,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Name")]
        [SSH.SSHSession[]]$SSHSession
        )

        Begin{}
        Process
        {
            if ($Index.Count -gt 0)
            {
                $sessions2remove = @()
                 foreach($i in $Index)
                {
                    Write-Verbose $i
                    foreach($session in $Global:SshSessions)
                    {
                        if ($session.Index -eq $i)
                        {
                            $sessions2remove += $session
                        }
                    }
                }

                foreach($badsession in $sessions2remove)
                {
                     Write-Verbose "Removing session $($badsession.index)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $global:SshSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.index) Removed"
                }
            }

            if ($SSHSession.Count -gt 0)
            {
                $sessions2remove = @()
                 foreach($i in $SSHSession)
                {
                    foreach($ssh in $Global:SshSessions)
                    {
                        if ($ssh -eq $i)
                        {
                            $sessions2remove += $ssh
                        }
                    }
                }

                foreach($badsession in $sessions2remove)
                {
                     Write-Verbose "Removing session $($badsession.index)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $Global:SshSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.index) Removed"
                }
            }

        }
        End{}
    
}


<#
.Synopsis
   Executes a given command on a remote SSH host.
.DESCRIPTION
   Executes a given command on a remote SSH hosst given credentials to the host or using an existing
   SSH Session.

.EXAMPLE
    Executes the "uname -a" command against several sessions

    PS C:\> Invoke-SSHCommand -Command "uname -a" -Index 0,2,3


    Host       : 192.168.1.163
    Output     : Linux debian6 2.6.32-5-amd64 #1 SMP Sun Sep 23 10:07:46 UTC 2012 x86_64 GNU/Linux
             
    ExitStatus : 0

    Host       : 192.168.1.155
    Output     : Linux ole6 2.6.39-300.17.1.el6uek.x86_64 #1 SMP Fri Oct 19 11:29:17 PDT 2012 x86_64 x86_64 x86_64 GNU/Linux
             
    ExitStatus : 0

    Host       : 192.168.1.234
    Output     : Linux ubuntusrv 3.2.0-29-generic #46-Ubuntu SMP Fri Jul 27 17:03:23 UTC 2012 x86_64 x86_64 x86_64 GNU/Linux
             
    ExitStatus : 0

.PARAMETER Command
    Command to execute in remote host.

.PARAMETER Index
    Index number of Session to execute command against.

.PARAMETER Session
    SSH Session to execute command against.

.NOTES
    AUTHOR: Carlos Perez carlos_perez@darkoprator.com
.LINK
    http://sshnet.codeplex.com/
    http://www.darkoperator.com/
#>
function Invoke-SSHCommand
{
    [CmdletBinding()]
    param(

        [Parameter(Mandatory=$true)]
        [string]$Command,
        
        [Parameter(Mandatory=$false,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Name")]
        [SSH.SSHSession[]]$SSHSession,

        [Parameter(Mandatory=$false,
        ParameterSetName = "Index")]
        [int32[]]$Index = $null

    )

    Begin{}
    Process
    {
        
        if ($SSHSession)
        {
            foreach($s in $SshSession)
            {
                if ($s.session.isconnected)
                {
                    $result = $S.session.RunCommand($Command)
                }
                else
                {
                    $s.session.connect()
                    $result = $s.session.RunCommand($Command)
                }
                if ($result)
                    {
                        $ResultObj = New-Object psobject -Property @{Output = $result.Result; ExitStatus = $result.ExitStatus; Host = $s.Host}
                        $ResultObj.pstypenames.insert(0,'Renci.SshNet.SshCommand')
                        $ResultObj
                    }
            }
        }
        elseif ($Index.Length -gt 0)
        {
            foreach($s in $SshSessions)
            {
                if($s.Index -in $Index)
                {
                    Write-Verbose "Running command against $($s.Index)"
                    if ($s.session.isconnected)
                    {
                        $result = $S.session.RunCommand($Command)
                    }
                    else
                    {
                        $s.session.connect()
                        $result = $s.session.RunCommand($Command)
                    }
                    if ($result)
                    {
                        $ResultObj = New-Object psobject -Property @{Output = $result.Result; ExitStatus = $result.ExitStatus; Host = $s.Host}
                        $ResultObj.pstypenames.insert(0,'Renci.SshNet.SshCommand')
                        $ResultObj
                    }
                }
            }
        }

    }
    End{}
}

##############################################################################################
# SSH Port Forwarding


function New-SSHPortForward
{
    [CmdletBinding()]
    param(
    [Parameter(Mandatory=$true)]
    [String]$LocalAdress = 'localhost',

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
        $SSHFWP = New-Object Renci.SshNet.ForwardedPortLocal($LocalAdress, $LocalPort, $RemoteAddress, $RemotePort)
    }
    Process
    {
        if ($Index -ne $null)
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
        elseif ($SSHSession)
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


function New-SSHDynamicPortForward
{
    [CmdletBinding()]
    param(
    [Parameter(Mandatory=$true)]
    [String]$LocalAdress = 'localhost',

    [Parameter(Mandatory=$true)]
    [Int32]$LocalPort,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
    [Alias("Session")]
    [SSH.SSHSession]$SSHSession,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Index",
        ValueFromPipeline=$true)]
    [Int32]$Index
    )

     Begin
    {
        # Initialize the ForwardPort Object
        $SSHFWP = New-Object Renci.SshNet.ForwardedPortDynamic($LocalAdress, $LocalPort)
    }
    Process
    {
        if ($Index -ne $null)
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
        elseif ($SSHSession)
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


function Get-SSHPortForward
{
    [CmdletBinding()]
    param(

    [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
    [Alias("Session")]
    [SSH.SSHSession]$SSHSession,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Index",
        ValueFromPipeline=$true)]
    [Int32]$Index
    )

     Begin
    {
    }
    Process
    {
        if ($Index -ne $null)
        {
            Write-Verbose "Finding session with Index $Index"
            foreach($session in $Global:SshSessions)
            {
                if ($session.index -eq $Index)
                {
                    Write-Verbose "Session with index $Index found."
                    $session.Session.ForwardedPorts
                }
            }
        }
        elseif ($SSHSession)
        {
            if ($SSHSession -in $Global:SshSessions)
            {
                $SSHSession.Session.ForwardedPorts
            }
            else
            {
                Write-Error "The Session does not appear in the list of created sessions."
            }
        }
    }
    End{}
}


function Stop-SSHPortForward
{
    [CmdletBinding()]
    param(

    [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
    [Alias("Session")]
    [SSH.SSHSession]$SSHSession,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Index",
        ValueFromPipeline=$true)]
    [Int32]$Index,

    [Parameter(Mandatory=$true)]
    [Int32]$BoundPort
    )

     Begin
    {
    }
    Process
    {
        if ($Index -ne $null)
        {
            Write-Verbose "Finding session with Index $Index"
            foreach($session in $Global:SshSessions)
            {
                Write-Verbose $session.index
                if ($session.index -eq $Index)
                {
                    $ports = $session.Session.ForwardedPorts
                    foreach($p in $ports)
                    {
                        if ($p.BoundPort -eq $BoundPort)
                        {
                            $p.Stop()
                            $p
                        }
                    }
                }
            }
        }
        elseif ($SSHSession)
        {
            if ($SSHSession -in $Global:SshSessions)
            {
                $ports = $SSHSession.Session.ForwardedPorts
                foreach($p in $ports)
                {
                    if ($p.BoundPort -eq $BoundPort)
                    {
                        $p.Stop()
                        $p
                    }
                }
            }
            else
            {
                Write-Error "The Session does not appear in the list of created sessions."
            }
        }
    }
    End{}
}


function Start-SSHPortForward
{
    [CmdletBinding()]
    param(

    [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
    [Alias("Session")]
    [SSH.SSHSession]$SSHSession,

    [Parameter(Mandatory=$true,
        ParameterSetName = "Index",
        ValueFromPipeline=$true)]
    [Int32]$Index,

    [Parameter(Mandatory=$true)]
    [Int32]$BoundPort
    )

     Begin
    {
    }
    Process
    {
        if ($Index -ne $null)
        {
            Write-Verbose "Finding session with Index $Index"
            foreach($session in $Global:SshSessions)
            {
                Write-Verbose $session.index
                if ($session.index -eq $Index)
                {
                    $ports = $session.Session.ForwardedPorts
                    foreach($p in $ports)
                    {
                        if ($p.BoundPort -eq $BoundPort)
                        {
                            $p.Start()
                            $p
                        }
                    }
                }
            }
        }
        elseif ($SSHSession)
        {
            if ($SSHSession -in $Global:SshSessions)
            {
                $ports = $SSHSession.Session.ForwardedPorts
                foreach($p in $ports)
                {
                    if ($p.BoundPort -eq $BoundPort)
                    {
                        $p.Start()
                        $p
                    }
                }
            }
            else
            {
                Write-Error "The Session does not appear in the list of created sessions."
            }
        }
    }
    End{}
}

########################################################################################
# SFTP Functions


<#
.Synopsis
   Get current SFTP Sessions that are available for interaction.
.DESCRIPTION
   Get current SFTP Sessions that are available for interaction.

.EXAMPLE
    Get list of current sessions available.

    PS C:\> Get-SFTPSession

    Index Host                                                                                Connected                                                                   
    ----- ----                                                                                ---------                                                                   
      0   192.168.1.155                                                                         True  

.PARAMETER Index
    Index number of Session to retrive.

.NOTES
    AUTHOR: Carlos Perez carlos_perez@darkoprator.com
.LINK
    http://sshnet.codeplex.com/
    http://www.darkoperator.com/
#>

function Get-SFTPSession 
{
    [CmdletBinding()]
    param( 
        [Parameter(Mandatory=$false)]
        [Int32[]] $Index
    )

    Begin{}
    Process
    {
        if ($Index.Length -gt 0)
        {
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session
                    }
                }
            }
        }
        else
        {
            # Can not reference SFTPSessions directly so as to be able
            # to remove the sessions when Remove-Sftpession is used
            $return_sessions = @()
            foreach($s in $Global:SFTPSessions){$return_sessions += $s}
            $return_sessions
        }
    }
    End{}
}


<#
.Synopsis
   Close and Remove a SFTP Session
.DESCRIPTION
   Close and Remove a SFTP Session specified by Index or SFTP Session Object.
.EXAMPLE
   Close a SFTP Session

    PS C:\> Remove-SFTPSession -Index 0 -Verbose
    VERBOSE: 0
    VERBOSE: Removing session 0
    True
    VERBOSE: Session 0 Removed
#>

function Remove-SFTPSession
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$false,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession
        )

        Begin{}
        Process
        {
            if ($Index.Count -gt 0)
            {
                $sessions2remove = @()
                 foreach($i in $Index)
                {
                    Write-Verbose $i
                    foreach($session in $Global:SFTPSessions)
                    {
                        if ($session.Index -eq $i)
                        {
                            $sessions2remove += $session
                        }
                    }
                }

                foreach($badsession in $sessions2remove)
                {
                     Write-Verbose "Removing session $($badsession.index)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $Global:SFTPSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.index) Removed"
                }
            }

            if ($SFTPSession.Count -gt 0)
            {
                $sessions2remove = @()
                 foreach($i in $SFTPSession)
                {
                    foreach($ssh in $global:SFTPSessions)
                    {
                        if ($ssh -eq $i)
                        {
                            $sessions2remove += $ssh
                        }
                    }
                }

                foreach($badsession in $sessions2remove)
                {
                     Write-Verbose "Removing session $($badsession.index)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $Global:SFTPSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.index) Removed"
                }
            }

        }
        End{}
    
}


<#
.Synopsis
   Get a List of Files for SFTP Session.
.DESCRIPTION
   Get a collection of objection representing files on a given path on a SFTP Session.
.EXAMPLE
   List files in the /tmp path on a remote SFTP Session

   C:\Users\Carlos> Get-SFTPDirectoryList -Index 0 -Path "/tmp"


    FullName       : /tmp/vmware-root
    LastAccessTime : 4/13/2013 5:44:56 PM
    LastWriteTime  : 4/13/2013 5:44:56 PM
    Length         : 4096
    UserId         : 0

    FullName       : /tmp/..
    LastAccessTime : 4/13/2013 5:31:46 PM
    LastWriteTime  : 4/13/2013 7:11:05 PM
    Length         : 4096
    UserId         : 0

    FullName       : /tmp/vmware-config1
    LastAccessTime : 4/13/2013 5:44:23 PM
    LastWriteTime  : 4/13/2013 5:44:23 PM
    Length         : 4096
    UserId         : 0

    FullName       : /tmp/.
    LastAccessTime : 4/13/2013 8:11:43 PM
    LastWriteTime  : 4/13/2013 8:11:41 PM
    Length         : 4096
    UserId         : 0

    FullName       : /tmp/yum.conf.security
    LastAccessTime : 4/13/2013 6:19:22 PM
    LastWriteTime  : 4/13/2013 6:19:21 PM
    Length         : 865
    UserId         : 0

    FullName       : /tmp/.ICE-unix
    LastAccessTime : 4/13/2013 5:27:15 PM
    LastWriteTime  : 4/13/2013 5:27:15 PM
    Length         : 4096
    UserId         : 0

.PARAMETER Index
    Index number of Session to interact with.

.PARAMETER Path
    Remote path to list.

.NOTES
    AUTHOR: Carlos Perez carlos_perez@darkoprator.com
.LINK
    http://sshnet.codeplex.com/
    http://www.darkoperator.com/
#>

function Get-SFTPDirectoryList
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        [Parameter(Mandatory=$true)]
        [string]$Path

     )

     Begin{}
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session.Session.ListDirectory($Path)
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $Global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $ssh.Session.ListDirectory($Path)
                    }
                }
             }       
         }
     }
     End{}
}


<#
.Synopsis
   Create Directory on Remote Server via SFTP
.DESCRIPTION
   Create Directory on Remote Server via SFTP specified by Index or SFTP Session Object.
.EXAMPLE
   Create a folder in the /tmp directory on servia via a SFTP Session

   PS C:\> New-SFTPDirectory -Index 0 -Path "/tmp/temporaryfolder"
#>

function New-SFTPDirectory
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        [Parameter(Mandatory=$true)]
        [string]$Path

     )

     Begin{}
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $Global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session.Session.CreateDirectory($Path)
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $Global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $ssh.Session.CreateDirectory($Path)
                    }
                }
             }       
         }
     }
     End{}
}


<#
.Synopsis
   Remove Directory on Remote Server via SFTP
.DESCRIPTION
   Remove Directory on Remote Server via SFTP specified by Index or SFTP Session Object.
.EXAMPLE
   Remove a folder in the /tmp directory on servia via a SFTP Session

   PS C:\> Remove-SFTPDirectory -Index 0 -Path "/tmp/temporaryfolder"
#>

function Remove-SFTPDirectory
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        [Parameter(Mandatory=$true)]
        [string]$Path

     )

     Begin{}
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session.Session.DeleteDirectory($Path)
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $ssh.Session.DeleteDirectory($Path)
                    }
                }
             }       
         }
     }
     End{}
}


<#
.Synopsis
   Change the current folder location of a SFTP Session
.DESCRIPTION
   Change the current folder location of a SFTP Session specified by Index or SFTP Session Object.
.EXAMPLE
   Change a SFTP Session current folder

   PS C:\> Get-SFTPCurrentDirectory -Index 0
    /root

    PS C:\> Set-SFTPDirectoryPath -Index 0 -Path "/tmp"

    PS C:\> Get-SFTPCurrentDirectory -Index 0
    /tmp
#>

function Set-SFTPDirectoryPath
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        [Parameter(Mandatory=$true)]
        [string]$Path

     )

     Begin{}
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session.Session.ChangeDirectory($Path)
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $ssh.Session.ChangeDirectory($Path)
                    }
                }
             }       
         }
     }
     End{}
}


<#
.Synopsis
   Get the current location of a SFTP Session
.DESCRIPTION
   Get the current location of a SFTP Session specified by Index or SFTP Session Object.
.EXAMPLE
   Get current folder location of a SFTP Session

   PS C:\> Get-SFTPCurrentDirectory -Index 0
    /root
#>

function Get-SFTPCurrentDirectory
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession
     )

     Begin{}
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $session.Session.WorkingDirectory
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $ssh.Session.WorkingDirectory
                    }
                }
             }       
         }
     }
    End{}
}


<#
.Synopsis
   Download remote file from a SFTP Session
.DESCRIPTION
   Download remote file from a SFTP Session specified by Index or SFTP Session Object
.EXAMPLE
   Download the anaconda configuration file of a remote Red Hat Session via a SFTP Session

   PS C:\> Get-SFTPFile -Index 0 -RemoteFile "/root/anaconda-ks.cfg" -LocalPath $env:homepath\Desktop

    PS C:\> ls $env:homepath\Desktop -Filter *.cfg


        Directory: C:\Users\Carlos\Desktop


    Mode                LastWriteTime     Length Name                                                                                                           
    ----                -------------     ------ ----                                                                                                           
    -a---         4/13/2013   8:40 PM       1337 anaconda-ks.cfg  
#>

function Get-SFTPFile
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        # Full path of file on remote system.
        [Parameter(Mandatory=$true)]
        [string]$RemoteFile,

        # Directory Path to save the file.
        [Parameter(Mandatory=$true)]
        [ValidateScript({Test-Path $_})]
        [string]$LocalPath,

        # Append to start of filename
        [Parameter(Mandatory=$false)]
        [string]$FileNameAppend
     )

     Begin
     {
        
     }
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $FileName = Split-Path $RemoteFile -Leaf
                        $LocalFile = "$((Resolve-Path $LocalPath).Path)\$filename"
                        if ($FileNameAppend)
                        {
                            $FileName = "$($FileNameAppend.Trim())$FileName"
                        }
                        $LocalFileStream = [System.IO.File]::Create($LocalFile)
                        $session.Session.DownloadFile($RemoteFile, $LocalFileStream)
                        $LocalFileStream.Flush()
                        $LocalFileStream.Close()
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $FileName = Split-Path $RemoteFile -Leaf
                        $LocalFile = "$((Resolve-Path $LocalPath).Path)\$filename"
                        if ($FileNameAppend)
                        {
                            $FileName = "$($FileNameAppend.Trim())$FileName"
                        }
                        $LocalFileStream = [System.IO.File]::Create($LocalFile)
                        $ssh.Session.DownloadFile($RemoteFile, $LocalFileStream)
                        $LocalFileStream.Flush()
                        $LocalFileStream.Close()
                    }
                }
             }       
         }
     }
    End{}
}


<#
.Synopsis
   Upload local file using a SFTP Session to remote system 
.DESCRIPTION
   Upload local file using a SFTP Session to remote system given the local file
   and remote path to place the file. The session can be specified by session index or
   a SFTP Session object.
.EXAMPLE
    Upload local file in to remote system /tmp directory

    PS C:\> Set-SFTPFile -Index 0 -RemotePath "/tmp" -LocalFile $env:homepath\Desktop\anaconda-ks.cfg  -Verbose
    VERBOSE: Uploading \Users\Carlos\Desktop\anaconda-ks.cfg as /tmp/anaconda-ks.cfg
    VERBOSE: Successfully Uploaded file to /tmp/anaconda-ks.cfg
#>

function Set-SFTPFile
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        # Full path of where to upload file on remote system.
        [Parameter(Mandatory=$true)]
        [string]$RemotePath,

        # Local File to upload to remote host.
        [Parameter(Mandatory=$true)]
        [ValidateScript({Test-Path $_})]
        [string]$LocalFile
     )

     Begin
     {
        
     }
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        $LocalFileName = Split-Path $LocalFile -Leaf
                        $RemoteFile = "$RemotePath/$LocalFileName"
                        Write-Verbose "Uploading $LocalFile as $RemoteFile"
                        $LocalFileStream = [System.IO.File]::OpenRead((resolve-path $LocalFile).path)
                        $session.Session.UploadFile($LocalFileStream, $RemoteFile)
                        $LocalFileStream.Close()
                        Write-Verbose "Successfully Uploaded file to $RemoteFile"
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        $LocalFileName = Split-Path $LocalFile -Leaf
                        $RemoteFile = "$RemotePath/$LocalFileName"
                        Write-Verbose "Uploading $LocalFile as $RemoteFile"
                        $LocalFileStream = [System.IO.File]::OpenRead($LocalFile)
                        $ssh.Session.UploadFile( $LocalFileStream, $RemoteFile)
                        $LocalFileStream.Close()
                        Write-Verbose "Successfully Uploaded file to $RemoteFile"
                    }
                }
             }       
         }
     }
    End{}
}


<#
.Synopsis
   Deletes a file on a remote system via a SFTP Session
.DESCRIPTION
   Deletes a file on a remote system via a SFTP Session specified by index or SFTP Session object.
.EXAMPLE
   Deleting file on /tmp directory.
    
    PS C:\> Remove-SFTPFile -Index 0 -RemoteFile "/tmp/anaconda-ks.cfg" -Verbose
    VERBOSE: Deleting /tmp/anaconda-ks.cfg
    VERBOSE: Deleted /tmp/anaconda-ks.cfg
#>

function Remove-SFTPFile
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        # Full path of where to upload file on remote system.
        [Parameter(Mandatory=$true)]
        [string]$RemoteFile
     )

     Begin
     {
        
     }
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        Write-Verbose "Deleting $RemoteFile"
                        $session.Session.DeleteFile($RemoteFile)
                        Write-Verbose "Deleted $RemoteFile"
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        Write-Verbose "Deleting $RemoteFile"
                        $ssh.Session.DeleteFile($RemoteFile)
                        Write-Verbose "Deleted $RemoteFile"
                    }
                }
             }       
         }
     }
    End{}
}


<#
.Synopsis
   Move or Rename remote file via a SFTP Session
.DESCRIPTION
   Move or Rename remote file via a SFTP Session  specified by index or SFTP Session object.
.EXAMPLE
   Rename file by moving it

    PS C:\> Move-SFTPFile -Index 0 -OriginalPath /tmp/anaconda-ks.cfg -NewPath /tmp/anaconda-ks.cfg.old -Verbose
    VERBOSE: Renaming /tmp/anaconda-ks.cfg to /tmp/anaconda-ks.cfg.old
    VERBOSE: File renamed
#>

function Move-SFTPFile
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
        ParameterSetName = "byname",
        ValueFromPipelineByPropertyName=$true)]
        [Int32[]] $Index,

        [Parameter(Mandatory=$true,
        ParameterSetName = "Session",
        ValueFromPipeline=$true)]
        [Alias("Session")]
        [SSH.SFTPSession[]]$SFTPSession,

        [Parameter(Mandatory=$true)]
        [string]$OriginalPath,

        [Parameter(Mandatory=$true)]
        [string]$NewPath
     )

     Begin
     {
        
     }
     Process
     {
        if ($Index.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $Index)
            {
                foreach($session in $global:SFTPSessions)
                {
                    if ($session.Index -eq $i)
                    {
                        Write-Verbose "Renaming $OriginalPath to $NewPath"
                        $session.Session.RenameFile($OriginalPath, $NewPath)
                        Write-Verbose "File renamed"
                    }
                 }
            }
        }

        if ($SFTPSession.Count -gt 0)
        {
            $sessions2remove = @()
            foreach($i in $SFTPSession)
            {
                foreach($ssh in $global:SFTPSessions)
                {
                    if ($ssh -eq $i)
                    {
                        Write-Verbose "Renaming $OriginalPath to $NewPath"
                        $ssh.Session.RenameFile($OriginalPath, $NewPath)
                        Write-Verbose "File renamed"
                    }
                }
             }       
         }
     }
    End{}
}

$global:SshSessions = New-Object System.Collections.ArrayList
$global:SFTPSessions = New-Object System.Collections.ArrayList
