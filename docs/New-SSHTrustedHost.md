---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHTrustedHost

## SYNOPSIS
Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.

## SYNTAX

### Local (Default)
```
New-SSHTrustedHost -HostName <Object> -Name <String> [-FingerPrint] <Object> [<CommonParameters>]
```

### Store
```
New-SSHTrustedHost -HostName <Object> -Name <String> [-FingerPrint] <Object> -KnowHostStore <Object>
 [<CommonParameters>]
```

## DESCRIPTION
Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.

## EXAMPLES

### EXAMPLE 1
```
New-SSHTrustedHost -SSHHost 192.168.10.20 -FingerPrint a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b -Verbose
VERBOSE: Adding to trusted SSH Host list 192.168.10.20 with a fingerprint of
a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b
VERBOSE: SSH Host has been added.
```

## PARAMETERS

### -FingerPrint
SSH Server Fingerprint.

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
{{ Fill HostName Description }}

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### -Name
{{ Fill Name Description }}

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

## NOTES

## RELATED LINKS
