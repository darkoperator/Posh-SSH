---
external help file: Posh-SSH.psm1-Help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SFTPFileStream

## SYNOPSIS
Create a IO Stream over SFTP for a file on a remote host.

## SYNTAX

### Index (Default)
```
New-SFTPFileStream [-SessionId] <Int32> [-Path] <String> [-FileMode] <String> [-FileAccess] <String>
```

### Session
```
New-SFTPFileStream [-SFTPSession] <SftpSession> [-Path] <String> [-FileMode] <String> [-FileAccess] <String>
```

## DESCRIPTION
Create a IO Stream over SFTP for a file on a remote host.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
$bashhistory = New-SFTPFileStream -SessionId 0 -Path /home/admin/.bash_history -FileMode Open -FileAccess Read

PS C:\> $bashhistory


 CanRead      : True
 CanSeek      : True
 CanWrite     : False
 CanTimeout   : True
 Length       : 830
 Position     : 0
 IsAsync      : False
 Name         : /home/admin/.bash_history
 Handle       : {0, 0, 0, 0}
 Timeout      : 00:00:30
 ReadTimeout  :
 WriteTimeout :

 PS C:\> $streamreader = New-Object System.IO.StreamReader -ArgumentList $bashhistory
 PS C:\> while ($streamreader.Peek() -ge 0) {$streamreader.ReadLine()}
 ls
 exit
 ssh-keygen -t rsa
 mv ~/.ssh/id_rsa.pub ~/.ssh/authorized_keys
 chmod 600 ~/.ssh/authorized_keys
 vim /etc/ssh/sshd_config
 sudo vim /etc/ssh/sshd_config

 PS C:\>
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
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Path
Path of file to create a stream for.

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

### -FileMode
Specifies how the operating system should open a file.
Options are: * Append - Opens the file if it exists and seeks to the end of the file, or creates a new file.

* Create - Specifies that the operating system should create a new file.
* CreateNew - Specifies that the operating system should create a new file
* Open - Specifies that the operating system should open an existing file.
* OpenOrCreate - pecifies that the operating system should open a file if it exists; otherwise, a new file should be

created.
* Truncate - Specifies that the operating system should open an existing file.
When the file is opened, it should be truncated so that its size is zero bytes.

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

### -FileAccess
Defines constants for read, write, or read/write access to a file.
* Read -  Read access to the file.
Data can be read from the file.
* ReadWrite - Read and write access to the file.
Data can be written to and read from the file.
* Write -  Write access to the file.
Data can be written to the file.

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

### -SFTPSession
SFTP Session Object of an exiting session.

```yaml
Type: SftpSession
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

### SSH.SftpSession

## OUTPUTS

### Renci.SshNet.Sftp.SftpFileStream

## NOTES

## RELATED LINKS

