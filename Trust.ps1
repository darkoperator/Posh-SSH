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