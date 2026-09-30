#Requires -Version 5.1
<#
.SYNOPSIS
  SSO loginKey test: prints exact apiName / requestTime / apiSecret (headers) then POSTs.

.EXAMPLE
  PowerShell (one line — do NOT use CMD caret ^):
  .\test-mashhad-sso-loginkey.ps1 -SecretKey "YOUR_SECRET" -ApiName "FinancialAssistant" -ClientId "53db42619cf3C333b13a18D34fbd9111"

.EXAMPLE
  CMD:
  run-test-mashhad-sso-loginkey.cmd -SecretKey "YOUR_SECRET" -ApiName FinancialAssistant -ClientId 53db42619cf3C333b13a18D34fbd9111
#>
param(
    [string]$ApiName = "FinancialAssistant",
    [string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$State = "test",
    [int]$UserType = 0,
    [int]$DomainID = 0,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [switch]$HashUpper,
    [switch]$UseLegacyLkeyClientId,
    [int]$RequestTimeoutSec = 60,
    [switch]$PrintOnly,
    [switch]$PauseBeforeSend
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')
$ApiName = $ApiName.Trim()
$SecretKey = $SecretKey.Trim()
$ClientId = $ClientId.Trim()

if ([string]::IsNullOrWhiteSpace($SecretKey)) {
    throw 'SecretKey is empty after trim (check copy/paste; no trailing spaces).'
}

function ConvertFrom-SsoGetCurrentTimeBody([string]$Raw) {
    if ([string]::IsNullOrWhiteSpace($Raw)) { return $null }
    $t = $Raw.Trim()
    if ($t.StartsWith('{')) {
        try {
            $j = $t | ConvertFrom-Json
            if ($null -ne $j.Data -and -not [string]::IsNullOrWhiteSpace([string]$j.Data)) {
                return [string]$j.Data
            }
        }
        catch { }
    }
    if ($t -match '<Data[^>]*>([^<]+)</Data>') {
        return $Matches[1]
    }
    return $null
}

function Enable-SsoTlsSession {
    if ($env:OS -notlike '*Windows*') { return }
    try {
        # PowerShell 5.1: TLS 1.2 only (avoid SSL3/TLS1.0 handshake failures)
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        [Net.ServicePointManager]::CheckCertificateRevocationList = $false
        Write-Host 'TLS: TLS 1.2, revocation check off for this session.'
    }
    catch {
        Write-Host ('TLS note: ' + $_.Exception.Message)
    }
}

function Get-SsoCurrentTime([string]$RootUrl, [int]$TimeoutSec) {
    $uri = $RootUrl + '/api/Authentication/getCurrentTime'
    $errors = New-Object System.Collections.Generic.List[string]

    Enable-SsoTlsSession

    try {
        $r = Invoke-RestMethod -Uri $uri -Method Get -TimeoutSec $TimeoutSec
        if ($null -ne $r.Data -and -not [string]::IsNullOrWhiteSpace([string]$r.Data)) {
            Write-Host ('getCurrentTime OK via Invoke-RestMethod')
            return [string]$r.Data
        }
        if ($r -is [string]) {
            $parsed = ConvertFrom-SsoGetCurrentTimeBody ([string]$r)
            if ($parsed) {
                Write-Host 'getCurrentTime OK via Invoke-RestMethod (raw)'
                return $parsed
            }
        }
        $errors.Add('Invoke-RestMethod: empty Data')
    }
    catch {
        $errors.Add('Invoke-RestMethod: ' + $_.Exception.Message)
    }

    try {
        $wr = Invoke-WebRequest -Uri $uri -Method Get -TimeoutSec $TimeoutSec -UseBasicParsing
        $parsed = ConvertFrom-SsoGetCurrentTimeBody $wr.Content
        if ($parsed) {
            Write-Host 'getCurrentTime OK via Invoke-WebRequest (JSON or XML)'
            return $parsed
        }
        $errors.Add('Invoke-WebRequest: could not read Data from body')
    }
    catch {
        $errors.Add('Invoke-WebRequest: ' + $_.Exception.Message)
    }

    try {
        Add-Type -AssemblyName System.Net.Http -ErrorAction Stop | Out-Null
        $client = New-Object System.Net.Http.HttpClient
        $client.Timeout = [TimeSpan]::FromSeconds($TimeoutSec)
        $task = $client.GetStringAsync($uri)
        if ($task.Wait([TimeSpan]::FromSeconds($TimeoutSec + 5))) {
            $body = $task.Result
            $parsed = ConvertFrom-SsoGetCurrentTimeBody $body
            if ($parsed) {
                Write-Host 'getCurrentTime OK via HttpClient (JSON or XML)'
                $client.Dispose()
                return $parsed
            }
            $errors.Add('HttpClient: could not read Data from body')
        }
        else {
            $errors.Add('HttpClient: timeout')
        }
        $client.Dispose()
    }
    catch {
        $errors.Add('HttpClient: ' + $_.Exception.Message)
    }

    $curl = Get-Command curl.exe -ErrorAction SilentlyContinue
    if ($curl) {
        try {
            $out = & curl.exe --ssl-no-revoke -sS --connect-timeout 20 --max-time $TimeoutSec $uri 2>&1
            $text = ($out | Out-String).Trim()
            if ($text.Length -gt 0) {
                $parsed = ConvertFrom-SsoGetCurrentTimeBody $text
                if ($parsed) {
                    Write-Host 'getCurrentTime OK via curl.exe (JSON or XML)'
                    return $parsed
                }
            }
            $errors.Add('curl: empty or bad body: ' + $text)
        }
        catch {
            $errors.Add('curl: ' + $_.Exception.Message)
        }
    }
    else {
        $errors.Add('curl.exe not found')
    }

    Write-Host ''
    Write-Host 'getCurrentTime FAILED from this PC. Tried:'
    foreach ($e in $errors) { Write-Host ('  - ' + $e) }
    Write-Host ''
    Write-Host 'Checks:'
    Write-Host '  1) Browser: ' + $uri
    Write-Host '  2) CMD: curl.exe --ssl-no-revoke "' + $uri + '"'
    Write-Host '  3) VPN / proxy / antivirus SSL scan — try another network (mobile hotspot).'
    Write-Host '  4) If PS shows ">>" you have unclosed quotes — press Ctrl+C and paste as ONE line.'
    throw 'Cannot reach login.mashhad.ir getCurrentTime'
}

Enable-SsoTlsSession

if ([string]::IsNullOrWhiteSpace($ClientId) -and -not [string]::IsNullOrWhiteSpace($env:MASHHAD_SSO_CLIENT_ID)) {
    $ClientId = $env:MASHHAD_SSO_CLIENT_ID.Trim()
}
if ([string]::IsNullOrWhiteSpace($ClientId)) {
    throw @'
ClientId required for loginKey body (GUID from SSO portal).
PowerShell example (single line):
  .\test-mashhad-sso-loginkey.ps1 -SecretKey "..." -ClientId "53db42619cf3C333b13a18D34fbd9111" -ApiName "FinancialAssistant"
In PowerShell use backtick ` for line break, NOT caret ^ (that is CMD only).
'@
}

function Get-Sha256Hex([string]$Text, [bool]$Upper) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    # Same as RuleEngine / RayvarzResend: Encoding.ASCII + x2 hex
    $bytes = $sha.ComputeHash([System.Text.Encoding]::ASCII.GetBytes($Text))
    $hex = [BitConverter]::ToString($bytes).Replace("-", "")
    if ($Upper) { return $hex.ToUpperInvariant() }
    return $hex.ToLowerInvariant()
}

function Write-SsoOutboundPreview(
    [string]$Api,
    [string]$Cid,
    [string]$Secret,
    [string]$ReqTime,
    [bool]$Upper,
    [string]$BodyJson
) {
    $raw = $ReqTime + $Secret
    $hash = Get-Sha256Hex $raw $Upper
    $enc = if ($Upper) { 'upper' } else { 'lower' }

    Write-Host ''
    Write-Host '========== BEFORE SEND (exact values) =========='
    Write-Host '--- Request.Headers (SSO reads these) ---'
    Write-Host ('apiName     = ' + $Api)
    Write-Host ('requestTime = ' + $ReqTime)
    Write-Host ('apiSecret   = ' + $hash)
    Write-Host '--- Hash formula (SecretKey is NOT sent in header) ---'
    Write-Host ('SHA256(requestTime + SecretKey) hex ' + $enc)
    Write-Host ('SecretKey length = ' + $Secret.Length + ' chars')
    Write-Host ('Concat preview     = "' + $ReqTime + '" + [SecretKey]')
    Write-Host '--- Body JSON (loginKey) ---'
    Write-Host ('Time     = ' + $ReqTime + '  (must equal header requestTime)')
    Write-Host ('Hash     = ' + $hash + '  (must equal header apiSecret)')
    Write-Host ('ClientId = ' + $Cid + '  (body only; NOT header apiName)')
    Write-Host $BodyJson
    Write-Host '=============================================='
    Write-Host ''
}

function Invoke-LoginKey([string]$Api, [string]$Cid, [string]$Secret, [string]$ReqTime, [bool]$Upper) {
    $raw = $ReqTime + $Secret
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

    Write-SsoOutboundPreview -Api $Api -Cid $Cid -Secret $Secret -ReqTime $ReqTime -Upper $Upper -BodyJson $body

    if ($PrintOnly) {
        Write-Host 'PrintOnly: request NOT sent.'
        return $null
    }

    if ($PauseBeforeSend) {
        Read-Host 'Press Enter to POST loginKey to SSO'
    }

    Write-Host ('Sending POST ' + $BaseUrl + '/api/Authentication/loginKey (timeout ' + $RequestTimeoutSec + 's)...')
    Write-Host ('Started at ' + (Get-Date -Format 'HH:mm:ss') + ' - if nothing prints for ' + $RequestTimeoutSec + 's, network/firewall may block POST.')

    $uri = $BaseUrl + '/api/Authentication/loginKey'
    $headers = @{
        apiName     = $Api
        requestTime = $ReqTime
        apiSecret   = $hash
    }
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($body)
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers `
            -Body $bodyBytes `
            -ContentType 'application/json; charset=utf-8' `
            -TimeoutSec $RequestTimeoutSec -UseBasicParsing
        $sw.Stop()
        Write-Host ('HTTP ' + [int]$response.StatusCode + ' in ' + $sw.ElapsedMilliseconds + ' ms')
        Write-Host '--- Response ---'
        return ($response.Content | ConvertFrom-Json)
    }
    catch [System.Net.WebException] {
        $sw.Stop()
        $resp = $_.Exception.Response
        if ($null -ne $resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $text = $reader.ReadToEnd()
            $reader.Close()
            Write-Host ('HTTP ' + [int]$resp.StatusCode + ' in ' + $sw.ElapsedMilliseconds + ' ms')
            Write-Host '--- Response ---'
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                return ($text | ConvertFrom-Json)
            }
        }
        Write-Host ('Failed: ' + $_.Exception.Message)
        throw
    }
}

