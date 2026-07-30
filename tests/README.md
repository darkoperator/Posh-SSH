# Posh-SSH Tests

This directory contains tests for the Posh-SSH module.

## Test Types

### Unit Tests
- `Get-SSHSession.Tests.ps1` - Unit tests for Get-SSHSession function
- `Remove-SSHSession.Tests.ps1` - Unit tests for Remove-SSHSession function
- `Get-SSHAlgorithm.Tests.ps1` - Unit tests for Get-SSHAlgorithm, requires Pester 5 or later
- `HostKeyMatcher.Tests.ps1` - Unit tests for the host key trust rule (`SSH.HostKeyMatcher`),
  covering RFC 8332 RSA algorithm equivalence and, importantly, the cases that must be refused

### Integration Tests
- `Posh-SSH.Integration.Tests.ps1` - Comprehensive integration tests against a live SSH server

### Fixtures
- `Fixtures/FakeSshServer.ps1` - A minimal loopback server that serves a single crafted
  `SSH_MSG_KEXINIT`. Used by `Get-SSHAlgorithm.Tests.ps1` to test algorithm comparison, the
  directional split, and the probe's handling of malformed input without needing a real SSH
  server. It never implements key exchange or authentication.

All test files require **Pester 5 or later**.

**Writing tests for this suite**: establish shared state such as sessions in `BeforeAll`, not
inside an `It`. Pester 5 and later only guarantee that variables set in `BeforeAll` reach the
`It` blocks of the same and nested containers. Assigning a session inside an `It` and reading
it from a later context leaves it null, which turns a single failed connection into dozens of
unrelated-looking failures.

## Running Unit Tests

Unit tests use Pester and can be run without a live SSH server:

```powershell
# Run all unit tests
Invoke-Pester .\tests

# Run specific test file
Invoke-Pester .\tests\Get-SSHSession.Tests.ps1
```

## Running Integration Tests

Integration tests require a live SSH server with either password or SSH key authentication enabled.

### Prerequisites

1. A running SSH server (Linux/Unix system)
2. Valid credentials (username/password OR username/SSH key)
3. Write permissions to `/tmp` directory on the remote server
4. Pester module installed: `Install-Module -Name Pester -Force`

### Password Authentication

```powershell
# Basic password authentication
$password = ConvertTo-SecureString "YourPassword" -AsPlainText -Force
.\tests\Posh-SSH.Integration.Tests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -Password $password

# With custom port
.\tests\Posh-SSH.Integration.Tests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -Password $password -Port 2222
```

### SSH Key Authentication

```powershell
# With unencrypted private key
.\tests\Posh-SSH.Integration.Tests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -KeyPath "C:\Users\test\.ssh\id_rsa"

# With encrypted private key
$keyPassPhrase = ConvertTo-SecureString "KeyPassPhrase" -AsPlainText -Force
.\tests\Posh-SSH.Integration.Tests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -KeyPath "C:\Users\test\.ssh\id_rsa" -KeyPassPhrase $keyPassPhrase
```

### Using the Helper Script

A helper script is provided to simplify running integration tests:

```powershell
# Password authentication
.\tests\Run-IntegrationTests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -UsePassword

# Key authentication
.\tests\Run-IntegrationTests.ps1 -ComputerName "192.168.1.100" -UserName "testuser" -UseKey -KeyPath "C:\Users\test\.ssh\id_rsa"
```

## What the Integration Tests Cover

The integration test suite validates the following functionality:

### SSH Operations
- Session creation and management (New-SSHSession, Get-SSHSession, Remove-SSHSession)
- Command execution (Invoke-SSHCommand)
- Shell stream operations (New-SSHShellStream, Invoke-SSHStreamShellCommand)
- Port forwarding (New-SSHLocalPortForward, Start-SSHPortForward, Stop-SSHPortForward)

### SFTP Operations
- Session creation and management (New-SFTPSession, Get-SFTPSession, Remove-SFTPSession)
- Directory operations (Set-SFTPLocation, Get-SFTPLocation, Test-SFTPPath)
- File upload (Set-SFTPItem)
- File download (Get-SFTPItem)
- File manipulation (Rename-SFTPFile, Move-SFTPItem)
- Content operations (Get-SFTPContent, Set-SFTPContent)
- File/directory creation and deletion (New-SFTPItem, Remove-SFTPItem)
- Attribute retrieval (Get-SFTPPathAttribute)

### SCP Operations
- File upload (Set-SCPItem)
- File download (Get-SCPItem)

### Algorithm Discovery
- Reading the algorithms a live server offers (Get-SSHAlgorithm)
- Agreement between the reported overlap and what the live session negotiated
- Local library reporting with no host contacted

## Test Environment

The integration tests perform the following actions on the remote server:

1. Create test files in `/tmp` directory
2. Create a test directory: `/tmp/posh-ssh-test-dir`
3. Upload, download, rename, and move files
4. Clean up all created files and directories

**Note**: The tests clean up after themselves, but ensure you have permission to write to `/tmp` on the target server.

## Troubleshooting

### Connection Issues

If you encounter connection issues:
- Verify the SSH server is running: `ssh user@hostname`
- Check firewall rules allow SSH connections
- Verify credentials are correct
- For key authentication, ensure the public key is in `~/.ssh/authorized_keys` on the server

### Permission Issues

If you encounter permission errors:
- Verify the user has write permissions to `/tmp`
- Check SELinux/AppArmor policies if applicable
- Verify SSH server configuration allows the required operations

### Test Failures

If specific tests fail:
- Check the error message for details
- Verify the required commands exist on the target system (echo, whoami, pwd, etc.)
- Ensure the SSH server supports the features being tested (SFTP subsystem, port forwarding)
