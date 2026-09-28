# Leaves the machine idle and buildable: nothing listening, nothing holding a DLL.
# A live assetlen.API outlives the `dotnet run` that launched it and keeps
# assetlen.Service.dll / assetlen.SqlServer.dll open, so the next build fails with
# MSB3027/MSB3021 copy errors that read like a code problem (CLAUDE.md §0).
#
#   pwsh tools/stop-dev.ps1           stop the app and CLI build servers
#   pwsh tools/stop-dev.ps1 -KeepIde  leave build nodes owned by VS / VS Code alone

param([switch]$KeepIde)

$ports = 7264, 5140, 7025, 5042

$owners = (Get-NetTCPConnection -LocalPort $ports -State Listen -ErrorAction SilentlyContinue).OwningProcess
$apps   = (Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like 'assetlen*' }).Id
$runs   = (Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" |
           Where-Object { $_.CommandLine -match '\brun\b.*assetlen\.(API|Client)' }).ProcessId

$targets = @($owners) + @($apps) + @($runs) | Where-Object { $_ -and $_ -ne 0 } | Sort-Object -Unique
foreach ($id in $targets) {
    try { Stop-Process -Id $id -Force -ErrorAction Stop; Write-Host "stopped app process $id" } catch {}
}

if ($KeepIde) {
    # Only CLI-spawned MSBuild worker nodes; the IDEs' own nodes are parented by them.
    $ide = (Get-Process -ErrorAction SilentlyContinue |
            Where-Object { $_.ProcessName -match '^(devenv|Code|Microsoft\.VisualStudio.*|ServiceHub.*)$' }).Id
    $cliNodes = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" |
        Where-Object { $_.CommandLine -match 'MSBuild\.dll.*nodemode' } |
        Where-Object {
            $p = Get-CimInstance Win32_Process -Filter "ProcessId=$($_.ParentProcessId)" -ErrorAction SilentlyContinue
            -not ($p -and ($ide -contains $p.ProcessId -or $p.CommandLine -match 'vscode|csdevkit'))
        }
    foreach ($n in $cliNodes) {
        try { Stop-Process -Id $n.ProcessId -Force -ErrorAction Stop; Write-Host "stopped build node $($n.ProcessId)" } catch {}
    }
} else {
    dotnet build-server shutdown | Out-Null
    Write-Host "build servers shut down (MSBuild nodes, compiler, Razor)"
}

$left = Get-NetTCPConnection -LocalPort $ports -State Listen -ErrorAction SilentlyContinue
if ($left) { Write-Warning "still listening: $($left.LocalPort -join ', ')"; exit 1 }
Write-Host "idle: nothing on $($ports -join ', ')"
