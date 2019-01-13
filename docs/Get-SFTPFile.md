---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPFile

## SYNOPSIS
Download a filefrom a SSH Server using SFTP.

## SYNTAX

### Index (Default)
```
Get-SFTPFile [-SessionId] <Int32[]> [-RemoteFile] <String> [-LocalPath] <String> [-NoProgress] [-Overwrite]
```

### Session
```
Get-SFTPFile [-SFTPSession] <SftpSession[]> [-RemoteFile] <String> [-LocalPath] <String> [-NoProgress]
 [-Overwrite]
```

## DESCRIPTION
Download a folder from a SSH server using SFTP against a current session.

## EXAMPLES

### Example 1
```
PS C:\> Get-SFTPFile -SessionId 0 -RemoteFile "./.bash_history" -LocalPath "./bash_history_prod1"
```

Download from the current session path the bash_history file and save it locally.

## PARAMETERS

### -SessionId
Session Id number of an exiting SFTP Session.

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

### -RemoteFile
Full path on of file to download on the remote host.

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

### -LocalPath
Folder on the local machine where to download the file to.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PSPath

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoProgress
@{Text=}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Overwrite
If file is already present locally overwrite it.

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
SFTP Session Object of a currently connected session.

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

