---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPLocation

## SYNOPSIS
Change current location of the SFTP session.

## SYNTAX

### Index (Default)
```
Set-SFTPLocation [-SessionId] <Int32[]> [-Path] <String>
```

### Session
```
Set-SFTPLocation [-SFTPSession] <SftpSession[]> [-Path] <String>
```

## DESCRIPTION
Change current location of the SFTP session.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPLocation -SessionId 0 -Path /usr/local
PS C:\> Get-SFTPLocation -SessionId 0
/usr/local
```

Set the SFTP session current location to /usr/local

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
Remote path to change current location of the SFTP session.

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

