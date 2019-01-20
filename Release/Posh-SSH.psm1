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
                ForEach-Object {
                    $JobKey = $_.Key
                    #Write-Verbose $JobKey -Verbose
                    $_.Value
                } |
                ForEach-Object {
                    # Check if it completed or is past the timeout setting.
                    if ( $_.Async.IsCompleted -or $_.Duration.Elapsed.TotalSeconds -gt $TimeOut )
                    {
                        # Set the variable within this function to not use its value from outer scope in case the
                        # EndExecute call throws an exception
                        $Output = ''
                        $Output = $_.cmd.EndExecute($_.Async)

                        # Generate custom object to return to pipeline and client
                        [pscustomobject]@{
                            Output = $Output -replace '\n$' -split '\n'
                            ExitStatus = $_.cmd.ExitStatus
                            Error = $_.cmd.Error
                            Host = $_.Connection.Host
                            Duration = $_.Duration.Elapsed
                        } |
                        ForEach-Object {
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
                ForEach-Object {
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
            'string' {
                Write-Verbose "Matching on string $($ExpectString)"
                $found = $ShellStream.Expect($ExpectString, (New-TimeSpan -Seconds $TimeOut))}
            'Regex'  {
                Write-Verbose "Matching on pattern $($ExpectRegex)"
                $found = $ShellStream.Expect($ExpectRegex, (New-TimeSpan -Seconds $TimeOut))}
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
function Invoke-SSHStreamShellCommand {
    [CmdletBinding()]
    [Alias()]
    [OutputType([int])]
    Param (
        # SSH stream to use for command execution.
        [Parameter(Mandatory = $true,
                   ValueFromPipelineByPropertyName = $true,
                   Position = 0)]
        [Renci.SshNet.ShellStream]
        $ShellStream,

        # Command to execute on SSHStream.
        [Parameter(Mandatory = $true,
                   ValueFromPipelineByPropertyName = $true,
                   Position = 1)]
        [string]
        $Command,

        [Parameter(Mandatory = $false,
                   ValueFromPipelineByPropertyName = $true)]
        [string]
        $PrompPattern = '[\$%#>] $'
    )

    Begin {
        $promptRegEx = [regex]$PrompPattern
    }
    Process {
        # Discard any banner or previous command output
        do { 
            $ShellStream.read() | Out-Null
    
        } while ($ShellStream.DataAvailable)

        $ShellStream.writeline($Command)

        #discard line with command entered
        $ShellStream.ReadLine() | Out-Null
        Start-Sleep -Milliseconds 500

        $out = ''

        # read all output until there is no more
        do { 
            $out += $ShellStream.read()
    
        } while ($ShellStream.DataAvailable)

        $outputLines = $out.Split("`n")
        foreach ($line in $outputLines) {
            if ($line -notmatch $promptRegEx) {
                $line
            }
        }
    }
    End{}
}

########################################################################################
# SFTP Functions


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SFTPSession
{
    param(
        [Parameter(Mandatory=$false,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId
    )

    Begin{}
    Process
    {
        if ($SessionId.Length -gt 0)
        {
            foreach($i in $SessionId)
            {
                foreach($session in $global:SFTPSessions)
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
            # Can not reference SFTPSessions directly so as to be able
            # to remove the sessions when Remove-Sftpession is used
            $return_sessions = @()
            foreach($s in $Global:SFTPSessions){$return_sessions += $s}
            $return_sessions
        }
    }
    End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Remove-SFTPSession
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
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession
    )

        Begin {}
        Process
        {
            if ($PSCmdlet.ParameterSetName -eq 'Index')
            {
                $sessions2remove = @()
                 foreach($i in $SessionId)
                {
                    Write-Verbose $i
                    foreach($session in $Global:SFTPSessions)
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
                     $Global:SFTPSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.SessionId) Removed"
                }
            }

            if ($PSCmdlet.ParameterSetName -eq 'Session')
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
                     Write-Verbose "Removing session $($badsession.SessionId)"
                     if ($badsession.session.IsConnected)
                     {
                        $badsession.session.Disconnect()
                     }
                     $badsession.session.Dispose()
                     $Global:SFTPSessions.Remove($badsession)
                     Write-Verbose "Session $($badsession.SessionId) Removed"
                }
            }

        }
        End{}

}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SFTPChildItem
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$false,
                   Position=1)]
        [string]
        $Path,
        [Parameter(Mandatory=$false,
                   Position=2)]
        [switch]
        $Recursive

     )

     Begin
     {

        function Get-SFTPDirectoryRecursive
        {
            param($Path,$SFTPSession)

            $total = $Sess.Session.ListDirectory($Path)

            #List Files
            $total | Where-Object {$_.IsDirectory -eq $false}

            #Get items in a path
            $total | Where-Object {$_.IsDirectory -eq $true -and @('.','..') -notcontains $_.Name } |
            ForEach-Object {$_; Get-SFTPDirectoryRecursive -Path $_.FullName -SFTPSession $sess}

        }

        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($Sess in $ToProcess)
        {
            if ($Path.Length -eq 0)
            {
                $Path = $Sess.Session.WorkingDirectory
            }
            else
            {
                $Attribs = Get-SFTPPathAttribute -SFTPSession $Sess -Path $Path
                if (!$Attribs.IsDirectory)
                {
                    throw "Specified path of $($Path) is not a directory."
                }
            }
            if($Recursive)
            {
                Get-SFTPDirectoryRecursive -Path $Path -SFTPSession $Sess
            }
            else
            {
                $Sess.Session.ListDirectory($Path)
            }
        }
     }
     End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Test-SFTPPath
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $Path

     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($session in $ToProcess)
        {

            $session.Session.Exists($Path)
        }
     }
     End{}
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Remove-SFTPItem
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $Path,

        # Force the deletion of a none empty directory by recursively deleting all files in it.
        [Parameter(Mandatory=$false)]
        [switch]
        $Force

     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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

        foreach($session in $ToProcess)
        {

            if (Test-SFTPPath -SFTPSession $session -Path $Path)
            {
                $attr = Get-SFTPPathAttribute -SFTPSession $session -Path $Path
                if ($attr.IsDirectory)
                {
                    $content = Get-SFTPChildItem -SFTPSession $session -Path $Path
                    if ($content.count -gt 2 -and !$Force)
                    {
                        throw "Specified path of $($Path) is not an empty directory."
                    }
                    elseif ($Force)
                    {
                        Write-Verbose -Message "Recursively deleting $($Path)."
                        [SSH.SshModHelper]::DeleteDirectoryRecursive($Path, $session.Session)
                        return
                    }
                }
                Write-Verbose -Message "Removing $($Path)."
                $session.Session.Delete($Path)
                Write-Verbose -Message "$($Path) removed."
            }
            else
            {
                throw "Specified path of $($Path) does not exist."
            }

        }
     }
     End{}
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Move-SFTPItem
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   Position=1,
                   ValueFromPipelineByPropertyName = $true)]
        [Alias('FullName')]
        [string]
        $Path,

        [Parameter(Mandatory=$true,
                   Position=2)]
        [string]
        $Destination

     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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

        foreach($session in $ToProcess)
        {

            if (Test-SFTPPath -SFTPSession $session -Path $Path)
            {
                $itemInfo = $session.Session.Get($path)
                Write-Verbose("Moving $($path) to $($Destination).")
                $itemInfo.MoveTo($Destination)
            }
            else
            {
                throw "Specified path of $($Path) does not exist."
            }

        }
     }
     End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Set-SFTPLocation
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $Path

     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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

        foreach($session in $ToProcess)
        {
            $Attribs = Get-SFTPPathAttribute -SFTPSession $session -Path $Path
            if ($Attribs.IsDirectory)
            {
                Write-Verbose -Message "Changing current directory to $($Path)"
                $session.Session.ChangeDirectory($Path)
                Write-Verbose -Message 'Current directory changed.'
            }
            else
            {
                throw "Specified path of $($Path) is not a directory."
            }
        }
     }
     End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SFTPLocation
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession
     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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

        foreach($session in $ToProcess)
        {
            $session.Session.WorkingDirectory
        }

     }
    End{}
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Rename-SFTPFile
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        # Full path to file to rename
        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $Path,

        # New name for file.
        [Parameter(Mandatory=$true,
                   Position=2)]
        [string]
        $NewName
     )

     Begin
     {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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

        foreach($session in $ToProcess)
        {
            $attrib = Get-SFTPPathAttribute -SFTPSession $session -Path $Path
            if ($attrib.IsRegularFile)
            {
                $ContainerPath = Split-Path -Path $Path |ForEach-Object {$_ -replace '\\','/'}
                Write-Verbose "Renaming $($Path) to $($NewName)"
                $session.Session.RenameFile($Path, "$($ContainerPath)/$($NewName)")
                Write-Verbose 'File renamed'
            }
            else
            {
                Write-Error -Message "The specified path $($Path) is not to a file."
            }
        }
     }
    End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SFTPPathAttribute
{
    [CmdletBinding()]
    [OutputType([Renci.SshNet.Sftp.SftpFileAttributes])]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipeline=$true,
                   Position=1)]
        [string]
        $Path
    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($session in $ToProcess)
        {

            if (Test-SFTPPath -SFTPSession $session -Path $Path)
            {
                $session.Session.GetAttributes($Path)
            }
            else
            {
                throw "Path $($Path) does not exist on the target host."
            }
        }
    }
    End
    {
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Set-SFTPPathAttribute
{
    [CmdletBinding()]
    [OutputType([Renci.SshNet.Sftp.SftpFileAttributes])]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipeline=$true,
                   Position=1)]
        [string]
        $Path,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [datetime]
        $LastAccessTime,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [datetime]
        $LastWriteTime,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [int]
        $GroupId,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [int]
        $UserId,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $GroupCanExecute,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $GroupCanRead,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $GroupCanWrite,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OthersCanExecute,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OthersCanRead,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OthersCanWrite,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OwnerCanExecute,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OwnerCanRead,

        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true)]
        [bool]
        $OwnerCanWrite
    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($session in $ToProcess)
        {

            if (Test-SFTPPath -SFTPSession $session -Path $Path)
            {
                $currentAttrib = $session.Session.GetAttributes($Path)

                if($PSBoundParameters.ContainsKey("OwnerCanWrite"))
                {
                    $currentAttrib.OwnerCanWrite = $OwnerCanWrite
                }

                if($PSBoundParameters.ContainsKey("OwnerCanRead"))
                {
                    $currentAttrib.OwnerCanRead = $OwnerCanRead
                }

                if($PSBoundParameters.ContainsKey("OwnerCanExecute"))
                {
                    $currentAttrib.OwnerCanExecute = $OwnerCanExecute
                }

                if($PSBoundParameters.ContainsKey("OthersCanWrite"))
                {
                    $currentAttrib.OthersCanWrite = $OthersCanWrite
                }

                if($PSBoundParameters.ContainsKey("OthersCanRead"))
                {
                    $currentAttrib.OthersCanRead = $OthersCanRead
                }

                if($PSBoundParameters.ContainsKey("OthersCanExecute"))
                {
                    $currentAttrib.OthersCanExecute = $OthersCanExecute
                }


                if($PSBoundParameters.ContainsKey("GroupCanWrite"))
                {
                    $currentAttrib.GroupCanWrite = $GroupCanWrite
                }

                if($PSBoundParameters.ContainsKey("GroupCanRead"))
                {
                    $currentAttrib.GroupCanRead = $GroupCanRead
                }

                if($PSBoundParameters.ContainsKey("GroupCanExecute"))
                {
                    $currentAttrib.GroupCanExecute = $GroupCanExecute
                }

                if($PSBoundParameters.ContainsKey("UserId"))
                {
                    $currentAttrib.UserId = $UserId
                }

                if($PSBoundParameters.ContainsKey("GroupId"))
                {
                    $currentAttrib.GroupId = $GroupId
                }

                if($PSBoundParameters.ContainsKey("LastWriteTime"))
                {
                    $currentAttrib.LastWriteTime = $LastWriteTime
                }

                if($PSBoundParameters.ContainsKey("LastAccessTime"))
                {
                    $currentAttrib.LastAccessTime = $LastAccessTime
                }

                $session.Session.SetAttributes($Path, $currentAttrib)


            }
            else
            {
                throw "Path $($Path) does not exist on the target host."
            }
        }
    }
    End
    {
    }
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SFTPSymlink
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [String]
        $Path,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [String]
        $LinkPath
    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($session in $ToProcess)
        {
            $filepath = Test-SFTPPath -SFTPSession $session -Path $Path
            $linkstatus = Test-SFTPPath -SFTPSession $session -path $LinkPath
            if (($filepath) -and (!$linkstatus))
            {
                try
                {
                    Write-Verbose -Message "Creating symlink for $($Path) to $($LinkPath)"
                    $session.session.SymbolicLink($Path, $LinkPath)
                    $session.session.Get($LinkPath)
                }
                catch
                {
                    Write-Error -Exception $_.Exception
                }

            }
            else
            {
                if ($linkstatus)
                {
                    Write-Error -Message "A file already exists in the path of the link $($linkstatus)"
                }

                if (!$filepath)
                {
                    Write-Error -Message "The path $($Path) to link does not exist"
                }
            }
        }
    }
    End
    {
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SFTPContent
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [String]
        $Path,

        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [ValidateSet('String', 'Byte', 'MultiLine')]
        [string]
        $ContentType = 'String',

        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateSet('ASCII','Unicode', 'UTF7', 'UTF8', 'UTF32', 'BigEndianUnicode')]
        [string]
        $Encoding='UTF8'
    )

    Begin
    {

        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
                {
                    if ($SessionId -contains $session.SessionId)
                    {
                        $ToProcess += $session
                    }
                }
            }
        }

        # Set encoding.
        switch ($Encoding)
        {
            'ASCII' {
                $ContentEncoding = [System.Text.Encoding]::ASCII
            }

            'Unicode' {
                $ContentEncoding = [System.Text.Encoding]::Unicode
            }

            'UTF7' {
                $ContentEncoding = [System.Text.Encoding]::UTF7
            }

            'UTF8' {
                $ContentEncoding = [System.Text.Encoding]::UTF8
            }

            'UTF32' {
                $ContentEncoding = [System.Text.Encoding]::UTF32
            }

            'BigEndianUnicode'{
                $ContentEncoding = [System.Text.Encoding]::BigEndianUnicode
            }
        }
    }
    Process
    {
        foreach($session in $ToProcess)
        {

            $attrib = Get-SFTPPathAttribute -SFTPSession $session -Path $Path
            if ($attrib.IsRegularFile)
            {
                try
                {
                switch ($ContentType)
                {
                    'String' {

                        $session.session.ReadAllText($Path, $ContentEncoding)

                     }

                    'Byte' {

                        $session.session.ReadAllBytes($Path)

                     }

                    'MultiLine' {

                        $session.session.ReadAllLines($Path, $Value, $ContentEncoding)

                    }
                    Default {$session.session.ReadAllBytes($Path)}
                }
            }
                catch
                {
                Write-Error -Exception $_.Exception -Message "Failed to get content to file $($Path)"
            }
            }
            else
            {
                Write-Error -Message "The specified path $($Path) is not to a file."
            }
        }
    }
    End
    {
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function Set-SFTPContent
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    [OutputType([Renci.SshNet.Sftp.SftpFile])]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32[]]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [String]
        $Path,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        $Value,

        [Parameter(Mandatory=$false,
                   ValueFromPipelineByPropertyName=$true)]
        [ValidateSet('ASCII','Unicode', 'UTF7', 'UTF8', 'UTF32', 'BigEndianUnicode')]
        [string]
        $Encoding='UTF8',

        [switch]
        $Append


    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
                {
                    if ($SessionId -contains $session.SessionId)
                    {
                        $ToProcess += $session
                    }
                }
            }
        }

        # Set encoding.
        switch ($Encoding)
        {
            'ASCII' {
                $ContentEncoding = [System.Text.Encoding]::ASCII
            }

            'Unicode' {
                $ContentEncoding = [System.Text.Encoding]::Unicode
            }

            'UTF7' {
                $ContentEncoding = [System.Text.Encoding]::UTF7
            }

            'UTF8' {
                $ContentEncoding = New-Object System.Text.UTF8Encoding $false
            }

            'UTF32' {
                $ContentEncoding = [System.Text.Encoding]::UTF32
            }

            'BigEndianUnicode'{
                $ContentEncoding = [System.Text.Encoding]::BigEndianUnicode
            }
        }

    }
    Process
    {
        foreach($session in $ToProcess)
        {
            $ValueType = $Value.GetType().Name
            write-verbose -message  "Saving a $($ValueType) to $($Path)."
            try
            {
                switch ($ValueType)
                {
                    'string[]' {
                        if ($Append)
                        {
                            $session.session.AppendAllLines($Path, $Value, $ContentEncoding)
                        }
                        else
                        {
                            $session.session.WriteAllLines($Path, $Value, $ContentEncoding)
                        }
                        $session.session.Get($Path)
                     }

                    'byte[]' {
                        if ($Append)
                        {
                            $session.session.WriteAllBytes($Path, $Value)
                        }
                        else
                        {
                            $session.session.WriteAllBytes($Path, $Value)
                        }
                        $session.session.Get($Path)
                     }

                    'string' {
                        if ($Append)
                        {
                            $session.session.AppendAllText($Path, $Value, $ContentEncoding)
                        }
                        else
                        {
                            $session.session.WriteAllText($Path, $Value, $ContentEncoding)
                        }

                        $session.session.Get($Path)
                    }
                    Default {Write-Error -Message "The value of type $($ValueType) is not supported."}
                }
            }
            catch
            {
                Write-Error -Exception $_.Exception -Message "Failed to write content to file $($Path)"
            }
        }
    }
    End
    {
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SFTPFileStream
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    [OutputType([Renci.SshNet.Sftp.SftpFileStream])]
    Param
    (
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Alias('Index')]
        [Int32]
        $SessionId,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [String]
        $Path,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=2)]
        [ValidateSet('Append', 'Create', 'CreateNew', 'Open', 'OpenOrCreate', 'Truncate')]
        [string]
        $FileMode,

        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=3)]
        [ValidateSet('Read', 'ReadWrite', 'Write')]
        [string]
        $FileAccess
    )

    Begin
    {
        $ToProcess = $null
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                $sess = Get-SFTPSession -Index $SessionId
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

        # Set FileAccess.
        switch ($FileAccess)
        {
            'Read' {
                $StreamFileAccess = [System.IO.FileAccess]::Read
            }

            'ReadWrite' {
                $StreamFileAccess = [System.IO.FileAccess]::ReadWrite
            }

            'Write' {
                $StreamFileAccess = [System.IO.FileAccess]::Write
            }
        }

        # Set FileMode.
        switch ($FileMode)
        {
            'Append' {
                $StreamFileMode = [System.IO.FileMode]::Append
            }

            'Create' {
                $StreamFileMode = [System.IO.FileMode]::Create
            }

            'CreateNew' {
                $StreamFileMode = [System.IO.FileMode]::CreateNew
            }

            'Open' {
                $StreamFileMode = [System.IO.FileMode]::Open
            }

            'OpenOrCreate' {
                $StreamFileMode = [System.IO.FileMode]::OpenOrCreate
            }

            'Truncate' {
                $StreamFileMode = [System.IO.FileMode]::Truncate
            }
        }
    }
    Process
    {
        $ToProcess.Session.Open($Path, $StreamFileMode, $StreamFileAccess)
    }
    End
    {
    }
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SFTPItem
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

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $Path,

        [Parameter(Mandatory=$false,
                   Position=2)]
        [ValidateSet('File', 'Directory')]
        [string]
        $ItemType = 'File',

        [Parameter(Mandatory=$false)]
        [switch]
        $Recurse

    )

    Begin
    {
        $ToProcess = @()
        switch($PSCmdlet.ParameterSetName)
        {
            'Session'
            {
                $ToProcess = $SFTPSession
            }

            'Index'
            {
                foreach($session in $Global:SFTPSessions)
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
        foreach($sess in $ToProcess)
        {
            if (!$sess.Session.Exists($Path))
            {
                switch($ItemType)
                {
                    'Directory' {
                        Write-Verbose -Message "Creating directory $($Path)"
                        if ($Recurse) {
                            $components = $Path.Split('/')
                            $level = 0
                            $newPath = ''
                            if($Path[0] -eq '.') {
                                $newPath = '.'
                                $level = 1
                            } elseif ($Path[0] -eq '/') {
                                $level = 1
                            } else {
                                $level = 0
                            }

                            for ($level; $level -le ($components.Count -1); $level++ )
                            {
                                if ($level -gt 0)
                                {
                                    $newpath = $newPath + '/' + $components[$level]
                                }
                                else
                                {
                                    $newpath = $newPath + $components[$level]
                                }
                                Write-Verbose -message "Checking if $($newPath) exists."
                                if ($sess.Session.Exists($newPath)) {
                                    write-Verbose -message "$($newPath) exist."
                                    $attr = $sess.Session.GetAttributes($newPath)
                                    if (!$attr.IsDirectory) {
                                        throw "Path in recursive creation is not a directory."
                                    } else {
                                        write-Verbose -message "$($newPath) is directory"
                                    }
                                } else {
                                    Write-Verbose -Message "Creating $($newPath)"
                                    $sess.Session.CreateDirectory($newPath)
                                }
                            }
                            $sess.Session.Get($Path)
                        } else {
                            $sess.Session.CreateDirectory($Path)
                            Write-Verbose -Message 'Directory succesfully created.'
                            $sess.Session.Get($Path)
                        }
                    }

                    'File' {
                        Write-Verbose -Message "Creating file $($Path)"
                        $sess.Session.Create($Path).close()
                        Write-Verbose -Message 'File succesfully created.'
                        $sess.Session.Get($Path)
                    }

                }
            }
            else
            {
                Write-Error -Message "Specified path of $($Path) already exists."
            }
        }
    }
    End
    {
    }
}





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

# .ExternalHelp Posh-SSH.psm1-Help.xml
 function Get-SSHTrustedHost
 {
     [CmdletBinding()]
     [OutputType([int])]
     Param()

     Begin{}
     Process
     {
        $Test_Path_Result = Test-Path -Path "hkcu:\Software\PoshSSH"
        if ($Test_Path_Result -eq $false)
        {
            Write-Verbose -Message 'No previous trusted keys have been configured on this system.'
            New-Item -Path HKCU:\Software -Name PoshSSH | Out-Null
            return
        }
        $poshsshkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\PoshSSH', $true)

        $hostnames = $poshsshkey.GetValueNames()
        $TrustedHosts = @()
        foreach($h in $hostnames)
        {
            $TrustedHost = @{
                SSHHost        = $h
                Fingerprint = $poshsshkey.GetValue($h)
            }
            $TrustedHosts += New-Object -TypeName psobject -Property $TrustedHost
        }
     }
     End
     {
        $TrustedHosts
     }
 }


# .ExternalHelp Posh-SSH.psm1-Help.xml
 function New-SSHTrustedHost
 {
    [CmdletBinding()]
     Param
     (
         # IP Address of FQDN of host to add to trusted list.
         [Parameter(Mandatory=$true,
                    ValueFromPipelineByPropertyName=$true,
                    Position=0)]
         $SSHHost,

         # SSH Server Fingerprint.
         [Parameter(Mandatory=$true,
                    ValueFromPipelineByPropertyName=$true,
                    Position=1)]
         $FingerPrint
     )

     Begin
     {
     }
     Process
     {
        $softkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software', $true)
        if ( $softkey.GetSubKeyNames() -contains 'PoshSSH')
        {
            $poshsshkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\PoshSSH', $true)
        }
        else
        {
            Write-Verbose 'PoshSSH Registry key is not present for this user.'
            New-Item -Path HKCU:\Software -Name PoshSSH | Out-Null
            Write-Verbose 'PoshSSH Key created.'
            $poshsshkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\PoshSSH', $true)
        }
        Write-Verbose "Adding to trusted SSH Host list $($SSHHost) with a fingerprint of $($FingerPrint)"
        $poshsshkey.SetValue($SSHHost, $FingerPrint)
        Write-Verbose 'SSH Host has been added.'
     }
     End
     {
     }
 }

# .ExternalHelp Posh-SSH.psm1-Help.xml
 function Remove-SSHTrustedHost
 {
    [CmdletBinding()]
     Param
     (
         # Param1 help description
         [Parameter(Mandatory=$true,
                    ValueFromPipelineByPropertyName=$true,
                    Position=0)]
         [string]
         $SSHHost
     )

     Begin
     {
     }
     Process
     {
        $softkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software', $true)
        if ($softkey.GetSubKeyNames() -contains 'PoshSSH' )
        {
            $poshsshkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\PoshSSH', $true)
        }
        else
        {
            Write-warning 'PoshSSH Registry key is not present for this user.'
            return
        }
        Write-Verbose "Removing SSH Host $($SSHHost) from the list of trusted hosts."
        if ($poshsshkey.GetValueNames() -contains $SSHHost)
        {
            $poshsshkey.DeleteValue($SSHHost)
            Write-Verbose 'SSH Host has been removed.'
        }
        else
        {
            Write-Warning "SSH Hosts $($SSHHost) was not present in the list of trusted hosts."
        }
     }
     End{}
 }
