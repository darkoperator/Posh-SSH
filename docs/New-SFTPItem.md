---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SFTPItem

## SYNOPSIS
Create a file or directory on remote host using SFTP.

## SYNTAX

### Index (Default)
```
New-SFTPItem [-SessionId] <Int32[]> [-Path] <String> [[-ItemType] <String>] [-Recurse]
```

### Session
```
New-SFTPItem [-SFTPSession] <SftpSession[]> [-Path] <String> [[-ItemType] <String>] [-Recurse]
```

## DESCRIPTION
Create a file or directory on remote host using SFTP.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
New-SFTPItem -SessionId 0 -Path /tmp/help -ItemType Directory

FullName       : /tmp/help
 LastAccessTime : 3/17/2015 7:58:11 PM
 LastWriteTime  : 3/17/2015 7:58:11 PM
 Length         : 6
 UserId         : 1000


 PS C:\> Get-SFTPPathAttribute 0 -Path /tmp/help


 LastAccessTime    : 3/17/2015 7:58:11 PM
 LastWriteTime     : 3/17/2015 7:58:11 PM
 Size              : 6
 UserId            : 1000
 GroupId           : 1000
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
 OthersCanWrite    : False
 OthersCanExecute  : True
 Extensions        :
```

## PARAMETERS

### -SessionId
SSH Session Id for an existing session.

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
Path on remote host to create the item.

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

### -ItemType
Type of item to create.
Options are: * File

* Directory

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Recurse
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

### SSH.SftpSession[]

## OUTPUTS

## NOTES

## RELATED LINKS

