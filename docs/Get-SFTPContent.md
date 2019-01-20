---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPContent

## SYNOPSIS
Gets the content of the item at the specified location over SFTP.

## SYNTAX

### Index (Default)
```
Get-SFTPContent [-SessionId] <Int32[]> [-Path] <String> [[-ContentType] <String>] [-Encoding <String>]
```

### Session
```
Get-SFTPContent [-SFTPSession] <SftpSession[]> [-Path] <String> [[-ContentType] <String>] [-Encoding <String>]
```

## DESCRIPTION
Gets the content of the item at the specified location over SFTP.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Get-SFTPContent -SessionId 0 -Path  /etc/system-release
CentOS Linux release 7.0.1406 (Core)
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
Path to file to get content from.

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

### -ContentType
How should the content be retured for the file being read. 
* Byte - returns a byte array.

* MultiLine - Retuns a string array where each element represents a line in the file.
* String - returns a string with all the contents of the file.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Encoding
What type of encoding to use when content type is String or MultiLine.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
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

