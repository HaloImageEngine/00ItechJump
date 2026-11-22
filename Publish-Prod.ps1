# Production publish: copies Release API + Angular build into ./publish/
#   publish/api  — EmpireOneRestAPIITJ (IIS / ASP.NET 4.8)
#   publish/ui   — ItechJump static SPA (IIS + web.config)

$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$ApiProject = Join-Path $Root '00EmpireOneRestAPIITJ\EmpireOneRestAPIITJ.csproj'
$UiRoot = Join-Path $Root 'ItechJump'
$PublishRoot = Join-Path $Root 'publish'
$PublishApi = Join-Path $PublishRoot 'api'
$PublishUi = Join-Path $PublishRoot 'ui'

function New-VersionId {
    # C + YY + MM + DD (e.g. C261122 for 2026-11-22)
    return 'C' + (Get-Date).ToString('yyMMdd')
}

function Set-AngularVersionId {
    param(
        [string] $VersionTsPath,
        [string] $Value
    )
    if (-not (Test-Path $VersionTsPath)) { throw "Not found: $VersionTsPath" }
    $content = Get-Content $VersionTsPath -Raw
    $pattern = "(export const versionId = ')[^']+(')"
    if ($content -notmatch $pattern) { throw "versionId export not found in $VersionTsPath" }
    $content = [regex]::Replace($content, $pattern, "`${1}$Value`${2}", 1)
    Set-Content -Path $VersionTsPath -Value $content -Encoding UTF8 -NoNewline
}

function Set-WebConfigAppSetting {
    param(
        [string] $ConfigPath,
        [string] $Key,
        [string] $Value
    )
    if (-not (Test-Path $ConfigPath)) { throw "Config not found: $ConfigPath" }
    $content = Get-Content $ConfigPath -Raw
    $pattern = "(<add\s+key=`"$Key`"\s+value=`")[^`"]*(`"\s*/?>)"
    if ($content -match $pattern) {
        $content = [regex]::Replace($content, $pattern, "`${1}$Value`${2}", 1)
    } else {
        throw "appSettings key '$Key' not found in $ConfigPath"
    }
    Set-Content -Path $ConfigPath -Value $content -Encoding UTF8 -NoNewline
}

function Get-MsBuildPath {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($msbuild -and (Test-Path $msbuild)) { return $msbuild }
    }
    throw 'MSBuild not found. Install Visual Studio or Build Tools with the .NET Framework web workload.'
}

Write-Host "Publish root: $PublishRoot" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $PublishRoot | Out-Null

# --- REST API (Release, file-system publish) ---
$apiWebConfig = Join-Path $Root '00EmpireOneRestAPIITJ\Web.config'
$versionId = New-VersionId
Write-Host "Setting VersionId $versionId in Web.config ..." -ForegroundColor Cyan
Set-WebConfigAppSetting -ConfigPath $apiWebConfig -Key 'VersionId' -Value $versionId
$angularVersionTs = Join-Path $UiRoot 'src\environments\version.ts'
Write-Host "Setting Angular versionId $versionId in version.ts ..." -ForegroundColor Cyan
Set-AngularVersionId -VersionTsPath $angularVersionTs -Value $versionId

$msbuild = Get-MsBuildPath
Write-Host 'Publishing REST API (Release) ...' -ForegroundColor Cyan
& $msbuild $ApiProject `
    /t:WebPublish `
    /p:WebPublishMethod=FileSystem `
    /p:PublishUrl="$PublishApi\" `
    /p:Configuration=Release `
    /p:DeleteExistingFiles=True `
    /v:minimal
if ($LASTEXITCODE -ne 0) { throw "API publish failed (exit $LASTEXITCODE)." }

# --- Angular UI (production) ---
Write-Host 'Building Angular UI (production) ...' -ForegroundColor Cyan
Push-Location $UiRoot
try {
    if (-not (Test-Path '.\node_modules')) {
        Write-Host 'npm install ...' -ForegroundColor Yellow
        npm install
    }
    npm run build:prod
    if ($LASTEXITCODE -ne 0) { throw "UI build failed (exit $LASTEXITCODE)." }

    $distBrowser = Join-Path $UiRoot 'dist\ITechJump-mobile\browser'
    if (-not (Test-Path $distBrowser)) {
        throw "UI output not found at $distBrowser"
    }

    if (Test-Path $PublishUi) {
        Remove-Item -LiteralPath $PublishUi -Recurse -Force
    }
    New-Item -ItemType Directory -Path $PublishUi | Out-Null
    Copy-Item -Path (Join-Path $distBrowser '*') -Destination $PublishUi -Recurse -Force
    Copy-Item -Path (Join-Path $UiRoot 'web.config') -Destination $PublishUi -Force
} finally {
    Pop-Location
}

$stamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$prodEnv = Join-Path $UiRoot 'src\environments\environment.prod.ts'
$uiVersion = 'unknown'
$uiVersionId = $versionId
if (Test-Path $prodEnv) {
    $m = [regex]::Match((Get-Content $prodEnv -Raw), "appVersion:\s*'([^']+)'")
    if ($m.Success) { $uiVersion = $m.Groups[1].Value }
}
$versionTs = Join-Path $UiRoot 'src\environments\version.ts'
if (Test-Path $versionTs) {
    $mvUi = [regex]::Match((Get-Content $versionTs -Raw), "export const versionId = '([^']+)'")
    if ($mvUi.Success) { $uiVersionId = $mvUi.Groups[1].Value }
}
$apiVersion = 'unknown'
$apiVersionId = 'unknown'
$webConfig = Join-Path $PublishApi 'Web.config'
if (Test-Path $webConfig) {
    $wc = Get-Content $webConfig -Raw
    $mx = [regex]::Match($wc, 'key="AppVersion"\s+value="([^"]+)"')
    if ($mx.Success) { $apiVersion = $mx.Groups[1].Value }
    $mv = [regex]::Match($wc, 'key="VersionId"\s+value="([^"]+)"')
    if ($mv.Success) { $apiVersionId = $mv.Groups[1].Value }
}
@"
publishedUtc: $stamp
uiVersion: $uiVersion
uiVersionId: $uiVersionId
apiVersion: $apiVersion
apiVersionId: $apiVersionId
api: $PublishApi
ui: $PublishUi
"@ | Set-Content (Join-Path $PublishRoot 'build-info.txt') -Encoding UTF8

Write-Host ''
Write-Host 'Production publish complete.' -ForegroundColor Green
Write-Host "  API -> $PublishApi"
Write-Host "  UI  -> $PublishUi"
Write-Host ''
Write-Host 'Deploy each folder as its own IIS site (or UI behind CDN, API on api host).' -ForegroundColor DarkGray