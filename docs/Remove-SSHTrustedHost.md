---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Remove-SSHTrustedHost

## SYNOPSIS
Removes a given SSH Host from the list of trusted hosts.

## SYNTAX

```
Remove-SSHTrustedHost [-SSHHost] <String>
```

## DESCRIPTION
Removes a given SSH Host from the list of trusted hosts.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Remove-SSHTrustedHost -SSHHost 192.168.10.20 -Verbose
VERBOSE: Removing SSH Host 192.168.10.20 from the list of trusted hosts.
VERBOSE: SSH Host has been removed.
```

## PARAMETERS

### -SSHHost
IP Address of FQDN of host to add to trusted list.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### System.String

## OUTPUTS

## NOTES

## RELATED LINKS

