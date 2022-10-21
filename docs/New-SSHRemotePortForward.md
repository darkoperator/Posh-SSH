---
external help file: Posh-SSH-help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHRemotePortForward

## SYNOPSIS
Port forward a local port as a port on a remote server.

## SYNTAX

### Index (Default)
```
New-SSHRemotePortForward [-LocalAdress <String>] -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -SessionId <Int32> [<CommonParameters>]
```

### Session
```
New-SSHRemotePortForward [-LocalAdress <String>] -LocalPort <Int32> -RemoteAddress <String> -RemotePort <Int32>
 -SSHSession <SshSession> [<CommonParameters>]
```

## DESCRIPTION
Port forward a local port as a port on a remote server.
A connection on the remote host to the forwarded port will be redirected to the local host port.

## EXAMPLES

### Example 1
```
PS C:\> New-SSHRemotePortForward -LocalAdress localhost -LocalPort 5678 -RemoteAddress server1 -RemotePort 8765
 -SessionId 0
```
Setup Port forward a localhost:5678 as a port 8765 on a remote server server1.


## PARAMETERS

### -SessionId
SSH Session Id of an exiting session.

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

### -LocalAdress
Local Hostname/Address to forward

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
Local port to forward

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
Remote Hostname/Address to forward

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
Remote port to forward

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
SSH Session object.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### SSH.SshSession
System.Int32

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
