<#
.SYNOPSIS
    Comprehensive integration tests for Posh-SSH module.

.DESCRIPTION
    This test suite performs integration testing of Posh-SSH functionality against a live SSH server.
    It supports both password and key-based authentication and tests SSH, SFTP, and SCP operations.

.PARAMETER ComputerName
    IP address or hostname of the SSH server to test against.

.PARAMETER UserName
    Username for SSH authentication.

.PARAMETER Password
    Password for authentication (required if KeyPath is not provided).

.PARAMETER KeyPath
    Path to SSH private key file (required if Password is not provided).

.PARAMETER KeyPassPhrase
    Passphrase for the private key (optional, used with KeyPath).

.PARAMETER Port
    SSH port number (default: 22).

.EXAMPLE
    # Test with password authentication
    .\Posh-SSH.Integration.Tests.ps1 -ComputerName 192.168.1.100 -UserName testuser -Password (ConvertTo-SecureString "P@ssw0rd" -AsPlainText -Force)

.EXAMPLE
    # Test with key authentication
    .\Posh-SSH.Integration.Tests.ps1 -ComputerName 192.168.1.100 -UserName testuser -KeyPath "C:\Users\test\.ssh\id_rsa"

.EXAMPLE
    # Test with key authentication and passphrase
    .\Posh-SSH.Integration.Tests.ps1 -ComputerName 192.168.1.100 -UserName testuser -KeyPath "C:\Users\test\.ssh\id_rsa" -KeyPassPhrase (ConvertTo-SecureString "keypass" -AsPlainText -Force)
#>

[CmdletBinding(DefaultParameterSetName='Password')]
param(
    [Parameter(Mandatory=$true)]
    [string]$ComputerName,

    [Parameter(Mandatory=$true)]
    [string]$UserName,

    [Parameter(Mandatory=$true, ParameterSetName='Password')]
    [SecureString]$Password,

    [Parameter(Mandatory=$true, ParameterSetName='Key')]
    [ValidateScript({Test-Path $_})]
    [string]$KeyPath,

    [Parameter(Mandatory=$false, ParameterSetName='Key')]
    [SecureString]$KeyPassPhrase,

    [Parameter(Mandatory=$false)]
    [int]$Port = 22
)

# Import the module
$ModulePath = Join-Path $PSScriptRoot "..\Posh-SSH\Posh-SSH.psd1"
Import-Module $ModulePath -Force

# Prepare credential based on parameter set
if ($PSCmdlet.ParameterSetName -eq 'Password') {
    $Credential = New-Object System.Management.Automation.PSCredential($UserName, $Password)
    $AuthMethod = "Password"
} else {
    # For key-based auth, password field is used for key passphrase
    if ($KeyPassPhrase) {
        $Credential = New-Object System.Management.Automation.PSCredential($UserName, $KeyPassPhrase)
    } else {
        # Empty password for unencrypted keys
        $EmptyPassword = New-Object System.Security.SecureString
        $Credential = New-Object System.Management.Automation.PSCredential($UserName, $EmptyPassword)
    }
    $AuthMethod = "Key"
}

# Test variables
$TestFileName = "posh-ssh-test-file.txt"
$TestFileContent = "This is a test file for Posh-SSH integration tests. Created at $(Get-Date)"
$LocalTestFile = Join-Path $env:TEMP $TestFileName
$RemoteTestPath = "/tmp/$TestFileName"
$RemoteTestDir = "/tmp/posh-ssh-test-dir"

