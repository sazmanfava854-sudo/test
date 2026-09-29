#Requires -Version 5.1
<#
.SYNOPSIS
  Standalone Mashhad SSO loginKey test (SSO doc 1.0.2 sections 1.3.2 and 3.3.3).
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
    [switch]$UseLegacyLkeyClientId,
    [int]$RequestTimeoutSec = 60
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')
$ApiName = $ApiName.Trim()
$SecretKey = $SecretKey.Trim()
$ClientId = $ClientId.Trim()

if ([string]::IsNullOrWhiteSpace($SecretKey)) {
    throw 'SecretKey is empty after trim (check copy/paste; no trailing spaces).'
}

# Windows curl.exe often fails with CRYPT_E_REVOCATION_OFFLINE when CRL/OCSP is unreachable.
# This script uses Invoke-WebRequest only; disable revocation check for this diagnostic run.
if ($env:OS -like '*Windows*') {
    try {
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
        [System.Net.ServicePointManager]::CheckCertificateRevocationList = $false
        Write-Host 'TLS: revocation check off for this session (isolated server / no CRL access).'
    }
    catch {
        Write-Host ('TLS note: ' + $_.Exception.Message)
    }
}

if ([string]::IsNullOrWhiteSpace($ClientId) -and -not [string]::IsNullOrWhiteSpace($env:MASHHAD_SSO_CLIENT_ID)) {
    $ClientId = $env:MASHHAD_SSO_CLIENT_ID.Trim()
}
if ([string]::IsNullOrWhiteSpace($ClientId)) {
    throw 'ClientId is required: registered ClientID in loginKey body (e.g. 53db42619cf3C333b13a18D34fbd9111). Header apiName stays -ApiName.'
}
if ($ClientId -ne $ApiName) {
    Write-Host ('Using apiName (header)=' + $ApiName + ' and ClientId (body)=' + $ClientId)
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
    $secretLen = $Secret.Length

    Write-Host ""
    Write-Host "POST loginKey"
    Write-Host ('  apiName=' + $Api)
    Write-Host ('  requestTime=' + $ReqTime)
    Write-Host ('  apiSecret=' + $hash)
    Write-Host ('  body ClientId=' + $Cid)
    Write-Host ('  hash input: SecretKey(' + $secretLen + ' chars) + requestTime')
    Write-Host ('  Waiting for SSO POST loginKey (timeout ' + $RequestTimeoutSec + 's)...')

    $uri = $BaseUrl + '/api/Authentication/loginKey'
    $headers = @{
        apiName     = $Api
        requestTime = $ReqTime
        apiSecret   = $hash
    }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers `
            -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) `
            -ContentType 'application/json; charset=utf-8' `
            -TimeoutSec $RequestTimeoutSec -UseBasicParsing
        $sw.Stop()
        Write-Host ('  Done: HTTP ' + [int]$response.StatusCode + ' in ' + $sw.ElapsedMilliseconds + ' ms')
        return ($response.Content | ConvertFrom-Json)
    }
    catch [System.Net.WebException] {
        $sw.Stop()
        $resp = $_.Exception.Response
        if ($null -ne $resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $text = $reader.ReadToEnd()
            $reader.Close()
            Write-Host ('  Done: HTTP ' + [int]$resp.StatusCode + ' in ' + $sw.ElapsedMilliseconds + ' ms')
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                return ($text | ConvertFrom-Json)
            }
        }
        Write-Host ('  Failed after ' + $sw.ElapsedMilliseconds + ' ms: ' + $_.Exception.Message)
        throw
    }
    catch {
        $sw.Stop()
        Write-Host ('  Failed after ' + $sw.ElapsedMilliseconds + ' ms: ' + $_.Exception.Message)
        throw
    }
}

Write-Host '== 1) GET getCurrentTime =='
$timeResp = Invoke-RestMethod -Uri ($BaseUrl + '/api/Authentication/getCurrentTime') -Method Get -TimeoutSec 60
$requestTime = [string]$timeResp.Data
if ([string]::IsNullOrWhiteSpace($requestTime)) { throw 'getCurrentTime returned no Data' }
Write-Host ('requestTime: ' + $requestTime + ' (use exact string in SHA256; doc sample: 1643714953)')
Write-Host ('SecretKey length after trim: ' + $SecretKey.Length + ' characters')
if ($SecretKey.Length -ne 32) {
    Write-Host 'NOTE: Many SSO SecretKeys are 32 chars. Extra/missing chars often cause 403.'
}

$attempts = @(
    @{ Label = 'configured'; Api = $ApiName; Cid = $ClientId; Upper = [bool]$HashUpper }
)
if ($UseLegacyLkeyClientId -and $ClientId -ne $ApiName) {
    $attempts += @{ Label = 'ClientId=ApiName'; Api = $ApiName; Cid = $ApiName; Upper = [bool]$HashUpper }
}
if (-not $HashUpper) {
    $attempts += @{ Label = 'hex upper'; Api = $ApiName; Cid = $ApiName; Upper = $true }
}

$last = $null
foreach ($a in $attempts) {
    Write-Host ''
    Write-Host ('== Attempt: ' + $a.Label + ' ==')
    $last = Invoke-LoginKey -Api $a.Api -Cid $a.Cid -Secret $SecretKey -ReqTime $requestTime -Upper $a.Upper
    $last | ConvertTo-Json -Depth 5
    if ($last.ErrorCode -eq 0 -and $last.Data.loginKey) {
        Write-Host ''
        Write-Host ('OK - loginKey: ' + $last.Data.loginKey)
        Write-Host ('Next: ' + $BaseUrl + '/Authentication/Start/' + $last.Data.loginKey)
        exit 0
    }
}

Write-Host ''
if ($null -ne $last -and $last.ErrorCode -eq 403) {
    Write-Host '403 - Check apiName and SecretKey with SSO admin (Client info missmatched).'
    Write-Host '    ClientId in body must be application username, not lkey GUID.'
}
exit 3
