# Starts EmpireOneRestAPIITJ (IIS Express) and ItechJump (Angular testbed config).
# UI: http://localhost:4200 (proxies /api → API)  |  API: http://localhost:64150

$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$ApiRoot = Join-Path $Root '00EmpireOneRestAPIITJ'
$UiRoot = Join-Path $Root 'ItechJump'
$SiteName = 'EmpireOneRestAPIITJ'

function Get-IisExpressPath {
    $candidates = @(
        "${env:ProgramFiles}\IIS Express\iisexpress.exe",
        "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe"
    )
    foreach ($path in $candidates) {
        if (Test-Path $path) { return $path }
    }
    throw 'IIS Express not found. Install Visual Studio Build Tools or IIS Express.'
}

function Get-ApiHostConfig {
    param([string] $ProjectRoot)

    $template = Join-Path $ProjectRoot 'testbed\applicationhost.config'
    if (-not (Test-Path $template)) {
        throw "Missing $template. Restore the API testbed folder from source control."
    }

    $resolvedApi = (Resolve-Path $ProjectRoot).Path
    $tempConfig = Join-Path $env:TEMP "itechjump-testbed-applicationhost.config"
    $content = Get-Content $template -Raw
    $content = $content.Replace('__API_PHYSICAL_PATH__', $resolvedApi)
    Set-Content -Path $tempConfig -Value $content -Encoding UTF8
    return $tempConfig
}

function Test-ApiReachable {
    param([string] $Url)

    $status = (& curl.exe -s -o NUL -w '%{http_code}' $Url 2>$null)
    return ($status -eq '200')
}

function Wait-ForHttps {
    param(
        [string] $Url,
        [int] $TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-ApiReachable -Url $Url) {
            return $true
        }
        Start-Sleep -Seconds 2
    }
    return $false
}

if (-not (Test-Path $ApiRoot)) { throw "API project not found: $ApiRoot" }
if (-not (Test-Path $UiRoot)) { throw "UI project not found: $UiRoot" }

$iis = Get-IisExpressPath
$hostConfig = Get-ApiHostConfig -ProjectRoot $ApiRoot

Write-Host 'Starting REST API (IIS Express) on http://localhost:64150 ...' -ForegroundColor Cyan
$apiProcess = Start-Process -FilePath $iis -ArgumentList @(
    "/config:`"$hostConfig`"",
    "/site:$SiteName",
    '/systray:false'
) -PassThru -WindowStyle Minimized

$pingUrl = 'http://localhost:64150/api/status/ping'
if (-not (Wait-ForHttps -Url $pingUrl)) {
    Write-Warning "API did not respond at $pingUrl within the timeout. Continuing anyway - check IIS Express output if the UI cannot reach the API."
} else {
    Write-Host 'REST API is responding.' -ForegroundColor Green
}

Write-Host 'Starting Angular UI (testbed -> local API) on http://localhost:4200 ...' -ForegroundColor Cyan
Write-Host 'Press Ctrl+C to stop the UI; the script will also stop IIS Express.' -ForegroundColor DarkGray

try {
    Push-Location $UiRoot
    if (-not (Test-Path '.\node_modules')) {
        Write-Host 'Running npm install in ItechJump (first run) ...' -ForegroundColor Yellow
        npm install
    }
    npm run start:local
} finally {
    Pop-Location
    if ($apiProcess -and -not $apiProcess.HasExited) {
        Write-Host 'Stopping IIS Express ...' -ForegroundColor DarkGray
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
}