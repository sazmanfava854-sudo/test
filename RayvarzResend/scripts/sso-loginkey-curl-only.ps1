#Requires -Version 5.1
# Only curl.exe for HTTP (for PCs where Invoke-RestMethod times out).
param(
    [Parameter(Mandatory)][string]$SecretKey,
    [string]$ApiName = "FinancialAssistant",
    [string]$ClientId = "53db42619cf3C333b13a18D34fbd9111",
    [string]$BaseUrl = "https://login.mashhad.ir",
    [string]$RequestTime = "",
    [int]$TimeoutSec = 60,
    [switch]$SkipPost,
    [switch]$ProbeSwaps,
    [switch]$RuleEngineJsonBody
)

$SecretKey = $SecretKey.Trim()
$ApiName = $ApiName.Trim()
$ClientId = $ClientId.Trim()
$BaseUrl = $BaseUrl.TrimEnd('/')
$uriTime = $BaseUrl + "/api/Authentication/getCurrentTime"

function Get-Hash([string]$Time, [string]$Secret) {
    $raw = $Secret + $Time
    $bytes = [Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::ASCII.GetBytes($raw))
    ($bytes | ForEach-Object { $_.ToString("x2") }) -join ''
}

function Build-RuleEngineBodyJson([string]$Time, [string]$Hash, [string]$Cid) {
    # همان ترتیب و شکل SSO.cs در RuleEngine
    return @"
{
 ""Time"": "$Time",
""Hash"": "$Hash",
""ClientId"": "$Cid",
""State"": ""test"",
""UserType"": 0,
""DomainID"": 0
} "
}

function Invoke-LoginKeyCurl(
    [string]$Time,
    [string]$Hash,
    [string]$HeaderApiName,
    [string]$BodyClientId,
    [string]$BodyJson,
    [string]$Label
) {
    Write-Host ""
    Write-Host "========== $Label =========="
    Write-Host "HEADER apiName     = $HeaderApiName"
    Write-Host "HEADER requestTime = $Time"
    Write-Host "HEADER apiSecret   = $Hash"
    Write-Host "BODY  ClientId     = $BodyClientId"
    Write-Host "hash raw (RuleEngine) = [SecretKey]+$Time"
    Write-Host "body = $BodyJson"

    if ($SkipPost) { return }

    $tmp = Join-Path $env:TEMP ("loginkey-" + [Guid]::NewGuid().ToString("N") + ".json")
    [IO.File]::WriteAllText($tmp, $BodyJson, (New-Object Text.UTF8Encoding $false))
    $url = $BaseUrl + "/api/Authentication/loginKey"
    try {
        & curl.exe --ssl-no-revoke -sS --connect-timeout 15 --max-time $TimeoutSec -w "`nHTTP_CODE:%{http_code}`n" -X POST $url `
            -H "apiName: $HeaderApiName" -H "requestTime: $Time" -H "apiSecret: $Hash" `
            -H "Content-Type: application/json" --data-binary "@$tmp"
    }
    finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    }
}

if (-not (Get-Command curl.exe -ErrorAction SilentlyContinue)) {
    Write-Error "curl.exe not found (Windows 10+ includes it)."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($RequestTime)) {
    Write-Host "GET getCurrentTime (curl)..."
    $json = (& curl.exe --ssl-no-revoke -sS --connect-timeout 15 --max-time $TimeoutSec $uriTime 2>&1 | Out-String).Trim()
    Write-Host $json
    $o = $json | ConvertFrom-Json
    if ($o.ErrorCode -ne 0 -or [string]::IsNullOrWhiteSpace([string]$o.Data)) {
        Write-Error "getCurrentTime failed"
        exit 1
    }
    $RequestTime = [string]$o.Data
}
else {
    $RequestTime = $RequestTime.Trim()
}

$hash = Get-Hash $RequestTime $SecretKey
Write-Host "requestTime = $RequestTime"
Write-Host "apiSecret   = $hash (SHA256 ASCII hex lower, Secret+time — same as RuleEngine SSO.cs)"

if ($RuleEngineJsonBody) {
    $bodyJson = Build-RuleEngineBodyJson $RequestTime $hash $ClientId
}
else {
    $bodyJson = (@{
        Time = $RequestTime; Hash = $hash; ClientId = $ClientId
        State = "test"; UserType = 0; DomainID = 0
    } | ConvertTo-Json -Compress)
}

if ($ProbeSwaps) {
    Invoke-LoginKeyCurl -Time $RequestTime -Hash $hash -HeaderApiName $ApiName -BodyClientId $ClientId -BodyJson $bodyJson -Label "A) normal: apiName=ApiName, body ClientId=GUID"
    Invoke-LoginKeyCurl -Time $RequestTime -Hash $hash -HeaderApiName $ClientId -BodyClientId $ApiName -BodyJson (Build-RuleEngineBodyJson $RequestTime $hash $ApiName) -Label "B) swapped headers: apiName=GUID, body ClientId=ApiName"
    Invoke-LoginKeyCurl -Time $RequestTime -Hash $hash -HeaderApiName $ApiName -BodyClientId $ApiName -BodyJson (Build-RuleEngineBodyJson $RequestTime $hash $ApiName) -Label "C) body ClientId=ApiName (LoginKeyBodyClientIdIsApiName)"
    Invoke-LoginKeyCurl -Time $RequestTime -Hash $hash -HeaderApiName $ClientId -BodyClientId $ClientId -BodyJson (Build-RuleEngineBodyJson $RequestTime $hash $ClientId) -Label "D) apiName=GUID, body ClientId=GUID"
    exit 0
}

Invoke-LoginKeyCurl -Time $RequestTime -Hash $hash -HeaderApiName $ApiName -BodyClientId $ClientId -BodyJson $bodyJson -Label "loginKey"
