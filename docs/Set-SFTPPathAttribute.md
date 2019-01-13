---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPPathAttribute

## SYNOPSIS
Sets one or more attributes on a scefied item to a remote server though a SFTP Session.

## SYNTAX

### Index
```
Set-SFTPPathAttribute [-SessionId] <Int32[]> [-Path] <String> [-LastAccessTime <DateTime>]
 [-LastWriteTime <DateTime>] [-GroupId <Int32>] [-UserId <Int32>] [-GroupCanExecute <Boolean>]
 [-GroupCanRead <Boolean>] [-GroupCanWrite <Boolean>] [-OthersCanExecute <Boolean>] [-OthersCanRead <Boolean>]
 [-OthersCanWrite <Boolean>] [-OwnerCanExecute <Boolean>] [-OwnerCanRead <Boolean>] [-OwnerCanWrite <Boolean>]
```

### Session
```
Set-SFTPPathAttribute [-SFTPSession] <SftpSession[]> [-Path] <String> [-LastAccessTime <DateTime>]
 [-LastWriteTime <DateTime>] [-GroupId <Int32>] [-UserId <Int32>] [-GroupCanExecute <Boolean>]
 [-GroupCanRead <Boolean>] [-GroupCanWrite <Boolean>] [-OthersCanExecute <Boolean>] [-OthersCanRead <Boolean>]
 [-OthersCanWrite <Boolean>] [-OwnerCanExecute <Boolean>] [-OwnerCanRead <Boolean>] [-OwnerCanWrite <Boolean>]
```

## DESCRIPTION
Sets one or more attributes on a scefied item to a remote server though a SFTP Session.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPPathAttribute -SessionId 0 -Path /usr/local/test_tools/dns_test.py -GroupCanExecute $true -OwnerCanExecute $true -OthersCanExecute $true
```

Make a specific file executable by all.

## PARAMETERS

### -SessionId
Session Id of an existing SFTPSession.

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
Remote path we want to set attribute on.

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

### -LastAccessTime
Set the last access time.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LastWriteTime
Set the last write time.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -GroupId
Set a group id.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 0
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -UserId
Set the user id.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 0
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -GroupCanExecute
Set if the owning group can execute.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -GroupCanRead
Set if the owning group can read.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -GroupCanWrite
Set if the owning group can write.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OthersCanExecute
Set if others can execute.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OthersCanRead
Set if others can read.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OthersCanWrite
Set if others can write.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OwnerCanExecute
Set if owner can execute.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OwnerCanRead
Set if owner can read.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OwnerCanWrite
Set if owner can write.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SFTPSession
Existing SFTPSession object.

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

### System.DateTime

### System.Int32

### System.Boolean

### SSH.SftpSession[]

## OUTPUTS

### Renci.SshNet.Sftp.SftpFileAttributes

## NOTES

## RELATED LINKS

