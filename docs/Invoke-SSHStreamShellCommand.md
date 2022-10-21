---
external help file: Posh-SSH.psm1-Help.xml
Module Name: Posh-SSH
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Invoke-SSHStreamShellCommand

## SYNOPSIS

## SYNTAX

```
Invoke-SSHStreamShellCommand [-ShellStream] <ShellStream> [-Command] <String> [-PrompPattern <String>]
 [<CommonParameters>]
```

## DESCRIPTION
Invoke command in SSH stream shell

## EXAMPLES

### Example 1
```
PS C:\> Invoke-SSHStreamShellCommand -ShellStream $stream -Command 'cat /etc/passwd'
```

Invoke 'cat /etc/passwd' in $shell shell stream

## PARAMETERS

### -ShellStream
SSH stream to use for command execution.

```yaml
Type: ShellStream
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Command
Command to execute on SSHStream.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PrompPattern
Put lines to output until PromtPattern meet

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: [\$%#>] $
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.Int32
## NOTES

## RELATED LINKS