Write-Host '== 1) GET getCurrentTime =='
$requestTime = Get-SsoCurrentTime -RootUrl $BaseUrl -TimeoutSec $RequestTimeoutSec
Write-Host ('Data (will become header requestTime and body Time): ' + $requestTime)

$attempts = @(
    @{ Label = 'configured'; Api = $ApiName; Cid = $ClientId; Upper = [bool]$HashUpper }
)
if (-not $HashUpper -and -not $PrintOnly) {
    $attempts += @{ Label = 'hex upper retry'; Api = $ApiName; Cid = $ClientId; Upper = $true }
}

$last = $null
foreach ($a in $attempts) {
    Write-Host ''
    Write-Host ('== Attempt: ' + $a.Label + ' ==')
    $last = Invoke-LoginKey -Api $a.Api -Cid $a.Cid -Secret $SecretKey -ReqTime $requestTime -Upper $a.Upper
    if ($PrintOnly) { continue }
    if ($null -ne $last) {
        $last | ConvertTo-Json -Depth 5
    }
    if ($null -ne $last -and $last.ErrorCode -eq 0 -and $last.Data.loginKey) {
        Write-Host ('OK - loginKey: ' + $last.Data.loginKey)
        exit 0
    }
    if ($null -ne $last -and $last.ErrorCode -eq 403) { break }
}

if ($PrintOnly) { exit 0 }

Write-Host ''
if ($null -ne $last -and $last.ErrorCode -eq 403) {
    Write-Host '403 Client info missmatched - check apiName, ClientId body, SecretKey with SSO portal.'
}
exit 3
