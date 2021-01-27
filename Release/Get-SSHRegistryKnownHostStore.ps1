<#
    .SYNOPSIS
       Get KnownHosts from registry (readonly)
    .DESCRIPTION
       Get KnownHosts from registry (readonly)
       It is windows-only compatibility cmdlet
#>
function Get-SSHRegistryKnownHostStore {
    class SSHRegistryKeyStore: SSH.Stores.MemoryStore {
          [void] OnGetKeys() {
              $p = Get-ItemProperty HKCU:\SOFTWARE\PoshSSH
              $HostKeys = $this.HostKeys
              $p | Get-Member -MemberType NoteProperty |
              Where-Object { $_.Name -notin 'PSPath', 'PSParentPath', 'PSChildName', 'PSDrive', 'PSProvider' } |
              ForEach-Object {
                 $name = $_.Name
                 $hostData = [SSH.Stores.KnownHostValue]@{HostKeyName='ssh-rsa'; Fingerprint=$p.$name}
                 $HostKeys.AddOrUpdate($name, $hostData, { return $hostData } )
              }
          }
          [bool]SetKey([string]$HostName, [string]$KeyType, [string]$Fingerprint) {
             return $false
          }
          [bool]RemoveByHost([string] $Host) {
              return $false
          }
          [bool]RemoveByFingerprint([string] $Fingerprint) {
              return $false
          }
    }

    New-Object SSHRegistryKeyStore
}
