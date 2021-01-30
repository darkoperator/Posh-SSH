---
external help file: Posh-SSH-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHTrustedHost

## SYNOPSIS
Add a new trusted host for Posh-SSH to use. 

## SYNTAX

### Local (Default)
```
New-SSHTrustedHost [-HostName] <Object> -Name <String> [-FingerPrint] <Object> [<CommonParameters>]
```

### Store
```
New-SSHTrustedHost [-HostName] <Object> -Name <String> [-FingerPrint] <Object> -KnowHostStore <Object>
 [<CommonParameters>]
```

## DESCRIPTION
Add a new trusted host for Posh-SSH to use. By default it will store the new host in the default Posh-SSH hosts.json file unless a KnownHost store is specified.

## EXAMPLES

### Example 1
```
PS C:\> $inmem = New-SSHMemoryKnownHost
PS C:\> New-SSHTrustedHost -KnowHostStore $inmem -HostName 192.168.1.165 -FingerPrint 3c:bf:26:9f:d9:63:d7:48:b8:fc:7b:32:e8:f9:5a:b4 -Name Pi
```

Add new host entry to a store. 

## PARAMETERS

### -FingerPrint
SSH finger print for the the host to be added. 

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -HostName
FQDN or IP address of the host.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -KnowHostStore
Known Host IStore either from New-SSHMemoryKnownHost, Get-SSHJsonKnownHost or Get-SSHOpenSSHKnownHost.

```yaml
Type: Object
Parameter Sets: Store
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Friendly name for the key, in the case of OpenSSH it is the ciphers used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
