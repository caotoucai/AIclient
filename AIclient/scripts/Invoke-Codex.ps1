param([Parameter(Mandatory = $true)][string]$RequestPath)
$ErrorActionPreference = 'Stop'
$utf8 = New-Object System.Text.UTF8Encoding($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
try {
    $request = Get-Content -LiteralPath $RequestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if (!(Test-Path -LiteralPath $request.codexPath -PathType Leaf)) { throw 'Codex path does not exist. Configure the local codex.ps1 or codex.exe path.' }
    $cliArgs = @('--ask-for-approval', 'never', 'exec', '--cd', [string]$request.workingDirectory, '--sandbox', 'read-only', '--skip-git-repo-check', '--color', 'never', '--json', '--output-schema', [string]$request.schemaPath, '--output-last-message', [string]$request.outputPath)
    if ($request.model) { $cliArgs += @('--model', [string]$request.model) }
    foreach ($image in $request.images) { $cliArgs += @('--image', [string]$image) }
    $cliArgs += '-'
    $prompt = Get-Content -LiteralPath $request.promptPath -Raw -Encoding UTF8
    $prompt | & $request.codexPath @cliArgs
    exit $LASTEXITCODE
} catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
