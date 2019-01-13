---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SFTPContent

## SYNOPSIS

## SYNTAX

### Index (Default)
```
Set-SFTPContent [-SessionId] <Int32[]> [-Path] <String> [-Value] <Object> [-Encoding <String>] [-Append]
```

### Session
```
Set-SFTPContent [-SFTPSession] <SftpSession[]> [-Path] <String> [-Value] <Object> [-Encoding <String>]
 [-Append]
```

## DESCRIPTION
{{Fill in the Description}}

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -SessionId
{{Fill SessionId Description}}

```yaml
Type: Int32[]
Parameter Sets: Index
Aliases: Index

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SFTPSession
{{Fill SFTPSession Description}}

```yaml
Type: SftpSession[]
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Path
{{Fill Path Description}}

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

### -Value
{{Fill Value Description}}

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Encoding
{{Fill Encoding Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: UTF8
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Append
{{Fill Append Description}}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

## OUTPUTS

### Renci.SshNet.Sftp.SftpFile

## NOTES

## RELATED LINKS

