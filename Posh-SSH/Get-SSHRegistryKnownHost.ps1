class SSHRegistryKeyStore: SSH.Stores.IStore {
      hidden $hostKeys = @{}
      hidden $loaded = $False
      [System.Tuple[[string],[string]]]GetKey([string]$HostName) {
         if (-not $this.loaded) {
            $p = Get-ItemProperty HKCU:\SOFTWARE\PoshSSH
            $p | Get-Member -MemberType NoteProperty |
            Where-Object { $_.Name -notin 'PSPath', 'PSParentPath', 'PSChildName', 'PSDrive', 'PSProvider' } |
            ForEach-Object {
               $name = $_.Name
               $hostData = [System.Tuple[[string],[string]]]::new('ssh-rsa', $p.$name)
               $this.hostKeys.Add($name, $hostData)
            }
            $this.loaded = $true
         }
         return $this.hostKeys[$HostName]
      }
      [bool]SetKey([string]$HostName, [string]$KeyType, [string]$Fingerprint) {
         Write-Warning "Set Keys not supported in registry store"
         return $false
      }
}

New-Object SSHRegistryKeyStore
