---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPLocation

## SYNOPSIS
Get the current working location for a SFTP connection.

## SYNTAX

### Index (Default)
```
Get-SFTPLocation [-SessionId] <Int32[]> [<CommonParameters>]
```

### Session
```
Get-SFTPLocation [-SFTPSession] <SftpSession[]> [<CommonParameters>]
```

## DESCRIPTION
Get the current working location for a SFTP connection.

## EXAMPLES

### Example 1
```
PS C:\>  Get-SFTPLocation -SessionId 0
/home/carlos
```

Get the current SFTP location a given session is executing under.

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

Required: True
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
