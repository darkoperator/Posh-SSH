---
external help file: Posh-SSH.psm1-Help.xml
online version: 
schema: 2.0.0
---

# Get-SFTPCurrentDirectory

## SYNOPSIS
Get the current location of a SFTP Session.

## SYNTAX

### Index (Default)
```
Get-SFTPCurrentDirectory [-SessionId] <Int32[]>
```

### Session
```
Get-SFTPCurrentDirectory [-SFTPSession] <SftpSession[]>
```

## DESCRIPTION
Get the current location of a SFTP Session specified by SessionId or SFTP Session Object.

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

