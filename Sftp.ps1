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

        Begin{}
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
function Get-SFTPDirectoryList
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
            $Sess.Session.ListDirectory($Path)
        }
     }
     End{}
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SFTPDirectory
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

        foreach($sess in $ToProcess)
        { 
            if (!$sess.Session.Exists($Path))
            {
                Write-Verbose -Message "Creating directory $($Path)"
                $sess.Session.CreateDirectory($Path)
                Write-Verbose -Message 'Successful directory creation.'
                $sess.Session.Get($Path)
            }
            else
            {
                Write-Error -Message "Specified path of $($Path) already exists."
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
function Remove-SFTPDirectory
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
                   Position=0)]
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
                Write-Verbose -Message "Deleting directory $($Path)."
                $session.Session.DeleteDirectory($Path)
                Write-Verbose -Message 'Directory was deleted.'
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
function Set-SFTPCurrentDirectory
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
function Get-SFTPCurrentDirectory
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
function Remove-SFTPFile
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

        # Full path of where to upload file on remote system.
        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $RemoteFile
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
            $Attrib = Get-SFTPPathAttribute -SFTPSession $session -Path $RemoteFile
            if ($Attrib.IsRegularFile)
            {
                Write-Verbose  -message "Deleting $RemoteFile"
                $session.Session.DeleteFile($RemoteFile)
                Write-Verbose -message "Deleted $RemoteFile"
            }
            else
            {
                throw "The specified remote file $($RemoteFile) is not a file."
            }
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

<#
.Synopsis
   Create a Symbolic Link on the remote host via SFTP.
.DESCRIPTION
   Create a Symbolic Link on the remote host via SFTP.
#>
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
            $linkstatus = Test-SFTPPath -SFTPSession $session -path $LinkPath`
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
                    Write-Error -Exception $_ 
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

<#
.Synopsis
   Gets the content of the item at the specified location over SFTP.
.DESCRIPTION
   Gets the content of the item at the specified location over SFTP.
.EXAMPLE
   PS C:\> Get-SFTPContent -SessionId 0 -Path  /etc/system-release
   CentOS Linux release 7.0.1406 (Core)
#>
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
                Write-Error -Exception $_ -Message "Failed to get content to file $($Path)"
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


<#
.Synopsis
   Writes or replaces the content in an item with new content over SFTP.
.DESCRIPTION
   Writes or replaces the content in an item with new content over SFTP
.EXAMPLE
    PS C:\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "My example message`n"


    FullName       : /tmp/example.txt
    LastAccessTime : 3/16/2015 10:40:16 PM
    LastWriteTime  : 3/16/2015 10:40:55 PM
    Length         : 22
    UserId         : 1000



    PS C:\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt
    My example message

    PS C:\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "New message`n" -Append


    FullName       : /tmp/example.txt
    LastAccessTime : 3/16/2015 10:40:59 PM
    LastWriteTime  : 3/16/2015 10:41:18 PM
    Length         : 34
    UserId         : 1000



    PS C:\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt
    My example message
    New message
#>
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
                Write-Error -Exception $_ -Message "Failed to write content to file $($Path)"
            }
        }
    }
    End
    {
    }
}

<#
.Synopsis
   Create a IO Stream over SFTP for a file on a remote host.
.DESCRIPTION
   Create a IO Stream over SFTP for a file on a remote host.
.EXAMPLE
   PS C:\> $bashhistory = New-SFTPFileStream -SessionId 0 -Path /home/admin/.bash_history -FileMode Open -FileAccess Read
   PS C:\> $bashhistory


    CanRead      : True
    CanSeek      : True
    CanWrite     : False
    CanTimeout   : True
    Length       : 830
    Position     : 0
    IsAsync      : False
    Name         : /home/admin/.bash_history
    Handle       : {0, 0, 0, 0}
    Timeout      : 00:00:30
    ReadTimeout  :
    WriteTimeout :

    PS C:\> $streamreader = New-Object System.IO.StreamReader -ArgumentList $bashhistory
    PS C:\> while ($streamreader.Peek() -ge 0) {$streamreader.ReadLine()}
    ls
    exit
    ssh-keygen -t rsa
    mv ~/.ssh/id_rsa.pub ~/.ssh/authorized_keys
    chmod 600 ~/.ssh/authorized_keys
    vim /etc/ssh/sshd_config
    sudo vim /etc/ssh/sshd_config

    PS C:\> 


#>
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