#Requires -Version 5.1
<#
.SYNOPSIS
  Standalone Mashhad SSO loginKey test. SSO doc pages 20-26.

.PARAMETER ApiName
  Application name from SSO portal.

.PARAMETER ClientId
  Body ClientId. Default: HeaderApiName or ApiName.

.PARAMETER HeaderApiName
  HTTP header apiName. Default: ApiName.

.PARAMETER SecretKey
  SecretKey (omit if using -SecretKeyFile).

.PARAMETER SecretKeyFile
  File with one line: full SecretKey only.

.PARAMETER Probe
  Try several header/body ClientId combinations.
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [string]$ClientId,
    [string]$HeaderApiName,
    [string]$SecretKey,
    [string]$SecretKeyFile,
    [string]$State = 'test',
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = 'https://login.mashhad.ir',
    [switch]$HashUpper,
    [switch]$Probe
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')

if (-not [string]::IsNullOrWhiteSpace($SecretKeyFile)) {
    if (-not (Test-Path -LiteralPath $SecretKeyFile)) {
        throw ('SecretKeyFile not found: ' + $SecretKeyFile)
    }
    $SecretKey = (Get-Content -LiteralPath $SecretKeyFile -Raw).Trim()
}

if ([string]::IsNullOrWhiteSpace($SecretKey)) {
    throw 'SecretKey is empty. Use -SecretKeyFile path\to\sso-secret.txt or splatting with full key.'
}

Write-Host ('SecretKey length: ' + $SecretKey.Length + ' characters')
if ($SecretKey.Length -lt 16) {
    Write-Warning 'SecretKey looks too short. Copy the full key from SSO portal into sso-secret.txt'
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

    Write-Host ''
    Write-Host ('--- try: header apiName=[' + $HeaderApi + '] body ClientId=[' + $BodyClientId + '] ---')

    $timeResp = Invoke-RestMethod -Uri ($Base + '/api/Authentication/getCurrentTime') -Method Get -TimeoutSec 60
    $requestTime = [string]$timeResp.Data
    if ([string]::IsNullOrWhiteSpace($requestTime)) {
        throw 'getCurrentTime returned no Data'
    }

    $raw = $Secret + $requestTime
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($raw))
    $hash = [BitConverter]::ToString($hashBytes).Replace('-', '')
    if ($UpperHash) { $hash = $hash.ToUpperInvariant() } else { $hash = $hash.ToLowerInvariant() }

    $bodyObj = @{
        Time     = $requestTime
        Hash     = $hash
        ClientId = $BodyClientId
        State    = $StateVal
        UserType = $UserTypeVal
        DomainID = $DomainIdVal
    }
    $body = $bodyObj | ConvertTo-Json -Compress

    $headers = @{
        apiName     = $HeaderApi
        requestTime = $requestTime
        apiSecret   = $hash
    }

    $uri = $Base + '/api/Authentication/loginKey'
    $resp = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body -ContentType 'application/json' -TimeoutSec 60
    $resp | ConvertTo-Json -Depth 5
    return $resp
}

if ($Probe) {
    $guid = '53db42619cf3C333b13a18D34fbd9111'
    $scenarios = @(
        @{ H = $ApiName; C = $ApiName },
        @{ H = $ApiName; C = $guid },
        @{ H = $guid; C = $guid }
    )
    if (-not [string]::IsNullOrWhiteSpace($ClientId)) {
        $scenarios += @{ H = $ApiName; C = $ClientId }
        $scenarios += @{ H = $ClientId; C = $ClientId }
    }

    Write-Host '== PROBE mode =='
    foreach ($s in $scenarios) {
        $r = Invoke-MashhadLoginKeyTest -HeaderApi $s.H -BodyClientId $s.C -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -UpperHash ([bool]$HashUpper)
        if ($r.ErrorCode -eq 0 -and $r.Data.loginKey) {
            Write-Host ('OK - header apiName=[' + $s.H + '] body ClientId=[' + $s.C + ']')
            Write-Host ('loginKey: ' + $r.Data.loginKey)
            Write-Host ($BaseUrl + '/Authentication/Start/' + $r.Data.loginKey)
            return
        }
    }
    Write-Host 'All probe scenarios failed. Check SecretKey and SSO portal registration.'
    return
}

if ([string]::IsNullOrWhiteSpace($HeaderApiName)) {
    $HeaderApiName = $ApiName
}
if ([string]::IsNullOrWhiteSpace($ClientId)) {
    $ClientId = $HeaderApiName
}

Write-Host ('Using header apiName=' + $HeaderApiName + ' body ClientId=' + $ClientId)
Write-Host ''
Write-Host 'Step 1: getCurrentTime. Step 2: loginKey'

$enc = if ($HashUpper) { 'upper' } else { 'lower' }
Write-Host ('apiSecret/Hash = SHA256(SecretKey+requestTime) hex ' + $enc)

$resp = Invoke-MashhadLoginKeyTest -HeaderApi $HeaderApiName -BodyClientId $ClientId -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -UpperHash ([bool]$HashUpper)

if ($resp.ErrorCode -eq 0 -and $resp.Data.loginKey) {
    Write-Host ''
    Write-Host ('OK - loginKey: ' + $resp.Data.loginKey)
    Write-Host ($BaseUrl + '/Authentication/Start/' + $resp.Data.loginKey)
}
elseif ($resp.ErrorCode -eq 403) {
    Write-Host ''
    Write-Host '403 Client info missmatched.'
    Write-Host 'Check SecretKey file length, then run with -Probe switch.'
}
