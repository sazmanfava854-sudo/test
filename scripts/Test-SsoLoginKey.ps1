#Requires -Version 5.1
<#
.SYNOPSIS
  Standalone Mashhad SSO loginKey test (no RayvarzResend app). See SSO doc pages 20-26.

.PARAMETER ApiName
  Application name / portal username (default for header apiName and body ClientId).

.PARAMETER ClientId
  Body ClientId. If omitted, defaults to HeaderApiName (or ApiName).

.PARAMETER HeaderApiName
  Value for HTTP header apiName. If omitted, defaults to ApiName. Some portals require GUID here.

.PARAMETER SecretKey
  SecretKey for this application (do not paste in chat). Omit if using -SecretKeyFile.

.PARAMETER SecretKeyFile
  Path to a text file containing only the SecretKey (one line, no quotes). Avoids PowerShell quoting issues.

.PARAMETER Probe
  Try common header/body ClientId combinations (same SecretKey). Use after fixing SecretKey length.

.NOTES
  Secret must be complete. If PS shows ">>" the quote was not closed — use splatting:

    $sso = @{ ApiName = 'FinancialAssistant'; SecretKey = 'PASTE_FULL_SECRET' }
    .\Test-SsoLoginKey.ps1 @sso

  GUID header try:

    $id = '53db42619cf3C333b13a18D34fbd9111'
    $sso = @{ ApiName = 'FinancialAssistant'; ClientId = $id; HeaderApiName = $id; SecretKey = 'PASTE_FULL_SECRET' }
    .\Test-SsoLoginKey.ps1 @sso
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [string]$ClientId,
    [string]$HeaderApiName,
    [string]$SecretKey,
    [string]$SecretKeyFile,
    [string]$State = "test",
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [switch]$HashUpper,
    [switch]$Probe
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')

if (-not [string]::IsNullOrWhiteSpace($SecretKeyFile)) {
    if (-not (Test-Path -LiteralPath $SecretKeyFile)) {
        throw "SecretKeyFile not found: $SecretKeyFile"
    }
    $SecretKey = (Get-Content -LiteralPath $SecretKeyFile -Raw).Trim()
}

