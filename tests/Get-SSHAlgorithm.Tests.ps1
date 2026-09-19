<#
.SYNOPSIS
    Unit tests for Get-SSHAlgorithm.

.DESCRIPTION
    Runs without a live SSH server. The local parameter set needs no network at all; the
    remote parameter set is exercised against a loopback fixture that serves a crafted
    SSH_MSG_KEXINIT, including deliberately malformed ones.

    Requires Pester 5 or later.
#>

BeforeAll {
    $ModulePath = Join-Path $PSScriptRoot '..\Posh-SSH\Posh-SSH.psd1'
    Import-Module $ModulePath -Force -ErrorAction Stop

    $script:Fixture = Join-Path $PSScriptRoot 'Fixtures\FakeSshServer.ps1'

    function Get-FreePort {
        $l = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
        $l.Start()
        $p = $l.LocalEndpoint.Port
        $l.Stop()
        return $p
    }

    # Starts the fixture and waits until it reports it is listening. Returns the job and the
    # port it bound to; the caller is responsible for Stop-FakeServer.
    function Start-FakeServer {
        param([hashtable]$Arguments = @{})

        $port = Get-FreePort
        $Arguments['Port'] = $port

        $job = Start-Job -ScriptBlock {
            param($Path, $Splat)
            & $Path @Splat
        } -ArgumentList $script:Fixture, $Arguments

        $deadline = (Get-Date).AddSeconds(30)
        while ((Get-Date) -lt $deadline) {
            $out = Receive-Job -Job $job -Keep -ErrorAction SilentlyContinue
            if ($out -and ($out -join "`n") -match 'READY') { break }
            if ($job.State -eq 'Failed') { throw "fixture failed to start: $($job.ChildJobs[0].JobStateInfo.Reason)" }
            Start-Sleep -Milliseconds 150
        }
        if ((Get-Date) -ge $deadline) { throw "fixture did not report READY within 30s" }

        return [pscustomobject]@{ Job = $job; Port = $port }
    }

    function Stop-FakeServer {
        param($Server)
        if ($Server -and $Server.Job) {
            Stop-Job -Job $Server.Job -ErrorAction SilentlyContinue
            Remove-Job -Job $Server.Job -Force -ErrorAction SilentlyContinue
        }
    }
}

AfterAll {
    Remove-Module Posh-SSH -Force -ErrorAction SilentlyContinue
}

