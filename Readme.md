# Description
Powershell module that leverages a custom version of the SSH.NET Library (http://sshnet.codeplex.com/) to provide basic SSH functionality in Powershell.
The main purpose of the module is to facilitate automating actions against one or multiple SSH enabled servers.

# Install
To install the module including all source code you can just run in a PowerShell v3 the following command:
<pre>
iex (New-Object Net.WebClient).DownloadString("https://gist.github.com/darkoperator/6152630/raw/c67de4f7cd780ba367cccbc2593f38d18ce6df89/instposhsshdev")
</pre>

# Support

* Provides functionality for automating SSH, SFTP and SCP actions.
* Supports diffie-hellman-group-exchange-sha256, diffie-hellman-group-exchange-sha1, diffie-hellman-group14-sha1 and diffie-hellman-group1-sha1 key exchange methods.
* Supports 3des-cbc, aes128-cbc, aes192-cbc, aes256-cbc, aes128-ctr, aes192-ctr, aes256-ctr, blowfish-cbc, cast128-cbc, arcfour and twofish encryptions.
* Supports hmac-md5, hmac-sha1, hmac-ripemd160, hmac-sha2-256, hmac-sha2-256-96, hmac-md5-96 and hmac-sha1-96 hashing algorithms.
* Supports publickey, password and keyboard-interactive authentication methods
* Supports RSA and DSA private key
* Supports DES-EDE3-CBC, DES-EDE3-CFB, DES-CBC, AES-128-CBC, AES-192-CBC and AES-256-CBC algorithms for private key encryption
* Supports SOCKS4, SOCKS5 and HTTP Proxy
* Remote, dynamic and local port forwarding

# ChangeLog
## Version 1.7.8
* Fixed typo on variable for colums when creating a shell stream.
* New cmdlets for PortForwarding
  * Get-SSHPortForward
  * New-SSHDynamicPortForward
  * New-SSHLocalPortForward
  * New-SSHRemotePortForward
  * Start-SSHPortForward
  * Stop-SSHPortForward

## Version 1.7.7
* Fixed typo on parameter set name for NoProgress parameter in Get-SFTPFile.

## Version 1.7.6

* Fixed problem where wrong help file was shipped.
* Fixed problem where host comparison was not case insensitive for server key fingerprint.
* 
## Version 1.7.5

* New-SFTPItem can now create sub directories in a path if they do not exist when -Recurse parameter is used.
* New -Force parameter on New-SSHSession, New-SFTPSession, Get-SCPFolder, Set-SCPFolder, Get-SCPFile and Set-SCPFile that will disable any host key checking.
* Better warning on Remove-SFTPItem when it is not an empty directory.
* New function Set-SFTPPathAttribute for setting SFTP Path Attribute.
* PSCredential attributte added to all cmdlets and functions that take credentials.
* Added NoProgress parameter to SCP and SFTP cmdlets

## Version 1.7.4
* Fixed index problem for sessions when adding and removing them. Thanks to BornToBeRoot for the PR.
* Added a recursive option to the Get-SFTPChildItem function. Thanks to aaroneuph for the PR.

## Version 1.7.3

* Made some of the SFTP cmdlets will now honor the erroraction variable with the exception of a problem during transfer where a terminating error will be raised and should be handled in a Try{}Catch{} block.
* Will pass the domain during logon for those cases where SSH server is connected to an AD infrastructure.

## Version 1.7.2
* Fix problem with Get-SFTPFile cmdlet. It was creating a empty file before checking if a file existed causing error or blanking a exiting file accidentally.
- Add session and session id properties to a generated streem to address request in issue #34

## Version 1.7.1
* Fix typo in trust submodule.

## Version 1.7
* **New-SFTPDirectory** is replaced by **New-SFTPItem** to match how PowerShell refers to files and directories.
* **Remove-SFTPFile** and **Remove-SFTPDirectory** are replaced by **Remove-SFTPItem** to match how PowerShell refers to files and directories.
* **Set-SFTPDirectoryPath** is replaced by **Set-SFTPLocation** to match how PowerShell refers to files and directories.
* **Get-SFTPCurrentWorkingDirectory** is replaced by **Get-SFTPLocation** to match how PowerShell refers to files and directories.
* **Get-SFTPDirectoryList** is replaced by **Get-SFTPChildItem** to match how PowerShell refers to files and directories.
* **Index** Parameter and Property are now **SessionId**. All cmdlets and function have Index as an Alias so as to not break existing scripts.
* **On Set-SCPFile the parameter RemoteFile is now changed to RemotePath and one only needs to give the Path to where to copy the file.**
* **On Set-SCPFolder the parameter RemoteFile is now changed to RemotePath and one only needs to give the Path to where to copy the folde.**
* **On New-SFTPSession, New-SSHSession, Set-SCPFile and Set-SCPFolder the AcceptKey parameter is now a switch.**
* New function **New-SSHShellStream** for easier creation of shell stream objects.

```PowerShell
C:\PS>$SSHStream = New-SSHShellStream -Index 0
PS C:\> $SSHStream.WriteLine("uname -a")
PS C:\> $SSHStream.read()
Last login: Sat Mar 14 20:02:16 2015 from infidel01.darkoperator.com
[admin@localhost ~]$ uname -a
Linux localhost.localdomain 3.10.0-123.el7.x86_64 #1 SMP Mon Jun 30 12:09:22 UTC 2014 x86_64
x86_64 x86_64 GNU/Linux
[admin@localhost ~]$
```

* New function **Invoke-SSHStreamExpectSecureAction **for passing passwords to prompt on a shell stream.

```PowerShell
C:\PS>Invoke-SSHStreamExpectSecureAction -ShellStream $stream -Command 'su -' -ExpectString 'Password:' -SecureAction (read-host -AsSecureString) -Verbose

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

* New function **Invoke-SSHStreamExpectAction** for executing expect actions on a shell stream.
* New function **Get-SFTPPathAttribute** to get attributes of a given path. 

```PowerShell
C:\PS>Get-SFTPPathAttribute -SessionId 0 -Path "/tmp"

 LastAccessTime    : 2/27/2015 6:38:43 PM
 LastWriteTime     : 2/27/2015 7:01:01 PM
 Size              : 512
 UserId            : 0
 GroupId           : 0
 IsSocket          : False
 IsSymbolicLink    : False
 IsRegularFile     : False
 IsBlockDevice     : False
 IsDirectory       : True
 IsCharacterDevice : False
 IsNamedPipe       : False
 OwnerCanRead      : True
 OwnerCanWrite     : True
 OwnerCanExecute   : True
 GroupCanRead      : True
 GroupCanWrite     : True
 GroupCanExecute   : True
 OthersCanRead     : True
 OthersCanWrite    : True
 OthersCanExecute  : True
 Extensions        :
```
* New function **New-SFTPFileStream** to create a IO Stream of a file on a host via SFTP.

```PowerShell
PS C:\> $bashhistory = New-SFTPFileStream -SessionId 0 -Path /home/admin/.bash_history -FileMode Open -FileAccess Read
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
* New function **New-SFTPSymlink** to create symbolic link on a a remote host via SFTP.
* New function **Set-SFTPContent** to get the content of a file on a remote host via SFTP.

```PowerShell
PS C:\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "My example message`n"

FullName       : /tmp/example.txt
LastAccessTime : 3/16/2015 10:40:16 PM
LastWriteTime  : 3/16/2015 10:40:55 PM
Length         : 22
UserId         : 1000



PS C:\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt
My example message

PS C:\> Set-SFTPContent -SessionId 0 -Path /tmp/example.txt -Value "New message`n" -Append


FullName       : /tmp/example.txt
LastAccessTime : 3/16/2015 10:40:59 PM
LastWriteTime  : 3/16/2015 10:41:18 PM
Length         : 34
UserId         : 1000



PS C:\> Get-SFTPContent -SessionId 0 -Path /tmp/example.txt
My example message
New message
```
* New function **Get-SFTPContent** to set the content of a file on a remote host via SFTP.


```PowerShell
PS C:\> Get-SFTPContent -SessionId 0 -Path  /etc/system-release
CentOS Linux release 7.0.1406 (Core)
```
* Added support for ssh.com (SSH-2) private keys.
* Added support on acceptable group of up to 8192 bits for SHA-1 and SHA-256 Diffie-Hellman Group and Key Exchange
* Several fixes when connecting though a proxy.
* SCP Speed is now almost 3 times faster.
* SFTP cmdlets for upload and download now show progress and are written in C#.
* All cmdlet return ErrorRecords.
* SFTP functions verify that the path given on the remote host exist and that it is a directory.
* SFTP functions verify that the file given on the remote host exits and that it is a file.
* When uploading files via SFTP overwriting of the target file is now optional.
* Address issue when progress message could get stuck in the PowerShell window after upload or download of a files was finished.
* Fix problem when using key files and connecting to alternate SSH port numbers, the port number was being ignored.
* Fix registry access problem when setting trusted host.
* Fix problem when enumerating trusted hosts and the registry key for them was not present.
* SCP, SFTP Session and SSH Session cmdlets when verbose messages are selected will show the SSH certificate fingerprint of the host one is connecting with. 
* Disabled Zlib Compression.
* Fix ShellStream.ReadLine produces incorrect output when reading multi-byte characters.
* Fix ScpClient: Missing files when using DirectoryUpload.
* Fix SendKeepAlive causes SocketException when connection is dropped.
* Fix stuck loop on key exchange using arcfour encryption 
* Reduced default buffer size for SftpClient from 64 KB to 32 KB as some SSH servers apply a hard limit of 64 KB at the transport level.
* Optimization of payload size for both read and write operations (SftpClient only)
* Increase window size from 1MB to 2MB
* Increase buffer size from 16KB to 64KB for SftpClient
* Take into account the maximum remote packet size of the channel for write operations
* Increase maximum size of packets that we can receive from 32 KB to 64 KB

 

## Version 1.6
* Fixed problem with ProxyServer option.

## Version 1.5
* Supports PowerShell 2.0 by popular demand.
* Refactored all C Sharp code to comply with naming guidelines and best practices.
* Fixed several bugs the main one being the not allowing use of alternate SSH port.

## Version 1.4
* Disabled PorForward commands because of bug in library.
* Fix upload and download speed issues in SFTP and SCP.

## Version 1.3
* Option to auto accept SSH Fingerprint (Don't personally like it but gotten enough requests to make me do it)
* Set index to default parameter set.
* Added keep alive for connections.
* Enabled Dynamic Port Forward function.
* Help XML file now properly shows parameter sets.
* Fixed several typos.

## Version 1.2
* Added support for zlib compression.
* Disabbled Dynamic Port Forward function, there seems to be problems with the library.

## Version 1.1
* Added functions for managing SSH Trusted Host list.
* SCP, SSH Session and SFTP Session cmdlets now verify the SSH Host Fingerprint.
* Complete refactor of the cmdlets for SSH Session, SFTP Session and SCP.
* Added Download and Upload Progress to SCP cmdlets.
* Patched the Renci SSH .Net library to correct problems when uploading using SCP.

# Know Issues
* No Compression support at the moment.
* Once a Download or Upload is started only way to cancel is by closing PowerShell.
* Port Forwarding support in the library used is still not good enough to enable functions. 