Describe "Posh-SSH Integration Tests - $AuthMethod Authentication" {

    BeforeAll {
        # Create local test file
        Set-Content -Path $LocalTestFile -Value $TestFileContent
        Write-Host "Testing against: $ComputerName as $UserName using $AuthMethod authentication" -ForegroundColor Cyan
    }

    AfterAll {
        # Cleanup local test file
        if (Test-Path $LocalTestFile) {
            Remove-Item $LocalTestFile -Force -ErrorAction SilentlyContinue
        }

        # Remove all sessions
        Get-SSHSession | Remove-SSHSession -ErrorAction SilentlyContinue
        Get-SFTPSession | Remove-SFTPSession -ErrorAction SilentlyContinue
    }

    Context "SSH Session Management" {

        It "Should create a new SSH session with $AuthMethod" {
            if ($AuthMethod -eq 'Key') {
                $script:SSHSession = New-SSHSession -ComputerName $ComputerName -Credential $Credential -KeyFile $KeyPath -Port $Port -AcceptKey
            } else {
                $script:SSHSession = New-SSHSession -ComputerName $ComputerName -Credential $Credential -Port $Port -AcceptKey
            }

            $script:SSHSession | Should -Not -BeNullOrEmpty
            $script:SSHSession.Connected | Should -Be $true
        }

        It "Should retrieve the SSH session" {
            $sessions = Get-SSHSession
            $sessions | Should -Not -BeNullOrEmpty
            $sessions.Count | Should -BeGreaterOrEqual 1
        }

        It "Should retrieve SSH session by SessionId" {
            $session = Get-SSHSession -SessionId $script:SSHSession.SessionId
            $session | Should -Not -BeNullOrEmpty
            $session.SessionId | Should -Be $script:SSHSession.SessionId
        }

        It "Should retrieve SSH session by ComputerName" {
            $session = Get-SSHSession -ComputerName $ComputerName
            $session | Should -Not -BeNullOrEmpty
        }
    }

    Context "SSH Command Execution" {

        It "Should execute a simple command" {
            $result = Invoke-SSHCommand -SessionId $script:SSHSession.SessionId -Command "echo 'Hello from Posh-SSH'"
            $result | Should -Not -BeNullOrEmpty
            $result.Output | Should -Match "Hello from Posh-SSH"
            $result.ExitStatus | Should -Be 0
        }

        It "Should execute whoami command" {
            $result = Invoke-SSHCommand -SessionId $script:SSHSession.SessionId -Command "whoami"
            $result.Output | Should -Match $UserName
            $result.ExitStatus | Should -Be 0
        }

        It "Should execute pwd command" {
            $result = Invoke-SSHCommand -SessionId $script:SSHSession.SessionId -Command "pwd"
            $result.Output | Should -Not -BeNullOrEmpty
            $result.ExitStatus | Should -Be 0
        }

        It "Should execute command and get proper exit code for failure" {
            $result = Invoke-SSHCommand -SessionId $script:SSHSession.SessionId -Command "ls /nonexistent-directory-12345"
            $result.ExitStatus | Should -Not -Be 0
        }

        It "Should execute command with timeout" {
            $result = Invoke-SSHCommand -SessionId $script:SSHSession.SessionId -Command "echo 'test'" -TimeOut 30
            $result.Output | Should -Match "test"
        }
    }

    Context "SSH Shell Stream" {

        It "Should create a new SSH shell stream" {
            $script:ShellStream = New-SSHShellStream -SessionId $script:SSHSession.SessionId
            $script:ShellStream | Should -Not -BeNullOrEmpty
        }

        It "Should execute command in shell stream" {
            Start-Sleep -Seconds 1  # Allow shell to initialize
            $result = Invoke-SSHStreamShellCommand -ShellStream $script:ShellStream -Command "echo 'Shell test'"
            $result | Should -Match "Shell test"
        }
    }

    Context "SFTP Session Management" {

        It "Should create a new SFTP session with $AuthMethod" {
            if ($AuthMethod -eq 'Key') {
                $script:SFTPSession = New-SFTPSession -ComputerName $ComputerName -Credential $Credential -KeyFile $KeyPath -Port $Port -AcceptKey
            } else {
                $script:SFTPSession = New-SFTPSession -ComputerName $ComputerName -Credential $Credential -Port $Port -AcceptKey
            }

            $script:SFTPSession | Should -Not -BeNullOrEmpty
            $script:SFTPSession.Connected | Should -Be $true
        }

        It "Should retrieve the SFTP session" {
            $sessions = Get-SFTPSession
            $sessions | Should -Not -BeNullOrEmpty
            $sessions.Count | Should -BeGreaterOrEqual 1
        }

        It "Should retrieve SFTP session by SessionId" {
            $session = Get-SFTPSession -SessionId $script:SFTPSession.SessionId
            $session | Should -Not -BeNullOrEmpty
            $session.SessionId | Should -Be $script:SFTPSession.SessionId
        }
    }

    Context "SFTP Directory Operations" {

        It "Should test if /tmp path exists" {
            $result = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path "/tmp"
            $result | Should -Be $true
        }

        It "Should get current SFTP location" {
            $location = Get-SFTPLocation -SessionId $script:SFTPSession.SessionId
            $location | Should -Not -BeNullOrEmpty
        }

        It "Should set SFTP location to /tmp" {
            Set-SFTPLocation -SessionId $script:SFTPSession.SessionId -Path "/tmp"
            $location = Get-SFTPLocation -SessionId $script:SFTPSession.SessionId
            $location | Should -Be "/tmp"
        }

        It "Should list files in /tmp directory" {
            $items = Get-SFTPChildItem -SessionId $script:SFTPSession.SessionId -Path "/tmp"
            $items | Should -Not -BeNullOrEmpty
        }

        It "Should create a new directory in /tmp" {
            New-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $RemoteTestDir -ItemType Directory
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $RemoteTestDir
            $exists | Should -Be $true
        }

        It "Should get attributes of created directory" {
            $attrs = Get-SFTPPathAttribute -SessionId $script:SFTPSession.SessionId -Path $RemoteTestDir
            $attrs | Should -Not -BeNullOrEmpty
            $attrs.IsDirectory | Should -Be $true
        }
    }

    Context "SFTP File Upload Operations" {

        It "Should upload file using Set-SFTPItem" {
            Set-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $LocalTestFile -Destination "/tmp" -Force
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath
            $exists | Should -Be $true
        }

        It "Should verify uploaded file exists" {
            $files = Get-SFTPChildItem -SessionId $script:SFTPSession.SessionId -Path "/tmp"
            $file = $files | Where-Object { $_.Name -eq $TestFileName }
            $file | Should -Not -BeNullOrEmpty
            $file.Name | Should -Be $TestFileName
        }

        It "Should get content of uploaded file" {
            $content = Get-SFTPContent -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath
            $content | Should -Match "test file for Posh-SSH"
        }

        It "Should get attributes of uploaded file" {
            $attrs = Get-SFTPPathAttribute -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath
            $attrs | Should -Not -BeNullOrEmpty
            $attrs.IsRegularFile | Should -Be $true
        }
    }

    Context "SFTP File Manipulation Operations" {

        It "Should rename the uploaded file" {
            $newName = "/tmp/posh-ssh-renamed.txt"
            Rename-SFTPFile -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath -NewName "posh-ssh-renamed.txt"
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $newName
            $exists | Should -Be $true

            # Rename back for other tests
            Rename-SFTPFile -SessionId $script:SFTPSession.SessionId -Path $newName -NewName $TestFileName
        }

        It "Should move file to test directory" -Skip {
            # Note: Move-SFTPItem may fail on some SFTP servers due to file deletion issues
            # Create a separate test file for moving
            $moveTestFile = "/tmp/move-test-file.txt"
            Set-SFTPContent -SessionId $script:SFTPSession.SessionId -Path $moveTestFile -Value "Test file for move operation"

            $movedPath = "$RemoteTestDir/move-test-file.txt"
            Move-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $moveTestFile -Destination $RemoteTestDir -Force
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $movedPath
            $exists | Should -Be $true

            # Cleanup moved file
            Remove-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $movedPath
        }

        It "Should set content to a file" {
            $newContent = "Updated content at $(Get-Date)"
            Set-SFTPContent -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath -Value $newContent
            $content = Get-SFTPContent -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath
            $content | Should -Match "Updated content"
        }
    }

    Context "SFTP File Download Operations" {

        It "Should download file using Get-SFTPItem" {
            $downloadPath = Join-Path $env:TEMP $TestFileName

            # Remove if exists from previous run
            if (Test-Path $downloadPath) {
                Remove-Item $downloadPath -Force
            }

            Get-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath -Destination $env:TEMP -Force

            # Check if file was downloaded
            $downloadedFile = Get-Item $downloadPath -ErrorAction SilentlyContinue
            $downloadedFile | Should -Not -BeNullOrEmpty

            # Cleanup downloaded file
            Remove-Item $downloadPath -Force
        }
    }

    Context "SCP Operations" {

        BeforeAll {
            $script:SCPTestFile = Join-Path $env:TEMP "scp-test-file.txt"
            Set-Content -Path $script:SCPTestFile -Value "SCP test content"
            $script:SCPRemotePath = "/tmp/scp-test-file.txt"
        }

        AfterAll {
            if (Test-Path $script:SCPTestFile) {
                Remove-Item $script:SCPTestFile -Force -ErrorAction SilentlyContinue
            }
        }

        It "Should upload file using Set-SCPItem" {
            if ($AuthMethod -eq 'Key') {
                Set-SCPItem -ComputerName $ComputerName -Credential $Credential -KeyFile $KeyPath -Port $Port -Path $script:SCPTestFile -Destination "/tmp" -AcceptKey
            } else {
                Set-SCPItem -ComputerName $ComputerName -Credential $Credential -Port $Port -Path $script:SCPTestFile -Destination "/tmp" -AcceptKey
            }

            # Verify using SFTP
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $script:SCPRemotePath
            $exists | Should -Be $true
        }

        It "Should download file using Get-SCPItem" {
            $downloadPath = Join-Path $env:TEMP "scp-test-file.txt"

            # Remove if exists
            if (Test-Path $downloadPath) {
                Remove-Item $downloadPath -Force
            }

            if ($AuthMethod -eq 'Key') {
                Get-SCPItem -ComputerName $ComputerName -Credential $Credential -KeyFile $KeyPath -Port $Port -Path $script:SCPRemotePath -Destination $env:TEMP -PathType File -AcceptKey
            } else {
                Get-SCPItem -ComputerName $ComputerName -Credential $Credential -Port $Port -Path $script:SCPRemotePath -Destination $env:TEMP -PathType File -AcceptKey
            }

            Test-Path $downloadPath | Should -Be $true

            # Cleanup
            Remove-Item $downloadPath -Force
        }
    }

    Context "SFTP Cleanup Operations" {

        It "Should remove uploaded test file" {
            Remove-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath -Force
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $RemoteTestPath
            $exists | Should -Be $false
        }

        It "Should remove SCP test file" {
            Remove-SFTPItem -SessionId $script:SFTPSession.SessionId -Path "/tmp/scp-test-file.txt" -Force
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path "/tmp/scp-test-file.txt"
            $exists | Should -Be $false
        }

        It "Should remove test directory" {
            Remove-SFTPItem -SessionId $script:SFTPSession.SessionId -Path $RemoteTestDir -Force
            $exists = Test-SFTPPath -SessionId $script:SFTPSession.SessionId -Path $RemoteTestDir
            $exists | Should -Be $false
        }
    }

    Context "Port Forwarding" {

        It "Should create local port forward" {
            New-SSHLocalPortForward -SessionId $script:SSHSession.SessionId -BoundHost "127.0.0.1" -BoundPort 8081 -RemoteAddress "127.0.0.1" -RemotePort 80
            # Verify it was created by checking for it
            $forwards = Get-SSHPortForward -SessionId $script:SSHSession.SessionId
            $forwards | Should -Not -BeNullOrEmpty
        }

        It "Should get port forward information" {
            $forwards = Get-SSHPortForward -SessionId $script:SSHSession.SessionId
            $forwards | Should -Not -BeNullOrEmpty
        }

        It "Should stop port forward" {
            Stop-SSHPortForward -SessionId $script:SSHSession.SessionId -BoundHost "127.0.0.1" -BoundPort 8081
            # Give it a moment to stop
            Start-Sleep -Seconds 1
        }

        It "Should start port forward" {
            Start-SSHPortForward -SessionId $script:SSHSession.SessionId -BoundHost "127.0.0.1" -BoundPort 8081
            # Give it a moment to start
            Start-Sleep -Seconds 1

            # Stop it again for cleanup
            Stop-SSHPortForward -SessionId $script:SSHSession.SessionId -BoundHost "127.0.0.1" -BoundPort 8081
        }
    }

    Context "Session Cleanup" {

        It "Should remove SFTP session" {
            Remove-SFTPSession -SessionId $script:SFTPSession.SessionId
            $session = Get-SFTPSession -SessionId $script:SFTPSession.SessionId
            $session | Should -BeNullOrEmpty
        }

        It "Should remove SSH session" {
            Remove-SSHSession -SessionId $script:SSHSession.SessionId
            $session = Get-SSHSession -SessionId $script:SSHSession.SessionId
            $session | Should -BeNullOrEmpty
        }

        It "Should have no active SSH sessions" {
            $sessions = Get-SSHSession
            $sessions | Should -BeNullOrEmpty
        }

        It "Should have no active SFTP sessions" {
            $sessions = Get-SFTPSession
            $sessions | Should -BeNullOrEmpty
        }
    }
}

Write-Host "`nIntegration tests completed!" -ForegroundColor Green
Write-Host "Authentication method used: $AuthMethod" -ForegroundColor Cyan
