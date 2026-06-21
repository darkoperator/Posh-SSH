# Building Posh-SSH

This document describes how to build and package the Posh-SSH module for distribution.

## Prerequisites

- **Visual Studio 2019 or 2022** (Community, Professional, or Enterprise) OR **Visual Studio Build Tools**
- **PowerShell 5.1 or PowerShell 7.x**
- **.NET Framework 4.7.2 SDK** (for .NET Framework build)
- **.NET Core SDK 2.0+** (for .NET Standard 2.0 build)

## Build Script

The repository includes an automated build script: `Build-Module.ps1`

### Basic Usage

Build and package the module:

```powershell
.\Build-Module.ps1
```

This will:
1. Compile the C# binary module (PoshSSH.dll) from source
2. Copy the netstandard2.0 version to the module directory
3. Test that the module loads correctly
4. Create a distribution ZIP file: `Posh-SSH-{version}.zip`

### Advanced Options

#### Build in Debug mode

```powershell
.\Build-Module.ps1 -Configuration Debug
```

#### Skip compilation (use existing binaries)

```powershell
.\Build-Module.ps1 -SkipBuild
```

#### Skip module loading tests

```powershell
.\Build-Module.ps1 -SkipTests
```

#### Custom output location

```powershell
.\Build-Module.ps1 -OutputPath "C:\Releases"
```

#### Combine options

```powershell
.\Build-Module.ps1 -Configuration Release -OutputPath "C:\Releases" -SkipTests
```

### Build Output

The build script produces:

1. **Compiled Binary**: `Posh-SSH/PoshSSH.dll` (netstandard2.0)
2. **Distribution Package**: `Posh-SSH-{version}.zip`

The ZIP file contains:
- Complete module ready for installation
- All required assemblies and dependencies
- Documentation (README.md, License.md, CHANGELOG.md)
- **NO source code** - only the distributable module

### Package Contents

```
Posh-SSH-3.2.6.zip
├── CHANGELOG.md
├── License.md
├── README.md
└── Posh-SSH/
    ├── Posh-SSH.psd1           # Module manifest
    ├── Posh-SSH.psm1           # PowerShell module
    ├── PoshSSH.dll             # Binary cmdlets (netstandard2.0)
    ├── Add-SshIdentity.ps1     # Helper script
    ├── Assembly/               # Dependencies
    │   ├── BouncyCastle.Cryptography.dll
    │   ├── Renci.SshNet.dll
    │   └── ... (other dependencies)
    ├── en-US/                  # Help files
    └── Format/                 # Format definitions
```

## Manual Build Steps

If you need to build manually without using the script:

### 1. Compile the C# Project

```powershell
# Navigate to source directory
cd Source\PoshSSH

# Build using MSBuild (adjust path to your MSBuild installation)
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
    PoshSSH.sln /p:Configuration=Release /t:Clean,Build
```

### 2. Copy the Compiled DLL

**IMPORTANT**: Use the **netstandard2.0** version, not the .NET Framework version!

```powershell
# Copy the netstandard2.0 DLL to the module directory
Copy-Item -Path "Source\PoshSSH\PoshSSH.Core\bin\Release\netstandard2.0\PoshSSH.dll" `
          -Destination "Posh-SSH\PoshSSH.dll" `
          -Force
```

### 3. Test Module Loading

```powershell
# Remove any loaded version
Remove-Module Posh-SSH -ErrorAction SilentlyContinue

# Import the module
Import-Module .\Posh-SSH\Posh-SSH.psd1 -Force

# Verify it loaded
Get-Module Posh-SSH
Get-Command -Module Posh-SSH
```

### 4. Create Distribution Package

```powershell
# Create a clean copy for distribution
$tempDir = New-Item -ItemType Directory -Path "$env:TEMP\Posh-SSH-Package" -Force

# Copy module files (no source code)
Copy-Item -Path "Posh-SSH\*" -Destination "$tempDir\Posh-SSH" -Recurse -Exclude "*.pdb"
Copy-Item -Path "README.md", "License.md", "CHANGELOG.md" -Destination $tempDir

# Create ZIP
$version = (Import-PowerShellDataFile "Posh-SSH\Posh-SSH.psd1").ModuleVersion
Compress-Archive -Path "$tempDir\*" -DestinationPath "Posh-SSH-$version.zip" -Force

# Cleanup
Remove-Item $tempDir -Recurse -Force
```

## Version Management

To update the version number:

1. Edit `Posh-SSH/Posh-SSH.psd1`
2. Update the `ModuleVersion` field:
   ```powershell
   ModuleVersion = '3.2.7'
   ```
3. Update `CHANGELOG.md` with version notes
4. Run the build script
5. Create a git tag:
   ```bash
   git tag -a v3.2.7 -m "Version 3.2.7"
   ```

## Troubleshooting

### MSBuild Not Found

If the build script cannot find MSBuild:

1. Install Visual Studio or Visual Studio Build Tools
2. Ensure MSBuild is in one of the default locations, or
3. Manually specify the path by editing the `Find-MSBuild` function in `Build-Module.ps1`

### Wrong DLL Version

Always verify you're using the **netstandard2.0** version:

```powershell
# Check DLL size (netstandard2.0 should be ~46 KB)
Get-Item Posh-SSH\PoshSSH.dll | Select-Object Length, LastWriteTime

# Verify it loads
Import-Module .\Posh-SSH\Posh-SSH.psd1 -Force
Get-Command New-SSHSession | Select-Object DLL
```

### Build Warnings

You may see a warning about Newtonsoft.Json when building the .NET Framework version. This is expected and can be ignored - the module uses the version from the Assembly folder.

## CI/CD Integration

The build script is designed to work in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Build Posh-SSH
  shell: pwsh
  run: |
    .\Build-Module.ps1 -SkipTests

- name: Upload Package
  uses: actions/upload-artifact@v2
  with:
    name: Posh-SSH-Package
    path: Posh-SSH-*.zip
```

## Additional Resources

- **Source Code**: `Source/PoshSSH/` - C# cmdlet implementations
- **Tests**: `tests/` - Pester unit and integration tests
- **Documentation**: `docs/` - Command documentation (Markdown)
- **Module**: `Posh-SSH/` - Distributable module directory