Describe 'Get-SSHAlgorithm' {

    Context 'Local parameter set' {

        BeforeAll {
            $script:Local = @(Get-SSHAlgorithm)
        }

        It 'Should not throw when given no parameters' {
            { Get-SSHAlgorithm } | Should -Not -Throw
        }

        It 'Should return one object per algorithm category' {
            $script:Local.Count | Should -Be 5
        }

        It 'Should cover every expected category' {
            $categories = $script:Local.Category | Sort-Object
            $categories | Should -Be @('Compression','Encryption','HostKey','KeyExchange','Mac')
        }

        It 'Should report the bundled library version' {
            foreach ($row in $script:Local) {
                $row.LibraryVersion | Should -Not -BeNullOrEmpty
                $row.LibraryVersion | Should -Match '^\d+\.\d+\.\d+'
            }
        }

        It 'Should populate ClientSupported for every category' {
            foreach ($row in $script:Local) {
                $row.ClientSupported.Count | Should -BeGreaterThan 0
            }
        }

        It 'Should leave ServerOffered and Common empty' {
            foreach ($row in $script:Local) {
                $row.ServerOffered.Count | Should -Be 0
                $row.Common.Count | Should -Be 0
            }
        }

        It 'Should report HasCommon as null, since there is no server to compare against' {
            foreach ($row in $script:Local) {
                $row.HasCommon | Should -BeNullOrEmpty
                ($null -eq $row.HasCommon) | Should -BeTrue
            }
        }

        It 'Should not populate host details' {
            foreach ($row in $script:Local) {
                $row.ComputerName | Should -BeNullOrEmpty
                $row.ServerVersion | Should -BeNullOrEmpty
            }
        }

        It 'Should report Direction as Both' {
            foreach ($row in $script:Local) {
                $row.Direction | Should -Be 'Both'
            }
        }

        It 'Should emit objects of type SSH.AlgorithmComparison' {
            $script:Local[0] | Should -BeOfType 'SSH.AlgorithmComparison'
        }
    }

    Context 'Remote parameter set against a well-formed server' {

        BeforeAll {
            $script:Server = Start-FakeServer
            $script:Remote = @(Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $script:Server.Port)
        }

        AfterAll {
            Stop-FakeServer $script:Server
        }

        It 'Should return one object per category when both directions match' {
            $script:Remote.Count | Should -Be 5
        }

        It 'Should record the host and port that were probed' {
            foreach ($row in $script:Remote) {
                $row.ComputerName | Should -Be '127.0.0.1'
                $row.Port | Should -Be $script:Server.Port
            }
        }

        It 'Should capture the server identification string' {
            $script:Remote[0].ServerVersion | Should -Be 'SSH-2.0-FakeServer'
        }

        It 'Should skip preamble lines before the identification string' {
            # the fixture sends one non-version line first; parsing it would have thrown
            $script:Remote[0].ServerVersion | Should -Not -Match 'preamble'
        }

        It 'Should report what the server offered' {
            $kex = $script:Remote | Where-Object Category -eq 'KeyExchange'
            $kex.ServerOffered | Should -Contain 'ecdh-sha2-nistp256'
            $kex.ServerOffered | Should -Contain 'diffie-hellman-group14-sha256'
        }

        It 'Should report pseudo-algorithms verbatim, as ssh -vv does' {
            $kex = $script:Remote | Where-Object Category -eq 'KeyExchange'
            $kex.ServerOffered | Should -Contain 'kex-strict-s-v00@openssh.com'
            $kex.ServerOffered | Should -Contain 'ext-info-s'
        }

        It 'Should intersect the two sides' {
            $kex = $script:Remote | Where-Object Category -eq 'KeyExchange'
            $kex.Common | Should -Contain 'ecdh-sha2-nistp256'
            # a pseudo-algorithm is offered by the server but not supported as a real kex
            $kex.Common | Should -Not -Contain 'ext-info-s'
            $kex.HasCommon | Should -BeTrue
        }

        It 'Should order Common by client preference, not server preference' {
            $kex = $script:Remote | Where-Object Category -eq 'KeyExchange'
            $clientOrder = $kex.ClientSupported | Where-Object { $kex.Common -contains $_ }
            # RFC 4253 7.1 picks the first client entry the server also offers
            ($kex.Common -join ',') | Should -Be ($clientOrder -join ',')
        }
    }

    Context 'Remote parameter set when a category has no overlap' {

        BeforeAll {
            # the shape of issue #632: the server offers only ciphers this client cannot do
            $script:Server = Start-FakeServer -Arguments @{
                Cipher = @('aes128-ocb@example.com','some-future-cipher@example.com')
            }
            $script:Remote = @(Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $script:Server.Port)
        }

        AfterAll {
            Stop-FakeServer $script:Server
        }

        It 'Should mark the failing category as having nothing in common' {
            $enc = $script:Remote | Where-Object Category -eq 'Encryption'
            $enc.Common.Count | Should -Be 0
            $enc.HasCommon | Should -BeFalse
        }

        It 'Should still report what each side offered for that category' {
            $enc = $script:Remote | Where-Object Category -eq 'Encryption'
            $enc.ServerOffered | Should -Contain 'aes128-ocb@example.com'
            $enc.ClientSupported.Count | Should -BeGreaterThan 0
        }

        It 'Should leave the other categories intact' {
            $others = $script:Remote | Where-Object Category -ne 'Encryption'
            foreach ($row in $others) {
                $row.HasCommon | Should -BeTrue
            }
        }

        It 'Should let the failing category be found by filtering on HasCommon' {
            $failing = $script:Remote | Where-Object { -not $_.HasCommon }
            $failing.Count | Should -Be 1
            $failing.Category | Should -Be 'Encryption'
        }
    }

    Context 'Remote parameter set when the server offers different lists per direction' {

        BeforeAll {
            $script:Server = Start-FakeServer -Arguments @{
                MacClientToServer = @('hmac-sha2-256')
                MacServerToClient = @('hmac-sha2-512-etm@openssh.com')
            }
            $script:Remote = @(Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $script:Server.Port)
        }

        AfterAll {
            Stop-FakeServer $script:Server
        }

        It 'Should split only the asymmetric category into two rows' {
            $script:Remote.Count | Should -Be 6
            ($script:Remote | Where-Object Category -eq 'Mac').Count | Should -Be 2
        }

        It 'Should label the two directions' {
            $macs = $script:Remote | Where-Object Category -eq 'Mac'
            ($macs.Direction | Sort-Object) | Should -Be @('ClientToServer','ServerToClient')
        }

        It 'Should report the correct list for each direction' {
            $ctos = $script:Remote | Where-Object { $_.Category -eq 'Mac' -and $_.Direction -eq 'ClientToServer' }
            $stoc = $script:Remote | Where-Object { $_.Category -eq 'Mac' -and $_.Direction -eq 'ServerToClient' }
            $ctos.ServerOffered | Should -Be @('hmac-sha2-256')
            $stoc.ServerOffered | Should -Be @('hmac-sha2-512-etm@openssh.com')
        }

        It 'Should keep symmetric categories collapsed to one row' {
            ($script:Remote | Where-Object Category -eq 'Encryption').Count | Should -Be 1
            ($script:Remote | Where-Object Category -eq 'Encryption').Direction | Should -Be 'Both'
        }
    }

    Context 'Malformed and hostile input' {

        It 'Should reject a packet whose message number is not SSH_MSG_KEXINIT' {
            $server = Start-FakeServer -Arguments @{ Mode = 'BadMessageNumber' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw -ExpectedMessage '*SSH_MSG_KEXINIT*'
            } finally { Stop-FakeServer $server }
        }

        It 'Should reject an implausible packet length rather than allocating it' {
            $server = Start-FakeServer -Arguments @{ Mode = 'HugePacketLength' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw -ExpectedMessage '*implausible packet length*'
            } finally { Stop-FakeServer $server }
        }

        It 'Should fail cleanly when the packet is truncated' {
            $server = Start-FakeServer -Arguments @{ Mode = 'Truncated' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw -ExpectedMessage '*connection was closed*'
            } finally { Stop-FakeServer $server }
        }

        It 'Should fail when no identification string is ever sent' {
            $server = Start-FakeServer -Arguments @{ Mode = 'NoIdentification' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw
            } finally { Stop-FakeServer $server }
        }

        It 'Should stop reading preamble lines rather than looping forever' {
            $server = Start-FakeServer -Arguments @{ Mode = 'TooManyPreambleLines' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw -ExpectedMessage '*No SSH identification string*'
            } finally { Stop-FakeServer $server }
        }

        It 'Should reject an over-long identification line' {
            $server = Start-FakeServer -Arguments @{ Mode = 'LongIdentification' }
            try {
                { Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $server.Port -ErrorAction Stop } |
                    Should -Throw -ExpectedMessage '*longer than*'
            } finally { Stop-FakeServer $server }
        }

        It 'Should report a non-terminating error for an unreachable host and continue' {
            $port = Get-FreePort   # nothing is listening here
            $errors = @()
            $result = Get-SSHAlgorithm -ComputerName '127.0.0.1' -Port $port -ConnectionTimeout 2 -ErrorAction SilentlyContinue -ErrorVariable errors
            $result | Should -BeNullOrEmpty
            $errors.Count | Should -BeGreaterThan 0
        }
    }
}
