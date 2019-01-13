---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Move-SFTPItem

## SYNOPSIS
Move or rename a specified item in a SFTP session.

## SYNTAX

### Index (Default)
```
Move-SFTPItem [-SessionId] <Int32[]> [-Path] <String> [-Destination] <String>
```

### Session
```
Move-SFTPItem [-SFTPSession] <SftpSession[]> [-Path] <String> [-Destination] <String>
```

## DESCRIPTION
Move or rename a specified item in a SFTP session.

## EXAMPLES

### Example 1
```
PS C:\> Move-SFTPItem -SessionId 0 -Path "/tmp/dev_app" -Destination "/usr/local/share/app"
```

Move a folder in to /usr/local with a new name.

## PARAMETERS

### -SessionId
Session Id of an existing SFTPSession.

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: Index

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SFTPSession
Existing SFTPSession object.

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Path
Full path of item to be moved.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FullName

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Destination
New destination full path.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

