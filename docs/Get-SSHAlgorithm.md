---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHAlgorithm

## SYNOPSIS
Report the SSH algorithms this module supports, and optionally compare them against those a remote host offers.

## SYNTAX

### Local (Default)
```
Get-SSHAlgorithm [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Remote
```
Get-SSHAlgorithm [-ComputerName] <String[]> [-Port <Int32>] [-ConnectionTimeout <Int32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Reports the key exchange, host key, encryption, MAC and compression algorithms supported by the
Renci.SshNet library bundled with this module, together with that library's version.

When a computer name is given the cmdlet also reads the algorithms the remote host advertises and
reports, for each category, which algorithms the two sides have in common. A connection that fails
with a message such as `Client encryption algorithm not found` means one category has no algorithm
in common; this cmdlet identifies which one.

No credentials are required. A SSH server advertises its algorithms before authentication, so the
probe works against hosts you have no account on. The cmdlet performs no key exchange, does not
authenticate, and does not add anything to the trusted host store.

The algorithms in the Common property are listed in client preference order. RFC 4253 selects the
first algorithm on the client's list that the server also offers, so the first entry is the
algorithm that would actually be negotiated.

Note that the HostKey row reports every host key algorithm the library supports. A real session may
offer fewer, because `New-SSHSession` and the other session cmdlets narrow that list to the key
types already recorded for the host in the trusted host store.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-SSHAlgorithm
```

List the algorithms supported by the bundled library, and the library version, without connecting
to anything.

### Example 2
```powershell
PS C:\> Get-SSHAlgorithm -ComputerName 192.168.1.169
```

Compare the algorithms supported locally against those offered by the host.

### Example 3
```powershell
PS C:\> Get-SSHAlgorithm -ComputerName sftp.example.com | Where-Object { -not $_.HasCommon }
```

Show only the categories where the client and the server have no algorithm in common. This is the
quickest way to explain a connection that fails during algorithm negotiation.

### Example 4
```powershell
PS C:\> Get-SSHAlgorithm -ComputerName 192.168.1.169 -Port 2222 |
          Select-Object Category, Direction, @{n='Negotiated';e={$_.Common | Select-Object -First 1}}
```

Show which algorithm would be selected in each category.

## PARAMETERS

### -ComputerName
FQDN or IP Address of the host whose offered algorithms should be read.

```yaml
Type: String[]
Parameter Sets: Remote
Aliases: HostName, Computer, IPAddress, Host

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ConnectionTimeout
Connection timeout interval in seconds.

```yaml
Type: Int32
Parameter Sets: Remote
Aliases:

Required: False
Position: Named
Default value: 10
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Port
SSH TCP Port number to use for the probe.

```yaml
Type: Int32
Parameter Sets: Remote
Aliases:

Required: False
Position: Named
Default value: 22
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

### System.Int32

## OUTPUTS

### SSH.AlgorithmComparison
One object per algorithm category. Categories are KeyExchange, HostKey, Encryption, Mac and
Compression.

Direction is Both when the server offers the same list in each direction, which is almost always
the case. When a server offers different ciphers, MACs or compression for each direction, that
category is reported twice, as ClientToServer and ServerToClient.

In the default parameter set, where no host is contacted, ServerOffered and Common are empty and
HasCommon is null.

## NOTES

## RELATED LINKS

[New-SSHSession]()

[New-SFTPSession]()
