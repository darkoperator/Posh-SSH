---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHTrustedHost

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Get-SSHTrustedHost [[-HostName] <String[]>] [-TrustedHostStore <ITrustedHostStore>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Get Trusted Host record from KnownHostStore

## EXAMPLES

### EXAMPLE 1
```
PS C:\> Get-SSHTrustedHost -HostName 'server1'
```

Get Trusted Host record for server1 from default KnownHostStore

### EXAMPLE 2
```
PS C:\> Get-SSHTrustedHost -HostName 'server1' -KnownHostStore (Get-SSHRegistryKnownHost)
```

Get Trusted Host record for server1 from registry(deprecated) KnownHostStore

## PARAMETERS

### -HostName
FQDN or IP Address of host

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: ComputerName, IPAddress

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TrustedHostStore
Trusted Host ITrustedHostStore either from New-SSHMemoryTrustedHostStore, Get-SSHJsonTrustedHostStore or Get-SSHOpenSSHTrustedHostStore.

```yaml
Type: ITrustedHostStore
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### SSH.Stores.ITrustedHostStore
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
