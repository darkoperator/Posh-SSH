---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Test-SFTPPath

## SYNOPSIS
Test if a File or Directory exists on Remote Server via SFTP

## SYNTAX

### Index (Default)
```
Test-SFTPPath [-SessionId] <Int32[]> [-Path] <String>
```

### Session
```
Test-SFTPPath [-SFTPSession] <SftpSession[]> [-Path] <String>
```

## DESCRIPTION
Test if a File or Directory exists on Remote Server via SFTP specified by Index or SFTP Session Object.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Test-SFTPPath -SessionId 0 -Path "/tmp/temporaryfolder"
```

### -------------------------- EXAMPLE 2 --------------------------
```
Test-SFTPPath -SessionId 0 -Path "/apps/myfile-1.0.0.ipa"
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
Path on remote host to test.

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

