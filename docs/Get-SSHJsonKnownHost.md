---
external help file: PoshSSH.dll-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Get-SSHJsonKnownHost

## SYNOPSIS
Get known hosts stored in a JSON file created by Posh-SSH.
If a file is not specified it will default to $HOME\.poshssh\hosts.json.
If the file specified is not present it will be created.

## SYNTAX

```
Get-SSHJsonKnownHost [[-LocalFile] <String>] [<CommonParameters>]
```

## DESCRIPTION
Get known hosts stored in a JSON file created by Posh-SSH.
If a file is not specified it will default to $HOME\.poshssh\hosts.json.
If the file specified is not present it will be created.

## EXAMPLES

### Example 1
```
PS C:\> $jsontest = Get-SSHJsonKnownHost -LocalFile .\test.json
PS C:\> $jsontest.SetKey("192.168.1.1","Router","12:f8:7e:78:61:b4:bf:e2:de:24:15:96:4e:d4:72:53")
True
PS C:\> $jsontest.GetAllKeys()

HostName    HostKeyName Fingerprint
--------    ----------- -----------
192.168.1.1 Router      12:f8:7e:78:61:b4:bf:e2:de:24:15:96:4e:d4:72:53
```

Create a JSON Known Host Store object and using its methods add a new entry.

## PARAMETERS

### -LocalFile
JSON known_hosts file.
If none is specified %HOME%/.poshssh/hosts.json is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PSPath

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
