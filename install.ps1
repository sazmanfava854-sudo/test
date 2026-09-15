# Copy RuleTrace to C:\ruletrace (no parentheses in path) then build.
$ErrorActionPreference = 'Stop'
$dest = 'C:\ruletrace'
$src = $PSScriptRoot

Write-Host "Copying from:"
Write-Host "  $src"
Write-Host "to:"
Write-Host "  $dest"
Write-Host ''

if (-not (Test-Path -LiteralPath (Join-Path $src 'RuleTrace.sln'))) {
    Write-Host 'ERROR: RuleTrace.sln not found next to install.ps1'
    Write-Host 'Open the INNER folder from the GitHub ZIP (the one with .sln and build.cmd).'
    exit 1
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item -LiteralPath (Join-Path $src '*') -Destination $dest -Recurse -Force

Write-Host 'Copy OK. Building...'
Write-Host ''
Set-Location -LiteralPath $dest
& (Join-Path $dest 'build.ps1')
