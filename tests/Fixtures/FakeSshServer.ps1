<#
.SYNOPSIS
    Minimal fake SSH server that serves a single crafted SSH_MSG_KEXINIT.

.DESCRIPTION
    Used by Get-SSHAlgorithm.Tests.ps1 to exercise the algorithm probe without needing a
    real SSH server. Accepts one connection, performs the RFC 4253 version exchange, sends
    a KEXINIT built from the supplied name-lists, then closes.

    The Mode parameter selects deliberately malformed behaviour so the probe's hardening can
    be tested. Nothing here implements key exchange or authentication.

.PARAMETER Port
    TCP port to listen on, bound to loopback only.

.PARAMETER Mode
    Normal              - a well-formed KEXINIT.
    BadMessageNumber    - a well-formed packet whose message number is not 20.
    HugePacketLength    - a packet length far beyond the accepted maximum.
    Truncated           - a valid length followed by too few bytes, then close.
    NoIdentification    - preamble lines that never contain a version string.
    TooManyPreambleLines- more preamble lines than the probe will read.
    LongIdentification  - an identification line longer than the RFC 4253 limit.

.PARAMETER Cipher
    Cipher name-list the server advertises in both directions.

.PARAMETER MacClientToServer
    MAC name-list advertised for the client to server direction.

.PARAMETER MacServerToClient
    MAC name-list advertised for the server to client direction. Set it different from
    MacClientToServer to exercise the directional split.
#>
param(
    [int]$Port = 22022,

    [ValidateSet('Normal','BadMessageNumber','HugePacketLength','Truncated','NoIdentification','TooManyPreambleLines','LongIdentification')]
    [string]$Mode = 'Normal',

    [string[]]$Cipher = @('aes128-gcm@openssh.com','aes256-gcm@openssh.com'),
    [string[]]$MacClientToServer = @('hmac-sha2-256','hmac-sha2-512'),
    [string[]]$MacServerToClient = @('hmac-sha2-256','hmac-sha2-512')
)

$ErrorActionPreference = 'Stop'

function Add-Bytes([System.Collections.Generic.List[byte]]$list, [byte[]]$bytes) {
    if ($bytes.Length -gt 0) { $list.AddRange($bytes) }
}

function Add-NameList([System.Collections.Generic.List[byte]]$list, [string[]]$names) {
    $s = [System.Text.Encoding]::ASCII.GetBytes(($names -join ','))
    $len = [System.BitConverter]::GetBytes([uint32]$s.Length)
    [array]::Reverse($len)
    Add-Bytes $list ([byte[]]$len)
    Add-Bytes $list ([byte[]]$s)
}

# --- build the KEXINIT payload ----------------------------------------------

$kex         = @('ecdh-sha2-nistp256','diffie-hellman-group14-sha256','ext-info-s','kex-strict-s-v00@openssh.com')
$hostkey     = @('rsa-sha2-256','rsa-sha2-512')
$compression = @('none','zlib@openssh.com')

$payload = [System.Collections.Generic.List[byte]]::new()
$payload.Add($(if ($Mode -eq 'BadMessageNumber') { [byte]21 } else { [byte]20 }))

$cookie = New-Object byte[] 16
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($cookie)
Add-Bytes $payload $cookie

Add-NameList $payload $kex
Add-NameList $payload $hostkey
Add-NameList $payload $Cipher              # ciphers client -> server
Add-NameList $payload $Cipher              # ciphers server -> client
Add-NameList $payload $MacClientToServer
Add-NameList $payload $MacServerToClient
Add-NameList $payload $compression
Add-NameList $payload $compression
Add-NameList $payload @('')                # languages client -> server
Add-NameList $payload @('')                # languages server -> client

$payload.Add([byte]0)                      # first_kex_packet_follows
Add-Bytes $payload ([byte[]]@(0,0,0,0))    # reserved

# --- frame it ----------------------------------------------------------------

$payloadLen = $payload.Count
$padLen = (8 - ((5 + $payloadLen) % 8)) % 8
if ($padLen -lt 4) { $padLen += 8 }
$packetLen = 1 + $payloadLen + $padLen

if ($Mode -eq 'HugePacketLength') { $packetLen = 999999 }

$lenBytes = [System.BitConverter]::GetBytes([uint32]$packetLen)
[array]::Reverse($lenBytes)

$packet = [System.Collections.Generic.List[byte]]::new()
Add-Bytes $packet ([byte[]]$lenBytes)
$packet.Add([byte]$padLen)
Add-Bytes $packet $payload.ToArray()
Add-Bytes $packet (New-Object byte[] $padLen)

if ($Mode -eq 'Truncated') {
    # keep the announced length but send only the first few bytes of the body
    $packet = [System.Collections.Generic.List[byte]]::new($packet.GetRange(0, 10))
}

# --- serve one connection ----------------------------------------------------

$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
$listener.Start()
Write-Output "READY on 127.0.0.1:$Port mode=$Mode"

try {
    $client = $listener.AcceptTcpClient()
    $stream = $client.GetStream()

    switch ($Mode) {
        'NoIdentification' {
            $text = "not a version string`r`nstill not one`r`n"
        }
        'TooManyPreambleLines' {
            $sb = New-Object System.Text.StringBuilder
            for ($i = 0; $i -lt 150; $i++) { [void]$sb.Append("preamble line $i`r`n") }
            [void]$sb.Append("SSH-2.0-TooLateServer`r`n")
            $text = $sb.ToString()
        }
        'LongIdentification' {
            $text = "SSH-2.0-" + ('x' * 400) + "`r`n"
        }
        default {
            $text = "Some preamble line before the version string`r`nSSH-2.0-FakeServer`r`n"
        }
    }

    $banner = [System.Text.Encoding]::ASCII.GetBytes($text)
    $stream.Write($banner, 0, $banner.Length)
    $stream.Flush()

    if ($Mode -notin @('NoIdentification','LongIdentification')) {
        Start-Sleep -Milliseconds 150
        $arr = $packet.ToArray()
        $stream.Write($arr, 0, $arr.Length)
        $stream.Flush()
        Start-Sleep -Milliseconds 400
    }

    $client.Close()
} finally {
    $listener.Stop()
}
