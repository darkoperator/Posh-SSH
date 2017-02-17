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
. "$PSScriptRoot\PortForward.ps1"


# SSH Functions
##############################################################################################


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SSHSession 
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    param( 
        [Parameter(Mandatory=$false,
                   ParameterSetName = 'Index',
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$false,
                   ParameterSetName = 'ComputerName',
                   Position=0)]
        [Alias('Server', 'HostName', 'Host')]        
        [string[]]
        $ComputerName,

        [Parameter(Mandatory=$false,
                   ParameterSetName = 'ComputerName',
                   Position=0)]        
        [switch]
        $ExactMatch
    )

    Begin{}
    Process
    {
        if ($PSCmdlet.ParameterSetName -eq 'Index')
        {
            if ( $PSBoundParameters.ContainsKey('SessionId') )
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
        else # ParameterSetName -eq 'ComputerName'
        {
            # Only check to see if it contains ComputerName.  If it get's it without having any values somehow, then don't return anything as they did something odd.
            if ( $PSBoundParameters.ContainsKey('ComputerName') )
            {
                foreach($s in $ComputerName)
                {
                    foreach($session in $SshSessions)
                    {
                        if ($session.Host -like $s -and ( -not $ExactMatch -or $session.Host -eq $s ) )
                        {
                            $session
                        }
                    }
                }
            }
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
        $TimeOut = 60,

        [Parameter(Mandatory=$false, Position=3)]
        [int]
        $ThrottleLimit = 32
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

        # array to keep track of all the running jobs.
        $Script:AsyncProcessing = @{}
        $JobId = 0
        function CheckAsyncProcessing
        {
            process
            {
                $RemoveJobs = @()
                $Script:AsyncProcessing.GetEnumerator() | 
                % { 
                    $JobKey = $_.Key
                    #Write-Verbose $JobKey -Verbose
                    $_.Value 
                } |
                % {
                    # Check if it completed or is past the timeout setting.
                    if ( $_.Async.IsCompleted -or $_.Duration.Elapsed.TotalSeconds -gt $TimeOut )
                    {
                        $Output = $_.cmd.EndExecute($_.Async)
                    
                        # Generate custom object to return to pipeline and client
                        [pscustomobject]@{
                            Output = $Output -replace '\n$' -split '\n'
                            ExitStatus = $_.cmd.ExitStatus
                            Error = $_.cmd.Error
                            Host = $_.Connection.Host
                            Duration = $_.Duration.Elapsed
                        } | 
                        % { 
                            $_.pstypenames.insert(0,'Renci.SshNet.SshCommand'); 

                            #Return object to pipeline
                            $_
                        }

                        # Set this object as having been processed.
                        $_.Processed = $true
                        $RemoveJobs += $JobKey                     
                    }
                }

                # Remove all the items that are done.                
                #[int[]]$Script:AsyncProcessing.Keys | 
                $RemoveJobs |
                % {
                    if ($Script:AsyncProcessing.$_.Processed)
                    {
                        $Script:AsyncProcessing.Remove( $_ )
                    }
                }
            }
        }
    }


    Process
    {
        foreach($Connection in $ToProcess)
        {
            if ($Connection.Session.IsConnected)
            {
                if ($EnsureConnection)
                {
                    try
                    {
                        $Connection.Session.Connect()
                    }
                    catch
                    {                            
                        if ( $_.Exception.InnerException.Message -ne 'The client is already connected.' )
                        { Write-Error -Exception $_.Exception }
                    }
                }                
            }
            else
            {
                try
                {
                    $Connection.Session.Connect()
                }
                catch
                {
                    Write-Error "Unable to connect session to $($Connection.Host) :: $_"
                }
                
            }

            if ($Connection.Session.IsConnected)
            {
                while ($Script:AsyncProcessing.Count -ge $ThrottleLimit )
                {
                    CheckAsyncProcessing
                    Write-Debug "Throttle reached: $ThrottleLimit"
                    Start-Sleep -Milliseconds 50
                }

                $cmd = $Connection.session.CreateCommand($Command)
                $cmd.CommandTimeout = [timespan]::FromMilliseconds(50) #New-TimeSpan -Seconds $TimeOut

                # start asynchronious execution of the command.
                $Duration = [System.Diagnostics.Stopwatch]::StartNew()
                $Async = $cmd.BeginExecute()
                
                $Script:AsyncProcessing.Add( $JobId, ( [pscustomobject]@{
                    cmd = $cmd
                    Async = $Async
                    Connection = $Connection
                    Duration = $Duration
                    Processed = $false
                }))

                $JobId++
            }

            CheckAsyncProcessing

        }
    }
    End
    {
        $Done = $false
        while ( -not $Done )
        {
           CheckAsyncProcessing

            if ( $Script:AsyncProcessing.Count -eq 0 )
            { $Done = $true }
            else { Start-Sleep -Milliseconds 50 }
        }
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
 function Get-PoshSSHModVersion
 {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    Param()
    
    Begin
    {
       $CurrentVersion = $null
       $installed = (Get-Module -Name 'posh-SSH').Version       
    }
    Process
    {
       $webClient = New-Object System.Net.WebClient
       Try
       {
           $current = Invoke-Expression  $webClient.DownloadString('https://raw.github.com/darkoperator/Posh-SSH/master/Posh-SSH.psd1')
           $CurrentVersion = [Version]$current.ModuleVersion
       }
       Catch
       {
           Write-Warning 'Could not retrieve the current version.'
       }

       if ( $installed -eq $null )
       {
           Write-Error 'Unable to locate Posh-SSH.'
       }
       elseif ( $CurrentVersion -gt $installed )
       {
           Write-Warning 'You are running an outdated version of the module.'
       }

       $props = @{
           InstalledVersion = $installed
           CurrentVersion   = $CurrentVersion
       }
       New-Object -TypeName psobject -Property $props
     }
     End{}
 }

 # .ExternalHelp Posh-SSH.psm1-Help.xml
function Invoke-SSHCommandStream
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
        [SSH.SSHSession]
        $SSHSession,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   Position=0)]
        [Alias('Index')]
        [int32]
        $SessionId,

        # Ensures a connection is made by reconnecting before command.
        [Parameter(Mandatory=$false)]
        [switch]
        $EnsureConnection,

        [Parameter(Mandatory=$false,
                   Position=2)]
        [int]
        $TimeOut = 60,

        [Parameter()]
        [string]
        $HostVariable,

        [Parameter()]
        [string]
        $ExitStatusVariable
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

                if ( $PSBoundParameters.ContainsKey('HostVariable') )
                { Set-Variable -Scope 1 -Name $HostVariable -Value $Connection.Host }


                # start asynchronious execution of the command.
                $Async = $cmd.BeginExecute()
                $Duration = [System.Diagnostics.Stopwatch]::StartNew()
                $Reader = New-Object System.IO.StreamReader -ArgumentList $cmd.OutputStream

                try
                {
                    Write-Verbose "IsCompleted Before: $($Async.IsCompleted)"
                    while(-not $Async.IsCompleted -and $Duration.Elapsed -lt $cmd.CommandTimeout )
                    {                   
                        Write-Verbose "IsCompleted During: $($Async.IsCompleted)"
                        #if ( $Reader.Peek() -gt -1)
                        while ( $Reader.Peek() -gt -1)
                        {
                            $Result = $Reader.ReadLine()                            
                            $Result -replace '\n\r$'
                        }
                        Start-Sleep -Milliseconds 5
                    }                    
                }
                catch
                {
                    Write-Error -Message "Error with Command: $_"                    
                }
                finally
                {
                    Write-Verbose "IsCompleted After: $($Async.IsCompleted)"
                    # Using finally clause to make sure the command is ended if Ctrl-C is used to cancel it.

                    while ( $Reader.Peek() -gt -1)
                    {
                        $Result = $Reader.ReadLine()                            
                        $Result -replace '\n\r$'
                    }
                    
                    if (-not $Async.IsCompleted -and $Duration.Elapsed -ge $cmd.CommandTimeout )
                    {
                        $cmd.EndExecute($Async)
                     
                    }
                    elseif ( -not $Async.IsCompleted )
                    {
                        $cmd.CancelAsync() 
                        Write-Warning "Canceled execution"
                        
                    }

                }

            if ( $PSBoundParameters.ContainsKey('ExitStatusVariable') )
            {
                Set-Variable -Scope 1 -Name $ExitStatusVariable -Value @{
                    Host = $Connection.Host
                    ExitStatus = $cmd.ExitStatus
                    Error = $cmd.Error
                }
            }
        }
    }
    End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SSHShellStream
{
    [CmdletBinding(DefaultParameterSetName="Index")]
    [OutputType([Renci.SshNet.ShellStream])]
    Param
    (
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

        # Name of the terminal.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [string]
        $TerminalName = "PoshSSHTerm",

        # The columns
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [int]
        $Columns=80,

        # The rows.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [int]
        $Rows=24,

        # The width.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [int]
        $Width= 800,

        # The height.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [int]
        $Height=600,

        # Size of the buffer.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [int]
        $BufferSize=1000
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
        $stream = $ToProcess.Session.CreateShellStream($TerminalName, $Columns, $Rows, $Width, $Height, $BufferSize)
        Add-Member -InputObject $stream -MemberType NoteProperty -Name SessionId -Value $ToProcess.SessionId
        Add-Member -InputObject $stream -MemberType NoteProperty -Name Session -Value $ToProcess.Session
        $stream
    }
    End
    {
    }
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Invoke-SSHStreamExpectAction
{
    [CmdletBinding(DefaultParameterSetName='String')]
    [OutputType([Bool])]
    Param
    (
        # SSH Shell Stream. 
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ValueFromPipeline=$true,
                   Position=0)]
        [Renci.SshNet.ShellStream]
        $ShellStream,

        # Initial command that will generate the output to be evaluated by the expect pattern.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [string]
        $Command,

        # String on what to trigger the action on.
        [Parameter(Mandatory=$true,
                   ParameterSetName='String',
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [string]
        $ExpectString,

        # Regular expression on what to trigger the action on.
        [Parameter(Mandatory=$true,
                   ParameterSetName='Regex',
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [regex]
        $ExpectRegex,

        # Command to execute once an expression is matched.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=3)]
        [string]
        $Action,

        # Number of seconds to wait for a match.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true,
                   Position=4)]
        [int]
        $TimeOut = 10
    )

    Begin
    {
    }
    Process
    {
        Write-Verbose -Message "Executing command $($Command)."
        $ShellStream.WriteLine($Command)
        Write-Verbose -Message "Waiting for match."
        switch ($PSCmdlet.ParameterSetName)
        {
            'string' {$found = $ShellStream.Expect($ExpectString, (New-TimeSpan -Seconds $TimeOut))}
            'Regex'  {$found = $ShellStream.Expect($ExpectRegex, (New-TimeSpan -Seconds $TimeOut))}
        }
        
        if ($found -ne $null)
        {
            Write-Verbose -Message "Executing action: $($Action)."
            $ShellStream.WriteLine($Action)
            Write-Verbose -Message 'Action has been executed.'
            $true
        }
        else
        {
            Write-Verbose -Message 'Expect timeout without achiving a match.'
            $false
        }
    }
    End
    {
    }
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Invoke-SSHStreamExpectSecureAction
{
    [CmdletBinding(DefaultParameterSetName='String')]
    [OutputType([Bool])]
    Param
    (
        # SSH Shell Stream. 
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ValueFromPipeline=$true,
                   Position=0)]
        [Renci.SshNet.ShellStream]
        $ShellStream,

        # Initial command that will generate the output to be evaluated by the expect pattern.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [string]
        $Command,

        # String on what to trigger the action on.
        [Parameter(Mandatory=$true,
                   ParameterSetName='String',
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [string]
        $ExpectString,

        # Regular expression on what to trigger the action on.
        [Parameter(Mandatory=$true,
                   ParameterSetName='Regex',
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [regex]
        $ExpectRegex,

        # SecureString representation of action once an expression is matched.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=3)]
        [securestring]
        $SecureAction,

        # Number of seconds to wait for a match.
        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true,
                   Position=4)]
        [int]
        $TimeOut = 10
    )

    Begin
    {
    }
    Process
    {
        Write-Verbose -Message "Executing command $($Command)."
        $ShellStream.WriteLine($Command)
        Write-Verbose -Message "Waiting for match."
        switch ($PSCmdlet.ParameterSetName)
        {
            'string' {
                Write-Verbose -Message 'Matching by String.'
                $found = $ShellStream.Expect($ExpectString, (New-TimeSpan -Seconds $TimeOut))
             }

            'Regex'  {
                Write-Verbose -Message 'Matching by RegEx.'
                $found = $ShellStream.Expect($ExpectRegex, (New-TimeSpan -Seconds $TimeOut))
             }
        }
        
        if ($found -ne $null)
        {
            Write-Verbose -Message "Executing action."
            $ShellStream.WriteLine([Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureAction)))
            Write-Verbose -Message 'Action has been executed.'
            $SecureAction.Dispose()
            $true
        }
        else
        {
            Write-Verbose -Message 'Expect timeout without achiving a match.'
            $false
        }
    }
    End
    {
    }
}

