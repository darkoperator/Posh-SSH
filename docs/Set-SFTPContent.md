---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Set-SFTPContent

## SYNOPSIS
Writes or replaces the content in an item with new content over SFTP.

## SYNTAX

### Index (Default)
```
Set-SFTPContent [-SessionId] <Int32[]> [-Path] <String> [-Value] <Object> [-Encoding <String>] [-Append]
```

### Session
```
Set-SFTPContent [-SFTPSession] <SftpSession[]> [-Path] <String> [-Value] <Object> [-Encoding <String>]
 [-Append]
```

## DESCRIPTION
Writes or replaces the content in an item with new content over SFTP

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "My example message`n"
```

FullName       : /tmp/example.txt LastAccessTime : 3/16/2015 10:40:16 PM LastWriteTime  : 3/16/2015 10:40:55 PM Length         : 22 UserId         : 1000

PS C:\\\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt My example message

PS C:\\\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "New message\`n" -Append

FullName       : /tmp/example.txt LastAccessTime : 3/16/2015 10:40:59 PM LastWriteTime  : 3/16/2015 10:41:18 PM Length         : 34 UserId         : 1000

PS C:\\\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt My example message New message

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
Path to file to set content from.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Value
Value to set as content of the file.

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Encoding
Set the encoding of the content to be added to the file.
* ASCII

* Unicode
* UTF7
* UTF8
* UTF32
* BigEndianUnicode

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Append
@{Text=}

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

### System.String

### System.Object

### SSH.SftpSession[]

## OUTPUTS

### Renci.SshNet.Sftp.SftpFile

## NOTES

## RELATED LINKS

