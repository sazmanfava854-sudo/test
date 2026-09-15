# RuleTrace build (PowerShell) — works when cmd.exe breaks on paths like ...-88fc(44)\...
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

Write-Host '============================================'
Write-Host ' RuleTrace build  v21c-cross-class'
Write-Host " Folder: $PWD"
Write-Host '============================================'

if (-not (Test-Path -LiteralPath 'RuleTrace.sln')) {
    Write-Host ''
    Write-Host 'ERROR: RuleTrace.sln not found.'
    Write-Host 'Run install.ps1 to copy to C:\ruletrace, or open the inner GitHub ZIP folder.'
    exit 1
}

$msbuild = $null
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path -LiteralPath $vswhere) {
    $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if ($found) { $msbuild = $found }
}

if (-not $msbuild) {
    $candidates = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path -LiteralPath $c) { $msbuild = $c; break }
    }
}

if (-not $msbuild) {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) { $msbuild = 'dotnet'; $msbuildArgs = @('msbuild') }
}

if (-not $msbuild) {
    Write-Host ''
    Write-Host 'ERROR: MSBuild not found. Install Visual Studio Build Tools (.NET desktop).'
    exit 1
}

Write-Host "MSBuild: $msbuild"
Write-Host ''

if ($msbuild -eq 'dotnet') {
    & dotnet msbuild 'RuleTrace.sln' /nologo /v:m /t:Rebuild /p:Configuration=Release '/p:Platform=Any CPU'
} else {
    & $msbuild 'RuleTrace.sln' /nologo /v:m /t:Rebuild /p:Configuration=Release '/p:Platform=Any CPU'
}

if ($LASTEXITCODE -ne 0) {
    Write-Host ''
    Write-Host 'BUILD FAILED'
    exit $LASTEXITCODE
}

if (-not (Test-Path -LiteralPath 'bin\RuleTrace.exe')) {
    Write-Host 'ERROR: bin\RuleTrace.exe missing after build'
    exit 1
}

Write-Host ''
Write-Host '============================================'
Write-Host " OK  ->  $PWD\bin\RuleTrace.exe"
Write-Host ' Window title must be: RuleTrace v21c-cross-class'
Write-Host '============================================'
Start-Process -LiteralPath (Join-Path $PWD 'bin\RuleTrace.exe')
