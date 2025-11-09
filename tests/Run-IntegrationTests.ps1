<#
.SYNOPSIS
    Helper script to run Posh-SSH integration tests.

.DESCRIPTION
    This script simplifies running the Posh-SSH integration tests by prompting for
    credentials interactively and executing the test suite.

.PARAMETER ComputerName
    IP address or hostname of the SSH server to test against.

.PARAMETER UserName
    Username for SSH authentication.

.PARAMETER UsePassword
    Use password authentication (script will prompt for password).

.PARAMETER UseKey
    Use SSH key authentication.

.PARAMETER KeyPath
    Path to SSH private key file (required when using -UseKey).

.PARAMETER NeedKeyPassPhrase
    Indicates that the SSH key is encrypted and requires a passphrase.

.PARAMETER Port
    SSH port number (default: 22).

.EXAMPLE
    # Run tests with password authentication
    .\Run-IntegrationTests.ps1 -ComputerName 192.168.1.100 -UserName testuser -UsePassword

.EXAMPLE
    # Run tests with unencrypted key
    .\Run-IntegrationTests.ps1 -ComputerName 192.168.1.100 -UserName testuser -UseKey -KeyPath "C:\Users\test\.ssh\id_rsa"

.EXAMPLE
    # Run tests with encrypted key
    .\Run-IntegrationTests.ps1 -ComputerName 192.168.1.100 -UserName testuser -UseKey -KeyPath "C:\Users\test\.ssh\id_rsa" -NeedKeyPassPhrase

.EXAMPLE
    # Run tests with custom port
    .\Run-IntegrationTests.ps1 -ComputerName 192.168.1.100 -UserName testuser -UsePassword -Port 2222
#>

[CmdletBinding(DefaultParameterSetName='Password')]
param(
    [Parameter(Mandatory=$true)]
    [string]$ComputerName,

    [Parameter(Mandatory=$true)]
    [string]$UserName,

    [Parameter(Mandatory=$true, ParameterSetName='Password')]
    [switch]$UsePassword,

    [Parameter(Mandatory=$true, ParameterSetName='Key')]
    [switch]$UseKey,

    [Parameter(Mandatory=$true, ParameterSetName='Key')]
    [ValidateScript({Test-Path $_})]
    [string]$KeyPath,

    [Parameter(Mandatory=$false, ParameterSetName='Key')]
    [switch]$NeedKeyPassPhrase,

    [Parameter(Mandatory=$false)]
    [int]$Port = 22
)

# Check if Pester is installed
if (-not (Get-Module -ListAvailable -Name Pester)) {
    Write-Host "Pester module is not installed. Installing..." -ForegroundColor Yellow
    try {
        Install-Module -Name Pester -Force -Scope CurrentUser
        Write-Host "Pester installed successfully." -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to install Pester: $_"
        exit 1
    }
}

# Prepare test parameters
$testParams = @{
    ComputerName = $ComputerName
    UserName = $UserName
    Port = $Port
}

if ($PSCmdlet.ParameterSetName -eq 'Password') {
    Write-Host "Running integration tests with password authentication" -ForegroundColor Cyan
    Write-Host "You will be prompted for the password..." -ForegroundColor Yellow

    $securePassword = Read-Host "Enter password for $UserName" -AsSecureString
    $testParams['Password'] = $securePassword

} else {
    Write-Host "Running integration tests with SSH key authentication" -ForegroundColor Cyan
    Write-Host "Key file: $KeyPath" -ForegroundColor Yellow

    $testParams['KeyPath'] = $KeyPath

    if ($NeedKeyPassPhrase) {
        Write-Host "You will be prompted for the key passphrase..." -ForegroundColor Yellow
        $securePassPhrase = Read-Host "Enter passphrase for SSH key" -AsSecureString
        $testParams['KeyPassPhrase'] = $securePassPhrase
    }
}

# Display test information
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Posh-SSH Integration Test Runner" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Target Server: $ComputerName" -ForegroundColor White
Write-Host "Port: $Port" -ForegroundColor White
Write-Host "Username: $UserName" -ForegroundColor White
Write-Host "Auth Method: $($PSCmdlet.ParameterSetName)" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

# Run the integration tests
$testScript = Join-Path $PSScriptRoot "Posh-SSH.Integration.Tests.ps1"

try {
    & $testScript @testParams
    Write-Host "`nTest execution completed!" -ForegroundColor Green
}
catch {
    Write-Error "Test execution failed: $_"
    exit 1
}
