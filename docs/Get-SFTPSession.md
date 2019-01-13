---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPSession

## SYNOPSIS
Get current SFTP Sessions that are available for interaction.

## SYNTAX

```
Get-SFTPSession [[-SessionId] <Int32[]>]
```

## DESCRIPTION
Get current SFTP Sessions that are available for interaction.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Get-SFTPSession

SessionId  Host                                            Connected
---------  ----                                            ---------
    0      192.168.1.180                                     True
```

## PARAMETERS

### -SessionId
SSH Session Id for an existing session.

```yaml
Type: Int32[]
Parameter Sets: (All)
Aliases: Index

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

## OUTPUTS

## NOTES
AUTHOR: Carlos Perez carlos_perez@darkoprator.com

## RELATED LINKS

