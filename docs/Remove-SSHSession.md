---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHSession

## SYNOPSIS
Removes and Closes an existing SSH Session.

## SYNTAX

### Index (Default)
```
Remove-SSHSession [-SessionId] <Int32[]> [<CommonParameters>]
```

### Session
```
Remove-SSHSession [[-SSHSession] <SshSession[]>] [<CommonParameters>]
```

## DESCRIPTION
Removes and Closes an existing SSH Session.
The session can be a SSH Session object or they can be specified by Session Id

## EXAMPLES

### EXAMPLE 1
```
Remove-SSHSession -SessionId 0
True
```
Remove a SSH Session specified by SessionId

### -------------------------- EXAMPLE 2 --------------------------
```
Get-SSHSession | Remove-SSHSession
True
True
```

Remove all sessions


## PARAMETERS

### -SessionId
Session Id for an exiting SSH session.

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: Index

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SSHSession
SSH Session Object for an exiting session.

```yaml
Type: SshSession[]
Parameter Sets: Session
Aliases: Name

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Int32[]
### SSH.SshSession[]
## OUTPUTS

## NOTES
AUTHOR: Carlos Perez carlos_perez@darkoprator.com

## RELATED LINKS
