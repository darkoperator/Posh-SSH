---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPContent

## SYNOPSIS

## SYNTAX

### Index (Default)
```
Set-SFTPContent [-SessionId] <Int32[]> [-Path] <String> [-Value] <Object> [-Encoding <String>] [-Append]
 [<CommonParameters>]
```

### Session
```
Set-SFTPContent [-SFTPSession] <SftpSession[]> [-Path] <String> [-Value] <Object> [-Encoding <String>]
 [-Append] [<CommonParameters>]
```

## DESCRIPTION
Set the content of a specific item on a remote server through an SFTP session.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "My example message`n"
```

Set the content of /tmp/example.txt to "My example message`n" in sftp sesison 0


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
Path to existing remote file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Value
Content to upload

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Encoding
Content encoding

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: UTF8
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
Accepted values: ASCII, Unicode, UTF7, UTF8, UTF32, BigEndianUnicode
```

### -Append
Append content to file

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### Renci.SshNet.Sftp.SftpFile
## NOTES

## RELATED LINKS
