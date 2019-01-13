---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHSession

## SYNOPSIS
Removes and Closes an existing SSH Session.

## SYNTAX

### Index (Default)
```
Remove-SSHSession [-SessionId] <Int32[]>
```

### Session
```
Remove-SSHSession [[-SSHSession] <SshSession[]>]
```

## DESCRIPTION
Removes and Closes an existing SSH Session.
The session can be a SSH Session object or they can be specified by Session Id

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Remove-SSHSession -SessionId 0
True
```

Remove a SSH Session specified by SessionId

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

## INPUTS

### System.Int32[]

### SSH.SshSession[]

## OUTPUTS

## NOTES
AUTHOR: Carlos Perez carlos_perez@darkoprator.com

## RELATED LINKS

