---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Invoke-SSHCommandStream

## SYNOPSIS

## SYNTAX

### Index (Default)
```
Invoke-SSHCommandStream [-Command] <String> [-SessionId] <Int32> [-EnsureConnection] [[-TimeOut] <Int32>]
 [-HostVariable <String>] [-ExitStatusVariable <String>]
```

### Session
```
Invoke-SSHCommandStream [-Command] <String> [-SSHSession] <SshSession> [-EnsureConnection] [[-TimeOut] <Int32>]
 [-HostVariable <String>] [-ExitStatusVariable <String>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Command
@{Text=}

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

### -SessionId
Session Id for an exiting SSH session.

```yaml
Type: Int32
Parameter Sets: Index
Aliases: Index

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EnsureConnection
@{Text=}

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

### -TimeOut
@{Text=}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HostVariable
@{Text=}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExitStatusVariable
@{Text=}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SSHSession
@{Text=}

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Name

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

## INPUTS

### SSH.SshSession

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

