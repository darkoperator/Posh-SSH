---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# New-SFTPDirectory

## SYNOPSIS
Create Directory on Remote Server via SFTP.

## SYNTAX

### Index (Default)
```
New-SFTPDirectory [-SessionId] <Int32[]> [-Path] <String>
```

### Session
```
New-SFTPDirectory [-SFTPSession] <SftpSession[]> [-Path] <String>
```

## DESCRIPTION
Create Directory on Remote Server via SFTP specified by Index or SFTP Session Object.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -SessionId
The SessionId number of an existing SFTP Session.

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
Full path on remote host including directory name to be created.

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

