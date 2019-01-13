---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPItem

## SYNOPSIS
Upload a scefied item to a remote server though a SFTP Session.

## SYNTAX

### Index (Default)
```
Set-SFTPItem [-SessionId] <Int32[]> [-Destination] <String> [-Path] <String[]> [-Force]
```

### Session
```
Set-SFTPItem [-SFTPSession] <SftpSession[]> [-Destination] <String> [-Path] <String[]> [-Force]
```

## DESCRIPTION
Upload a scefied item to a remote server though a SFTP Session.
The item can be either a file or a folder.

## EXAMPLES

### Example 1
```
PS C:\> Set-SFTPItem -SessionId 0 -Destination /tmp -Path ./Development/dns_test.py -Force
```

## PARAMETERS

### -Destination
Remote path where to upload the item to.

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

### -Force
Overrite item on remote host if it already pressent.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: 3
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Local path to item to upload

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: PSPath

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SFTPSession
Existing SFTPSession object.

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SessionId
Session Id of an existing SFTPSession.

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### System.Int32[]
SSH.SftpSession\[\] System.String System.String\[\]

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

