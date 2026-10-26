param([string]$ArchivePath)
$ErrorActionPreference = 'Stop'
$clientRoot = Split-Path -Parent $PSScriptRoot
$targetRoot = Join-Path $clientRoot 'tools'
$sdk = Join-Path $targetRoot 'platform-tools'
$version = '37.0.1'
$checksum = '45F4D63113E895EBDE0C90F194099A4676B6AC653BD28D54314A9E022BBC1A99'
$source = 'https://dl.google.com/android/repository/platform-tools_r37.0.1-win.zip'
$required = @('adb.exe', 'AdbWinApi.dll', 'AdbWinUsbApi.dll', 'libwinpthread-1.dll', 'source.properties', 'NOTICE.txt')
$complete = @($required | Where-Object { !(Test-Path -LiteralPath (Join-Path $sdk $_)) }).Count -eq 0
if ($complete -and (Get-Content -LiteralPath (Join-Path $sdk 'source.properties') -Raw).Contains("Pkg.Revision=$version")) { return }
if (!$ArchivePath) {
    $cache = Join-Path $clientRoot 'artifacts\dependencies'
    New-Item -ItemType Directory -Force -Path $cache | Out-Null
    $ArchivePath = Join-Path $cache "platform-tools_r$version-win.zip"
    Write-Host "Downloading official Android platform-tools $version..."
    Invoke-WebRequest -UseBasicParsing -Uri $source -OutFile $ArchivePath -TimeoutSec 90
}
$actual = (Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA256).Hash
if ($actual -ne $checksum) { throw 'ADB package SHA-256 mismatch; nothing was extracted.' }
New-Item -ItemType Directory -Force -Path $targetRoot | Out-Null
Expand-Archive -LiteralPath $ArchivePath -DestinationPath $targetRoot -Force
if (@($required | Where-Object { !(Test-Path -LiteralPath (Join-Path $sdk $_)) }).Count -gt 0) { throw 'ADB package is incomplete.' }
@{ version=$version; source=$source; sha256=$checksum } | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $sdk 'AIclient-package.json')
Write-Host "ADB $version ready: $sdk"
