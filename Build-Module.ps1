<#
.SYNOPSIS
    Build script for Posh-SSH module.

.DESCRIPTION
    This script:
    1. Compiles the C# binary module (PoshSSH.dll) using MSBuild
    2. Copies the netstandard2.0 version to the module directory
    3. Creates a clean distribution package (ZIP file) without source code
    4. Validates the module can be loaded

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER SkipBuild
    Skip the compilation step and only create the package from existing binaries.

.PARAMETER SkipTests
    Skip module loading tests.

.PARAMETER OutputPath
    Path where the ZIP file will be created. Default: repository root

.EXAMPLE
    .\Build-Module.ps1

.EXAMPLE
    .\Build-Module.ps1 -Configuration Debug

.EXAMPLE
    .\Build-Module.ps1 -SkipBuild -OutputPath "C:\Releases"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,

    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

#region Helper Functions

function Write-Step {
    param([string]$Message)
    Write-Host "`n===================================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "===================================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Find-MSBuild {
    # Try to find MSBuild in common locations
    $msbuildPaths = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($path in $msbuildPaths) {
        if (Test-Path $path) {
            return $path
        }
    }

    # Try to find via vswhere
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($vsPath) {
            $msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuildPath) {
                return $msbuildPath
            }
        }
    }

    throw "MSBuild not found. Please install Visual Studio or Visual Studio Build Tools."
}

#endregion

#region Main Script

