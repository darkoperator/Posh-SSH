Import-Module .\Posh-SSH.psd1

Describe "Remove-SSHSession" {
    Context "Parameters" { 
                                 
    }

    Context "Action" { 
        
        $Global:SshSessions = New-Object System.Collections.ArrayList
        $session = New-Object SSH.SshSession
        $session.host = "dummy"
        $session.SessionId = 0
        $session.Session = New-Object Renci.SshNet.SshClient -ArgumentList "localhost","user","pass"
        $Global:SshSessions.Add($session)

        It "Should remove a session by index" {
            Remove-SSHSession -Index 0
            $sessions = Get-SSHSession
            $sessions | should be $null
        }

        It "Removing a non-existing session should not throw" {
            { Remove-SSHSession -SessionId 0 } | Should not throw
        }

        $Global:SshSessions.Add($session)

        It "Should remove a session submitted through the pipeline" {
            $session | Remove-SSHSession

            $sessions = Get-SSHSession
            $sessions | Should be $null
        }

        $Global:SshSessions.Add($session)

        It "Should remove a session by parameter" {
            Remove-SSHSession -SSHSession $session
            $sessions = Get-SSHSession
            $sessions | Should be $null
        }
                             
    }
}

Remove-Module Posh-SSH -ErrorAction SilentlyContinue
Remove-Variable sshsessions -Scope Global -ErrorAction SilentlyContinue