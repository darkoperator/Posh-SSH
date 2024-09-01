---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHRemotePortForward

## SYNOPSIS

## SYNTAX

### Index (Default)
```
New-SSHRemotePortForward [-LocalAdress <String>] -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -SessionId <Int32> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Session
```
New-SSHRemotePortForward [-LocalAdress <String>] -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -SSHSession <SshSession> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -LocalAdress
{{ Fill LocalAdress Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 127.0.0.1
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalPort
{{ Fill LocalPort Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoteAddress
{{ Fill RemoteAddress Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePort
{{ Fill RemotePort Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -SSHSession
{{ Fill SSHSession Description }}

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Session

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SessionId
{{ Fill SessionId Description }}

```yaml
Type: Int32
Parameter Sets: Index
Aliases: Index

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS
