---
external help file: Posh-SSH.psm1-Help.xml
online version: 
schema: 2.0.0
---

# Get-SFTPDirectoryList

## SYNOPSIS
Get a List of Files for SFTP Session.

## SYNTAX

### Index (Default)
```
Get-SFTPDirectoryList [-SessionId] <Int32[]> [[-Path] <String>]
```

### Session
```
Get-SFTPDirectoryList [-SFTPSession] <SftpSession[]> [[-Path] <String>]
```

## DESCRIPTION
Get a collection of objection representing files on a given path on a SFTP Session.

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
Remote path to list.

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

