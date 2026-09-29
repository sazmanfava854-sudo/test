#Requires -Version 5.1
param(
    [string]$ApiName = "FinancialAssistant",
    [string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$BaseUrl = "https://login.mashhad.ir",
    [int]$TimeoutSec = 45,
    [switch]$SkipPost,
    [switch]$UseCurl
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

function Post-LoginKeyWithHttpClient([string]$Url, [hashtable]$Headers, [byte[]]$BodyBytes, [int]$MaxSec) {
    Write-Host "Using .NET HttpClient (Timeout=${MaxSec}s)..."
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        Add-Type -AssemblyName System.Net.Http -ErrorAction SilentlyContinue | Out-Null
        $handler = New-Object System.Net.Http.HttpClientHandler
        $client = New-Object System.Net.Http.HttpClient($handler)
        $client.Timeout = [TimeSpan]::FromSeconds($MaxSec)
        $req = New-Object System.Net.Http.HttpRequestMessage([System.Net.Http.HttpMethod]::Post, $Url)
        foreach ($k in $Headers.Keys) {
            $req.Headers.TryAddWithoutValidation([string]$k, [string]$Headers[$k]) | Out-Null
        }
        $content = New-Object System.Net.Http.ByteArrayContent(,$BodyBytes)
        $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/json; charset=utf-8")
        $req.Content = $content
        Write-Host ("POST started at " + (Get-Date -Format "HH:mm:ss") + " (wait up to ${MaxSec}s)...")
        $task = $client.SendAsync($req)
        if (-not $task.Wait([TimeSpan]::FromSeconds($MaxSec + 5))) {
            $sw.Stop()
            Write-Host ("TIMEOUT after " + $sw.ElapsedMilliseconds + " ms (no response body).")
            $client.Dispose()
            return $null
        }
        $resp = $task.Result
        $bodyTask = $resp.Content.ReadAsStringAsync()
        $bodyTask.Wait([TimeSpan]::FromSeconds(30)) | Out-Null
        $text = $bodyTask.Result
        $sw.Stop()
        Write-Host ("HTTP " + [int]$resp.StatusCode + " in " + $sw.ElapsedMilliseconds + " ms")
        if ($text) { Write-Host $text }
        $client.Dispose()
        return $text
    }
    catch {
        $sw.Stop()
        Write-Host ("HttpClient error after " + $sw.ElapsedMilliseconds + " ms: " + $_.Exception.Message)
        if ($_.Exception.InnerException) {
            Write-Host ("  Inner: " + $_.Exception.InnerException.Message)
        }
        return $null
    }
}

function Post-LoginKeyWithIwr([string]$Url, [hashtable]$Headers, [byte[]]$BodyBytes, [int]$MaxSec) {
    Write-Host "Using Invoke-WebRequest (TimeoutSec=$MaxSec)..."
    $sw = [Diagnostics.Stopwatch]::StartNew()
    Write-Host ("POST started at " + (Get-Date -Format "HH:mm:ss") + "...")
    try {
        $r = Invoke-WebRequest -Uri $Url -Method Post -Headers $Headers `
            -Body $BodyBytes -ContentType "application/json; charset=utf-8" `
            -TimeoutSec $MaxSec -UseBasicParsing
        $sw.Stop()
        Write-Host ("HTTP " + [int]$r.StatusCode + " in " + $sw.ElapsedMilliseconds + " ms")
        Write-Host $r.Content
        return $r.Content
    }
    catch [System.Net.WebException] {
        $sw.Stop()
        Write-Host ("WebException after " + $sw.ElapsedMilliseconds + " ms: " + $_.Exception.Message)
        if ($_.Exception.Response) {
            $sr = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
            $t = $sr.ReadToEnd()
            $sr.Close()
            Write-Host ("HTTP " + [int]$_.Exception.Response.StatusCode)
            if ($t) { Write-Host $t; return $t }
        }
        return $null
    }
    catch {
        $sw.Stop()
        Write-Host ("Error after " + $sw.ElapsedMilliseconds + " ms: " + $_.Exception.Message)
        return $null
    }
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
        $out = & curl.exe --ssl-no-revoke -sS --connect-timeout 15 --max-time $MaxSec -w "`nHTTP_CODE:%{http_code}" -X POST $Url `
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

Write-Host "========== 0) Quick reachability (GET getCurrentTime) =========="
try {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $requestTime = [string](Invoke-RestMethod -Uri ($BaseUrl + "/api/Authentication/getCurrentTime") -Method Get -TimeoutSec 30).Data
    $sw.Stop()
    Write-Host ("OK in " + $sw.ElapsedMilliseconds + " ms")
}
catch {
    Write-Host ("getCurrentTime FAILED: " + $_.Exception.Message)
    Write-Host "If GET fails, POST will not work from this network."
    exit 1
}
Write-Host "requestTime = $requestTime"

Write-Host ""
Write-Host "========== 1) BEFORE HASH =========="
Write-Host "apiName (header)   = $ApiName"
Write-Host "ClientId (body)    = $ClientId"
Write-Host "SecretKey length   = $($SecretKey.Length)"

Write-Host ""
Write-Host "========== 2) AFTER HASH =========="
$apiSecret = Get-ApiSecretAscii $SecretKey $requestTime
Write-Host "apiSecret / Hash   = $apiSecret"

$bodyJson = (@{
    Time = $requestTime; Hash = $apiSecret; ClientId = $ClientId
    State = "test"; UserType = 0; DomainID = 0
} | ConvertTo-Json -Compress)

Write-Host ""
Write-Host "========== 3) BODY JSON =========="
Write-Host $bodyJson

if ($SkipPost) {
    Write-Host ""
    Write-Host "SkipPost: no HTTP POST."
    exit 0
}

$url = $BaseUrl + "/api/Authentication/loginKey"
Write-Host ""
Write-Host "========== 4) POST loginKey =========="
Write-Host $url

$headers = @{ apiName = $ApiName; requestTime = $requestTime; apiSecret = $apiSecret }
$bodyBytes = [Text.Encoding]::UTF8.GetBytes($bodyJson)

$content = $null
if ($UseCurl) {
    $content = Post-LoginKeyWithCurl -Url $url -Api $ApiName -ReqTime $requestTime -SecretHash $apiSecret -BodyJson $bodyJson -MaxSec $TimeoutSec
}

if (-not $content) {
    $content = Post-LoginKeyWithHttpClient -Url $url -Headers $headers -BodyBytes $bodyBytes -MaxSec $TimeoutSec
}

if (-not $content) {
    Write-Host ""
    Write-Host "HttpClient had no usable response - trying Invoke-WebRequest..."
    $content = Post-LoginKeyWithIwr -Url $url -Headers $headers -BodyBytes $bodyBytes -MaxSec $TimeoutSec
}

if (-not $content -and -not $UseCurl) {
    Write-Host ""
    Write-Host "Still no response - trying curl as last resort (or use -UseCurl)..."
    $content = Post-LoginKeyWithCurl -Url $url -Api $ApiName -ReqTime $requestTime -SecretHash $apiSecret -BodyJson $bodyJson -MaxSec $TimeoutSec
}

if (-not $content) {
    Write-Host ""
    Write-Host "NO RESPONSE within ${TimeoutSec}s."
    Write-Host "- GET getCurrentTime worked; only POST may be blocked (firewall/proxy)."
    Write-Host "- Run the same script on a PC with normal internet (not locked-down server)."
    Write-Host "- Optional: Test-NetConnection login.mashhad.ir -Port 443"
    exit 2
}

exit 0
