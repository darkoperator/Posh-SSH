# Secure script to publish Posh-SSH module to PowerShell Gallery
# This script prompts for the API key securely (masked input)

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Module path
$modulePath = Join-Path $PSScriptRoot "Posh-SSH"

# Verify module exists
if (-not (Test-Path $modulePath)) {
    Write-Error "Module directory not found at: $modulePath"
    exit 1
}

# Get module version
$manifest = Test-ModuleManifest -Path (Join-Path $modulePath "Posh-SSH.psd1") -ErrorAction Stop
$moduleVersion = $manifest.Version

Write-Host "Preparing to publish Posh-SSH version $moduleVersion to PowerShell Gallery" -ForegroundColor Cyan
Write-Host ""

# Prompt for API key securely
Write-Host "Please enter your PowerShell Gallery API Key:" -ForegroundColor Yellow
$apiKeySecure = Read-Host -AsSecureString
$apiKeyPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiKeySecure)
)

if ([string]::IsNullOrWhiteSpace($apiKeyPlain)) {
    Write-Error "API key cannot be empty"
    exit 1
}

Write-Host ""
Write-Host "Publishing module..." -ForegroundColor Green

try {
    Publish-Module -Path $modulePath -NuGetApiKey $apiKeyPlain -Verbose
    Write-Host ""
    Write-Host "Successfully published Posh-SSH $moduleVersion to PowerShell Gallery!" -ForegroundColor Green
}
catch {
    Write-Error "Failed to publish module: $_"
    exit 1
}
finally {
    # Clear the API key from memory
    $apiKeyPlain = $null
    $apiKeySecure = $null
    [System.GC]::Collect()
}
