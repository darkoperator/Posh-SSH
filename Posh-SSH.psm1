##############################################################################################
# SSH Function

function Get-SSHSession 
{
    [CmdletBinding()]
    param( 
        [Parameter(Mandatory=$false)]
        [Int32[]] $Index
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


.EXAMPLE


.PARAMETER Command
    Command to execute in remote host.

.PARAMETER ComputerName
    Single computer or list of computer names or addresses to connect to run command against.

.PARAMETER Credential
    PowerShell Credential object for use in authentication. For password authentication the username and
    password combination will be used for authentication. When a Key file is used the given user is used
    to specify the user the key belong to and the password will be used as the passphrase for the key.

.PARAMETER Port
    Port number in use by the SSH service on the target system you are connecting to.

.PARAMETER ProxyServer
    Proxy server name or IP Adress to use for connection.

.PARAMETER ProxyPort
    Port to connect to on proxy server to route connection.

.PARAMETER ProxyCredential
    PowerShell Credential Object with the credetials for use to connect to proxy server if required.

.PARAMTER  ProxyType
    Type of Proxy being used (HTTP, Socks4 or Socks5).

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
            write "running as session"
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
                        $result
                        #New-Object psobject -Property @{Result = $result.Result; Error = $result; ExitStatus = $result.ExitStatus; Host = $s.Host}

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
                        $result
                        #New-Object psobject -Property @{Result = $result.Result; Error = $result; ExitStatus = $result.ExitStatus; Host = $s.Host}

                    }
                }
                else
                {
                    Write-Warning "Index specified was not found."
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

# Download File
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

# Upload File
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
