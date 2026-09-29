# Brings ASSETLEN up for testing: clean stop, build once, API on the https
# profile (the client hardcodes https://localhost:7264), then the client.
# Stop with tools/stop-dev.ps1 when done.
#
#   pwsh tools/start-dev.ps1            API + client
#   pwsh tools/start-dev.ps1 -ApiOnly   API only (enough for tools/e2e-all.sh)

param([switch]$ApiOnly)

$root = Split-Path $PSScriptRoot -Parent
& "$PSScriptRoot/stop-dev.ps1" -KeepIde | Out-Null

dotnet build "$root/assetlen.sln" -nologo -v:q
if ($LASTEXITCODE -ne 0) { Write-Error "build failed"; exit 1 }

$logs = Join-Path $root 'tools/fixtures/logs'
New-Item -ItemType Directory -Force $logs | Out-Null

# One string with the path quoted: Start-Process joins an array unquoted, and the repo path has spaces.
Start-Process dotnet -ArgumentList "run --no-build --project `"$root/assetlen.API`" --launch-profile https" `
    -WorkingDirectory $root -WindowStyle Hidden `
    -RedirectStandardOutput "$logs/api.out.log" -RedirectStandardError "$logs/api.err.log"

if (-not $ApiOnly) {
    Start-Process dotnet -ArgumentList "run --no-build --project `"$root/assetlen.Client`" --launch-profile https" `
        -WorkingDirectory $root -WindowStyle Hidden `
        -RedirectStandardOutput "$logs/client.out.log" -RedirectStandardError "$logs/client.err.log"
}

$want = if ($ApiOnly) { @(7264) } else { @(7264, 7025) }
for ($i = 0; $i -lt 90; $i++) {
    $up = (Get-NetTCPConnection -LocalPort $want -State Listen -ErrorAction SilentlyContinue).LocalPort | Sort-Object -Unique
    if (@($up).Count -eq $want.Count) { break }
    Start-Sleep -Seconds 1
}
if (@($up).Count -ne $want.Count) { Write-Error "not listening after 90s — see $logs"; exit 1 }

Write-Host "API     https://localhost:7264   (e2e: bash tools/e2e-all.sh https://localhost:7264/api)"
if (-not $ApiOnly) { Write-Host "Client  https://localhost:7025" }
Write-Host "Stop:   pwsh tools/stop-dev.ps1"
