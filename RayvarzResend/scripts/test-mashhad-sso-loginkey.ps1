#Requires -Version 5.1
<#
.SYNOPSIS
  تست مستقل loginKey مطابق سند SSO مشهد (بدون RayvarzResend).

.PARAMETER ApiName
  نام کاربری کاربردی برنامه (هدر apiName) — از پورتال شناسه شهروندی، نه نام نمایشی برنامه.

.PARAMETER ClientId
  شناسه کاربردی برنامه (ClientId در بدنه).

.PARAMETER SecretKey
  SecretKey همان کاربردی برنامه.
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [Parameter(Mandatory)][string]$ClientId,
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$State = "test",
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [switch]$HashUpper
)

$BaseUrl = $BaseUrl.TrimEnd('/')

Write-Host "== 1) GET getCurrentTime =="
$timeResp = Invoke-RestMethod -Uri "$BaseUrl/api/Authentication/getCurrentTime" -Method Get -TimeoutSec 60
$requestTime = [string]$timeResp.Data
if ([string]::IsNullOrWhiteSpace($requestTime)) {
    throw "getCurrentTime returned no Data"
}
Write-Host "requestTime: $requestTime"

$raw = $SecretKey + $requestTime
$sha = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($raw))
$hash = [BitConverter]::ToString($hashBytes).Replace("-", "")
if ($HashUpper) { $hash = $hash.ToUpperInvariant() } else { $hash = $hash.ToLowerInvariant() }

Write-Host "apiSecret/Hash = SHA256(SecretKey+requestTime) hex $(if ($HashUpper) { 'upper' } else { 'lower' })"

$body = @{
    Time     = $requestTime
    Hash     = $hash
    ClientId = $ClientId
    State    = $State
    UserType = $UserType
    DomainID = $DomainID
} | ConvertTo-Json -Compress

Write-Host ""
Write-Host "== 2) POST loginKey =="
Write-Host "Headers: apiName=$ApiName, requestTime=$requestTime, apiSecret=<hash>"

$headers = @{
    apiName     = $ApiName
    requestTime = $requestTime
    apiSecret   = $hash
}

try {
    $resp = Invoke-RestMethod -Uri "$BaseUrl/api/Authentication/loginKey" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 60
} catch {
    Write-Host $_.Exception.Message
    throw
}

$resp | ConvertTo-Json -Depth 5

if ($resp.ErrorCode -eq 0 -and $resp.Data.loginKey) {
    Write-Host ""
    Write-Host "OK — loginKey: $($resp.Data.loginKey)"
    Write-Host "Next: $BaseUrl/Authentication/Start/$($resp.Data.loginKey)"
} elseif ($resp.ErrorCode -eq 403) {
    Write-Host ""
    Write-Host "403 → apiName / ClientId / SecretKey با ثبت پورتال برای این کاربردی برنامه مطابقت ندارد."
}
