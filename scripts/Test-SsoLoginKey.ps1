#Requires -Version 5.1
<#
.SYNOPSIS
  Standalone Mashhad SSO loginKey test (no RayvarzResend app). See SSO doc pages 20-26.

.PARAMETER ApiName
  Application API username (header apiName) from SSO portal - not the display title.

.PARAMETER ClientId
  Registered ClientId for loginKey body. If omitted, defaults to ApiName (per SSO doc: ClientID = portal username).

.PARAMETER SecretKey
  SecretKey for this application (do not paste in chat).

.EXAMPLE
  .\Test-SsoLoginKey.ps1 -ApiName "FinancialAssistant" -SecretKey "your-secret"

.EXAMPLE
  .\Test-SsoLoginKey.ps1 -ApiName "FinancialAssistant" -ClientId "53db42619cf3C333b13a18D34fbd9111" -SecretKey "your-secret"
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [string]$ClientId,
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$State = "test",
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [switch]$HashUpper
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')

if ([string]::IsNullOrWhiteSpace($ClientId)) {
    $ClientId = $ApiName
}

Write-Host "Using apiName=$ApiName, body ClientId=$ClientId"
Write-Host ""

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

$enc = if ($HashUpper) { "upper" } else { "lower" }
Write-Host "apiSecret/Hash = SHA256(SecretKey+requestTime) hex $enc"

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
Write-Host "Headers: apiName=$ApiName, requestTime=$requestTime, apiSecret=(hash)"

$headers = @{
    apiName     = $ApiName
    requestTime = $requestTime
    apiSecret   = $hash
}

try {
    $resp = Invoke-RestMethod -Uri "$BaseUrl/api/Authentication/loginKey" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 60
} catch {
    Write-Host $_.Exception.Message
    if ($_.ErrorDetails.Message) {
        Write-Host $_.ErrorDetails.Message
    }
    throw
}

$resp | ConvertTo-Json -Depth 5

if ($resp.ErrorCode -eq 0 -and $resp.Data.loginKey) {
    Write-Host ""
    Write-Host "OK - loginKey: $($resp.Data.loginKey)"
    Write-Host "Next: $BaseUrl/Authentication/Start/$($resp.Data.loginKey)"
} elseif ($resp.ErrorCode -eq 403) {
    Write-Host ""
    Write-Host "403 - apiName, ClientId, or SecretKey does not match SSO portal registration for this application."
    Write-Host "Tip: omit -ClientId so body ClientId = ApiName, unless the portal lists a different ClientId."
}