if ([string]::IsNullOrWhiteSpace($SecretKey)) {
    throw @"
SecretKey is empty.
  Option A — file (best if key has special chars like * or '):
    Set-Content -LiteralPath 'D:\Khoshdel\RayvarzResend\sso-secret.txt' -Value 'PASTE_KEY' -NoNewline
    .\Test-SsoLoginKey.ps1 -ApiName 'FinancialAssistant' -SecretKeyFile 'D:\Khoshdel\RayvarzResend\sso-secret.txt'
  Option B — splatting:
    `$sso = @{ ApiName = 'FinancialAssistant'; SecretKey = 'FULL_SECRET' }
    .\Test-SsoLoginKey.ps1 @sso
"@
}

Write-Host "SecretKey length: $($SecretKey.Length) characters (must match portal; not shown)"
if ($SecretKey.Length -lt 16) {
    Write-Warning "SecretKey looks too short — check copy/paste from SSO portal."
}

function Invoke-MashhadLoginKeyTest {
    param(
        [string]$HeaderApi,
        [string]$BodyClientId,
        [string]$Secret,
        [string]$Base,
        [string]$StateVal,
        [int]$UserTypeVal,
        [int]$DomainIdVal,
        [bool]$UpperHash
    )

    Write-Host ""
    Write-Host "--- try: header apiName=[$HeaderApi] body ClientId=[$BodyClientId] ---"

    $timeResp = Invoke-RestMethod -Uri "$Base/api/Authentication/getCurrentTime" -Method Get -TimeoutSec 60
    $requestTime = [string]$timeResp.Data
    if ([string]::IsNullOrWhiteSpace($requestTime)) {
        throw "getCurrentTime returned no Data"
    }

    $raw = $Secret + $requestTime
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($raw))
    $hash = [BitConverter]::ToString($hashBytes).Replace("-", "")
    if ($UpperHash) { $hash = $hash.ToUpperInvariant() } else { $hash = $hash.ToLowerInvariant() }

    $body = @{
        Time     = $requestTime
        Hash     = $hash
        ClientId = $BodyClientId
        State    = $StateVal
        UserType = $UserTypeVal
        DomainID = $DomainIdVal
    } | ConvertTo-Json -Compress

    $headers = @{
        apiName     = $HeaderApi
        requestTime = $requestTime
        apiSecret   = $hash
    }

    $resp = Invoke-RestMethod -Uri "$Base/api/Authentication/loginKey" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 60
    $resp | ConvertTo-Json -Depth 5
    return $resp
}

if ($Probe) {
    $guid = '53db42619cf3C333b13a18D34fbd9111'
    $scenarios = @(
        @{ H = $ApiName; C = $ApiName; Label = "header=ApiName, body=ApiName" },
        @{ H = $ApiName; C = $guid; Label = "header=ApiName, body=GUID" }
    )
    if (-not [string]::IsNullOrWhiteSpace($ClientId)) {
        $scenarios += @{ H = $ApiName; C = $ClientId; Label = "header=ApiName, body=-ClientId" }
        $scenarios += @{ H = $ClientId; C = $ClientId; Label = "header=ClientId, body=ClientId" }
    }
    $scenarios += @{ H = $guid; C = $guid; Label = "header=GUID, body=GUID" }

    Write-Host "== PROBE mode (same SecretKey) =="
    foreach ($s in $scenarios) {
        Write-Host "Scenario: $($s.Label)"
        $r = Invoke-MashhadLoginKeyTest -HeaderApi $s.H -BodyClientId $s.C -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -UpperHash ([bool]$HashUpper)
        if ($r.ErrorCode -eq 0 -and $r.Data.loginKey) {
            Write-Host "OK - use header apiName=[$($s.H)] body ClientId=[$($s.C)]"
            Write-Host "loginKey: $($r.Data.loginKey)"
            Write-Host "Next: $BaseUrl/Authentication/Start/$($r.Data.loginKey)"
            return
        }
    }
    Write-Host ""
    Write-Host "All probe scenarios returned 403 or error. Verify SecretKey and portal Username/ClientId fields with SSO admin."
    return
}

if ([string]::IsNullOrWhiteSpace($HeaderApiName)) {
    $HeaderApiName = $ApiName
}
if ([string]::IsNullOrWhiteSpace($ClientId)) {
    $ClientId = $HeaderApiName
}

Write-Host "Using header apiName=$HeaderApiName, body ClientId=$ClientId"
Write-Host ""
Write-Host "== 1) GET getCurrentTime + 2) POST loginKey =="

$enc = if ($HashUpper) { "upper" } else { "lower" }
Write-Host "apiSecret/Hash = SHA256(SecretKey+requestTime) hex $enc"

$resp = Invoke-MashhadLoginKeyTest -HeaderApi $HeaderApiName -BodyClientId $ClientId -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -UpperHash ([bool]$HashUpper)

if ($resp.ErrorCode -eq 0 -and $resp.Data.loginKey) {
    Write-Host ""
    Write-Host "OK - loginKey: $($resp.Data.loginKey)"
    Write-Host "Next: $BaseUrl/Authentication/Start/$($resp.Data.loginKey)"
} elseif ($resp.ErrorCode -eq 403) {
    Write-Host ""
    Write-Host "403 Client info missmatched — usual causes:"
    Write-Host "  1) SecretKey wrong or incomplete (PS prompt >> means unclosed quote)"
    Write-Host "  2) header apiName must be GUID while body uses same GUID — try -HeaderApiName"
    Write-Host "  3) apiName/ClientId not registered for this SecretKey"
    Write-Host ""
    Write-Host "Try: .\Test-SsoLoginKey.ps1 -ApiName 'FinancialAssistant' -SecretKey '...' -Probe"
}
