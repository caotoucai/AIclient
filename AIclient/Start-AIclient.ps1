param([switch]$Build)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'scripts\Prepare-Adb.ps1')
$project = Join-Path $PSScriptRoot 'src\AIclient\AIclient.csproj'
$app = Join-Path $PSScriptRoot 'src\AIclient\bin\Release\net9.0-windows\AIclient.exe'
if ($Build -or !(Test-Path -LiteralPath $app)) {
    dotnet build $project -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'AIclient build failed.' }
}
Start-Process -FilePath $app -WorkingDirectory $PSScriptRoot
