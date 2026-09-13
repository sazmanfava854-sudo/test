# Build RuleTrace → bin\RuleTrace.exe (with Sara DLLs copied, *.dll.config removed)
param(
    [string]$DllPath = "C:\Users\sadathoseini-sh\Desktop\dll10",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild `
    -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1

if (-not $msbuild) { throw "MSBuild not found. Install Visual Studio Build Tools." }

& $msbuild "RuleTrace.sln" /t:Rebuild /p:Configuration=$Configuration /p:Platform="Any CPU" /p:DllPath="$DllPath"

Write-Host ""
Write-Host "Output: $root\bin\RuleTrace.exe"
Write-Host "Run:    cd bin; .\RuleTrace.exe --help"
