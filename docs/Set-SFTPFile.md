---
external help file: PoshSSH.dll-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Set-SFTPFile

## SYNOPSIS
Copy a local file to a remote host using an existing SFTP Session.

## SYNTAX

### Index (Default)
```
Set-SFTPFile [-SessionId] <Int32[]> [-RemotePath] <String> [-LocalFile] <String> [-Overwrite]
```

### Session
```
Set-SFTPFile [-SFTPSession] <SftpSession[]> [-RemotePath] <String> [-LocalFile] <String> [-Overwrite]
```

## DESCRIPTION
Copy a local file to a remote host using an existing SFTP Session.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -SessionId
@{Text=}

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
@{Text=}

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
@{Text=}

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

### -Overwrite
@{Text=}

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
@{Text=}

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

