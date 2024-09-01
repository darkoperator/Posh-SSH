---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHSession

## SYNOPSIS
Get current SSH Session that are available for interaction.

## SYNTAX

### Index (Default)
```
Get-SSHSession [[-SessionId] <Int32[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ComputerName
```
Get-SSHSession [[-ComputerName] <String[]>] [-ExactMatch] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Get current SSH Session that are available for interaction.

## EXAMPLES

### EXAMPLE 1
```
Get-SSHSession

SessionId  Host                                            Connected
---------  ----                                            ---------
    0      192.168.1.180                                     True
```

## PARAMETERS

### -SessionId
Session Id for an exiting SSH session.

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: Index

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComputerName
ComputerName for an exiting SSH session.

```yaml
Type: String[]
Parameter Sets: ComputerName
Aliases: Server, HostName, Host

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExactMatch
Only exact match when searching by ComputerName.

```yaml
Type: SwitchParameter
Parameter Sets: ComputerName
Aliases:

Required: False
Position: 0
Default value: False
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

## OUTPUTS

## NOTES
AUTHOR: Carlos Perez carlos_perez@darkoprator.com

## RELATED LINKS
