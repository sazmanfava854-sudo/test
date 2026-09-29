#Requires -Version 5.1
<#
.SYNOPSIS
  Mashhad SSO loginKey test per doc v1.0.2 (pages 19-26).

.NOTES
  Doc rules:
    Header apiName     = portal Username (table 1 row 2), e.g. FinancialAssistant
    Header requestTime = string from getCurrentTime Data (same value in body Time)
    Header apiSecret   = SHA256(SecretKey + requestTime) hex
    Body ClientId      = portal ClientId (may differ from apiName)
    Body Hash          = same as header apiSecret

  Pass Secret without broken quotes (no >> prompt). Use splatting:

    $sso = @{
      ApiName        = 'FinancialAssistant'
      PortalClientId = '53db42619cf3C333b13a18D34fbd9111'
      SecretKey      = 'FULL_SECRET_FROM_PORTAL'
    }
    .\Test-SsoLoginKey.ps1 @sso -Probe
#>
param(
    [Parameter(Mandatory)][string]$ApiName,
    [string]$PortalClientId,
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
    $SecretKey = (Get-Content -LiteralPath $SecretKeyFile -Raw -Encoding UTF8).Trim()
    if ($SecretKey.Length -gt 0 -and [int][char]$SecretKey[0] -eq 0xFEFF) {
        $SecretKey = $SecretKey.Substring(1).Trim()
        Write-Warning 'Removed UTF-8 BOM from secret file.'
    }
}

if ([string]::IsNullOrWhiteSpace($SecretKey)) {
    throw 'SecretKey is empty. Use splatting with full SecretKey from SSO portal.'
}

Write-Host ('SecretKey length: ' + $SecretKey.Length + ' characters')

function Get-Sha256Hex {
    param([string]$Text, [bool]$Upper)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hashBytes = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    $hex = [BitConverter]::ToString($hashBytes).Replace('-', '')
    if ($Upper) { return $hex.ToUpperInvariant() }
    return $hex.ToLowerInvariant()
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
        [string]$HashMode
    )

    Write-Host ''
    Write-Host ('--- apiName=[' + $HeaderApi + '] body ClientId=[' + $BodyClientId + '] hashMode=' + $HashMode + ' ---')

    $timeResp = Invoke-RestMethod -Uri ($Base + '/api/Authentication/getCurrentTime') -Method Get -TimeoutSec 60
    $requestTime = [string]$timeResp.Data
    if ([string]::IsNullOrWhiteSpace($requestTime)) {
        throw 'getCurrentTime returned no Data'
    }

    switch ($HashMode) {
        'Secret+Time'       { $raw = $Secret + $requestTime; $upper = [bool]$HashUpper }
        'Secret+TimeUpper'  { $raw = $Secret + $requestTime; $upper = $true }
        'Time+Secret'       { $raw = $requestTime + $Secret; $upper = [bool]$HashUpper }
        'Secret+TimeMs'     { $raw = $Secret + ([string]([int64]$requestTime * 1000)); $upper = [bool]$HashUpper }
        default             { $raw = $Secret + $requestTime; $upper = [bool]$HashUpper }
    }

    $hash = Get-Sha256Hex -Text $raw -Upper $upper

    $bodyJson = (@{
        Time     = $requestTime
        Hash     = $hash
        ClientId = $BodyClientId
        State    = $StateVal
        UserType = $UserTypeVal
        DomainID = $DomainIdVal
    } | ConvertTo-Json -Compress)

    Write-Host ('requestTime=' + $requestTime + ' apiSecret[0..7]=' + $hash.Substring(0, [Math]::Min(8, $hash.Length)) + '...')

    $uri = $Base + '/api/Authentication/loginKey'
    $req = [System.Net.HttpWebRequest]::Create($uri)
    $req.Method = 'POST'
    $req.ContentType = 'application/json; charset=utf-8'
    $req.Headers.Add('apiName', $HeaderApi)
    $req.Headers.Add('requestTime', $requestTime)
    $req.Headers.Add('apiSecret', $hash)

    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyJson)
    $req.ContentLength = $bodyBytes.Length
    $stream = $req.GetRequestStream()
    $stream.Write($bodyBytes, 0, $bodyBytes.Length)
    $stream.Close()

    try {
        $response = $req.GetResponse()
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $json = $reader.ReadToEnd()
        $reader.Close()
        $response.Close()
        $resp = $json | ConvertFrom-Json
    }
    catch [System.Net.WebException] {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $json = $reader.ReadToEnd()
        $reader.Close()
        if ($json) { $resp = $json | ConvertFrom-Json } else { throw $_ }
    }

    Write-Host ('ErrorCode=' + $resp.ErrorCode + ' Message=' + $resp.ErrorMessage)
    return $resp
}

$defaultGuid = '53db42619cf3C333b13a18D34fbd9111'
if ([string]::IsNullOrWhiteSpace($PortalClientId)) {
    $PortalClientId = $defaultGuid
}
if ([string]::IsNullOrWhiteSpace($HeaderApiName)) {
    $HeaderApiName = $ApiName
}
if ([string]::IsNullOrWhiteSpace($ClientId)) {
    $ClientId = $PortalClientId
}

$hashModes = @('Secret+Time', 'Secret+TimeUpper', 'Time+Secret', 'Secret+TimeMs')

if ($Probe) {
    Write-Host '== PROBE (doc + common portal layouts) =='
    Write-Host ('Doc: header apiName=Username [' + $ApiName + '], body ClientId=[' + $PortalClientId + ']')

    $scenarios = @(
        @{ H = $ApiName; C = $PortalClientId; N = 'DOC: header Username, body ClientId GUID' },
        @{ H = $ApiName; C = $ApiName; N = 'header Username, body same Username' },
        @{ H = $PortalClientId; C = $PortalClientId; N = 'header GUID, body GUID' },
        @{ H = $ApiName; C = $ClientId; N = 'header Username, body -ClientId' }
    )

    foreach ($s in $scenarios) {
        Write-Host ('Scenario: ' + $s.N)
        foreach ($hm in $hashModes) {
            $r = Invoke-MashhadLoginKeyTest -HeaderApi $s.H -BodyClientId $s.C -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -HashMode $hm
            if ($r.ErrorCode -eq 0 -and $r.Data.loginKey) {
                Write-Host ('OK - ' + $s.N + ' hash=' + $hm)
                Write-Host ('loginKey: ' + $r.Data.loginKey)
                Write-Host ($BaseUrl + '/Authentication/Start/' + $r.Data.loginKey)
                return
            }
        }
    }
    Write-Host 'All scenarios failed: fix SecretKey with SSO admin OR confirm Username/ClientId on portal.'
    return
}

Write-Host ('DOC layout: header apiName=' + $HeaderApiName + ' body ClientId=' + $ClientId)
$resp = Invoke-MashhadLoginKeyTest -HeaderApi $HeaderApiName -BodyClientId $ClientId -Secret $SecretKey -Base $BaseUrl -StateVal $State -UserTypeVal $UserType -DomainIdVal $DomainID -HashMode 'Secret+Time'

if ($resp.ErrorCode -eq 0 -and $resp.Data.loginKey) {
    Write-Host ('OK - loginKey: ' + $resp.Data.loginKey)
    Write-Host ($BaseUrl + '/Authentication/Start/' + $resp.Data.loginKey)
}
elseif ($resp.ErrorCode -eq 403) {
    Write-Host '403 per doc: header apiSecret/apiName/requestTime OR body ClientId/Hash/Time do not match portal registration.'
    Write-Host 'Use -Probe and splatting for SecretKey (avoid "e* and >> broken quotes).'
}
