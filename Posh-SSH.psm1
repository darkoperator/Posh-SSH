# Set up of Session variables.
##############################################################################################
if (!(Test-Path variable:Global:SshSessions ))
{
    $global:SshSessions = New-Object System.Collections.ArrayList
}

if (!(Test-Path variable:Global:SFTPSessions ))
{
    $global:SFTPSessions = New-Object System.Collections.ArrayList
}

# Dot Sourcing of functions
##############################################################################################
# Library has to many bugs on forwarding still
#. "$PSScriptRoot\PortForward.ps1"
. "$PSScriptRoot\Trust.ps1"
. "$PSScriptRoot\Sftp.ps1"


# SSH Functions
##############################################################################################


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
        [Parameter(Mandatory=$false,
                   Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId
    )

    Begin{}
    Process
    {
        if ($SessionId)
        {
            foreach($i in $SessionId)
            {
                foreach($session in $SshSessions)
                {
                    if ($session.SessionId -eq $i)
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

    PS C:\> Remove-SSHSession -SessionId 0
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$false,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Name')]
        [SSH.SSHSession[]]
        $SSHSession
        )

        Begin{}
        Process
        {
            if ($PSCmdlet.ParameterSetName -eq 'Index')
            {
                $sessions2remove = @()
                 foreach($i in $SessionId)
                {
                    foreach($session in $Global:SshSessions)
                    {
                        if ($session.SessionId -eq $i)
                        {
                            $sessions2remove += $session
                        }
                    }
                }

                foreach($badsession in $sessions2remove)
                {
                     Write-Verbose "Removing session $($badsession.SessionId)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $global:SshSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.SessionId) Removed"
                }
            }

            if ($PSCmdlet.ParameterSetName -eq 'Session')
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
                     Write-Verbose "Removing session $($badsession.SessionId)"
                     if ($badsession.session.IsConnected) 
                     { 
                        $badsession.session.Disconnect() 
                     }
                     $badsession.session.Dispose()
                     $Global:SshSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.SessionId) Removed"
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

    PS C:\> Invoke-SSHCommand -Command "uname -a" -SessionId 0,2,3


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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(

        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]$Command,
        
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Name')]
        [SSH.SSHSession[]]
        $SSHSession,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   Position=0)]
        [Alias('Index')]
        [int32[]]
        $SessionId = $null,

        # Ensures a connection is made by reconnecting before command.
        [Parameter(Mandatory=$false)]
        [switch]
        $EnsureConnection,

        [Parameter(Mandatory=$false,
                   Position=2)]
        [int]
        $TimeOut = 60

    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SSHSession
            }

            'Index'
            {
                foreach($session in $Global:SshSessions)
                {
                    if ($SessionId -contains $session.SessionId)
                    {
                        $ToProcess += $session
                    }
                }
            }
        }
    }
    Process
    {
        foreach($Connection in $ToProcess)
        {
            if ($Connection.session.isconnected)
                {
                    if ($EnsureConnection)
                    {
                        $Connection.session.connect()
                    }
                    $cmd = $Connection.session.CreateCommand($Command)
                }
                else
                {
                    $s.session.connect()
                    $cmd = $Connection.session.CreateCommand($Command)
                }

                $cmd.CommandTimeout = New-TimeSpan -Seconds $TimeOut

                # start asynchronious execution of the command.
                $Async = $cmd.BeginExecute()
                while($Async.IsCompleted)
                {
                    Write-Verbose -Message 'Waiting for command to finish execution'
                    try
                    {
                        Start-Sleep -Seconds 2
                    }
                    finally
                    {
                        $Output = $cmd.EndExecute($Async)
                    }
                }

                $Output = $cmd.EndExecute($Async)
                $ResultProps = @{}
                $ResultProps['Output'] = $Output
                $ResultProps['ExitStatus'] = $cmd.ExitStatus
                $ResultProps['Error'] = $cmd.Error
                $ResultProps['Host'] = $Connection.Host

                $ResultObj = New-Object psobject -Property $ResultProps
                $ResultObj.pstypenames.insert(0,'Renci.SshNet.SshCommand')
                $ResultObj


        }

    }
    End{}
}





 <#
 .Synopsis
    Gets the current installed version and the latest version of Posh-SSH.
 .DESCRIPTION
    Gets the current installed version and the latest version of Posh-SSH.
 .EXAMPLE
     Get-PoshSecModVersion

InstalledVersion                                                                                                     CurrentVersion                                                                                                      
----------------                                                                                                     --------------                                                                                                      
1.1                                                                                                                  1.1                                                                                                                 

 #>
 function Get-PoshSSHModVersion
 {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    Param()
    
    Begin
    {
       $currentversion = ''
       $installed = Get-Module -Name 'posh-SSH' -ListAvailable
    }
    Process
    {
       $webClient = New-Object System.Net.WebClient
       Try
       {
           $current = Invoke-Expression  $webClient.DownloadString('https://raw.github.com/darkoperator/Posh-SSH/master/Posh-SSH.psd1')
           $currentversion = $current.moduleversion
       }
       Catch
       {
           Write-Warning 'Could not retrieve the current version.'
       }
       $majorver,$minorver = $currentversion.split('.')
       if ($majorver -gt $installed.Version.Major)
       {
           Write-Warning 'You are running an outdated version of the module.'
       }
       elseif ($minorver -gt $installed.Version.Minor)
       {
           Write-Warning 'You are running an outdated version of the module.'
       } 
        
       $props = @{
           InstalledVersion = $installed.Version.ToString()
           CurrentVersion   = $currentversion
       }
       New-Object -TypeName psobject -Property $props
     }
     End{}
 }


