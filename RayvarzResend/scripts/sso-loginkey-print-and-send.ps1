#Requires -Version 5.1
param(
    [string]$ApiName = "FinancialAssistant",
    [string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [int]$TimeoutSec = 45,
    [switch]$SkipPost
)

$ErrorActionPreference = "Continue"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
[Net.ServicePointManager]::CheckCertificateRevocationList = $false

$ApiName = $ApiName.Trim()
$ClientId = $ClientId.Trim()
$SecretKey = $SecretKey.Trim()
$BaseUrl = $BaseUrl.TrimEnd('/')

function Get-ApiSecretAscii([string]$Secret, [string]$Time) {
    $raw = $Secret + $Time
    $bytes = [Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::ASCII.GetBytes($raw))
    ($bytes | ForEach-Object { $_.ToString("x2") }) -join ''
}

function Post-LoginKeyWithCurl([string]$Url, [string]$Api, [string]$ReqTime, [string]$SecretHash, [string]$BodyJson, [int]$MaxSec) {
    $curl = Get-Command curl.exe -ErrorAction SilentlyContinue
    if (-not $curl) {
        Write-Host "curl.exe not found."
        return $null
    }
    $tmp = Join-Path $env:TEMP ("sso-loginkey-" + [Guid]::NewGuid().ToString("N") + ".json")
    [IO.File]::WriteAllText($tmp, $BodyJson, (New-Object Text.UTF8Encoding $false))
    Write-Host "Using curl.exe --ssl-no-revoke (max ${MaxSec}s)..."
    try {
        $out = & curl.exe --ssl-no-revoke -sS --max-time $MaxSec -w "`nHTTP_CODE:%{http_code}" -X POST $Url `
            -H "apiName: $Api" `
            -H "requestTime: $ReqTime" `
            -H "apiSecret: $SecretHash" `
            -H "Content-Type: application/json" `
            --data-binary "@$tmp" 2>&1
        $text = ($out | Out-String).Trim()
        Write-Host $text
        return $text
    }
    finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    }
}

function Post-LoginKeyWithIwr([string]$Url, [hashtable]$Headers, [byte[]]$BodyBytes, [int]$MaxSec) {
    Write-Host "Using Invoke-WebRequest (TimeoutSec=$MaxSec)..."
    try {
        $r = Invoke-WebRequest -Uri $Url -Method Post -Headers $Headers `
            -Body $BodyBytes -ContentType "application/json; charset=utf-8" `
            -TimeoutSec $MaxSec -UseBasicParsing
        Write-Host ("HTTP " + [int]$r.StatusCode)
        Write-Host $r.Content
        return $r.Content
    }
    catch [System.Net.WebException] {
        Write-Host ("WebException: " + $_.Exception.Message)
        if ($_.Exception.Response) {
            $sr = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
            $t = $sr.ReadToEnd()
            $sr.Close()
            if ($t) { Write-Host $t; return $t }
        }
        return $null
    }
    catch {
        Write-Host ("Error: " + $_.Exception.Message)
        return $null
    }
}

Write-Host "========== 1) getCurrentTime =========="
try {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $requestTime = [string](Invoke-RestMethod -Uri ($BaseUrl + "/api/Authentication/getCurrentTime") -Method Get -TimeoutSec 30).Data
    $sw.Stop()
    Write-Host ("OK in " + $sw.ElapsedMilliseconds + " ms")
}
catch {
    Write-Host ("getCurrentTime FAILED: " + $_.Exception.Message)
    exit 1
}
Write-Host "requestTime = $requestTime"

Write-Host ""
Write-Host "========== 2) BEFORE HASH =========="
Write-Host "apiName (header)   = $ApiName"
Write-Host "ClientId (body)    = $ClientId"
Write-Host "SecretKey length   = $($SecretKey.Length)"

Write-Host ""
Write-Host "========== 3) AFTER HASH =========="
$apiSecret = Get-ApiSecretAscii $SecretKey $requestTime
Write-Host "apiSecret / Hash   = $apiSecret"

$bodyJson = (@{
    Time = $requestTime; Hash = $apiSecret; ClientId = $ClientId
    State = "test"; UserType = 0; DomainID = 0
} | ConvertTo-Json -Compress)

Write-Host ""
Write-Host "========== 4) BODY JSON =========="
Write-Host $bodyJson

if ($SkipPost) {
    Write-Host ""
    Write-Host "SkipPost: no HTTP POST."
    exit 0
}

$url = $BaseUrl + "/api/Authentication/loginKey"
Write-Host ""
Write-Host "========== 5) POST loginKey =========="
Write-Host $url

$headers = @{ apiName = $ApiName; requestTime = $requestTime; apiSecret = $apiSecret }
$bodyBytes = [Text.Encoding]::UTF8.GetBytes($bodyJson)

$content = Post-LoginKeyWithCurl -Url $url -Api $ApiName -ReqTime $requestTime -SecretHash $apiSecret -BodyJson $bodyJson -MaxSec $TimeoutSec
if (-not $content) {
    Write-Host ""
    Write-Host "curl failed or empty - trying Invoke-WebRequest..."
    $content = Post-LoginKeyWithIwr -Url $url -Headers $headers -BodyBytes $bodyBytes -MaxSec $TimeoutSec
}

if (-not $content) {
    Write-Host ""
    Write-Host "NO RESPONSE - firewall/proxy may block POST to login.mashhad.ir from this machine."
    Write-Host "Try from another PC with internet, or ask IT to allow HTTPS POST outbound."
    exit 2
}

exit 0
