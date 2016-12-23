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
            $total | ? {$_.IsDirectory -eq $false}
            
            #List non filtered directories
            $total | ? {$_.IsDirectory -eq $true -and @('.','..') -contains $_.name}           
            
            #Get items in a path
            $total | ? {$_.IsDirectory -eq $true -and @('.','..') -notcontains $_.Name } |
            % {Get-SFTPDirectoryRecursive -Path $_.FullName -SFTPSession $sess}
       
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
            
            if (Test-SFTPPath -SFTPSession $session -Path $Path)
            {
                $attr = Get-SFTPPathAttribute -SFTPSession $session -Path $Path
                if ($attr.IsDirectory)
                {
                    $content = Get-SFTPChildItem -SFTPSession $session -Path $Path 
                    if ($content.count -gt 2)
                    {
                        throw "Specified path of $($Path) is not an empty directory." 
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

                if($PSBoundParameters.ContainsKey("OwnerCanWrite"))
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
                                    try {
                                        $sess.Session.CreateDirectory($newPath)
                                    } catch {
                                        $_
                                        $return
                                    }
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

# Deprecated
##############

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
        Write-Warning -Message 'This function has been deprecated and replaced by New-SFTPItem'
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
        Write-Warning -Message 'This function has been deprecated and replaced by Get-SFTPChildItem'
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
        Write-Warning -Message 'This function has been deprecated and replaced by Remove-SFTPItem'
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
        Write-Warning -Message 'This function has been deprecated and replaced by Set-SFTPLocation'
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
        Write-Warning -Message 'This function has been deprecated and replaced by Get-SFTPLocation'
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
        Write-Warning -Message 'This function has been deprecated and replaced by Remove-SFTPItem'
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



