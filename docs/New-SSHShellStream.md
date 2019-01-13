---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHShellStream

## SYNOPSIS
Creates a SSH shell stream for a given SSH Session.

## SYNTAX

### Index (Default)
```
New-SSHShellStream [-SessionId] <Int32> [-TerminalName <String>] [-Columns <Int32>] [-Rows <Int32>]
 [-Width <Int32>] [-Height <Int32>] [-BufferSize <Int32>]
```

### Session
```
New-SSHShellStream [-SSHSession] <SshSession> [-TerminalName <String>] [-Columns <Int32>] [-Rows <Int32>]
 [-Width <Int32>] [-Height <Int32>] [-BufferSize <Int32>]
```

## DESCRIPTION
Creates a SSH shell stream for a given SSH Session.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
$SSHStream = New-SSHShellStream -Index 0
PS C:\> $SSHStream.WriteLine("uname -a")
PS C:\> $SSHStream.read()
Last login: Sat Mar 14 20:02:16 2015 from infidel01.darkoperator.com
[admin@localhost ~]$ uname -a
Linux localhost.localdomain 3.10.0-123.el7.x86_64 #1 SMP Mon Jun 30 12:09:22 UTC 2014 x86_64 x86_64 x86_64 GNU/Linux
[admin@localhost ~]$
```

## PARAMETERS

### -SessionId
SSH Session Id for an existing session.

```yaml
Type: Int32
Parameter Sets: Index
Aliases: Index

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -TerminalName
Name of the terminal.

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

### -Columns
The columns

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 80
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Rows
The rows.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 24
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Width
The width.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 800
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Height
The height.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 600
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -BufferSize
Size of the buffer.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: 1000
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SSHSession
SSH Session Object for an existing session.

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Session

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

## INPUTS

### System.Int32

### System.String

### SSH.SshSession

## OUTPUTS

### Renci.SshNet.ShellStream

## NOTES

## RELATED LINKS

