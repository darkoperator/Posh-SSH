---
external help file: Posh-SSH-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHPortForward

## SYNOPSIS
Get a list of forwarded TCP Ports for a SSH Session

## SYNTAX

### Index (Default)
```
Get-SSHPortForward [-SessionId] <Int32> [<CommonParameters>]
```

### Session
```
Get-SSHPortForward [-SSHSession] <SshSession> [<CommonParameters>]
```

## DESCRIPTION
Get a list of forwarded TCP Ports for a SSH Session

## EXAMPLES

### EXAMPLE 1
```
Get list of configured forwarded ports
```

PS C:\\\> Get-SSHPortForward -Index 0


 BoundHost : 0.0.0.0
 BoundPort : 8081
 Host      : 10.10.10.1
 Port      : 80
 IsStarted : True

## PARAMETERS

### -SSHSession
{{ Fill SSHSession Description }}

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
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
Position: 1
Default value: 0
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS
