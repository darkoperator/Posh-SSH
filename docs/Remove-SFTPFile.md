---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Remove-SFTPFile

## SYNOPSIS
Deletes a file on a remote system via a SFTP Session.

## SYNTAX

### Index (Default)
```
Remove-SFTPFile [-SessionId] <Int32[]> [-RemoteFile] <String>
```

### Session
```
Remove-SFTPFile [-SFTPSession] <SftpSession[]> [-RemoteFile] <String>
```

## DESCRIPTION
Deletes a file on a remote system via a SFTP Session specified by index or SFTP Session object.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -SessionId
Session Id for an existing SFTP session.

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

### -RemoteFile
Full path of where to upload file on remote system.

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

