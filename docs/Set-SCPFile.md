---
external help file: PoshSSH.dll-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Set-SCPFile

## SYNOPSIS
Copies a file to a SSH server using SCP.

## SYNTAX

### NoKey (Default)
```
Set-SCPFile [-ComputerName] <String[]> [-Credential] <PSCredential> [-Port <Int32>] [-ProxyServer <String>]
 [-ProxyPort <Int32>] [-ProxyCredential <PSCredential>] [-ProxyType <String>] [-LocalFile] <String>
 [-RemotePath] <String> [-OperationTimeout <Int32>] [-ConnectionTimeout <Int32>] [-AcceptKey <Boolean>]
 [-Force] [-ErrorOnUntrusted] [-NoProgress]
```

### Key
```
Set-SCPFile [-ComputerName] <String[]> [-Credential] <PSCredential> [-Port <Int32>] [-ProxyServer <String>]
 [-ProxyPort <Int32>] [-ProxyCredential <PSCredential>] [-ProxyType <String>] [-KeyFile <String>]
 [-LocalFile] <String> [-RemotePath] <String> [-OperationTimeout <Int32>] [-ConnectionTimeout <Int32>]
 [-AcceptKey <Boolean>] [-Force] [-ErrorOnUntrusted] [-NoProgress]
```

### KeyString
```
Set-SCPFile [-ComputerName] <String[]> [-Credential] <PSCredential> [-Port <Int32>] [-ProxyServer <String>]
 [-ProxyPort <Int32>] [-ProxyCredential <PSCredential>] [-ProxyType <String>] [-KeyString <String[]>]
 [-LocalFile] <String> [-RemotePath] <String> [-OperationTimeout <Int32>] [-ConnectionTimeout <Int32>]
 [-AcceptKey <Boolean>] [-Force] [-ErrorOnUntrusted] [-NoProgress]
```

## DESCRIPTION
Copies a specified file to a SSH Server using SCP given its full path and name.

## EXAMPLES

### Example 1
```
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -ComputerName
FQDN or IP Address of host to establish a SCP Session.

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

### -Credential
SSH Credentials to use for connecting to a server.
If a key file is used the password field is used for the Key passphrase.

```yaml
Type: PSCredential
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Port
SSH TCP Port number to use for the SCP connection.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
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
Default value: None
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
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -LocalFile
Full path and file name on the local system of the file to upload.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FullName

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -RemotePath
@{Text=}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -OperationTimeout
@{Text=}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ConnectionTimeout
@{Text=}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -AcceptKey
Automatically accepts a new SSH fingerprint for a host,

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Force
Do not perform any host key validation of the host.

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

### -ErrorOnUntrusted
Throw a terminating error if the host key is not a trusted one.

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

### -NoProgress
Dont show a progress bar during uploading of the file.

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

### -KeyFile
OpenSSH format SSH private key file.

```yaml
Type: String
Parameter Sets: Key
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -KeyString
OpenSSH key in a string array to be used for authentication.

```yaml
Type: String[]
Parameter Sets: KeyString
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: True
```

## INPUTS

### System.String[]

### System.Management.Automation.PSCredential

### System.Int32

### System.String

### System.Boolean

## OUTPUTS

## NOTES

## RELATED LINKS

