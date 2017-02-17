---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# New-SFTPSymlink

## SYNOPSIS
Create a Symbolic Link on the remote host via SFTP.

## SYNTAX

### Index (Default)
```
New-SFTPSymlink [-SessionId] <Int32[]> [-Path] <String> [-LinkPath] <String>
```

### Session
```
New-SFTPSymlink [-SFTPSession] <SftpSession[]> [-Path] <String> [-LinkPath] <String>
```

## DESCRIPTION
Create a Symbolic Link on the remote host via SFTP.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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
Path on remote host to link to.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -LinkPath
Path on remote host to create as the symbolic link.

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

## NOTES

## RELATED LINKS

