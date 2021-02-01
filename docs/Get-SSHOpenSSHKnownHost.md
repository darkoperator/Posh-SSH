---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHOpenSSHKnownHost

## SYNOPSIS
Get known_hosts stored in a OpenSSH file.
If a file is not specified it will default to $HOME\.ssh\known_hosts.
If the file specified is not present it will be created.

## SYNTAX

```
Get-SSHOpenSSHKnownHost [[-LocalFile] <String>] [<CommonParameters>]
```

## DESCRIPTION
Get known_hosts stored in a OpenSSH file.
If a file is not specified it will default to $HOME\.ssh\known_hosts.
If the file specified is not present it will be created.

## EXAMPLES

### Example 1
```
PS C:\> $SSHKnown = Get-SSHOpenSSHKnownHost
PS C:\> $SSHKnown.GetAllKeys()

HostName      HostKeyName         Fingerprint
--------      -----------         -----------
192.168.1.165 ecdsa-sha2-nistp256 3c:bf:26:9f:d9:63:d7:48:b8:fc:7b:32:e8:f9:5a:b4
```

Get known hosts already stored by the Windows 10 OpenSSH client in the users home folder.

## PARAMETERS

### -LocalFile
OpenSSH known_hosts file.
If none is specified %HOME%/.ssh/known_hosts is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PSPath

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
