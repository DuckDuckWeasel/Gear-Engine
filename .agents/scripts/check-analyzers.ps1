[CmdletBinding()]
param(
    [string]$ProjectPath = (Get-Location).Path,
    [int]$TimeoutMinutes = 10,
    [int]$AnalyzerTestsTimeoutMinutes = 10,
    [switch]$IncludeTestAssemblies,
    [string[]]$BuildProjectNames = @(
        "Scaffold.VisualScripting.Core.csproj",
        "Scaffold.VisualScripting.Authoring.csproj",
        "Scaffold.VisualScripting.Unity.csproj",
        "Scaffold.VisualScripting.Editor.csproj"
    ),
    # Use when Windows Application Control / WDAC blocks Scaffold.Mvvm.Analyzers.dll during dotnet test (0x800711C7).
    [switch]$SkipMvvmAnalyzerTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resolvedProjectPath = (Resolve-Path $ProjectPath).Path
$agentsDir = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$testingSuiteConfigScript = Join-Path $agentsDir (Join-Path "testing" "TestingSuite.Config.ps1")
. $testingSuiteConfigScript
$testingSuiteConfig = Get-TestingSuiteConfig -ProjectPath $resolvedProjectPath
$analyzerTestsProjectPath = Join-Path $resolvedProjectPath "Analyzers/Scaffold/Scaffold.Analyzers.Tests/Scaffold.Analyzers.Tests.csproj"
$mvvmAnalyzerTestsProjectPath = Join-Path $resolvedProjectPath "Generators/Scaffold.Mvvm.Analyzers.Tests/Scaffold.Mvvm.Analyzers.Tests.csproj"

function Resolve-SolutionPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath
    )

    $solutionFiles = @(Get-ChildItem -Path $ResolvedProjectPath -Filter "*.sln" -File | Sort-Object -Property FullName)
    if ($solutionFiles.Count -eq 0) {
        return $null
    }

    if ($solutionFiles.Count -eq 1) {
        return $solutionFiles[0]
    }

    $projectFolderName = Split-Path -Path $ResolvedProjectPath -Leaf
    $preferredName = "$projectFolderName.sln"
    $preferredMatch = $solutionFiles | Where-Object { $_.Name -ieq $preferredName } | Select-Object -First 1
    if ($preferredMatch) {
        return $preferredMatch
    }

    return $solutionFiles[0]
}

function New-AnalyzerSolutionCopy {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$Solution,
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath
    )

    $copyPath = Join-Path $ResolvedProjectPath (
        ".AnalyzerGate-" + [guid]::NewGuid().ToString("N") + ".sln")
    $normalizedLines = foreach ($line in Get-Content -LiteralPath $Solution.FullName) {
        if ($line -match '^Project\("(?<type>[^"]+)"\) = "[^"]+", "(?<path>[^"]+\.csproj)", "(?<id>[^"]+)"$') {
            $projectName = [System.IO.Path]::GetFileNameWithoutExtension(
                $matches['path'])
            'Project("{0}") = "{1}", "{2}", "{3}"' -f
                $matches['type'],
                $projectName,
                $matches['path'],
                $matches['id']
        } else {
            $line
        }
    }

    [System.IO.File]::WriteAllLines($copyPath, $normalizedLines)
    return $copyPath
}

function Try-GetRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,
        [Parameter(Mandatory = $true)]
        [string]$CandidatePath
    )

    try {
        $resolvedCandidate = (Resolve-Path $CandidatePath -ErrorAction Stop).Path
    } catch {
        return $CandidatePath -replace "\\", "/"
    }

    $baseUri = New-Object System.Uri(($BasePath.TrimEnd('\') + '\'))
    $candidateUri = New-Object System.Uri($resolvedCandidate)
    if ($baseUri.IsBaseOf($candidateUri)) {
        $relative = $baseUri.MakeRelativeUri($candidateUri).ToString()
        return [System.Uri]::UnescapeDataString($relative) -replace "\\", "/"
    }

    return $resolvedCandidate -replace "\\", "/"
}

function Get-ProjectPathFromBuildLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    if ($Line -match "\[(?<project>[^\]]+\.csproj)\]\s*$") {
        return $matches['project']
    }

    return $null
}

function Is-TestAssemblyProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    return $projectName -match "(?i)(^|[._-])(tests?|playmodetests?|editmodetests?)([._-]|$)"
}

