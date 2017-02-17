---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Set-SFTPPathAttribute

## SYNOPSIS

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
{{Fill in the Description}}

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -SessionId
{{Fill SessionId Description}}

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: Index

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SFTPSession
{{Fill SFTPSession Description}}

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Path
{{Fill Path Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LastAccessTime
{{Fill LastAccessTime Description}}

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
{{Fill LastWriteTime Description}}

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
{{Fill GroupId Description}}

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
{{Fill UserId Description}}

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
{{Fill GroupCanExecute Description}}

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
{{Fill GroupCanRead Description}}

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
{{Fill GroupCanWrite Description}}

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
{{Fill OthersCanExecute Description}}

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
{{Fill OthersCanRead Description}}

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
{{Fill OthersCanWrite Description}}

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
{{Fill OwnerCanExecute Description}}

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
{{Fill OwnerCanRead Description}}

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
{{Fill OwnerCanWrite Description}}

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

## INPUTS

## OUTPUTS

### Renci.SshNet.Sftp.SftpFileAttributes

## NOTES

## RELATED LINKS

