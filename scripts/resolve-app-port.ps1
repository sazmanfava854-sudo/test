# انتخاب پورت آزاد برای HR Performance — فقط عدد پورت را در stdout چاپ می‌کند
param(
    [int[]]$PreferredPorts = @(5000, 5280, 5080, 5180, 5050)
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$freePortScript = Join-Path $scriptDir 'free-port.ps1'

function Log([string]$Message) {
    [Console]::Error.WriteLine($Message)
}

function Test-PortListening {
    param([int]$Port)

    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        $conn = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        return $null -ne $conn -and @($conn).Count -gt 0
    }

    $matches = netstat -ano |
        Select-String 'LISTENING' |
        Select-String ":$Port\s"
    return $null -ne $matches -and $matches.Count -gt 0
}

foreach ($port in $PreferredPorts) {
    if (-not (Test-PortListening -Port $port)) {
        Write-Output $port
        exit 0
    }

    Log "پورت $port اشغال است — تلاش برای آزادسازی..."
    & $freePortScript -Port $port 1>$null
    Start-Sleep -Milliseconds 400

    if (-not (Test-PortListening -Port $port)) {
        Log "پورت $port آزاد شد."
        Write-Output $port
        exit 0
    }

    Log "پورت $port هنوز اشغال است — پورت بعدی..."
}

Log "هیچ پورت آزادی یافت نشد. پورت‌های امتحان‌شده: $($PreferredPorts -join ', ')"
exit 1
