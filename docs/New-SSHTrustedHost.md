---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHTrustedHost

## SYNOPSIS

## SYNTAX

### Local (Default)
```
New-SSHTrustedHost [-HostName] <Object> [-FingerPrint] <Object> [[-HostKeyName] <String>] [<CommonParameters>]
```

### Store
```
New-SSHTrustedHost [-HostName] <Object> [-FingerPrint] <Object> [[-HostKeyName] <String>]
 -KnowHostStore <Object> [<CommonParameters>]
```

## DESCRIPTION
Add new trusted host record to KnownHost store

## EXAMPLES

### Example 1
```powershell
PS C:\> New-SSHTrustedHost -HostName server1 -FingerPrint '53:68:e0:18:b9:13:8a:ea:49:d5:3a:1b:97:45:a5:69' -HostKeyName 'rsa'
```

Add new Trusted Host record for server1

### Example 2
```powershell
PS C:\> Get-SSHHostKey -ComputerName server2 | New-SSHTrustedHost
```

Add new Trusted Host record for server2 from scanned one

## PARAMETERS

### -HostName
IP Address of FQDN of host to add to trusted list.

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

### -FingerPrint
SSH Server Fingerprint.
(md5 of host public key)

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -HostKeyName
This is the hostkey cipher name.

```yaml
Type: String
Parameter Sets: (All)
Aliases: KeyCipherName

Required: False
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -KnowHostStore
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
