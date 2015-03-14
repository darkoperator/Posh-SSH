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


# .ExternalHelp Posh-SSH.psm1-Help.xml
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


# .ExternalHelp Posh-SSH.psm1-Help.xml
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


# .ExternalHelp Posh-SSH.psm1-Help.xml
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




# .ExternalHelp Posh-SSH.psm1-Help.xml
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


