---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Invoke-SSHStreamExpectSecureAction

## SYNOPSIS
Executes an action stored in a SecureString on a SSH ShellStream when output matches a desired string.

## SYNTAX

### String (Default)
```
Invoke-SSHStreamExpectSecureAction [-ShellStream] <ShellStream> [-Command] <String> [-ExpectString] <String>
 [-SecureAction] <SecureString> [[-TimeOut] <Int32>]
```

### Regex
```
Invoke-SSHStreamExpectSecureAction [-ShellStream] <ShellStream> [-Command] <String> [-ExpectRegex] <Regex>
 [-SecureAction] <SecureString> [[-TimeOut] <Int32>]
```

## DESCRIPTION
Executes an action stored in a secure string when an output is matched.
By the expect action being in a secure string this function is best used when a password must be provided to a promp protecting it in memory.
Examples uses would be for use in su or sudo commands.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Invoke-SSHStreamExpectSecureAction -ShellStream $stream -Command 'su -' -ExpectString 'Passord:' -SecureAction (read-host -AsSecureString) -Verbose

***********
VERBOSE: Executing command su -.
VERBOSE: Waiting for match.
VERBOSE: Executing action.
VERBOSE: Action has been executed.
True
PS C:\> $stream.Read()

Last login: Sat Mar 14 18:18:52 EDT 2015 on pts/0
Last failed login: Sun Mar 15 08:52:07 EDT 2015 on pts/0
There were 2 failed login attempts since the last successful login.
[root@localhost ~]#
```

### -------------------------- EXAMPLE 2 --------------------------
```
$sudoPassPropmp = [regex]':\s$'
PS C:\> Invoke-SSHStreamExpectSecureAction -ShellStream $stream -Command 'sudo ifconfig' -ExpectRege
x $sudoPassPropmp -SecureAction (read-host -AsSecureString) -Verbose
***********
VERBOSE: Executing command sudo ifconfig.
VERBOSE: Waiting for match.
VERBOSE: Matching by RegEx.
VERBOSE: Executing action.
VERBOSE: Action has been executed.
True
PS C:\> $stream.Read()

[sudo] password for admin:
ens192: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
       inet 192.168.1.180  netmask 255.255.240.0  broadcast 192.168.15.255
       inet6 fe80::20c:29ff:feeb:34a0  prefixlen 64  scopeid 0x20<link>
       ether 00:0c:29:eb:34:a0  txqueuelen 1000  (Ethernet)
       RX packets 300644  bytes 175076049 (166.9 MiB)
       RX errors 0  dropped 0  overruns 0  frame 0
       TX packets 139657  bytes 56363667 (53.7 MiB)
       TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0

lo: flags=73<UP,LOOPBACK,RUNNING>  mtu 65536
       inet 127.0.0.1  netmask 255.0.0.0
       inet6 ::1  prefixlen 128  scopeid 0x10<host>
       loop  txqueuelen 0  (Local Loopback)
       RX packets 0  bytes 0 (0.0 B)
       RX errors 0  dropped 0  overruns 0  frame 0
       TX packets 0  bytes 0 (0.0 B)
       TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0

[admin@localhost ~]$
```

## PARAMETERS

### -ShellStream
SSH Shell Stream.

```yaml
Type: ShellStream
Parameter Sets: (All)
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Command
Initial command that will generate the output to be evaluated by the expect pattern.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ExpectString
String on what to trigger the action on.

```yaml
Type: String
Parameter Sets: String
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SecureAction
SecureString representation of action once an expression is matched.

```yaml
Type: SecureString
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -TimeOut
Number of seconds to wait for a match.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: 4
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ExpectRegex
Regular expression on what to trigger the action on.

```yaml
Type: Regex
Parameter Sets: Regex
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### Renci.SshNet.ShellStream

### System.String

### System.Security.SecureString

### System.Int32

### System.Text.RegularExpressions.Regex

## OUTPUTS

### System.Boolean

## NOTES

## RELATED LINKS

