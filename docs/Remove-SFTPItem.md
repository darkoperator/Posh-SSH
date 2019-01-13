---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SFTPItem

## SYNOPSIS
Deletes the specified item on a SFTP session.

## SYNTAX

### Index (Default)
```
Remove-SFTPItem [-SessionId] <Int32[]> [-Path] <String> [-Force]
```

### Session
```
Remove-SFTPItem [-SFTPSession] <SftpSession[]> [-Path] <String> [-Force]
```

## DESCRIPTION
Deletes the specified item on a SFTP session.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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

### -Path
Path of item to be removed on remote host.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force
Force the deletion of a none empty directory by recursively deleting all files in it.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: False
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

## INPUTS

### System.Int32[]

### SSH.SftpSession[]

## OUTPUTS

## NOTES

## RELATED LINKS

