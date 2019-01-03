---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Set-SFTPFolder

## SYNOPSIS
Copy an local folder to a remote server using SFTP.

## SYNTAX

### Index (Default)
```
Set-SFTPFolder [-SessionId] <Int32[]> [-RemotePath] <String> [-LocalFolder] <String[]> [-Overwrite]
 [<CommonParameters>]
```

### Session
```
Set-SFTPFolder [-SFTPSession] <SftpSession[]> [-RemotePath] <String> [-LocalFolder] <String[]> [-Overwrite]
 [<CommonParameters>]
```

## DESCRIPTION
Copy an local folder to a remote server using SFTP to the specified path and name. 

## EXAMPLES

### Example 1
```powershell
PS C:\>  Set-SFTPFolder -SessionId 0 -RemotePath /tmp/3dmodel -LocalFolder 'C:\Users\Carlos\Desktop\3D Print Models\' -Verbose -Overwrite
```
Copy local folder to tmp folder with the specified name and overwrite existing content if it already exists.


## PARAMETERS

### -LocalFolder
Local folder to be copied to the target server.

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
Overwrite the content of the specied folder on the target host if it already exists.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePath
Full path and name of folder on target host to copy local folder as.

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
SFTP Session object to take action against.

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
Session ID of an existing SFTP session to take action against.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Int32[]

### SSH.SftpSession[]

### System.String

### System.String[]

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
