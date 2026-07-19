# انتخاب پورت آزاد برای HR Performance
param(
    [int[]]$PreferredPorts = @(5000, 5280, 5080, 5180, 5050),
    [string]$OutFile = ''
)

$ErrorActionPreference = 'Continue'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$freePortScript = Join-Path $scriptDir 'free-port.ps1'

function Write-Port([int]$Port) {
    if ($OutFile) {
        [System.IO.File]::WriteAllText($OutFile, "$Port")
    }
    [Console]::Out.WriteLine($Port)
}

function Test-PortListening {
    param([int]$Port)

    try {
        if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
            $conn = @(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
            if ($conn.Count -gt 0) { return $true }
        }
    } catch {
        # fallback to netstat
    }

    try {
        $lines = netstat -ano |
            Select-String 'LISTENING' |
            Select-String ":$Port\s"
        return $null -ne $lines -and @($lines).Count -gt 0
    } catch {
        return $false
    }
}

foreach ($port in $PreferredPorts) {
    if (-not (Test-PortListening -Port $port)) {
        Write-Port $port
        exit 0
    }

    if (Test-Path $freePortScript) {
        try {
            & $freePortScript -Port $port *> $null
        } catch {
            # ignore
        }
        Start-Sleep -Milliseconds 400
    }

    if (-not (Test-PortListening -Port $port)) {
        Write-Port $port
        exit 0
    }
}

# fallback: still return 5280 and let dotnet report bind error
Write-Port 5280
exit 0
