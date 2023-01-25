---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPPathInformation

## SYNOPSIS
Get the filesystem information for a specified path in a SFTP session.

## SYNTAX

### Index
```
Get-SFTPPathInformation [-SessionId] <Int32[]> [-Path] <String> [<CommonParameters>]
```

### Session
```
Get-SFTPPathInformation [-SFTPSession] <SftpSession[]> [-Path] <String> [<CommonParameters>]
```

## DESCRIPTION
Get the filesystem information for a specified path in a SFTP session.

## EXAMPLES

### EXAMPLE 1
```
Get-SFTPPathInformation -SessionId 0 -Path "/tmp"

FileSystemBlockSize : 4096
BlockSize           : 4096
TotalBlocks         : 238077722
FreeBlocks          : 197779304
AvailableBlocks     : 188098654
TotalNodes          : 60599664
FreeNodes           : 60257137
AvailableNodes      : 60257137
Sid                 : 80809960231584831832
IsReadOnly          : False
SupportsSetUid      : True
MaxNameLenght       : 255
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
Path to get information on.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Int32[]
### System.String
### SSH.SftpSession[]
## OUTPUTS

### Renci.SshNet.Sftp.SftpFileSytemInformation
## NOTES

## RELATED LINKS
