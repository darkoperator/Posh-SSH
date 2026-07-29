# ChangeLog

## Version 4.0.0-beta3

### Diagnostics

* **New cmdlet `Get-SSHAlgorithm`.** Reports the key exchange, host key, encryption, MAC and compression algorithms the bundled SSH.NET library supports, along with the library version. Given `-ComputerName` it also reads the algorithms the remote host advertises and reports, per category, what the two sides have in common — so `Get-SSHAlgorithm -ComputerName host | Where-Object { -not $_.HasCommon }` names the category responsible for a failed negotiation.
  * Requires no credentials. A server advertises its algorithms before authentication, so the probe works against hosts you have no account on. No key exchange is performed, nothing is authenticated, and nothing is written to the trusted host store.
  * `Common` is ordered by client preference, so its first entry is the algorithm that would actually be negotiated (RFC 4253 section 7.1).
  * A category is reported once as `Direction = Both` when the server offers the same list in each direction, and twice (`ClientToServer`, `ServerToClient`) when the lists differ.
* Algorithm negotiation failures from the session cmdlets now attach the client-supported list for the failing category and point at `Get-SSHAlgorithm`. This is applied through `ErrorRecord.ErrorDetails`, so the exception type, message and `FullyQualifiedErrorId` are unchanged and existing error handling is unaffected. Addresses the diagnosis problem behind #632.

### Bug fixes

* **`Get-SFTPItem` could not download to an absolute Windows path.** The replacement of `*` and `:` with `_`, added in beta2 so remote names containing Windows-illegal characters could be written, was applied to the whole combined destination path rather than just the file name. That rewrote the drive letter in `C:\folder` to `C_\folder`, turning an absolute path into a relative one and writing the file somewhere under the current directory, or failing outright. Only the remote file name is sanitized now.
* `Remove-SSHTrustedHost` no longer prompts by default. It declared `ConfirmImpact.High`, which meant every call raised a confirmation, and in a non-interactive session (a script, CI, or `pwsh -File`) that surfaced as an opaque `NullReferenceException` unless `-Confirm:$false` was passed. Impact is now `Medium`; `-Confirm` and `-WhatIf` still work.
* **`Move-SFTPItem` could not move an item into a directory.** `-Destination` was always treated as the target file path, so an existing destination directory made `-Force` call `DeleteFile` on the directory (which the server rejects), and without `-Force` it reported the directory as an existing file. When the destination is an existing directory the item's name is now appended to it; passing a file path still renames as before, and a destination that resolves to a directory is reported clearly instead of being deleted.
* A rejected host key now explains itself. The error reported only "Host key could not be verified", saying nothing about what the server presented or what was expected. It now names the host key type and fingerprint offered, lists the fingerprints already recorded for that host, states that `-AcceptKey` deliberately does not override a recorded key, and recommends the command to run once the change has been verified out of band. Delivered through `ErrorRecord.ErrorDetails`, so the exception itself is unchanged.

### Build

* `Build-Module.ps1` now copies `Renci.SshNet.dll` from the restore output into `Posh-SSH/Assembly/`, making the csproj `PackageReference` the single source of truth for the bundled library.
* The build fails if the bundled `Renci.SshNet.dll` does not match the version `PoshSSH.dll` was compiled against. The manifest loads that assembly from disk via `RequiredAssemblies`, so the two could previously drift apart unnoticed — which is what produced the 3.2.6 and 3.2.7 mismatches.

### Testing

The whole suite now requires **Pester 5 or later**, and passes with nothing skipped.

* `Get-SSHSession.Tests.ps1` and `Remove-SSHSession.Tests.ps1` rewritten. They used Pester 3/4 syntax and failed outright under Pester 5, and they imported the module by a path only valid from inside the module directory — so from the repository root the import silently failed and the commands under test resolved to whatever Posh-SSH was already installed. The import is now anchored to `$PSScriptRoot`. Coverage was extended to selection by id, by the `Index` alias, by wildcard and exact host name, several ids at once, and removing one session among many.
* New `Get-SSHAlgorithm.Tests.ps1`, plus `tests/Fixtures/FakeSshServer.ps1`, a loopback server that serves a single crafted `SSH_MSG_KEXINIT`. This tests algorithm comparison, the empty-intersection case behind #632, the per-direction split, and the probe's handling of malformed input without needing a real SSH server.
* The integration suite assigned sessions to `$script:` variables inside `It` blocks and read them from later contexts, which Pester 5 does not guarantee. A single failed connection therefore cascaded into dozens of failures pointing at SFTP and port forwarding rather than at the connection. Sessions are established in `BeforeAll` now, and `tests/README.md` records the rule for anyone adding tests.
* The integration suite gained an Algorithm Discovery section, including a check that the overlap `Get-SSHAlgorithm` reports contains the algorithms an established session actually negotiated.
* "Should move file to test directory" is no longer skipped. Its comment attributed the failure to server-specific behaviour; it was the `Move-SFTPItem` bug fixed above.

