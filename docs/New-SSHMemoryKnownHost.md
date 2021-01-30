---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHMemoryKnownHost

## SYNOPSIS
Creates a new in-memory known host IStore for temporary use when creating new SSH and SFTP Sessions.

## SYNTAX

```
New-SSHMemoryKnownHost [<CommonParameters>]
```

## DESCRIPTION
Creates a new in-memory known host IStore for temporary use when creating new SSH and SFTP Sessions.

## EXAMPLES

### Example 1
```
PS C:\> $inmem = New-SSHMemoryKnownHost
PS C:\> New-SSHTrustedHost -KnowHostStore $inmem -HostName 192.168.1.165 -FingerPrint 3c:bf:26:9f:d9:63:d7:48:b8:fc:7b:32:e8:f9:5a:b4 -Name Pi
True
PS C:\> $inmem.GetAllKeys()

HostName      HostKeyName Fingerprint
--------      ----------- -----------
192.168.1.165 Pi          3c:bf:26:9f:d9:63:d7:48:b8:fc:7b:32:e8:f9:5a:b4
```

Create and in-memory known host store and add a entry to it.

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
