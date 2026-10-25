param([Parameter(Mandatory = $true)][string]$ImagePath, [Parameter(Mandatory = $true)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$utf8 = New-Object Text.UTF8Encoding($false)
$directory = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($directory) | Out-Null
$codexPath = (Get-Command codex -ErrorAction Stop).Source
$schemaPath = Join-Path $directory 'response.schema.json'
$promptPath = Join-Path $directory 'prompt.txt'
$outputPath = Join-Path $directory 'response.json'
$requestPath = Join-Path $directory 'request.json'
[IO.File]::WriteAllText($schemaPath, '{"type":"object","additionalProperties":false,"required":["visibleDemoLabel","summary"],"properties":{"visibleDemoLabel":{"type":"boolean"},"summary":{"type":"string"}}}', $utf8)
[IO.File]::WriteAllText($promptPath, 'Inspect the attached screenshot only. Do not run any tools or commands. Does the image visibly say it is a demo or simulated screen? Return the specified JSON with a short Chinese summary. This is a connectivity check, not a real Bluetooth test.', $utf8)
$request = @{codexPath=$codexPath;model='';workingDirectory=$directory;outputPath=$outputPath;schemaPath=$schemaPath;promptPath=$promptPath;images=@([IO.Path]::GetFullPath($ImagePath))}
[IO.File]::WriteAllText($requestPath, ($request | ConvertTo-Json -Depth 8), $utf8)
$bridge = Join-Path $PSScriptRoot 'Invoke-Codex.ps1'
& $bridge -RequestPath $requestPath
