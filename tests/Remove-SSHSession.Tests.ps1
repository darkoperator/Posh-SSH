<#
.SYNOPSIS
    Unit tests for Remove-SSHSession.

.DESCRIPTION
    Needs no SSH server. Session objects are fabricated with an SshClient that is never
    connected; Remove-SSHSession checks IsConnected before disconnecting, so only Dispose
    runs, which is safe on an unconnected client.

    Requires Pester 5 or later.
#>

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '..\Posh-SSH\Posh-SSH.psd1') -Force -ErrorAction Stop

    function New-DummySshSession {
        param([int]$Id, [string]$HostName = 'dummy')

        $session = New-Object SSH.SshSession
        $session.Host = $HostName
        $session.SessionId = $Id
        $session.Session = New-Object Renci.SshNet.SshClient -ArgumentList 'localhost', 'user', 'pass'
        return $session
    }
}

AfterAll {
    Remove-Variable -Name SshSessions -Scope Global -ErrorAction SilentlyContinue
    Remove-Module Posh-SSH -Force -ErrorAction SilentlyContinue
}

Describe 'Remove-SSHSession' {

    # Each test starts from a single session, because every test removes one
    BeforeEach {
        $Global:SshSessions = New-Object System.Collections.ArrayList
        $Session = New-DummySshSession -Id 0
        [void]$Global:SshSessions.Add($Session)
    }

    Context 'Removing a session' {

        It 'Should remove a session by session id' {
            Remove-SSHSession -SessionId 0
            Get-SSHSession | Should -BeNullOrEmpty
        }

        It 'Should remove a session by the Index alias' {
            Remove-SSHSession -Index 0
            Get-SSHSession | Should -BeNullOrEmpty
        }

        It 'Should remove a session passed through the pipeline' {
            $Session | Remove-SSHSession
            Get-SSHSession | Should -BeNullOrEmpty
        }

        It 'Should remove a session passed as a parameter' {
            Remove-SSHSession -SSHSession $Session
            Get-SSHSession | Should -BeNullOrEmpty
        }
    }

    Context 'Removing a session that is not there' {

        It 'Should not throw for an id that does not exist' {
            { Remove-SSHSession -SessionId 42 } | Should -Not -Throw
        }

        It 'Should leave the existing sessions alone' {
            Remove-SSHSession -SessionId 42
            @(Get-SSHSession).Count | Should -Be 1
        }
    }

    Context 'Removing one of several sessions' {

        BeforeEach {
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 1))
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 2))
        }

        It 'Should remove only the requested session' {
            Remove-SSHSession -SessionId 1
            $remaining = @(Get-SSHSession)
            $remaining.Count | Should -Be 2
            $remaining.SessionId | Should -Not -Contain 1
        }

        It 'Should remove several sessions at once' {
            Remove-SSHSession -SessionId 0, 2
            $remaining = @(Get-SSHSession)
            $remaining.Count | Should -Be 1
            $remaining[0].SessionId | Should -Be 1
        }
    }
}
