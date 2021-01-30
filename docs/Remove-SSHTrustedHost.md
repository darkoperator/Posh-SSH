---
external help file: Posh-SSH-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHTrustedHost

## SYNOPSIS
Remove a KnwonHost entry from the default user location or from a KnownHost store. 

## SYNTAX

### Local (Default)
```
Remove-SSHTrustedHost [-HostName] <String> [<CommonParameters>]
```

### Store
```
Remove-SSHTrustedHost [-HostName] <String> -KnowHostStore <Object> [<CommonParameters>]
```

## DESCRIPTION
Remove a KnwonHost entry from the default user location or from a KnownHost store. 

## EXAMPLES



## PARAMETERS

### -HostName
Host name of the entry to remove. 

```yaml
Type: String
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
