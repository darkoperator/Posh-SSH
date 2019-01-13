---
external help file: Posh-SSH-help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHRemotePortForward

## SYNOPSIS
Port forward a local port as a port on a remote server.

## SYNTAX

### Index (Default)
```
New-SSHRemotePortForward -LocalAdress <String> -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -Index <Int32>
```

### Session
```
New-SSHRemotePortForward -LocalAdress <String> -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -SSHSession <SshSession>
```

## DESCRIPTION
Port forward a local port as a port on a remote server.
A connection on the remote host to the forwarded port will be redirected to the local host port.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Index
{{Fill Index Description}}

```yaml
Type: Int32
Parameter Sets: Index
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LocalAdress
{{Fill LocalAdress Description}}

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

### -LocalPort
{{Fill LocalPort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoteAddress
{{Fill RemoteAddress Description}}

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
{{Fill RemotePort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: Named
Default value: None
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
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

## INPUTS

### SSH.SshSession
System.Int32

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

