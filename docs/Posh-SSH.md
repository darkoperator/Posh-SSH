---
Module Name: Posh-SSH
Module Guid: 
Download Help Link: 
Help Version: 
Locale: 
---

# Posh-SSH Module
## Description
Provide SSH and SCP functionality for executing commands against remote hosts.

## Posh-SSH Cmdlets
### [Convert-SSHRegistryToJSonKnownHost](Convert-SSHRegistryToJSonKnownHost.md)
Convert windows registry key storage to Json

### [Get-PoshSSHModVersion](Get-PoshSSHModVersion.md)
Gets the current installed version and the latest version of Posh-SSH.

### [Get-SCPItem](Get-SCPItem.md)
Download from a remote server via SCP a file or directory.

### [Get-SFTPChildItem](Get-SFTPChildItem.md)
Gets the items and child items in a specified path.

### [Get-SFTPContent](Get-SFTPContent.md)
Gets the content of the item at the specified location over SFTP.

### [Get-SFTPItem](Get-SFTPItem.md)
Downloads via SFTP an item from a SSH server.

### [Get-SFTPLocation](Get-SFTPLocation.md)
Get the current working location for a SFTP connection.

### [Get-SFTPPathAttribute](Get-SFTPPathAttribute.md)
Get the attributes for a specified path in a SFTP session.

### [Get-SFTPSession](Get-SFTPSession.md)
Get current SFTP Sessions that are available for interaction.

### [Get-SSHHostKey](Get-SSHHostKey.md)
Returns host key record

### [Get-SSHJsonKnowHost](Get-SSHJsonKnowHost.md)
Get known hosts stored in a JSON file created by Posh-SSH.
If a file is not specified it will default to $HOME\.poshssh\hosts.json.
If the file specified is not present it will be created.

### [Get-SSHOpenSSHKnownHost](Get-SSHOpenSSHKnownHost.md)
Get known_hosts stored in a OpenSSH file.
If a file is not specified it will default to $HOME\.ssh\known_hosts.
If the file specified is not present it will be created.

### [Get-SSHPortForward](Get-SSHPortForward.md)
Get a list of forwarded TCP Ports for a SSH Session

### [Get-SSHRegistryKnownHost](Get-SSHRegistryKnownHost.md)
Get KnownHosts from registry (readonly)

### [Get-SSHSession](Get-SSHSession.md)
Get current SSH Session that are available for interaction.

### [Get-SSHTrustedHost](Get-SSHTrustedHost.md)


### [Invoke-SSHCommand](Invoke-SSHCommand.md)
Executes a given command on a remote SSH host.

### [Invoke-SSHCommandStream](Invoke-SSHCommandStream.md)


### [Invoke-SSHStreamExpectAction](Invoke-SSHStreamExpectAction.md)
Executes an action on a SSH ShellStream when output matches a desired string.

### [Invoke-SSHStreamExpectSecureAction](Invoke-SSHStreamExpectSecureAction.md)
Executes an action stored in a SecureString on a SSH ShellStream when output matches a desired string.

### [Invoke-SSHStreamShellCommand](Invoke-SSHStreamShellCommand.md)


### [Move-SFTPItem](Move-SFTPItem.md)
Move or rename a specified item in a SFTP session.

### [New-SFTPFileStream](New-SFTPFileStream.md)
Create a IO Stream over SFTP for a file on a remote host.

### [New-SFTPItem](New-SFTPItem.md)
Create a file or directory on remote host using SFTP.

### [New-SFTPSession](New-SFTPSession.md)
Creates an SSH Session against a SSH Server

### [New-SFTPSymlink](New-SFTPSymlink.md)
Create a Symbolic Link on the remote host via SFTP.

### [New-SSHDynamicPortForward](New-SSHDynamicPortForward.md)
Establishes a Dynamic Port Forward thru a stablished SSH Session.

### [New-SSHLocalPortForward](New-SSHLocalPortForward.md)
Redirects traffic from a local port to a remote host and port via a SSH Session.

### [New-SSHMemoryKnownHost](New-SSHMemoryKnownHost.md)
Creates a new in-memory known host IStore for temporary use when creating new SSH and SFTP Sessions.

### [New-SSHRemotePortForward](New-SSHRemotePortForward.md)
Port forward a local port as a port on a remote server.

### [New-SSHSession](New-SSHSession.md)
Creates an SSH Session against a SSH Server.
By default it will store known host fingerprints in $HOME\.poshss\hosts.json.

### [New-SSHShellStream](New-SSHShellStream.md)
Creates a SSH shell stream for a given SSH Session.

### [New-SSHTrustedHost](New-SSHTrustedHost.md)


### [New-SSHTrustedHost](New-SSHTrustedHost.md)


### [Remove-SFTPItem](Remove-SFTPItem.md)
Deletes the specified item on a SFTP session.

### [Remove-SFTPSession](Remove-SFTPSession.md)
Close and Remove a SFTP Session

### [Remove-SSHSession](Remove-SSHSession.md)
Removes and Closes an existing SSH Session.

### [Remove-SSHTrustedHost](Remove-SSHTrustedHost.md)


### [Rename-SFTPFile](Rename-SFTPFile.md)
Move or Rename remote file via a SFTP Session

### [Set-SCPItem](Set-SCPItem.md)
Upload an item, either file or directory to a remote system via SCP.

### [Set-SFTPContent](Set-SFTPContent.md)


### [Set-SFTPItem](Set-SFTPItem.md)
Upload a specific item to a remote server though a SFTP Session.

### [Set-SFTPLocation](Set-SFTPLocation.md)
Change current location of the SFTP session.

### [Set-SFTPPathAttribute](Set-SFTPPathAttribute.md)
Sets one or more attributes on a specified item to a remote server though a SFTP Session.

### [Start-SSHPortForward](Start-SSHPortForward.md)
Start a configured port forward configured for a SSH Session

### [Stop-SSHPortForward](Stop-SSHPortForward.md)
Stops a configured port forward configured for a SSH Session

### [Test-SFTPPath](Test-SFTPPath.md)
Test if a File or Directory exists on Remote Server via SFTP