## Version 4.0.0-beta2

Major release built on community contributions, especially from @MVKozlov for the multi-key trusted host work and the SSH.NET 2025 migration. The "known host" terminology is retired in favour of "trusted host store", reflecting the cleaner abstraction that now backs all three storage backends.

### Library

* Upgraded SSH.NET from 2024.0.0 to 2025.1.0. The forked `Renci.SshNetDev.dll` (Cisco-device patch from https://github.com/sshnet/SSH.NET/pull/972) is no longer required and has been dropped.
* New runtime dependency `BouncyCastle.Cryptography.dll` ships in `Assembly/` (transitive from SSH.NET 2025).

### Trusted host store (was: known host)

* Renamed all `*KnownHost*` cmdlets and types to `*TrustedHostStore*` / `*TrustedHost*`.
* Multiple host keys per host are now supported — a single hostname can hold multiple key fingerprints (key rotation, multiple algorithms).
* `MemoryTrustedHostStore` is now the base class for `JsonTrustedHostStore` and `OpenSSHTrustedHostStore`; the persistent stores override only the `OnKeyUpdated()` hook. Eliminates the triplicated CRUD logic from v3.x.
* Default host-key display switched from MD5 to SHA256.
* `~/.poshssh/hosts.json` schema changed for multi-key support and is **not backward compatible** with v3.x. Back up your `hosts.json` before upgrading.

### New cmdlets (binary / C#)

* `New-SSHTrustedHost`, `Add-SSHTrustedHost`, `Get-SSHTrustedHost`, `Remove-SSHTrustedHost` — were PowerShell functions in v3.x, now C# binary cmdlets.

### Renamed cmdlets

* `New-SSHMemoryKnownHost` → `New-SSHMemoryTrustedHostStore`
* `Get-SSHJsonKnownHost` → `Get-SSHJsonTrustedHostStore`
* `Get-SSHOpenSSHKnownHost` → `Get-SSHOpenSSHTrustedHostStore`
* `Get-SSHRegistryKnownHost` → `Get-SSHRegistryTrustedHostStore`
* `Convert-SSHRegistryToJsonKnownHost` → `Convert-SSHRegistryToJSonTrustedHost`

### Authentication and connection

* Support for multiple authentication methods on a single session (backward-compatible) — a password and a key can be supplied together and SSH.NET will try each.
* New `-Encoding` parameter on session and command cmdlets so non-ASCII output is decoded correctly.

### SCP and host key handling

* **New `-Overwrite` switch on `Get-SCPItem`.** Overwriting an existing destination file no longer requires `-Force`. Previously `-Force` was overloaded to mean both "do not verify the remote host fingerprint" and "clobber the destination", so anyone scripting a download that replaces a local file was forced to disable host key verification.
* `-Force` on `Get-SCPItem` and `Set-SCPItem` now documents as host key verification only. On `Get-SCPItem` it is still accepted as an overwrite gate for backward compatibility, including the long-standing quirk where `-Force:$false` permits overwriting while leaving verification switched on. No existing invocation changes behaviour.
* `Set-SCPItem` deliberately gains no `-Overwrite` switch: SCP uploads always replace the remote file, and SSH.NET exposes no pre-existence check on an `ScpClient`.
* Fix #633 — the overwrite notice on `Get-SCPItem` is now emitted with `WriteVerbose` instead of `WriteWarning`, and is spelled "Overwriting". Explicitly asking to overwrite is not a warning condition. The non-terminating error raised when the destination exists and neither switch was supplied is unchanged.
* Fix #582 — `Get-SCPItem -PathType File` no longer deletes the destination before the download starts. The transfer now lands in a temporary `.partial` file alongside the destination and is moved over it (`File.Replace`, falling back to delete-and-move on file systems that do not support it) only once the download succeeds; the temporary file is cleaned up on failure. A missing or unreadable remote source previously left the caller with neither file.
* Fix #174 — the host key warning now names the host: `Host key for <computer> is not being verified since the Force switch was used.` It is emitted from `NewSessionBase`, so the wording is now identical across every cmdlet that accepts `-Force` (`New-SSHSession`, `New-SFTPSession`, `Get-/Set-SCPItem`, `Get-/Set-SFTPItem`, `Get-SSHHostKey`). Scripts matching on the old warning text will need updating.

### Bug fixes

* Fix #604 and #533 — command/operation timeout behaviour in `Invoke-SSHCommand`.
* Fix #496 and #381 — long-standing edge cases.
* Wildcard `*` and `:` characters in remote filenames are now replaced with `_` so SCP/SFTP file operations don't fail on Windows-incompatible names.

### Project structure

* Removed the legacy .NET Framework 4.7.2 `PoshSSH.csproj` and the dual-solution layout. A single SDK-style `PoshSSH.Core/PoshSSH.Core.csproj` targets `netstandard2.0` and ships cross-platform (Windows PowerShell 5.1, PowerShell 7.x on Windows/Linux/macOS).
* Source files moved directly into `Source/PoshSSH/PoshSSH.Core/` — no more `<Compile Link>` indirection from a legacy project.
* Deleted dead `Get-/Set-SCPFile`, `Get-/Set-SCPFolder`, `Get-/Set-SFTPFile`, `Set-SFTPFolder` source files (already unexported in v3.x).
* Removed `bin/` build artifacts from version control.

### Build and test tooling

* `Build-Module.ps1` automates the release pipeline: `dotnet build`, manifest-driven cmdlet verification in a fresh shell, validation that every `RequiredAssemblies` and `FileList` entry exists on disk, and `Posh-SSH-{version}.zip` packaging with SHA256.
* `tests/Posh-SSH.Integration.Tests.ps1` plus `tests/Run-IntegrationTests.ps1` — full Pester integration suite covering password auth, key auth, encrypted key auth, SFTP file operations, SCP up/download, port forwarding, and session cleanup.
* `tests/Setup-LinuxTestVm.sh` — root-runnable Linux VM provisioning script that creates 12 SSH test accounts covering the full auth matrix: password, RSA 2048/4096, RSA with passphrase, RSA PKCS#1 PEM, Ed25519 ±passphrase, ECDSA P-256/P-384/P-521, multi-factor `AuthenticationMethods publickey,password`, and forced keyboard-interactive.
* `tests/README.md` documents the suite.

## Version 3.2.7

* Fixed assembly version mismatch - corrected distribution to include compatible Renci.SshNet.dll version (2024.0.0.0) matching the compiled PoshSSH.dll binary.

## Version 3.1.1

Module now uses the 2023 version of the SSH.Net library. This library provides now:

* Support for RSA-SHA256/512 signature algorithms
* Support for parsing OpenSSH keys with ECDSA 256/384/521 and RSA
* Added async support to SftpClient and SftpFileStream
* Added ISftpFile interface to SftpFile
* Improved performance and stability
* Added the ability to set the last write and access time for Sftp file

## Version 3.0.7

* New command `Get-SSHHostKey` for getting a host SSH key fingerprint. 
* Forked copy of SSH.Net with patch https://github.com/sshnet/SSH.NET/pull/972 to allow connection to some Cisco devices.

## Version 3.0

This release is possible thanks to @pcatrobrouillet, @soynerdito and specially @MVKozlov for all the fixes and improvements, would have not been possible without his contributions (Thanks you all)

* Windows PowerShell 5.1 and PowerShell 7.x Only supported.
* Cross platform support (Linux, Windows and MacOS).
* Updated SSH.NET library fixing multiple issued with OpenSSH and Cisco version strings.
* Support for more key formats. (OpenSSH ECDSA still not supported in the OpenSSH format)
* Support for password: and PASSCODE: prompts.
* Fix for Get-SFTPItem.
* No more registry used to store known hosts, uses by default a hosts.json file in $HOME\\.poshssh\hosts.json.
* Additional support for known hosts using ISotre method using a JSON KnownHosts, OpenSSH KnownHost and Memory KnownHosts in memory stores. 
* Set-SFTPFile and Set-SFTPFolder cmdlets removed (use Set-SFTPItem instead).
* Set-SCPFile and Set-SCPFolder cmdlets removed (use Set-SCPItem instead).
* Get-SCPFile and Get-SCPFolder cmdlets removed (use Get-SCPItem instead).
* Get-SFTPFile cmdlet removed (use Get-SFTPItem instead).
* Functions for migrating known hosts from registry to JSON added (Convert-SSHRegistryToJSonKnownHostStore, Get-SSHRegistryKnownHostStore). 
* Refactor to avoid using the $HOST variable
* Now new progress bar obey $ProgressPreference, so switch is not used. -NoProgress param removed.
* Fixed PowerCLI Co-existence issues. 
* RemotePathTransformation for SCPItem noun parameters with the new parameter -PathTransformation.



## Version 2.1

* Fixed issue where help was not loading.
* Fixed typo in Set-SFTPPathAttribute command when setting GroupCanChange.
* Removed use of the variable $output for command execution due to scope issues.
* New cmdlet Set-SFTPItem, in the next release it will replace Set-SFTPFile and Set-SFTPFolder cmdlets.
* New cmdlet Get-SFTPItem, in the next release it will replace Get-SFTPFile and Get-SFTPFolder cmdlets.
* Fix NullReferenceException when using proxy credentials.
* Added function Move-SFTPItem, this is will replace Rename-SFTPFile and will also add the functionality to move any item in a SFTP session.

## Version 2.0.2

* Set-SFTPContent will no longer add a BOM to UTF8 encoded files.
* Fixed issue with path resolution in Get-SCPFile.
* Fixed typo in New-SFTPSymlink.

## Version 2.0.1

* Get-SFTPItem was not displaying folders when recursively listing.
* Fixed issue when deleting none empty folders.

## Version 2.0

* Windows PowerShell 2.0 has been deprecated by Microsoft and several major versions have been released after it, for this reason PowerShell 2.0 is no longer supported by the module.
* New-SSHSession, New-SFTPSession, Set-SCPFile and Set-SCPFolder support the KeyString parameter, a string array of the content of a OpenSSH key for authentication.
* For Azure users when Force parameter is used it will not look in to the registry for exiting keys to validate against. Useful when ran under an account that is not a user.
* Set-SCPFile Better pipeline support when receiving objects from Get-Childitem.
* Set-SFTPFile LocalFile Parameter accepts a string[].

## Version 1.7.7

* Fixed typo on parameter set name for NoProgress parameter in Get-SFTPFile.

## Version 1.7.6

* Fixed problem where wrong help file was shipped.
* Fixed problem where host comparison was not case insensitive for server key fingerprint.

## Version 1.7.5

* New-SFTPItem can now create sub directories in a path if they do not exist when -Recurse parameter is used.
* New -Force parameter on New-SSHSession, New-SFTPSession, Get-SCPFolder, Set-SCPFolder, Get-SCPFile and Set-SCPFile that will disable any host key checking.
* Better warning on Remove-SFTPItem when it is not an empty directory.
* New function Set-SFTPPathAttribute for setting SFTP Path Attribute.
* PSCredential attribute added to all cmdlets and functions that take credentials.
* Added NoProgress parameter to SCP and SFTP cmdlets

## Version 1.7.4

* Fixed index problem for sessions when adding and removing them. Thanks to BornToBeRoot for the PR.
* Added a recursive option to the Get-SFTPChildItem function. Thanks to aaroneuph for the PR.

## Version 1.7.3

* Made some of the SFTP cmdlets will now honor the ErrorAction variable with the exception of a problem during transfer where a terminating error will be raised and should be handled in a Try{}Catch{} block.
* Will pass the domain during logon for those cases where SSH server is connected to an AD infrastructure.

## Version 1.7.2

* Fix problem with Get-SFTPFile cmdlet. It was creating a empty file before checking if a file existed causing error or blanking a exiting file accidentally.
* Add session and session id properties to a generated stream to address request in issue #34

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
