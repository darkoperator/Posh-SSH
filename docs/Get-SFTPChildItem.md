---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPChildItem

## SYNOPSIS
Gets the items and child items in a specified path.

## SYNTAX

### Index (Default)
```
Get-SFTPChildItem [-SessionId] <Int32[]> [[-Path] <String>] [-Recursive]
```

### Session
```
Get-SFTPChildItem [-SFTPSession] <SftpSession[]> [[-Path] <String>] [-Recursive]
```

## DESCRIPTION
Gets the items and child items in a specified path.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Get-SFTPChildItem -SessionId 0


FullName       : /home/admin/.
LastAccessTime : 3/17/2015 8:44:38 PM
LastWriteTime  : 3/12/2015 6:06:00 PM
Length         : 4096
UserId         : 1000

FullName       : /home/admin/..
LastAccessTime : 3/12/2015 12:42:46 PM
LastWriteTime  : 3/12/2015 11:45:06 AM
Length         : 18
UserId         : 0

FullName       : /home/admin/.bash_logout
LastAccessTime : 3/15/2015 9:16:49 AM
LastWriteTime  : 6/10/2014 12:31:53 AM
Length         : 18
UserId         : 1000

FullName       : /home/admin/.bash_profile
LastAccessTime : 3/16/2015 10:35:32 PM
LastWriteTime  : 6/10/2014 12:31:53 AM
Length         : 193
UserId         : 1000

FullName       : /home/admin/.bashrc
LastAccessTime : 3/16/2015 10:35:32 PM
LastWriteTime  : 6/10/2014 12:31:53 AM
Length         : 231
UserId         : 1000

FullName       : /home/admin/.bash_history
LastAccessTime : 3/16/2015 10:28:58 PM
LastWriteTime  : 3/16/2015 8:19:48 AM
Length         : 830
UserId         : 1000

FullName       : /home/admin/.ssh
LastAccessTime : 3/12/2015 6:11:43 PM
LastWriteTime  : 3/12/2015 6:04:37 PM
Length         : 41
UserId         : 1000

FullName       : /home/admin/.viminfo
LastAccessTime : 3/12/2015 6:06:00 PM
LastWriteTime  : 3/12/2015 6:06:00 PM
Length         : 577
UserId         : 1000
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
Path of directory whose content will be enumerated.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Recursive
@{Text=}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: 2
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

