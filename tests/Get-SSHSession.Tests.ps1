Import-Module .\Posh-SSH.psd1

Describe "Get-SSHSession" {
    Context "Parameters" { 
        It "should not throw if no parameter is given" {
            { Get-SSHSession } | Should not throw
        }

        It "Should not throw if index parameter is given" {
            { Get-SSHSession -SessionId 0 } | should not throw
        }
    }

    Context "ReturnData" { 
        $Global:SshSessions = New-Object System.Collections.ArrayList
        $session = New-Object SSH.SshSession
        $session.host = "dummy"
        $session.SessionID = 0
        $Global:SshSessions.Add($session)
    
        $Global:session = New-Object SSH.SshSession
        $session.host = "dummy"
        $session.SessionID = 1
        $SshSessions.Add($session)

        $Allsessions = Get-SSHSession
    
        It "Should return data with no parameters" {        
            { $Allsessions } | should not be null
        }

        It "Should have 2 session objects" {
           ($Allsessions | Measure-Object).count | should be 2
        }

        $OneSession = Get-SSHSession -SessionId 1

        It "Should return the object of the specified index" {
            $OneSession.SessionId | should be 1
        }
        
        It "Should return an object of type sshSession" {
            { $OneSession -is [SSH.SshSession] } | should be $true
        }
    }
}

Remove-Module Posh-SSH -ErrorAction SilentlyContinue
Remove-Variable sshsessions -Scope Global -ErrorAction SilentlyContinue