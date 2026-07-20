# آزاد کردن پورت — فقط پروسه‌های مرتبط با dotnet/HR را متوقف می‌کند
param(
    [Parameter(Mandatory = $true)]
    [int]$Port
)

$ErrorActionPreference = 'SilentlyContinue'
$safeProcessNames = @('dotnet', 'HRPerformance.API', 'iisexpress', 'w3wp')

function Get-ListeningPids {
    param([int]$Port)

    $pids = [System.Collections.Generic.HashSet[int]]::new()

    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
            ForEach-Object { [void]$pids.Add([int]$_.OwningProcess) }
    }

    if ($pids.Count -eq 0) {
        netstat -ano |
            Select-String 'LISTENING' |
            Select-String ":$Port\s" |
            ForEach-Object {
                $parts = ($_ -split '\s+') | Where-Object { $_ }
                if ($parts.Count -ge 1 -and $parts[-1] -match '^\d+$') {
                    [void]$pids.Add([int]$parts[-1])
                }
            }
    }

    return @($pids | Where-Object { $_ -gt 0 })
}

function Test-PortListening {
    param([int]$Port)
    return (Get-ListeningPids -Port $Port).Count -gt 0
}

if (-not (Test-PortListening -Port $Port)) {
    exit 0
}

$failed = $false

foreach ($procId in (Get-ListeningPids -Port $Port)) {
    $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
    $name = if ($proc) { $proc.ProcessName } else { 'unknown' }

    if ($proc -and $safeProcessNames -notcontains $proc.ProcessName) {
        Write-Host "[Port $Port] اشغال توسط $name (PID $procId) — این سرویس سیستمی است و متوقف نمی‌شود."
        $failed = $true
        continue
    }

    Write-Host "[Port $Port] آزادسازی PID $procId ($name)..."
    Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 300

    if (Get-Process -Id $procId -ErrorAction SilentlyContinue) {
        Write-Host "خطا: امکان توقف PID $procId نیست (شاید نیاز به Run as Administrator)."
        $failed = $true
    } else {
        Write-Host "پروسه $procId متوقف شد."
    }
}

Start-Sleep -Milliseconds 500

if (Test-PortListening -Port $Port) {
    Write-Host ""
    Write-Host "پورت $Port هنوز اشغال است."
    Write-Host "  netstat -ano | findstr :$Port"
    Write-Host '  taskkill /PID <pid> /F'
    exit 1
}

exit 0
