<#
.SYNOPSIS
    Unit tests for Get-SSHSession.

.DESCRIPTION
    Needs no SSH server. Session objects are fabricated and placed in $Global:SshSessions,
    which is where the session cmdlets read from.

    Requires Pester 5 or later.
#>

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '..\Posh-SSH\Posh-SSH.psd1') -Force -ErrorAction Stop

    # A session object with no underlying SshClient. Do not read .Connected on these; it
    # dereferences the client and would throw.
    function New-DummySshSession {
        param([int]$Id, [string]$HostName = 'dummy')

        $session = New-Object SSH.SshSession
        $session.Host = $HostName
        $session.SessionId = $Id
        return $session
    }
}

AfterAll {
    Remove-Variable -Name SshSessions -Scope Global -ErrorAction SilentlyContinue
    Remove-Module Posh-SSH -Force -ErrorAction SilentlyContinue
}

Describe 'Get-SSHSession' {

    Context 'Parameters' {

        BeforeAll {
            $Global:SshSessions = New-Object System.Collections.ArrayList
        }

        It 'Should not throw when no parameter is given' {
            { Get-SSHSession } | Should -Not -Throw
        }

        It 'Should not throw when a session id is given' {
            { Get-SSHSession -SessionId 0 } | Should -Not -Throw
        }

        It 'Should not throw when a computer name is given' {
            { Get-SSHSession -ComputerName 'dummy' } | Should -Not -Throw
        }

        It 'Should accept Index as an alias for SessionId' {
            { Get-SSHSession -Index 0 } | Should -Not -Throw
        }

        It 'Should return nothing when there are no sessions' {
            Get-SSHSession | Should -BeNullOrEmpty
        }
    }

    Context 'Selecting by session id' {

        BeforeAll {
            $Global:SshSessions = New-Object System.Collections.ArrayList
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 0))
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 1))

            $AllSessions = @(Get-SSHSession)
            $OneSession = Get-SSHSession -SessionId 1
        }

        It 'Should return every session when no parameter is given' {
            $AllSessions | Should -Not -BeNullOrEmpty
            $AllSessions.Count | Should -Be 2
        }

        It 'Should return the session matching the requested id' {
            $OneSession | Should -Not -BeNullOrEmpty
            $OneSession.SessionId | Should -Be 1
        }

        It 'Should return an object of type SSH.SshSession' {
            $OneSession | Should -BeOfType 'SSH.SshSession'
        }

        It 'Should accept several ids at once' {
            @(Get-SSHSession -SessionId 0, 1).Count | Should -Be 2
        }

        It 'Should return nothing for an id that does not exist' {
            Get-SSHSession -SessionId 42 | Should -BeNullOrEmpty
        }
    }

    Context 'Selecting by computer name' {

        BeforeAll {
            $Global:SshSessions = New-Object System.Collections.ArrayList
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 0 -HostName 'alpha'))
            [void]$Global:SshSessions.Add((New-DummySshSession -Id 1 -HostName 'alphabet'))
        }

        It 'Should match with wildcards, since the comparison uses -like' {
            @(Get-SSHSession -ComputerName 'alpha*').Count | Should -Be 2
        }

        It 'Should not treat a plain name as a prefix' {
            $result = @(Get-SSHSession -ComputerName 'alpha')
            $result.Count | Should -Be 1
            $result[0].Host | Should -Be 'alpha'
        }

        It 'Should narrow a wildcard match when ExactMatch is used' {
            # -ExactMatch additionally requires equality, so a pattern matches nothing
            Get-SSHSession -ComputerName 'alpha*' -ExactMatch | Should -BeNullOrEmpty
        }

        It 'Should return the single exact host when ExactMatch is used with a plain name' {
            $result = @(Get-SSHSession -ComputerName 'alpha' -ExactMatch)
            $result.Count | Should -Be 1
            $result[0].Host | Should -Be 'alpha'
        }

        It 'Should return nothing for a host that does not exist' {
            Get-SSHSession -ComputerName 'gamma' | Should -BeNullOrEmpty
        }
    }
}
