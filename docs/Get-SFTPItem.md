---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SFTPItem

## SYNOPSIS
Downloads via SFTP an item from a SSH server.

## SYNTAX

### Index (Default)
```
Get-SFTPItem [-SessionId] <Int32[]> [-Path] <String[]> [-Destination] <String> [-NoProgress] [-Force]
 [-SkipSymLink]
```

### Session
```
Get-SFTPItem [-SFTPSession] <SftpSession[]> [-Path] <String[]> [-Destination] <String> [-NoProgress] [-Force]
 [-SkipSymLink]
```

## DESCRIPTION
Downloads via SFTP an item from a SSH server. An Item can be a directory or a file.

## EXAMPLES

### Example 1
```
PS C:\>Get-SFTPItem -SessionId 0 -Path .ssh -Destination ./ -Verbose
```

Downloads the .ssh folder from the server to the current directory.


## PARAMETERS

### -Destination
Local path where to download item to.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PSPath

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Force
Overrite item on the local host if it already pressent.

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

### -NoProgress
Do not show upload progress.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Path
Remote path of item to download.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### -SkipSymLink
Do not follow symboliclinks if present in a directory.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: 4
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

### System.Int32[]
SSH.SftpSession\[\] System.String\[\] System.String System.Management.Automation.SwitchParameter

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

