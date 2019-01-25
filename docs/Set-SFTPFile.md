---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPFile

## SYNOPSIS
Copy a local file to a remote host using an existing SFTP Session.

## SYNTAX

### Index (Default)
```
Set-SFTPFile [-SessionId] <Int32[]> [-RemotePath] <String> [-LocalFile] <String[]> [-Overwrite]
```

### Session
```
Set-SFTPFile [-SFTPSession] <SftpSession[]> [-RemotePath] <String> [-LocalFile] <String[]> [-Overwrite]
```

## DESCRIPTION
Copy a local file to a remote host using an existing SFTP Session.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPFile -SessionId 0 -RemotePath "/tmp/dns_validator.py" -LocalFile ./Development/dns_validator.py -Overwrite
```

Upload script to target system and overrite it if already present.

## PARAMETERS

### -SessionId
Session Id of an existing SFTPSession.

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -RemotePath
Remote path of directory to which the item will be uploaded. Should not include the filename nor the trailing slash.

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

### -LocalFile
Local path to item to upload

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: PSPath

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Overwrite
Overrite item on remote host if it already pressent.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: 3
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -SFTPSession
Existing SFTPSession object.

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### System.Int32[]

### System.String

### SSH.SftpSession[]

## OUTPUTS

## NOTES

## RELATED LINKS

