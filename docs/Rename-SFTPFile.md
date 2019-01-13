---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Rename-SFTPFile

## SYNOPSIS
Move or Rename remote file via a SFTP Session

## SYNTAX

### Index (Default)
```
Rename-SFTPFile [-SessionId] <Int32[]> [-Path] <String> [-NewName] <String>
```

### Session
```
Rename-SFTPFile [-SFTPSession] <SftpSession[]> [-Path] <String> [-NewName] <String>
```

## DESCRIPTION
Move or Rename remote file via a SFTP Session  specified by index or SFTP Session object.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Rename-SFTPFile -SessionId 0 -Path /tmp/anaconda-ks.cfg -NewName anaconda-ks.cfg.old -Verbose
 VERBOSE: Renaming /tmp/anaconda-ks.cfg to anaconda-ks.cfg.old
 VERBOSE: File renamed
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
Full path to file to rename

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

### -NewName
New name for file.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SFTPSession
Exiting SFTP Session object.

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

