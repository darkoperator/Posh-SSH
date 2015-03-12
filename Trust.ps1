 <#
 .Synopsis
    List Host and Fingerprint pairs that Posh-SSH trusts.
 .DESCRIPTION
    List Host and Fingerprint pairs that Posh-SSH trusts.
 .EXAMPLE
Get-SSHTrustedHost

SSHHost                                                     Fingerprint                                                                                                         
-------                                                     -----------                                                                                                         
192.168.1.143                                               a4:6e:80:33:3f:32:4:cb:be:e9:a0:80:1b:38:fd:3b                                                                      
192.168.10.3                                                27:ca:f8:39:7e:ba:a:ff:a3:2d:ff:75:16:a6:bc:18                                                                      
192.168.1.225                                               ea:8c:ec:93:1e:9d:ad:2e:41:bc:d0:b3:d8:a9:98:80         

 #>
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


 <#
 .Synopsis
    Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.
 .DESCRIPTION
    Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.
 .EXAMPLE
    New-SSHTrustedHost -SSHHost 192.168.10.20 -FingerPrint a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b -Verbose
VERBOSE: Adding to trusted SSH Host list 192.168.10.20 with a fingerprint of a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b
VERBOSE: SSH Host has been added.
 #>
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
        $softkey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software'. $true)
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

 <#
 .Synopsis
    Removes a given SSH Host from the list of trusted hosts.
 .DESCRIPTION
    Removes a given SSH Host from the list of trusted hosts.
 .EXAMPLE
    Remove-SSHTrustedHost -SSHHost 192.168.10.20 -Verbose
VERBOSE: Removing SSH Host 192.168.10.20 from the list of trusted hosts.
VERBOSE: SSH Host has been removed.
 #>
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