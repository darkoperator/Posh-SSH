---
external help file: Posh-SSH-help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# Start-SSHPortForward

## SYNOPSIS
Start a configured port forward configured for a SSH Session

## SYNTAX

### Index (Default)
```
Start-SSHPortForward [-SessionId] <Int32> [-BoundPort] <Int32> [-BoundHost] <String>
```

### Session
```
Start-SSHPortForward [-SSHSession] <SshSession> [-BoundPort] <Int32> [-BoundHost] <String>
```

## DESCRIPTION
Stops a configured port forward configured for a SSH Session given the session and port number

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Stop a currently working port forward thru a SSH Session
```

C:\Users\Carlos\> Get-SSHPortForward -Index 0


 BoundHost : 192.168.1.158
 BoundPort : 8081
 Host      : 10.10.10.1
 Port      : 80
 IsStarted : False



 C:\Users\Carlos\> Start-SSHPortForward -Index 0 -BoundPort 8081


 BoundHost : 192.168.1.158
 BoundPort : 8081
 Host      : 10.10.10.1
 Port      : 80
 IsStarted : True

## PARAMETERS

### -SSHSession
{{Fill SSHSession Description}}

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SessionId
{{Fill SessionId Description}}

```yaml
Type: Int32
Parameter Sets: Index
Aliases: Index

Required: True
Position: 1
Default value: 0
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -BoundPort
{{Fill BoundPort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -BoundHost
{{Fill BoundHost Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