function Should-IncludeBuildLine {
    param(
        [string]$Line,
        [Parameter(Mandatory = $true)]
        [bool]$IncludeTests
    )

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $true
    }

    if ($IncludeTests) {
        return $true
    }

    $projectPath = Get-ProjectPathFromBuildLine -Line $Line
    if ([string]::IsNullOrWhiteSpace($projectPath)) {
        return $true
    }

    return -not (Is-TestAssemblyProject -ProjectPath $projectPath)
}

function Invoke-DotNet {
    <#
        Runs `dotnet ...` directly with asynchronous output reads, avoiding pipe deadlocks
        while preserving the real process exit code on Windows, macOS, and Linux.
        Returns @{ ExitCode = int; LogPath = string }.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$DotNetArguments,
        [Parameter(Mandatory = $true)]
        [string]$LogFilePath,
        [int]$TimeoutMilliseconds = -1
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    foreach ($argument in $DotNetArguments) {
        if ($null -ne $argument) {
            [void]$psi.ArgumentList.Add($argument)
        }
    }
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.Environment["DOTNET_ROLL_FORWARD"] = "Major"

    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $psi
    [void]$p.Start()
    $standardOutputTask = $p.StandardOutput.ReadToEndAsync()
    $standardErrorTask = $p.StandardError.ReadToEndAsync()
    $timedOut = $false

    if ($TimeoutMilliseconds -gt 0) {
        $didExit = $p.WaitForExit($TimeoutMilliseconds)
        if (-not $didExit) {
            $timedOut = $true
            try {
                $p.Kill($true)
            } catch {
                try {
                    $p.Kill()
                } catch {
                }
            }
            $p.WaitForExit()
        }
    } else {
        $p.WaitForExit()
    }

    $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
    $standardError = $standardErrorTask.GetAwaiter().GetResult()
    $combinedOutput = $standardOutput
    if (-not [string]::IsNullOrEmpty($standardError)) {
        if (-not [string]::IsNullOrEmpty($combinedOutput) -and
            -not $combinedOutput.EndsWith([Environment]::NewLine)) {
            $combinedOutput += [Environment]::NewLine
        }
        $combinedOutput += $standardError
    }
    [System.IO.File]::WriteAllText($LogFilePath, $combinedOutput)

    $exitCode = if ($timedOut) { -1 } else { [int]$p.ExitCode }
    return @{ ExitCode = $exitCode; LogPath = $LogFilePath }
}

