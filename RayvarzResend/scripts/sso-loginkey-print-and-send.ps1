#Requires -Version 5.1
# تست loginKey — اول مقادیر قبل از هش، بعد apiSecret، بعد ارسال
param(
    [Parameter(Mandatory)][string]$ApiName = "FinancialAssistant",
    [Parameter(Mandatory)][string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$BaseUrl = "https://login.mashhad.ir"
)

$ErrorActionPreference = "Stop"
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

Write-Host "========== 1) getCurrentTime =========="
$requestTime = [string](Invoke-RestMethod -Uri ($BaseUrl + "/api/Authentication/getCurrentTime") -Method Get -TimeoutSec 60).Data
Write-Host "requestTime (header + body Time) = $requestTime"

Write-Host ""
Write-Host "========== 2) BEFORE HASH (inputs) =========="
Write-Host "header apiName     = $ApiName"
Write-Host "body   ClientId     = $ClientId"
Write-Host "SecretKey length   = $($SecretKey.Length)"
Write-Host "concat for SHA256  = [SecretKey] + requestTime"
Write-Host "concat ends with   = ... + $requestTime"

Write-Host ""
Write-Host "========== 3) AFTER HASH =========="
$apiSecret = Get-ApiSecretAscii $SecretKey $requestTime
Write-Host "header apiSecret   = $apiSecret"
Write-Host "body   Hash        = $apiSecret  (same as apiSecret)"
Write-Host "formula            = SHA256(ASCII(SecretKey + requestTime)) hex x2 lower"

$bodyJson = (@{
    Time     = $requestTime
    Hash     = $apiSecret
    ClientId = $ClientId
    State    = "test"
    UserType = 0
    DomainID = 0
} | ConvertTo-Json -Compress)

Write-Host ""
Write-Host "========== 4) BODY JSON =========="
Write-Host $bodyJson

Write-Host ""
Write-Host "========== 5) POST loginKey =========="
$headers = @{
    apiName     = $ApiName
    requestTime = $requestTime
    apiSecret   = $apiSecret
}
$r = Invoke-WebRequest -Uri ($BaseUrl + "/api/Authentication/loginKey") -Method Post -Headers $headers `
    -Body ([Text.Encoding]::UTF8.GetBytes($bodyJson)) -ContentType "application/json; charset=utf-8" `
    -TimeoutSec 60 -UseBasicParsing
Write-Host "HTTP $($r.StatusCode)"
Write-Host $r.Content
