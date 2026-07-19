# سازگاری با نسخه‌های قبلی — از free-port.ps1 استفاده می‌کند
param(
    [int]$Port = 5000
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
& (Join-Path $scriptDir 'free-port.ps1') -Port $Port
exit $LASTEXITCODE
