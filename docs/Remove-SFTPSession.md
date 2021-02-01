---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SFTPSession

## SYNOPSIS
Close and Remove a SFTP Session

## SYNTAX

### Index (Default)
```
Remove-SFTPSession [-SessionId] <Int32[]> [<CommonParameters>]
```

### Session
```
Remove-SFTPSession [[-SFTPSession] <SftpSession[]>] [<CommonParameters>]
```

## DESCRIPTION
Close and Remove a SFTP Session specified by Index or SFTP Session Object.

## EXAMPLES

### EXAMPLE 1
```
Remove-SFTPSession -SessionId 0 -Verbose
 VERBOSE: 0
 VERBOSE: Removing session 0
 True
 VERBOSE: Session 0 Removed
```

## PARAMETERS

### -SessionId
SFTP Session Id of an exiting session.

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

### -SFTPSession
SFTP Session Object of an exiting session.

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: Session

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
### SSH.SftpSession[]
## OUTPUTS

## NOTES

## RELATED LINKS
