# اگر قبلاً publish کرده‌اید — فقط اجرا (بدون build)
$exe = Join-Path $PSScriptRoot "publish\RayvarzResend.Web.exe"
if (-not (Test-Path $exe)) {
    Write-Host "اول publish-and-run.ps1 را یک‌بار اجرا کنید." -ForegroundColor Yellow
    exit 1
}
Set-Location (Split-Path $exe)
& $exe
