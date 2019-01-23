Import-Module .\Posh-SSH.psd1

$VerbosePreference = "Continue"

Describe "TrustedKeyMng Class" {


    It "Initializes Host Key File" {
        [SSH.TrustedKeyMng]::InitializeTrustedHostFile($PSCmdlet.CommandRuntime) | Should -Be $true
    }
    

    Context "SetKey - Static Method" {

        It "Stores a new key" {
            $hostName = "Server1"
            $hostKey = "a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0:a0"
            [SSH.TrustedKeyMng]::SetKey($hostName, $hostKey, $PSCmdlet.CommandRuntime)

            $newKey = [SSH.TrustedKeyMng]::GetKeys()
            $newKey | 
                Where-Object {$_.Host -eq $hostName} | 
                Select-Object -ExpandProperty Fingerprint | 
                Should -be $hostKey
        }
    }

    Context "GetKeys - Static Method" {

        It "Return no Errors" {
            [SSH.TrustedKeyMng]::GetKeys() | Should -Not -Throw

        }
    }
}