try {
    $scriptRoot = $PSScriptRoot
    $sourceDir = Join-Path $scriptRoot "Source\PoshSSH"
    $moduleDir = Join-Path $scriptRoot "Posh-SSH"
    $solutionFile = Join-Path $sourceDir "PoshSSH.sln"
    $coreProjectDir = Join-Path $sourceDir "PoshSSH.Core"
    $netStandardDll = Join-Path $coreProjectDir "bin\$Configuration\netstandard2.0\PoshSSH.dll"
    $targetDll = Join-Path $moduleDir "PoshSSH.dll"

    # Set output path
    if (-not $OutputPath) {
        $OutputPath = $scriptRoot
    }

    # Read version from manifest
    $manifestPath = Join-Path $moduleDir "Posh-SSH.psd1"
    $manifest = Import-PowerShellDataFile -Path $manifestPath
    $version = $manifest.ModuleVersion

    Write-Host "`nPosh-SSH Build Script" -ForegroundColor Magenta
    Write-Host "Version: $version" -ForegroundColor Magenta
    Write-Host "Configuration: $Configuration" -ForegroundColor Magenta
    Write-Host "Repository: $scriptRoot`n" -ForegroundColor Magenta

    #region Build Binary Module

    if (-not $SkipBuild) {
        Write-Step "Step 1: Compiling C# Binary Module"

        # Find MSBuild
        Write-Info "Locating MSBuild..."
        $msbuildPath = Find-MSBuild
        Write-Success "Found MSBuild: $msbuildPath"

        # Check if solution exists
        if (-not (Test-Path $solutionFile)) {
            throw "Solution file not found: $solutionFile"
        }

        # Clean previous builds
        Write-Info "Cleaning previous builds..."
        & $msbuildPath $solutionFile /t:Clean /p:Configuration=$Configuration /v:minimal /nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Clean failed with exit code $LASTEXITCODE"
        }

        # Build solution
        Write-Info "Building solution ($Configuration)..."
        & $msbuildPath $solutionFile /t:Build /p:Configuration=$Configuration /v:minimal /nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        # Verify DLL was created
        if (-not (Test-Path $netStandardDll)) {
            throw "Build succeeded but DLL not found: $netStandardDll"
        }

        $dllSize = (Get-Item $netStandardDll).Length
        Write-Success "Build completed successfully (DLL size: $([math]::Round($dllSize/1KB, 2)) KB)"

        # Copy DLL to module directory
        Write-Info "Copying netstandard2.0 DLL to module directory..."
        Copy-Item -Path $netStandardDll -Destination $targetDll -Force
        Write-Success "DLL copied to: $targetDll"

        # Copy dependencies to Assembly directory to ensure version compatibility
        Write-Info "Copying dependencies to Assembly directory..."
        $buildOutputDir = Join-Path $coreProjectDir "bin\$Configuration\netstandard2.0"
        $assemblyDir = Join-Path $moduleDir "Assembly"

        # Ensure Assembly directory exists
        if (-not (Test-Path $assemblyDir)) {
            New-Item -ItemType Directory -Path $assemblyDir -Force | Out-Null
        }

        # Copy dependency DLLs (exclude PoshSSH.dll as it goes to module root)
        $dependencyFiles = Get-ChildItem -Path $buildOutputDir -Filter "*.dll" |
            Where-Object { $_.Name -ne "PoshSSH.dll" -and $_.Name -ne "System.Management.Automation.dll" }

        foreach ($file in $dependencyFiles) {
            Copy-Item -Path $file.FullName -Destination $assemblyDir -Force
            Write-Info "  Copied: $($file.Name)"
        }

        Write-Success "Dependencies synchronized with build output"
    }
    else {
        Write-Step "Step 1: Skipping Build (using existing binaries)"

        if (-not (Test-Path $targetDll)) {
            throw "Module DLL not found: $targetDll. Cannot skip build."
        }
        Write-Success "Using existing DLL: $targetDll"
    }

    #endregion

    #region Test Module Loading

    if (-not $SkipTests) {
        Write-Step "Step 2: Testing Module Loading"

        Write-Info "Removing any loaded Posh-SSH module..."
        Remove-Module -Name Posh-SSH -ErrorAction SilentlyContinue

        Write-Info "Importing module..."
        Import-Module $manifestPath -Force -ErrorAction Stop

        $loadedModule = Get-Module -Name Posh-SSH
        if (-not $loadedModule) {
            throw "Module failed to load"
        }

        Write-Success "Module loaded: Version $($loadedModule.Version)"

        # Verify cmdlets
        $cmdlets = Get-Command -Module Posh-SSH -CommandType Cmdlet
        Write-Success "Binary cmdlets loaded: $($cmdlets.Count)"

        $functions = Get-Command -Module Posh-SSH -CommandType Function
        Write-Success "PowerShell functions loaded: $($functions.Count)"

        # Verify key cmdlets exist
        $keyCmdlets = @('New-SSHSession', 'New-SFTPSession', 'Get-SCPItem', 'Set-SCPItem')
        foreach ($cmdlet in $keyCmdlets) {
            $cmd = Get-Command -Name $cmdlet -ErrorAction SilentlyContinue
            if (-not $cmd) {
                throw "Key cmdlet not found: $cmdlet"
            }
        }
        Write-Success "All key cmdlets verified"

        # Clean up
        Remove-Module -Name Posh-SSH -ErrorAction SilentlyContinue
    }
    else {
        Write-Step "Step 2: Skipping Module Tests"
    }

    #endregion

    #region Create Distribution Package

    Write-Step "Step 3: Creating Distribution Package"

    # Create temporary directory for clean module
    $tempDir = Join-Path $env:TEMP "Posh-SSH-Build-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $tempModuleDir = Join-Path $tempDir "Posh-SSH"

    Write-Info "Creating temporary directory: $tempDir"
    New-Item -ItemType Directory -Path $tempModuleDir -Force | Out-Null

    # Copy module files (excluding source code, tests, etc.)
    Write-Info "Copying module files..."

    $itemsToCopy = @(
        @{Path = "$moduleDir\*.psd1"; Destination = $tempModuleDir},
        @{Path = "$moduleDir\*.psm1"; Destination = $tempModuleDir},
        @{Path = "$moduleDir\*.ps1"; Destination = $tempModuleDir},
        @{Path = "$moduleDir\*.dll"; Destination = $tempModuleDir},
        @{Path = "$moduleDir\Assembly"; Destination = "$tempModuleDir\Assembly"},
        @{Path = "$moduleDir\en-US"; Destination = "$tempModuleDir\en-US"},
        @{Path = "$moduleDir\Format"; Destination = "$tempModuleDir\Format"}
    )

    foreach ($item in $itemsToCopy) {
        if (Test-Path $item.Path) {
            Copy-Item -Path $item.Path -Destination $item.Destination -Recurse -Force
            Write-Info "  Copied: $(Split-Path $item.Path -Leaf)"
        }
    }

    # Copy root documentation files
    $docFiles = @("README.md", "License.md", "CHANGELOG.md")
    foreach ($doc in $docFiles) {
        $docPath = Join-Path $scriptRoot $doc
        if (Test-Path $docPath) {
            Copy-Item -Path $docPath -Destination $tempDir -Force
            Write-Info "  Copied: $doc"
        }
    }

    Write-Success "Module files copied"

    # Create ZIP file
    $zipFileName = "Posh-SSH-$version.zip"
    $zipPath = Join-Path $OutputPath $zipFileName

    # Remove existing ZIP if it exists
    if (Test-Path $zipPath) {
        Write-Info "Removing existing ZIP file..."
        Remove-Item -Path $zipPath -Force
    }

    Write-Info "Creating ZIP archive..."
    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -CompressionLevel Optimal -Force

    $zipSize = (Get-Item $zipPath).Length
    Write-Success "ZIP created: $zipPath ($([math]::Round($zipSize/1MB, 2)) MB)"

    # Clean up temporary directory
    Write-Info "Cleaning up temporary files..."
    Remove-Item -Path $tempDir -Recurse -Force

    #endregion

    #region Summary

    Write-Step "Build Complete!"

    Write-Host "Summary:" -ForegroundColor White
    Write-Host "  Version:        $version" -ForegroundColor White
    Write-Host "  Configuration:  $Configuration" -ForegroundColor White
    Write-Host "  Package:        $zipPath" -ForegroundColor White
    Write-Host "  Package Size:   $([math]::Round($zipSize/1MB, 2)) MB" -ForegroundColor White
    Write-Host ""

    # Calculate hash for verification
    Write-Info "Calculating package hash..."
    $hash = Get-FileHash -Path $zipPath -Algorithm SHA256
    Write-Host "  SHA256:         $($hash.Hash)" -ForegroundColor Gray
    Write-Host ""

    Write-Success "Build completed successfully!"

    #endregion

}
catch {
    Write-Host ""
    Write-ErrorMessage "Build failed: $_"
    Write-Host ""
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}

#endregion
