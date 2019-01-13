---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Invoke-SSHStreamExpectAction

## SYNOPSIS
Executes an action on a SSH ShellStream when output matches a desired string.

## SYNTAX

### String (Default)
```
Invoke-SSHStreamExpectAction [-ShellStream] <ShellStream> [-Command] <String> [-ExpectString] <String>
 [-Action] <String> [[-TimeOut] <Int32>]
```

### Regex
```
Invoke-SSHStreamExpectAction [-ShellStream] <ShellStream> [-Command] <String> [-ExpectRegex] <Regex>
 [-Action] <String> [[-TimeOut] <Int32>]
```

## DESCRIPTION
Executes an action on a SSH ShellStream when output matches a desired string.
Function returns true if an action was executed.

## EXAMPLES

### Example 1
```
PS C:\> Invoke-SSHStreamExpectAction -ShellStream $ShellStream -Command "config" -ExpectRegex '[\$%#>] $' -Action 'set interface eth0 address 10.10.10.240\24' -Verbose
```

Run second command if the regex for a prompt matches.

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

### -Action
Command to execute once an expression is matched.

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

### System.Int32

### System.Text.RegularExpressions.Regex

## OUTPUTS

### System.Boolean

## NOTES

## RELATED LINKS

