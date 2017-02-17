---
external help file: Posh-SSH.psm1-Help.xml
online version: http://sshnet.codeplex.com/
schema: 2.0.0
---

# Remove-SFTPDirectory

## SYNOPSIS

## SYNTAX

### Index (Default)
```
Remove-SFTPDirectory [-SessionId] <Int32[]> [-Path] <String>
```

### Session
```
Remove-SFTPDirectory [-SFTPSession] <SftpSession[]> [-Path] <String>
```

## DESCRIPTION

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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
Full path to directory to be removed.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 0
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

