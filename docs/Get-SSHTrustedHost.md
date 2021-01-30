---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHTrustedHost

## SYNOPSIS
List Host and Fingerprint pairs that Posh-SSH trusts.

## SYNTAX

### Local (Default)
```
Get-SSHTrustedHost [-HostName <String>] [<CommonParameters>]
```

### Store
```
Get-SSHTrustedHost -KnowHostStore <Object> [-HostName <String>] [<CommonParameters>]
```

## DESCRIPTION
List Host and Fingerprint pairs that Posh-SSH trusts.

## EXAMPLES

### EXAMPLE 1
```
Get-SSHTrustedHost
SSHHost                                                     Fingerprint

-------                                                     -----------

192.168.1.143                                               a4:6e:80:33:3f:32:4:cb:be:e9:a0:80:1b:38:fd:3b

192.168.10.3                                                27:ca:f8:39:7e:ba:a:ff:a3:2d:ff:75:16:a6:bc:18

192.168.1.225                                               ea:8c:ec:93:1e:9d:ad:2e:41:bc:d0:b3:d8:a9:98:80
```

## PARAMETERS

### -HostName
{{ Fill HostName Description }}

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
{{ Fill KnowHostStore Description }}

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

### System.Int32
## NOTES

## RELATED LINKS
