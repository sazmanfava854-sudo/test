# تست اتصال SQL — Integrated Security (همان IIS App Pool)
param(
    [string]$Server = "172.16.10.232,1433",
    [string]$Database = "HRPerformanceDB"
)

$whoami = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
Write-Host "Windows user: $whoami" -ForegroundColor Cyan

$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=10"
Write-Host "Server: $Server"
Write-Host "Database: $Database"
Write-Host ""

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT DB_NAME() AS Db, SUSER_SNAME() AS SqlLogin, SYSTEM_USER AS SystemUser"
    $reader = $cmd.ExecuteReader()
    if ($reader.Read()) {
        Write-Host "OK — اتصال موفق" -ForegroundColor Green
        Write-Host "  Database : $($reader['Db'])"
        Write-Host "  SQL login: $($reader['SqlLogin'])"
    }
    $reader.Close()
    $conn.Close()
    exit 0
}
catch {
    Write-Host "FAIL — اتصال ناموفق" -ForegroundColor Red
    Write-Host $_.Exception.Message
    Write-Host ""
    Write-Host "راه‌حل:" -ForegroundColor Yellow
    Write-Host "  1) DBA روی SQL اجرا کند: database\17_GrantSqlAccess_WindowsUser.sql"
    Write-Host "  2) یا SQL Login (hr_app) + appsettings.Production.json بدون Integrated Security"
    exit 1
}
