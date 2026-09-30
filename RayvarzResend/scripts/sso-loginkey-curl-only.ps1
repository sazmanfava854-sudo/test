#Requires -Version 5.1
# Only curl.exe for HTTP (for PCs where Invoke-RestMethod times out).
param(
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$ApiName = "FinancialAssistant",
    [string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [string]$BaseUrl = "https://login.mashhad.ir",
    [string]$RequestTime = "",
    [int]$TimeoutSec = 60,
    [switch]$SkipPost
)

$SecretKey = $SecretKey.Trim()
$BaseUrl = $BaseUrl.TrimEnd('/')
$uriTime = $BaseUrl + "/api/Authentication/getCurrentTime"

function Get-Hash([string]$Time, [string]$Secret) {
    $raw = $Secret + $Time
    $bytes = [Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::ASCII.GetBytes($raw))
    ($bytes | ForEach-Object { $_.ToString("x2") }) -join ''
}

if (-not (Get-Command curl.exe -ErrorAction SilentlyContinue)) {
    Write-Error "curl.exe not found (Windows 10+ includes it)."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($RequestTime)) {
    Write-Host "GET getCurrentTime (curl)..."
    $json = (& curl.exe --ssl-no-revoke -sS --connect-timeout 15 --max-time $TimeoutSec $uriTime 2>&1 | Out-String).Trim()
    Write-Host $json
    $o = $json | ConvertFrom-Json
    if ($o.ErrorCode -ne 0 -or [string]::IsNullOrWhiteSpace([string]$o.Data)) {
        Write-Error "getCurrentTime failed"
        exit 1
    }
    $RequestTime = [string]$o.Data
}
else {
    $RequestTime = $RequestTime.Trim()
}

$hash = Get-Hash $RequestTime $SecretKey
Write-Host "requestTime = $RequestTime"
Write-Host "apiSecret   = $hash"

$body = (@{
    Time = $RequestTime; Hash = $hash; ClientId = $ClientId
    State = "test"; UserType = 0; DomainID = 0
} | ConvertTo-Json -Compress)
Write-Host "body = $body"

if ($SkipPost) { exit 0 }

$tmp = Join-Path $env:TEMP ("loginkey-" + [Guid]::NewGuid().ToString("N") + ".json")
[IO.File]::WriteAllText($tmp, $body, (New-Object Text.UTF8Encoding $false))
$url = $BaseUrl + "/api/Authentication/loginKey"
Write-Host "POST loginKey (curl)..."
try {
    & curl.exe --ssl-no-revoke -sS --connect-timeout 15 --max-time $TimeoutSec -w "`nHTTP_CODE:%{http_code}`n" -X POST $url `
        -H "apiName: $ApiName" -H "requestTime: $RequestTime" -H "apiSecret: $hash" `
        -H "Content-Type: application/json" --data-binary "@$tmp"
}
finally {
    Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
}
