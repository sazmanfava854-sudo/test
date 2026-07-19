# آزاد کردن پورت 5000 در صورت اشغال بودن (معمولاً نمونه قبلی dotnet run)
param(
    [int]$Port = 5000
)

$ErrorActionPreference = 'SilentlyContinue'

function Get-ListeningPids {
    param([int]$Port)

    $pids = @()

    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        $pids = @(Get-NetTCPConnection -LocalPort $Port -State Listen |
            Select-Object -ExpandProperty OwningProcess -Unique)
    }

    if ($pids.Count -eq 0) {
        $pattern = ":$Port\s"
        netstat -ano |
            Select-String 'LISTENING' |
            Select-String $pattern |
            ForEach-Object {
                $parts = ($_ -split '\s+') | Where-Object { $_ }
                if ($parts.Count -ge 1) {
                    $last = $parts[-1]
                    if ($last -match '^\d+$') {
                        $pids += [int]$last
                    }
                }
            }
    }

    return @($pids | Where-Object { $_ -gt 0 } | Select-Object -Unique)
}

$pids = Get-ListeningPids -Port $Port
if ($pids.Count -eq 0) {
    exit 0
}

$failed = $false

foreach ($procId in $pids) {
    $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
    $name = if ($proc) { $proc.ProcessName } else { 'unknown' }
    Write-Host "[Port $Port] در حال استفاده توسط PID $procId ($name) - در حال آزادسازی..."

    Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
    if (Get-Process -Id $procId -ErrorAction SilentlyContinue) {
        Write-Host "خطا: امکان توقف PID $procId نیست."
        $failed = $true
    } else {
        Write-Host "پروسه $procId متوقف شد."
    }
}

if ($failed) {
    Write-Host ""
    Write-Host "پورت $Port هنوز اشغال است. دستی اجرا کنید:"
    Write-Host "  netstat -ano | findstr :$Port"
    Write-Host "  taskkill /PID <pid> /F"
    exit 1
}

Start-Sleep -Seconds 1
exit 0
