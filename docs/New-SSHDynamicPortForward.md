---
external help file: Posh-SSH-help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHDynamicPortForward

## SYNOPSIS
Establishes a Dynamic Port Forward thru a stablished SSH Session.

## SYNTAX

### Index (Default)
```
New-SSHDynamicPortForward [-BoundHost] <String> [-BoundPort] <Int32> [-SessionId] <Int32>
```

### Session
```
New-SSHDynamicPortForward [-BoundHost] <String> [-BoundPort] <Int32> [-SSHSession] <SshSession>
```

## DESCRIPTION
Dynamic port forwarding is a transparent mechanism available for applications, which
support the SOCKS4 or SOCKS5 client protoco.
In windows for best results the local address
to bind to should be the IP of the network interface.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
New-SSHDynamicPortForward -LocalAdress 192.168.28.131 -LocalPort 8081 -Index 0 -Verbose
```

VERBOSE: Finding session with Index 0
VERBOSE: 0
VERBOSE: Adding Forward Port Configuration to session 0
VERBOSE: Starting the Port Forward.
VERBOSE: Forwarding has been started.

## PARAMETERS

### -BoundHost
{{Fill BoundHost Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: Localhost
Accept pipeline input: False
Accept wildcard characters: False
```

### -BoundPort
{{Fill BoundPort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -SSHSession
{{Fill SSHSession Description}}

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
{{Fill SessionId Description}}

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

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

