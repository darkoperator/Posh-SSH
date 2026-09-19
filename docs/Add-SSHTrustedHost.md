---
external help file: PoshSSH.dll-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Add-SSHTrustedHost

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Add-SSHTrustedHost [-HostName] <String> [-Fingerprint] <String> [[-HostKeyName] <String>]
 [-TrustedHostStore <ITrustedHostStore>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```
PS C:\> Add-SSHTrustedHost -HostName server1 -FingerPrint '53:68:e0:18:b9:13:8a:ea:49:d5:3a:1b:97:45:a5:69' -HostKeyName 'rsa'
```

Add server1 trusted host

## PARAMETERS

### -Fingerprint
Fingerprint of hostkey for remote host

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

### -HostKeyName
HostKeyName (cipher name) of hostkey for remote host

```yaml
Type: String
Parameter Sets: (All)
Aliases: KeyCipherName
Accepted values: ssh-ed25519, ecdsa-sha2-nistp256, ecdsa-sha2-nistp384, ecdsa-sha2-nistp521, rsa-sha2-512, rsa-sha2-256, ssh-rsa, ssh-dss

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -HostName
FQDN or IP Address of host

```yaml
Type: String
Parameter Sets: (All)
Aliases: ComputerName, IPAddress

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -TrustedHostStore
Trusted Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.

```yaml
Type: ITrustedHostStore
Parameter Sets: (All)
Aliases: KnownHostStore

Required: False
Position: Named
Default value: None
Accept pipeline input: False
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

### System.String
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
