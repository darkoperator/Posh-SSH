#requires -Version 5.1
<#
.SYNOPSIS
    Build and package the Posh-SSH module.

.DESCRIPTION
    Compiles the netstandard2.0 binary module from Source/PoshSSH/PoshSSH.Core,
    drops the resulting PoshSSH.dll into Posh-SSH/, verifies the module loads
    and exposes the documented cmdlets, then bundles Posh-SSH/ into a distribution
    zip and reports its SHA256.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER SkipBuild
    Skip the dotnet build step (use the existing PoshSSH.dll for packaging).

.PARAMETER OutputPath
    Directory where the distribution zip is written. Defaults to the repo root.
#>
[CmdletBinding()]
param(
    [ValidateSet('Release','Debug')]
    [string]$Configuration = 'Release',

    [switch]$SkipBuild,

    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot   = $PSScriptRoot
$coreProj   = Join-Path $repoRoot 'Source/PoshSSH/PoshSSH.Core/PoshSSH.Core.csproj'
$moduleDir  = Join-Path $repoRoot 'Posh-SSH'
$manifest   = Join-Path $moduleDir 'Posh-SSH.psd1'
$dllTarget  = Join-Path $moduleDir 'PoshSSH.dll'
if (-not $OutputPath) { $OutputPath = $repoRoot }

function Step([string]$msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# --- preflight ---------------------------------------------------------------

Step "preflight"
foreach ($path in $coreProj, $manifest) {
    if (-not (Test-Path $path)) { throw "missing required path: $path" }
}
$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue)
if (-not $dotnet -and -not $SkipBuild) { throw "dotnet SDK not found in PATH" }
# used for any check that must run in a fresh shell so cached assemblies don't mask a problem
$ps = if (Get-Command pwsh -ErrorAction SilentlyContinue) { 'pwsh' } else { 'powershell' }

# --- build -------------------------------------------------------------------

if ($SkipBuild) {
    Step "skipping build (--SkipBuild)"
} else {
    Step "dotnet build -c $Configuration"
    & dotnet build $coreProj -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }

    $built = Join-Path $repoRoot "Source/PoshSSH/PoshSSH.Core/bin/$Configuration/netstandard2.0/PoshSSH.dll"
    if (-not (Test-Path $built)) { throw "build output missing: $built" }

    Step "copying $built -> $dllTarget"
    Copy-Item $built $dllTarget -Force
}

if (-not (Test-Path $dllTarget)) { throw "no PoshSSH.dll at $dllTarget" }

# --- bundled SSH.NET assembly ------------------------------------------------
#
# The manifest loads Assembly\Renci.SshNet.dll from disk via RequiredAssemblies, so the
# PackageReference in the csproj only governs compilation. Those two drifted apart in 3.2.6
# and again in 3.2.7 (see issue #632), each time invisibly. Sync the bundled copy from the
# restore output so the csproj is the single source of truth, then assert the result.

$renciTarget = Join-Path $moduleDir 'Assembly/Renci.SshNet.dll'

if (-not $SkipBuild) {
    $builtRenci = Join-Path $repoRoot "Source/PoshSSH/PoshSSH.Core/bin/$Configuration/netstandard2.0/Renci.SshNet.dll"
    if (-not (Test-Path $builtRenci)) { throw "restore output missing: $builtRenci" }
    Step "syncing bundled Renci.SshNet.dll from the restore output"
    Copy-Item $builtRenci $renciTarget -Force
}

if (-not (Test-Path $renciTarget)) { throw "no Renci.SshNet.dll at $renciTarget" }

Step "verifying bundled Renci.SshNet matches what PoshSSH.dll was compiled against"
$refScript = @"
`$ErrorActionPreference = 'Stop'
`$asm = [System.Reflection.Assembly]::LoadFrom('$dllTarget')
(`$asm.GetReferencedAssemblies() | Where-Object { `$_.Name -eq 'Renci.SshNet' } | Select-Object -First 1).Version.ToString()
"@
$refOutput = & $ps -NoProfile -NonInteractive -Command $refScript 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ($refOutput -join "`n") -ForegroundColor Red
    throw "could not read the Renci.SshNet reference from $dllTarget"
}
$referencedVersion = ($refOutput | Where-Object { $_ -is [string] -and $_.Trim() } | Select-Object -Last 1).Trim()
$bundledVersion = [System.Reflection.AssemblyName]::GetAssemblyName($renciTarget).Version.ToString()

