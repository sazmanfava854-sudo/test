#Requires -Version 5.1
<#
.SYNOPSIS
  Standalone Mashhad SSO loginKey test — SSO doc 1.0.2 section 1.3.2 / 3.3.3.

  Headers: apiName, requestTime (from getCurrentTime), apiSecret = SHA256(SecretKey + requestTime) hex.
  Body: Time (=requestTime), Hash (=same as apiSecret), ClientId, State, UserType, DomainID.

  IMPORTANT (doc table 1 row 2): apiName AND ClientId in body are the SSO *application username*,
  NOT the legacy lkey GUID used in Login.aspx?lkey=...
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [Parameter()][string]$ClientId = "",
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$State = "test",
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [switch]$HashUpper,
    [switch]$UseLegacyLkeyClientId
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')

if ([string]::IsNullOrWhiteSpace($ClientId)) {
    $ClientId = $ApiName
    Write-Host "ClientId not set — using ApiName per SSO doc 1.3.2 (ClientID = application username)."
}

function Get-Sha256Hex([string]$Text, [bool]$Upper) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Text))
    $hex = [BitConverter]::ToString($bytes).Replace("-", "")
    if ($Upper) { return $hex.ToUpperInvariant() }
    return $hex.ToLowerInvariant()
}

function Invoke-LoginKey([string]$Api, [string]$Cid, [string]$Secret, [string]$ReqTime, [bool]$Upper) {
    $raw = $Secret + $ReqTime
    $hash = Get-Sha256Hex $raw $Upper
    $bodyObj = @{
        Time     = $ReqTime
        Hash     = $hash
        ClientId = $Cid
        State    = $State
        UserType = $UserType
        DomainID = $DomainID
    }
    $body = $bodyObj | ConvertTo-Json -Compress

    Write-Host ""
    Write-Host "POST loginKey"
    Write-Host "  apiName=$Api"
    Write-Host "  requestTime=$ReqTime"
    Write-Host "  apiSecret=$hash"
    Write-Host "  body ClientId=$Cid"
    Write-Host "  hash input: SecretKey(${Secret.Length} chars) + requestTime"

    $curl = Get-Command curl.exe -ErrorAction SilentlyContinue
    if ($curl) {
        $respText = & curl.exe -sS --max-time 60 -X POST "$BaseUrl/api/Authentication/loginKey" `
            -H "apiName: $Api" `
            -H "requestTime: $ReqTime" `
            -H "apiSecret: $hash" `
            -H "Content-Type: application/json" `
            -d $body
        return ($respText | ConvertFrom-Json)
    }

    $headers = @{ apiName = $Api; requestTime = $ReqTime; apiSecret = $hash }
    return Invoke-RestMethod -Uri "$BaseUrl/api/Authentication/loginKey" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 60
}

Write-Host "== 1) GET getCurrentTime =="
$timeResp = Invoke-RestMethod -Uri "$BaseUrl/api/Authentication/getCurrentTime" -Method Get -TimeoutSec 60
$requestTime = [string]$timeResp.Data
if ([string]::IsNullOrWhiteSpace($requestTime)) { throw "getCurrentTime returned no Data" }
Write-Host "requestTime: $requestTime (use this exact string in hash — doc sample is seconds, e.g. 1643714953)"

if ($ClientId -ne $ApiName -and -not $UseLegacyLkeyClientId) {
    Write-Host ""
    Write-Host "WARNING: ClientId differs from ApiName. SSO 1.0.2 says ClientID = application username (same as apiName)."
    Write-Host "         lkey GUID is for old Login.aspx, not loginKey API. Will retry with ClientId=ApiName if 403."
}

$attempts = @(
    @{ Label = "configured"; Api = $ApiName; Cid = $ClientId; Upper = [bool]$HashUpper }
)
if ($ClientId -ne $ApiName -and -not $UseLegacyLkeyClientId) {
    $attempts += @{ Label = "doc ClientId=ApiName"; Api = $ApiName; Cid = $ApiName; Upper = [bool]$HashUpper }
}
if (-not $HashUpper) {
    $attempts += @{ Label = "hex upper"; Api = $ApiName; Cid = $ApiName; Upper = $true }
}

$last = $null
foreach ($a in $attempts) {
    Write-Host ""
    Write-Host "== Attempt: $($a.Label) =="
    $last = Invoke-LoginKey -Api $a.Api -Cid $a.Cid -Secret $SecretKey -ReqTime $requestTime -Upper $a.Upper
    $last | ConvertTo-Json -Depth 5
    if ($last.ErrorCode -eq 0 -and $last.Data.loginKey) {
        Write-Host ""
        Write-Host "OK - loginKey: $($last.Data.loginKey)"
        Write-Host "Next: $BaseUrl/Authentication/Start/$($last.Data.loginKey)"
        exit 0
    }
}

Write-Host ""
if ($last.ErrorCode -eq 403) {
    Write-Host "403 — If hash looks correct, fix apiName/SecretKey with SSO admin (Client info missmatched)."
    Write-Host "     Per SSO doc: apiName = registered username; ClientId in body should match (not lkey GUID)."
}
exit 3
