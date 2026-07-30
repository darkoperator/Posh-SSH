<#
.SYNOPSIS
    Unit tests for SSH.HostKeyMatcher, the rule that decides whether a presented host key is
    trusted.

.DESCRIPTION
    Needs no SSH server. This is the check that decides whether to trust a host, so the cases
    below pin both what it must accept and, more importantly, what it must refuse.

    Requires Pester 5 or later.
#>

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '..\Posh-SSH\Posh-SSH.psd1') -Force -ErrorAction Stop

    $script:Sha256 = 'QDDPnJpwrYLse8TilNNXcG0Y7j2xj66mKUx7B4yCEFs'
    $script:Legacy = '2a:c6:e1:81:e4:2e:19:b7:98:d0:1d:a1:b7:7e:82:81'

    function New-StoredKey {
        param([string]$HostKeyName, [string]$Fingerprint)

        $value = New-Object SSH.Stores.TrustedHostValue
        $value.HostKeyName = $HostKeyName
        $value.Fingerprint = $Fingerprint
        return $value
    }

    function Test-Trusted {
        param([object[]]$Stored, [string]$PresentedName, [string]$Fingerprint = $script:Sha256)

        # PowerShell will not implicitly convert Object[] to IEnumerable<TrustedHostValue>
        $typed = [SSH.Stores.TrustedHostValue[]]$Stored
        return [SSH.HostKeyMatcher]::IsTrusted($typed, $PresentedName, $Fingerprint, $script:Legacy)
    }
}

AfterAll {
    Remove-Module Posh-SSH -Force -ErrorAction SilentlyContinue
}

Describe 'HostKeyMatcher.NameMatches' {

    Context 'Exact and wildcard names' {

        It 'Should match an identical name' {
            [SSH.HostKeyMatcher]::NameMatches('ssh-ed25519', 'ssh-ed25519') | Should -BeTrue
        }

        It 'Should not match a different key type' {
            [SSH.HostKeyMatcher]::NameMatches('ssh-ed25519', 'ecdsa-sha2-nistp256') | Should -BeFalse
        }

        It 'Should treat an empty stored name as any key type' {
            [SSH.HostKeyMatcher]::NameMatches('', 'ssh-ed25519') | Should -BeTrue
            [SSH.HostKeyMatcher]::NameMatches($null, 'rsa-sha2-512') | Should -BeTrue
        }
    }

    Context 'RSA signature algorithms share the ssh-rsa key format (RFC 8332)' {

        # known_hosts records the key format, so an ssh-rsa entry is the only entry that will
        # ever exist for a host that negotiates rsa-sha2-256 or rsa-sha2-512.
        It 'Should match ssh-rsa against <presented>' -ForEach @(
            @{ presented = 'rsa-sha2-256' }
            @{ presented = 'rsa-sha2-512' }
            @{ presented = 'ssh-rsa' }
        ) {
            [SSH.HostKeyMatcher]::NameMatches('ssh-rsa', $presented) | Should -BeTrue
        }

        It 'Should match <stored> against ssh-rsa' -ForEach @(
            @{ stored = 'rsa-sha2-256' }
            @{ stored = 'rsa-sha2-512' }
        ) {
            [SSH.HostKeyMatcher]::NameMatches($stored, 'ssh-rsa') | Should -BeTrue
        }

        It 'Should match rsa-sha2-256 against rsa-sha2-512' {
            [SSH.HostKeyMatcher]::NameMatches('rsa-sha2-256', 'rsa-sha2-512') | Should -BeTrue
        }

        It 'Should not treat ssh-dss as an RSA algorithm' {
            [SSH.HostKeyMatcher]::NameMatches('ssh-dss', 'rsa-sha2-512') | Should -BeFalse
            [SSH.HostKeyMatcher]::NameMatches('ssh-rsa', 'ssh-dss') | Should -BeFalse
        }

        It 'Should not treat an RSA certificate algorithm as a bare RSA key' {
            [SSH.HostKeyMatcher]::NameMatches('ssh-rsa', 'rsa-sha2-512-cert-v01@openssh.com') | Should -BeFalse
        }
    }
}

Describe 'HostKeyMatcher.IsTrusted' {

    Context 'The fingerprint must always match' {

        It 'Should trust a matching name and fingerprint' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-ed25519' $script:Sha256) -PresentedName 'ssh-ed25519' |
                Should -BeTrue
        }

        It 'Should refuse a matching name with the wrong fingerprint' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-ed25519' 'WRONG') -PresentedName 'ssh-ed25519' |
                Should -BeFalse
        }

        It 'Should refuse a blank name with the wrong fingerprint' {
            # This is the regression that let a blank stored name bypass verification entirely,
            # because && bound tighter than || in the original predicate.
            Test-Trusted -Stored @(New-StoredKey '' 'WRONG') -PresentedName 'ssh-ed25519' |
                Should -BeFalse
        }

        It 'Should still accept a blank name as a key type wildcard when the fingerprint matches' {
            Test-Trusted -Stored @(New-StoredKey '' $script:Sha256) -PresentedName 'ssh-ed25519' |
                Should -BeTrue
        }

        It 'Should refuse an entry whose fingerprint is blank' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-ed25519' '') -PresentedName 'ssh-ed25519' |
                Should -BeFalse
        }

        It 'Should refuse a correct fingerprint under the wrong key type' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-dss' $script:Sha256) -PresentedName 'ssh-ed25519' |
                Should -BeFalse
        }
    }

    Context 'An ssh-rsa entry covers an rsa-sha2 host key' {

        It 'Should trust a server negotiating <presented> against a stored ssh-rsa entry' -ForEach @(
            @{ presented = 'rsa-sha2-512' }
            @{ presented = 'rsa-sha2-256' }
        ) {
            Test-Trusted -Stored @(New-StoredKey 'ssh-rsa' $script:Sha256) -PresentedName $presented |
                Should -BeTrue
        }

        It 'Should still require the fingerprint for an rsa-sha2 host key' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-rsa' 'WRONG') -PresentedName 'rsa-sha2-512' |
                Should -BeFalse
        }
    }

    Context 'Legacy fingerprints and multiple entries' {

        It 'Should accept the colon separated fingerprint written by older versions' {
            Test-Trusted -Stored @(New-StoredKey 'ssh-ed25519' $script:Legacy) -PresentedName 'ssh-ed25519' |
                Should -BeTrue
        }

        It 'Should trust when any one of several entries matches' {
            $stored = @(
                New-StoredKey 'ecdsa-sha2-nistp256' 'SOMETHING-ELSE'
                New-StoredKey 'ssh-ed25519' $script:Sha256
            )
            Test-Trusted -Stored $stored -PresentedName 'ssh-ed25519' | Should -BeTrue
        }

        It 'Should refuse when no entry matches' {
            $stored = @(
                New-StoredKey 'ecdsa-sha2-nistp256' 'SOMETHING-ELSE'
                New-StoredKey 'ssh-ed25519' 'ALSO-WRONG'
            )
            Test-Trusted -Stored $stored -PresentedName 'ssh-ed25519' | Should -BeFalse
        }

        It 'Should refuse when nothing is stored' {
            Test-Trusted -Stored @() -PresentedName 'ssh-ed25519' | Should -BeFalse
            [SSH.HostKeyMatcher]::IsTrusted($null, 'ssh-ed25519', $script:Sha256, $script:Legacy) | Should -BeFalse
        }
    }
}
