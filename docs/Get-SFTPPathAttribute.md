---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPPathAttribute

## SYNOPSIS
Get the attributes for a specified path in a SFTP session.

## SYNTAX

### Index
```
Get-SFTPPathAttribute [-SessionId] <Int32[]> [-Path] <String>
```

### Session
```
Get-SFTPPathAttribute [-SFTPSession] <SftpSession[]> [-Path] <String>
```

## DESCRIPTION
Get the attributes for a specified path in a SFTP session.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Get-SFTPPathAttribute -SessionId 0 -Path "/tmp"

 LastAccessTime    : 2/27/2015 6:38:43 PM
 LastWriteTime     : 2/27/2015 7:01:01 PM
 Size              : 512
 UserId            : 0
 GroupId           : 0
 IsSocket          : False
 IsSymbolicLink    : False
 IsRegularFile     : False
 IsBlockDevice     : False
 IsDirectory       : True
 IsCharacterDevice : False
 IsNamedPipe       : False
 OwnerCanRead      : True
 OwnerCanWrite     : True
 OwnerCanExecute   : True
 GroupCanRead      : True
 GroupCanWrite     : True
 GroupCanExecute   : True
 OthersCanRead     : True
 OthersCanWrite    : True
 OthersCanExecute  : True
 Extensions        :
```

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
Path to get information on.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
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

### SSH.SftpSession[]

## OUTPUTS

### Renci.SshNet.Sftp.SftpFileAttributes

## NOTES

## RELATED LINKS

