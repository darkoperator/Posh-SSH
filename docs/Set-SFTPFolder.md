---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPFolder

## SYNOPSIS
Uploads a folder to a given location using SFTP.

## SYNTAX

### Index (Default)
```
Set-SFTPFolder [-SessionId] <Int32[]> [-RemotePath] <String> [-LocalFolder] <String[]> [-Overwrite]
```

### Session
```
Set-SFTPFolder [-SFTPSession] <SftpSession[]> [-RemotePath] <String> [-LocalFolder] <String[]> [-Overwrite]
```

## DESCRIPTION
Uploads a folder to a given location using SFTP.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPFolder -SessionId 0 -RemotePath "/tmp/site_test" -LocalFolder ./Development/site_src -Overwrite
```

Upload local folder to tmp directory with a new name.

## PARAMETERS

### -LocalFolder
Local path to folder to be uploaded.

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
Overrite folder on target if it already exists.

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

### -RemotePath
Remote path where to upload the item to, including name.

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

## INPUTS

### System.Int32[]
SSH.SftpSession\[\] System.String System.String\[\]

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

