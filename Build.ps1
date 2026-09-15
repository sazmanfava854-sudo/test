# PowerShell build — does not use cmd.exe
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = $null
if (Test-Path -LiteralPath $vswhere) {
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $msbuild) {
  foreach ($p in @(
      "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
      "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
      "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
      "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )) {
    if (Test-Path -LiteralPath $p) { $msbuild = $p; break }
  }
}
if (-not $msbuild) {
  Write-Host 'MSBuild پیدا نشد. RuleTrace.sln را در Visual Studio باز کنید و F5 بزنید.'
  exit 1
}

Write-Host "MSBuild: $msbuild"
& $msbuild RuleTrace.sln /nologo /v:m /t:Rebuild /p:Configuration=Release /p:Platform='Any CPU'
if ($LASTEXITCODE -ne 0) { throw 'BUILD FAILED' }

$exe = Join-Path $PSScriptRoot 'bin\RuleTrace.exe'
Write-Host "OK -> $exe"
Start-Process -FilePath $exe
