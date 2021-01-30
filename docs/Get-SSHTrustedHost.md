---
external help file: Posh-SSH-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHTrustedHost

## SYNOPSIS
Get the current known hosts either from those trusted by Posh-SSH or from a IStore.

## SYNTAX

### Local (Default)
```
Get-SSHTrustedHost [-HostName <String>] [<CommonParameters>]
```

### Store
```
Get-SSHTrustedHost [-KnowHostStore] <Object> [-HostName <String>] [<CommonParameters>]
```

## DESCRIPTION
Get the current known hosts either from those trusted by Posh-SSH, also from a Known Host IStore either from New-SSHMemoryKnownHost, Get-SSHJsonKnownHost or Get-SSHOpenSSHKnownHost.

## EXAMPLES

### Example 1
```
PS C:\>  Get-SSHTrustedHost

HostName      HostKeyName Fingerprint
--------      ----------- -----------
192.168.1.165 ssh-ed25519 7a:da:ab:88:55:95:5b:34:89:f6:46:7f:13:c5:65:c1

```

Get currently stored known hosts.

### Example 2
```
PS C:\> $inmem = New-SSHMemoryKnownHost
PS C:\> $inmem.SetKey("192.168.1.1","Router","12:f8:7e:78:61:b4:bf:e2:de:24:15:96:4e:d4:72:53")
True
PS C:\> Get-SSHTrustedHost -KnowHostStore $inmem

HostName    HostKeyName Fingerprint
--------    ----------- -----------
192.168.1.1 Router      12:f8:7e:78:61:b4:bf:e2:de:24:15:96:4e:d4:72:53
```

Get stored known hosts from an Memory Known Host store.

## PARAMETERS

### -HostName
HostName as stored by Posh-SSH or in a IStore.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KnowHostStore
Known Host IStore either from New-SSHMemoryKnownHost, Get-SSHJsonKnownHost or Get-SSHOpenSSHKnownHost.

```yaml
Type: Object
Parameter Sets: Store
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### SSH.Stores.KnownHostRecord
## NOTES

## RELATED LINKS
