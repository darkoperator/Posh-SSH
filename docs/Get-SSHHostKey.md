---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHHostKey

## SYNOPSIS
Returns host key record

## SYNTAX

### (Default)
```
Get-SSHHostKey [-ComputerName] <string[]> [-Port <int>] [-ProxyServer <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [-
    ProxyType {HTTP | Socks4 | Socks5}] [<CommonParameters>]
```

## DESCRIPTION
Returns a host key entry.
The command attempts to connect to a host and returns the first host key.
The command can be used to scan multiple hosts and add their keys to the known hosts file.

## EXAMPLES

### Example 1
```
PS C:\> Get-SSHHostKey -ComputerName 192.168.1.234
```

Return host key record for server.

### Example 2
```
PS C:\> 'server' | Get-SSHHostKey | New-SSHTrustedHost
```

Return host key record for server and add it to trusted hosts.

## PARAMETERS

### -ComputerName
FQDN or IP Address of host to establish a SSH Session.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: HostName, Computer, IPAddress, Host

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Port
SSH TCP Port number to use for the SSH connection.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 22
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProxyServer
Proxy server name or IP Address to use for connection.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProxyPort
Port to connect to on proxy server to route connection.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 8080
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProxyCredential
PowerShell Credential Object with the credentials for use to connect to proxy server if required.

```yaml
Type: PSCredential
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProxyType
Type of Proxy being used (HTTP, Socks4 or Socks5).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: HTTP
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS
