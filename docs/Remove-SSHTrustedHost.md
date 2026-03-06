---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHTrustedHost

## SYNOPSIS
Remove trusted host record from KnownHost store

## SYNTAX

### Host (Default)
```
Remove-SSHTrustedHost [-HostName] <String[]> [-TrustedHostStore <ITrustedHostStore>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TrustedHost
```
Remove-SSHTrustedHost -TrustedHostRecord <TrustedHostRecord[]> [-TrustedHostStore <ITrustedHostStore>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Fingerprint
```
Remove-SSHTrustedHost [-Fingerprint] <String[]> [-WithHost] [-TrustedHostStore <ITrustedHostStore>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Remove trusted host record from KnownHost store

## EXAMPLES

### Example 1
```
PS C:\> Remove-SSHTrustedHost -HostName server1
```

Remove trusted host server1 from TrustedStore

### Example 2
```
PS C:\> Remove-SSHTrustedHost -Fingerprint '53:68:e0:18:b9:13:8a:ea:49:d5:3a:1b:97:45:a5:69'
```

Remove trusted host record with selected finterprint

### Example 3
```
PS C:\> Remove-SSHTrustedHost -Fingerprint '53:68:e0:18:b9:13:8a:ea:49:d5:3a:1b:97:45:a5:69' -WithHost
```

Remove trusted host with selected finterprint

### Example 4
```
PS C:\> Get-SSHTrustedHost server1 | Remove-SSHTrustedHost
```

Remove server1 trusted host record from trusted host list (one by one)

## PARAMETERS

### -HostName
FQDN or IP Address of host

```yaml
Type: String[]
Parameter Sets: Host
Aliases: ComputerName, IPAddress

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Fingerprint
Fingerprint (hostkey) of remote host

```yaml
Type: String[]
Parameter Sets: Fingerprint
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -TrustedHostRecord
TrustedHostRecord of remote host

```yaml
Type: TrustedHostRecord[]
Parameter Sets: TrustedHost
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -WithHost
Remove the host to which the selected fingerprint belongs

```yaml
Type: SwitchParameter
Parameter Sets: Fingerprint
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -TrustedHostStore
Known Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.

```yaml
Type: ITrustedHostStore
Parameter Sets: (All)
Aliases:

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
