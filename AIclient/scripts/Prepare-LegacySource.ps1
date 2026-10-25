param([Parameter(Mandatory=$true)][string]$Source, [Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference = 'Stop'
$text = [IO.File]::ReadAllText($Source, [Text.Encoding]::UTF8)
$startup = 'InitStartupFrames("VddmBackBoneFr48", "CemBackBoneFr23")'
$wakeup = '                    Wakup_DHU_By_Flexray();'
if (-not $text.Contains($startup) -or -not $text.Contains($wakeup)) { throw 'Legacy FlexRay source changed; review startup and wake behavior before building.' }
$text = $text.Replace($startup, 'InitStartupFrames(AIclient.DeviceHost.LegacyOptions.ErayFrame, AIclient.DeviceHost.LegacyOptions.ColdFrame)')
$text = $text.Replace($wakeup, '                    // AIclient: connect only; wake frames must be explicit configured commands.')
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Output)) | Out-Null
if (-not [IO.File]::Exists($Output) -or [IO.File]::ReadAllText($Output) -ne $text) { [IO.File]::WriteAllText($Output, $text, (New-Object Text.UTF8Encoding($false))) }
