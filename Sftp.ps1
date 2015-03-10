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
    param( 
        [Parameter(Mandatory=$false,
                   Position=0)]
        [Int32[]] 
        $Index
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]]
        $Index,

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
                    if ($Index -contains $session.Index)
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
            if ($Path -eq $null)
            {
                $Path = $Sess.Session.WorkingDirectory
            }
            else
            {
                $Attribs = Get-SFTPPathAttribute -SFTPSession $Sess -Path $Path
                if ($Attribs.IsDirectory)
                {
                    $Sess.Session.ListDirectory($Path)
                }
                else
                {
                    throw "Specified path of $($Path) is not a directory."
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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
            Write-Verbose -Message "Creating directory $($Path)"
            $sess.Session.CreateDirectory($Path)
            Write-Verbose -Message 'Successful directory creation.'
        }
     }
     End{}
}

<#
.Synopsis
   Test if a File or Directory exists on Remote Server via SFTP
.DESCRIPTION
   Test if a File or Directory exists on Remote Server via SFTP specified by Index or SFTP Session Object.
.EXAMPLE
   Test if a folder temporaryfolder exists in the /tmp directory on server via a SFTP Session

   PS C:\> Test-SFTPPath -Index 0 -Path "/tmp/temporaryfolder"
.EXAMPLE
   Test if a file myfile-1.0.0.ipa exists in the /apps directory on server via a SFTP Session

   PS C:\> Test-SFTPPath -Index 0 -Path "/apps/myfile-1.0.0.ipa"
#>

function Test-SFTPPath
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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

<#function Get-SFTPFile
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Session',
                   ValueFromPipeline=$true,
                   Position=0)]
        [Alias('Session')]
        [SSH.SFTPSession[]]
        $SFTPSession,

        # Full path of file on remote system.
        [Parameter(Mandatory=$true,
                   Position=1)]
        [string]
        $RemoteFile,

        # Directory Path to save the file.
        [Parameter(Mandatory=$true,
                   Position=2)]
        [ValidateScript({Test-Path -path $_ -PathType Container})]
        [string]
        $LocalPath,

        # Append to start of filename
        [Parameter(Mandatory=$false,
                   Position=3)]
        [string]
        $FileNameAppend
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
                    if ($Index -contains $session.Index)
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
            $Attrib = Get-SFTPPathAttGetribute -SFTPSession $session -Path $RemoteFile
            if ($Attrib.IsRegularFile)
            {
                $FileName = Split-Path $RemoteFile -Leaf
                $LocalFile = "$((Resolve-Path $LocalPath).Path)\$filename"
                Write-Verbose -Message "Downloading file $($FileName)"
                if ($FileNameAppend)
                {
                    $FileName = "$($FileNameAppend.Trim())$FileName"
                }
                $LocalFileStream = [System.IO.File]::Create($LocalFile)
                $session.Session.DownloadFile($RemoteFile, $LocalFileStream)
                $LocalFileStream.Flush()
                $LocalFileStream.Close()
            }
            else
            {
                throw "Specified remote file $($RemoteFile) is not a file."
            }
        }
     }
    End{}
}#>


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
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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


<#
.Synopsis
   Move or Rename remote file via a SFTP Session
.DESCRIPTION
   Move or Rename remote file via a SFTP Session  specified by index or SFTP Session object.
.EXAMPLE
   Rename file by moving it

    PS C:\> Rename-SFTPFile -Index 0 -Path /tmp/anaconda-ks.cfg -NewName anaconda-ks.cfg.old -Verbose
    VERBOSE: Renaming /tmp/anaconda-ks.cfg to anaconda-ks.cfg.old
    VERBOSE: File renamed
#>

function Rename-SFTPFile
{
    [CmdletBinding(DefaultParameterSetName='Index')]
    param(
        [Parameter(Mandatory=$true,
                   ParameterSetName = 'Index',
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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

<#
.Synopsis
   Get the attributes for a specified path in a SFTP session.
.DESCRIPTION
   Get the attributes for a specified path in a SFTP session.
.EXAMPLE
   Get-SFTPPathAttribute -Index 0 -Path "/tmp"


    LastAccessTime    : 2/27/2015 6:38:43 PM
    LastWriteTime     : 2/27/2015 7:01:01 PM
    Size              : 512
    UserId            : 0
    GroupId           : 0
    IsSocket          : False
    IsSymbolicLink    : False
    IsRegularFile     : False
    IsBlockDevice     : False
    IsDirectory       : True
    IsCharacterDevice : False
    IsNamedPipe       : False
    OwnerCanRead      : True
    OwnerCanWrite     : True
    OwnerCanExecute   : True
    GroupCanRead      : True
    GroupCanWrite     : True
    GroupCanExecute   : True
    OthersCanRead     : True
    OthersCanWrite    : True
    OthersCanExecute  : True
    Extensions        : 
#>
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
        [Int32[]] 
        $Index,

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
                    if ($Index -contains $session.Index)
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