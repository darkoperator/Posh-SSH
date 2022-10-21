---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHTrustedHost

## SYNOPSIS

## SYNTAX

### Local (Default)
```
Remove-SSHTrustedHost [-HostName] <String> [<CommonParameters>]
```

### Store
```
Remove-SSHTrustedHost [-HostName] <String> -KnownHostStore <Object> [<CommonParameters>]
```

## DESCRIPTION
Remove trusted host record from KnownHost store

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-SSHTrustedHost -HostName server1
```

Remove known host record for server1

## PARAMETERS

### -HostName
IP Address of FQDN of host to add to trusted list.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -KnownHostStore
Known Host Store

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

## OUTPUTS

## NOTES

## RELATED LINKS