if ($referencedVersion -ne $bundledVersion) {
    Write-Host "  PoshSSH.dll references Renci.SshNet $referencedVersion" -ForegroundColor Red
    Write-Host "  Assembly\Renci.SshNet.dll on disk is  $bundledVersion" -ForegroundColor Red
    throw "bundled Renci.SshNet.dll does not match the version PoshSSH.dll was compiled against"
}
Write-Host "  Renci.SshNet $bundledVersion (compiled against and bundled)" -ForegroundColor Green

# --- module load + cmdlet verification --------------------------------------

Step "parsing manifest"
$info = Test-ModuleManifest $manifest
# Cmdlets expected to load are derived from the manifest's own CmdletsToExport
# so renames in the manifest don't drift from this script.
$expectedCmdlets = @($info.ExportedCmdlets.Keys)
Write-Host "  manifest declares $($expectedCmdlets.Count) cmdlets"

Step "verifying module loads and exposes those cmdlets"
$verifyScript = @"
`$ErrorActionPreference = 'Stop'
Import-Module '$manifest' -Force
Get-Command -Module Posh-SSH -CommandType Cmdlet | Select-Object -ExpandProperty Name
"@
# run in a fresh pwsh/powershell so cached assemblies don't mask regressions
$output = & $ps -NoProfile -NonInteractive -Command $verifyScript 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ($output -join "`n") -ForegroundColor Red
    throw "module failed to import"
}
$loaded = $output | Where-Object { $_ -is [string] -and $_.Trim() } | ForEach-Object { $_.Trim() }
$missing = $expectedCmdlets | Where-Object { $loaded -notcontains $_ }
if ($missing) { throw "cmdlets declared in manifest but missing from built module: $($missing -join ', ')" }
$unexpected = $loaded | Where-Object { $expectedCmdlets -notcontains $_ }
if ($unexpected) { Write-Host "  WARNING: module exports cmdlets not in manifest: $($unexpected -join ', ')" -ForegroundColor Yellow }
Write-Host "  loaded cmdlets: $($loaded.Count)" -ForegroundColor Green

# --- manifest file declarations ---------------------------------------------

Step "validating RequiredAssemblies and FileList exist on disk"

function Resolve-ManifestPath([string]$p) {
    if ([System.IO.Path]::IsPathRooted($p)) { $p } else { Join-Path $moduleDir $p }
}

$declared = @()
foreach ($a in @($info.RequiredAssemblies)) { if ($a) { $declared += @{ Kind = 'RequiredAssemblies'; Path = $a } } }
foreach ($f in @($info.FileList))           { if ($f) { $declared += @{ Kind = 'FileList';           Path = $f } } }

$absent = @()
foreach ($d in $declared) {
    $resolved = Resolve-ManifestPath $d.Path
    if (-not (Test-Path $resolved)) { $absent += "$($d.Kind): $($d.Path)" }
}
if ($absent) {
    Write-Host "  files declared by the manifest but missing on disk:" -ForegroundColor Red
    $absent | ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
    throw "manifest declares files that are not in the module tree"
}
Write-Host "  validated $($declared.Count) manifest-declared files" -ForegroundColor Green

# --- version + zip -----------------------------------------------------------

Step "reading version from $manifest"
$version = $info.Version.ToString()
$prerelease = $info.PrivateData.PSData.Prerelease
$fullVersion = if ($prerelease) { "$version-$prerelease" } else { $version }
$zipName = "Posh-SSH-$fullVersion.zip"
$zipPath = Join-Path $OutputPath $zipName

Step "packaging $zipName"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
# Zip the entire Posh-SSH/ module directory. Source code is at Source/ (outside
# Posh-SSH/) so it's automatically excluded.
Compress-Archive -Path (Join-Path $moduleDir '*') -DestinationPath $zipPath -CompressionLevel Optimal

$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
$sizeKB = [math]::Round((Get-Item $zipPath).Length / 1KB, 1)

# --- summary -----------------------------------------------------------------

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  Build complete" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ("  Version : {0}" -f $fullVersion)
Write-Host ("  DLL     : {0}" -f $dllTarget)
Write-Host ("  Package : {0} ({1} KB)" -f $zipPath, $sizeKB)
Write-Host ("  SHA256  : {0}" -f $hash)
Write-Host ""
