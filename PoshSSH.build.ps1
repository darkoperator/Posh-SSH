[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug'
)

$BuildPath = [IO.Path]::Combine($PSScriptRoot, 'output')
$PowerShellPath = [IO.Path]::Combine($PSScriptRoot, 'Posh-SSH')
$CSharpPath = [IO.Path]::Combine($PSScriptRoot, 'Source', 'Posh-SSH')
$IsUnix = $PSEdition -eq 'Core' -and -not $IsWindows

$manifestItem = Get-Item ([IO.Path]::Combine($PowerShellPath, '*.psd1'))
$Manifest = Test-ModuleManifest -Path $manifestItem.FullName -ErrorAction Ignore -WarningAction Ignore
$Version = $Manifest.Version
$ModuleName = $manifestItem.BaseName
$ReleasePath = [IO.Path]::Combine($BuildPath, $ModuleName, $Version)

[xml]$csharpProjectInfo = Get-Content ([IO.Path]::Combine($CSharpPath, '*.csproj'))
$TargetFrameworks = @($csharpProjectInfo.Project.PropertyGroup.TargetFrameworks[0].Split(
        ';', [StringSplitOptions]::RemoveEmptyEntries))
$PSFramework = $TargetFrameworks[0]

task Clean {
    if (Test-Path $ReleasePath) {
        Remove-Item $ReleasePath -Recurse -Force
    }

    New-Item -ItemType Directory $ReleasePath | Out-Null
}

task BuildDocs {
    $helpParams = @{
        Path       = [IO.Path]::Combine($PSScriptRoot, 'docs')
        OutputPath = [IO.Path]::Combine($ReleasePath, 'en-US')
    }
    New-ExternalHelp @helpParams | Out-Null
}

task BuildManaged {
    Push-Location -Path $CSharpPath
    $arguments = @(
        'publish'
        '--configuration', $Configuration
        '--verbosity', 'q'
        '-nologo'
        "-p:Version=$Version"
    )
    try {
        foreach ($framework in $TargetFrameworks) {
            Write-Host "Compiling for $framework"
            dotnet @arguments --framework $framework

            if ($LASTEXITCODE) {
                throw "Failed to compiled code for $framework"
            }
        }
    }
    finally {
        Pop-Location
    }
}

task CopyToRelease {
    $copyParams = @{
        Path        = [IO.Path]::Combine($PowerShellPath, '*')
        Destination = $ReleasePath
        Recurse     = $true
        Force       = $true
    }
    Copy-Item @copyParams

    foreach ($framework in $TargetFrameworks) {
        $buildFolder = [IO.Path]::Combine($CSharpPath, 'bin', $Configuration, $framework, 'publish')
        $binFolder = [IO.Path]::Combine($ReleasePath, 'bin', $framework)
        if (-not (Test-Path -LiteralPath $binFolder)) {
            New-Item -Path $binFolder -ItemType Directory | Out-Null
        }
        Copy-Item ([IO.Path]::Combine($buildFolder, "*")) -Destination $binFolder
    }
}

task Sign {
    $certPath = $env:PSMODULE_SIGNING_CERT
    $certPassword = $env:PSMODULE_SIGNING_CERT_PASSWORD
    if (-not $certPath -or -not $certPassword) {
        return
    }

    [byte[]]$certBytes = [System.Convert]::FromBase64String($env:PSMODULE_SIGNING_CERT)
    $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($certBytes, $certPassword)
    $signParams = @{
        Certificate     = $cert
        TimestampServer = 'http://timestamp.digicert.com'
        HashAlgorithm   = 'SHA256'
    }

    Get-ChildItem -LiteralPath $ReleasePath -Recurse -ErrorAction SilentlyContinue |
        Where-Object Extension -in ".ps1", ".psm1", ".psd1", ".ps1xml", ".dll" |
        ForEach-Object -Process {
            $result = Set-AuthenticodeSignature -LiteralPath $_.FullName @signParams
            if ($result.Status -ne "Valid") {
                throw "Failed to sign $($_.FullName) - Status: $($result.Status) Message: $($result.StatusMessage)"
            }
        }
}

task Package {
    $nupkgPath = [IO.Path]::Combine($BuildPath, "$ModuleName.$Version*.nupkg")
    if (Test-Path $nupkgPath) {
        Remove-Item $nupkgPath -Force
    }

    $repoParams = @{
        Name               = 'LocalRepo'
        SourceLocation     = $BuildPath
        PublishLocation    = $BuildPath
        InstallationPolicy = 'Trusted'
    }
    if (Get-PSRepository -Name $repoParams.Name -ErrorAction SilentlyContinue) {
        Unregister-PSRepository -Name $repoParams.Name
    }

    Register-PSRepository @repoParams
    try {
        Publish-Module -Path $ReleasePath -Repository $repoParams.Name
    }
    finally {
        Unregister-PSRepository -Name $repoParams.Name
    }
}

task Analyze {
    $pssaSplat = @{
        Path        = $ReleasePath
        Settings    = [IO.Path]::Combine($PSScriptRoot, 'ScriptAnalyzerSettings.psd1')
        Recurse     = $true
        ErrorAction = 'SilentlyContinue'
    }
    $results = Invoke-ScriptAnalyzer @pssaSplat
    if ($null -ne $results) {
        $results | Out-String
        throw "Failed PsScriptAnalyzer tests, build failed"
    }
}

task DoTest {
    $resultsPath = [IO.Path]::Combine($BuildPath, 'TestResults')
    if (-not (Test-Path $resultsPath)) {
        New-Item $resultsPath -ItemType Directory -ErrorAction Stop | Out-Null
    }

    $resultsFile = [IO.Path]::Combine($resultsPath, 'Pester.xml')
    if (Test-Path $resultsFile) {
        Remove-Item $resultsFile -ErrorAction Stop -Force
    }

    $pesterScript = [IO.Path]::Combine($PSScriptRoot, 'tools', 'PesterTest.ps1')
    $pwsh = [Environment]::GetCommandLineArgs()[0] -replace '\.dll$', ''
    $arguments = @(
        '-NoProfile'
        '-NonInteractive'
        if (-not $IsUnix) {
            '-ExecutionPolicy', 'Bypass'
        }
        '-File', ('"{0}"' -f $pesterScript)
        '-TestPath', ('"{0}"' -f [IO.Path]::Combine($PSScriptRoot, 'tests'))
        '-OutputFile', ('"{0}"' -f $resultsFile)
    )

    if ($Configuration -eq 'Debug') {
        # We use coverlet to collect code coverage of our binary
        $unitCoveragePath = [IO.Path]::Combine($resultsPath, 'UnitCoverage.json')

        $arguments = @(
            '"{0}"' -f ([IO.Path]::Combine($ReleasePath, 'bin', $PSFramework))
            '--target', $pwsh
            '--targetargs', (($arguments -join " ") -replace '"', '\"')
            '--output', ([IO.Path]::Combine($resultsPath, 'Coverage.xml'))
            '--format', 'cobertura'
            if (Test-Path -LiteralPath $unitCoveragePath) {
                '--merge-with', $unitCoveragePath
            }
        )
        $pwsh = 'coverlet'
    }

    &$pwsh $arguments
    if ($LASTEXITCODE) {
        throw "Pester failed tests"
    }
}

task Build -Jobs Clean, BuildManaged, CopyToRelease, BuildDocs, Sign, Package

# FIXME: Work out why we need the obj and bin folder for coverage to work
task Test -Jobs BuildManaged, Analyze, DoTest

task . Build
