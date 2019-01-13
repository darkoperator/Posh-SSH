---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHTrustedHost

## SYNOPSIS
Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.

## SYNTAX

```
New-SSHTrustedHost [-SSHHost] <Object> [-FingerPrint] <Object>
```

## DESCRIPTION
Adds a new SSH Host and Fingerprint pait to the list of trusted SSH Hosts.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
New-SSHTrustedHost -SSHHost 192.168.10.20 -FingerPrint a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b -Verbose
VERBOSE: Adding to trusted SSH Host list 192.168.10.20 with a fingerprint of
a4:6e:80:33:3f:31:4:cb:be:e9:a0:80:fb:38:fd:3b
VERBOSE: SSH Host has been added.
```

## PARAMETERS

### -SSHHost
IP Address of FQDN of host to add to trusted list.

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -FingerPrint
SSH Server Fingerprint.

```yaml
Type: Object
Parameter Sets: (All)
Aliases: 

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### System.Object

## OUTPUTS

## NOTES

## RELATED LINKS