function Sync-UnityScriptAssemblyOutputs {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$BuildProjects
    )

    $scriptAssembliesPath = Join-Path $ResolvedProjectPath "Library/ScriptAssemblies"
    if (-not (Test-Path -LiteralPath $scriptAssembliesPath)) {
        return 0
    }

    $referenceNames = @(
        foreach ($buildProject in $BuildProjects) {
            foreach ($match in Select-String `
                -LiteralPath $buildProject `
                -Pattern '<ProjectReference Include="(?<path>[^"]+\.csproj)"' `
                -AllMatches) {
                foreach ($projectMatch in $match.Matches) {
                    [System.IO.Path]::GetFileNameWithoutExtension(
                        $projectMatch.Groups["path"].Value)
                }
            }
        }
    ) | Sort-Object -Unique

    $copiedCount = 0
    foreach ($referenceName in $referenceNames) {
        $sourcePath = Join-Path $scriptAssembliesPath ($referenceName + ".dll")
        if (-not (Test-Path -LiteralPath $sourcePath)) {
            continue
        }

        $outputDirectory = Join-Path $ResolvedProjectPath (
            "Temp/Bin/Debug/" + $referenceName)
        $null = New-Item -ItemType Directory -Path $outputDirectory -Force
        Copy-Item `
            -LiteralPath $sourcePath `
            -Destination (Join-Path $outputDirectory ($referenceName + ".dll")) `
            -Force
        $copiedCount++
    }

    return $copiedCount
}

# Runs analyzer unit tests, then builds the solution and prints deduplicated Scaffold analyzer diagnostics (SCA + SCM).
# Output format (parseable):
#   BUILD_EXIT:<code>
#   TOTAL:<n>                    (SCA + SCM rule hits combined)
#   RULE:<code>:<count>
#   FILE:<relative-path>:<count>
#   DIAG:<raw diagnostic line>
#   BLOCKER:<raw error line>
$analyzerTestsProjects = @(
    @{ Path = $analyzerTestsProjectPath; Label = "Scaffold.Analyzers.Tests" }
    @{ Path = $mvvmAnalyzerTestsProjectPath; Label = "Scaffold.Mvvm.Analyzers.Tests" }
) | Where-Object { Test-Path $_.Path }

if ($SkipMvvmAnalyzerTests.IsPresent) {
    $analyzerTestsProjects = @($analyzerTestsProjects | Where-Object { $_.Label -ne "Scaffold.Mvvm.Analyzers.Tests" })
    Write-Output "NOTE:Skipping Scaffold.Mvvm.Analyzers.Tests (SkipMvvmAnalyzerTests). Run those tests on a machine where the analyzer DLL is not blocked by policy."
}

# Normalize to a real array so .Count is safe under Set-StrictMode when the pipeline yields $null or a single item.
if ($null -eq $analyzerTestsProjects) {
    $analyzerTestsProjects = @()
}
elseif ($analyzerTestsProjects -isnot [array]) {
    $analyzerTestsProjects = @($analyzerTestsProjects)
}

if ($analyzerTestsProjects.Count -eq 0) {
    Write-Output ("NOTE:No analyzer test projects found (e.g. '{0}'). Analyzer unit tests skipped." -f $analyzerTestsProjectPath)
}

if ($analyzerTestsProjects.Count -gt 0) {
    if ($AnalyzerTestsTimeoutMinutes -lt 1) {
        Write-Output "TOTAL:-1"
        Write-Output "BLOCKER:AnalyzerTestsTimeoutMinutes must be 1 or greater."
        exit 1
    }

    foreach ($testsProject in $analyzerTestsProjects) {
        $analyzerTestsOutput = @()
        $testsTempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("dotnet-test-analyzers-" + [guid]::NewGuid().ToString("N"))
        $null = New-Item -ItemType Directory -Path $testsTempRoot -Force
        $testsLogPath = Join-Path $testsTempRoot "dotnet.log"
        $analyzerTestsExitCode = 1

        try {
            $testsTimeoutMilliseconds = $AnalyzerTestsTimeoutMinutes * 60 * 1000
            $testRun = Invoke-DotNet `
                -DotNetArguments @("test", $testsProject.Path, "-c", "Release", "--nologo") `
                -LogFilePath $testsLogPath `
                -TimeoutMilliseconds $testsTimeoutMilliseconds

            $analyzerTestsExitCode = $testRun.ExitCode

            if ($analyzerTestsExitCode -eq -1) {
                Write-Output "TOTAL:-1"
                Write-Output ("BLOCKER:Analyzer tests ({0}) timed out after {1} minute(s)." -f $testsProject.Label, $AnalyzerTestsTimeoutMinutes)
                exit 1
            }

            if (Test-Path $testsLogPath) {
                $analyzerTestsOutput += @(Get-Content $testsLogPath -ErrorAction SilentlyContinue)
            }
        }
        finally {
            if (Test-Path $testsTempRoot) {
                try {
                    [System.IO.Directory]::Delete($testsTempRoot, $true)
                } catch {
                    Start-Sleep -Milliseconds 250
                    try {
                        [System.IO.Directory]::Delete($testsTempRoot, $true)
                    } catch {
                    }
                }
            }
        }

        if ($analyzerTestsExitCode -ne 0) {
            Write-Output "TOTAL:-1"
            Write-Output ("BLOCKER:Analyzer tests ({0}) failed (exit code {1})." -f $testsProject.Label, $analyzerTestsExitCode)
            foreach ($line in $analyzerTestsOutput) {
                if ([string]::IsNullOrWhiteSpace($line)) { continue }
                Write-Output ("BLOCKER:{0}" -f $line)
            }
            exit 1
        }

        Write-Output ("NOTE:Analyzer tests passed ({0})." -f $testsProject.Label)
    }
}

$buildProjects = @(
    $BuildProjectNames |
        ForEach-Object { Join-Path $resolvedProjectPath $_ } |
        Where-Object { Test-Path -LiteralPath $_ }
)
if ($buildProjects.Count -eq 0) {
    Write-Output "TOTAL:0"
    Write-Output "NOTE:No configured analyzer build projects were found. Analyzer build skipped."
    exit 0
}

Write-Output ("NOTE:Building {0} configured project(s) without transitive project rebuilds." -f $buildProjects.Count)
$syncedAssemblyCount = Sync-UnityScriptAssemblyOutputs `
    -ResolvedProjectPath $resolvedProjectPath `
    -BuildProjects $buildProjects
Write-Output ("NOTE:Synced {0} Unity ScriptAssemblies dependency output(s)." -f $syncedAssemblyCount)
if (-not $IncludeTestAssemblies.IsPresent) {
    Write-Output "NOTE:Excluding diagnostics from test assemblies (use -IncludeTestAssemblies to include them)."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("dotnet-build-check-" + [guid]::NewGuid().ToString("N"))
$null = New-Item -ItemType Directory -Path $tempRoot -Force
$buildOutput = @()
$buildExitCode = 0

try {
    if ($TimeoutMinutes -lt 1) {
        throw "TimeoutMinutes must be 1 or greater."
    }

    $buildTimeoutMilliseconds = $TimeoutMinutes * 60 * 1000
    for ($projectIndex = 0; $projectIndex -lt $buildProjects.Count; $projectIndex++) {
        $buildProject = $buildProjects[$projectIndex]
        $buildLogPath = Join-Path $tempRoot ("build-{0}.log" -f $projectIndex)
        $buildRun = Invoke-DotNet `
            -DotNetArguments @(
                "build",
                $buildProject,
                "--no-incremental",
                "-p:BuildProjectReferences=false",
                "-p:MaxCpuCount=1",
                "--verbosity:minimal",
                "-warnAsMessage:MSB3277"
            ) `
            -LogFilePath $buildLogPath `
            -TimeoutMilliseconds $buildTimeoutMilliseconds

        if ($buildRun.ExitCode -eq -1) {
            Write-Output "BUILD_EXIT:-1"
            Write-Output "TOTAL:-1"
            Write-Output ("BLOCKER:Analyzer build for '{0}' timed out after {1} minute(s)." -f
                ([System.IO.Path]::GetFileName($buildProject)),
                $TimeoutMinutes)
            exit 1
        }

        if ($buildRun.ExitCode -ne 0) {
            $buildExitCode = $buildRun.ExitCode
        }

        if (Test-Path $buildLogPath) {
            $buildOutput += @(
                Get-Content $buildLogPath -ErrorAction SilentlyContinue |
                    Where-Object {
                        $_ -match ": (warning|error) (SCA[0-9]+|SCM[0-9]+)" -or
                        $_ -match "\berror\b"
                    }
            )
        }
    }
}
finally {
    if (Test-Path $tempRoot) {
        try {
            [System.IO.Directory]::Delete($tempRoot, $true)
        } catch {
            Start-Sleep -Milliseconds 250
            try {
                [System.IO.Directory]::Delete($tempRoot, $true)
            } catch {
            }
        }
    }
}

Write-Output "BUILD_EXIT:$buildExitCode"

$filteredBuildOutput = $buildOutput |
    Where-Object { Should-IncludeBuildLine -Line $_ -IncludeTests $IncludeTestAssemblies.IsPresent }

# SCA = Scaffold.Analyzers; SCM = Scaffold.Mvvm.Analyzers (MVVM pack)
$analyzerDiagnosticPattern = ": (warning|error) (SCA[0-9]+|SCM[0-9]+)"
$scaffoldAnalyzerLines = $filteredBuildOutput |
    Where-Object { $_ -match $analyzerDiagnosticPattern } |
    Sort-Object -Unique

$analyzerLineFilterSubstring = $testingSuiteConfig.AnalyzerIncludeOnlyLineContainsSubstring
if (-not [string]::IsNullOrWhiteSpace($analyzerLineFilterSubstring)) {
    $scaffoldAnalyzerLines = @(
        $scaffoldAnalyzerLines |
            Where-Object { $_ -like ('*' + $analyzerLineFilterSubstring + '*') }
    )
}

$analyzerExcludeSubstrings = @($testingSuiteConfig.AnalyzerExcludeIfLineContainsSubstrings | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($analyzerExcludeSubstrings.Count -gt 0) {
    $scaffoldAnalyzerLines = @(
        $scaffoldAnalyzerLines |
            Where-Object {
                $normalizedLine = $_.Replace('\', '/')
                $excluded = $false
                foreach ($sub in $analyzerExcludeSubstrings) {
                    $normalizedSub = $sub.Replace('\', '/')
                    if ($normalizedLine -like ('*' + $normalizedSub + '*')) {
                        $excluded = $true
                        break
                    }
                }
                -not $excluded
            }
    )
}

$total = if ($null -eq $scaffoldAnalyzerLines) { 0 } elseif ($scaffoldAnalyzerLines -is [array]) { $scaffoldAnalyzerLines.Count } else { 1 }
Write-Output "TOTAL:$total"

$ruleCounts = @{}
$fileCounts = @{}

foreach ($line in $scaffoldAnalyzerLines) {
    if ($line -match "\b(SCA[0-9]+|SCM[0-9]+)\b") {
        $rule = $matches[1]
        if ($ruleCounts.ContainsKey($rule)) {
            $ruleCounts[$rule] += 1
        } else {
            $ruleCounts[$rule] = 1
        }
    }

    $file = $null
    if ($line -match "(?<path>[A-Za-z]:\\[^:(]+\.cs)\(") {
        $file = Try-GetRelativePath -BasePath $resolvedProjectPath -CandidatePath $matches['path']
    } elseif ($line -match "(?<path>[^:\s][^:()]*\.cs)\(") {
        $file = ($matches['path'] -replace "\\", "/")
    }

    if ($file) {
        if ($fileCounts.ContainsKey($file)) {
            $fileCounts[$file] += 1
        } else {
            $fileCounts[$file] = 1
        }
    }
}

foreach ($entry in $ruleCounts.GetEnumerator() | Sort-Object -Property @{Expression='Value';Descending=$true}, @{Expression='Key';Descending=$false}) {
    Write-Output "RULE:$($entry.Key):$($entry.Value)"
}

foreach ($entry in $fileCounts.GetEnumerator() | Sort-Object -Property @{Expression='Value';Descending=$true}, @{Expression='Key';Descending=$false}) {
    Write-Output "FILE:$($entry.Key):$($entry.Value)"
}

foreach ($line in $scaffoldAnalyzerLines) {
    Write-Output "DIAG:$line"
}

# Compiler / tooling errors (not SCA/SCM analyzer codes). MSBuild engine errors use MSBxxxx.
$blockers = $filteredBuildOutput |
    Where-Object {
        $_ -match "\berror\b" -and
        $_ -notmatch "^\s*[0-9]+\s+Error\(s\)\s*$" -and
        $_ -notmatch ": error SCA[0-9]+" -and
        $_ -notmatch ": error SCM[0-9]+"
    } |
    Sort-Object -Unique

foreach ($line in $blockers) {
    Write-Output "BLOCKER:$line"
}

$scriptFailed = $false
if ($buildExitCode -ne 0) {
    $scriptFailed = $true
    Write-Output ("NOTE:Solution build failed with exit code {0}." -f $buildExitCode)
}

if (@($blockers).Count -gt 0) {
    $scriptFailed = $true
}

if ($scriptFailed) {
    exit 1
}
