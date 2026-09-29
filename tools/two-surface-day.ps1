<#
  ASSETLEN — the two-surface test (assetlen.md §11, test 2; plan.md P9).

    "Take one real site day. Hand-build the full log and the three-block brief.
     If a contractor says 'I'd have dropped two of those' and Peter says 'this
     is what I wanted,' the curation model holds."

  This builds the page the two of them are shown. Left: the Site Diary for the
  day — every message, every photo, who sent it, nothing condensed. Right: the
  brief for the same day as Assetlen assembles it — the three blocks that lead
  it, the truth floor beneath, and every frame with a box to tick "I'd drop
  this". Printed or opened in a browser, it is the thing to hand-correct.

  THE REAL EXPORT NEVER ENTERS THIS REPOSITORY, and neither does this page: it
  quotes the thread and embeds its photos. The script refuses an output path
  inside the repository.

  Usage (pwsh):
    ./tools/two-surface-day.ps1 -Email peter@example -Password '…' -Day 2026-09-22 `
        -Export "D:\...\WhatsApp Chat.zip" [-Media "D:\...\footage"] `
        [-Project <existing id>] [-Api https://localhost:7264/api] [-Out <file.html>]
#>
param(
    [Parameter(Mandatory)] [string] $Email,
    [Parameter(Mandatory)] [string] $Password,
    [Parameter(Mandatory)] [string] $Day,
    [string] $Export,
    [string] $Media,
    [string] $Project,
    [string] $Api = 'https://localhost:7264/api',
    [string] $Name = 'Two-surface test',
    [string] $Out = (Join-Path ([IO.Path]::GetTempPath()) "assetlen-two-surface-$Day.html")
)
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$outFull = [IO.Path]::GetFullPath($Out)
if ($outFull.StartsWith($repo, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to write the page inside the repository: $outFull"
}

$login = Invoke-RestMethod -SkipCertificateCheck -Method Post -Uri "$Api/Authorization/Login" `
    -ContentType 'application/json' -Body (@{ Email = $Email; Password = $Password } | ConvertTo-Json)
$h = @{ Authorization = "Bearer $($login.token)" }

if (-not $Project) {
    if (-not (Test-Path $Export)) { throw "-Export must point at the export (.txt or .zip), or pass -Project." }
    $created = Invoke-RestMethod -SkipCertificateCheck -Method Post -Uri "$Api/ProjectsRS/CreateProject" -Headers $h `
        -ContentType 'application/json' -Body (@{ ProjectName = $Name; Description = 'Two-surface test (§11)'; Currency = 'UGX'; Stages = @() } | ConvertTo-Json)
    $Project = $created.id
    $pre = Invoke-RestMethod -SkipCertificateCheck -Method Post -Uri "$Api/Ingest/UploadArchive" -Headers $h `
        -Form @{ file = Get-Item $Export; projectId = $Project }
    Write-Host "Preview: $($pre.messageCount) messages — read that number before trusting anything below."
    $null = Invoke-RestMethod -SkipCertificateCheck -Method Post -Uri "$Api/Ingest/CommitImport" -Headers $h `
        -ContentType 'application/json' -Body (@{ BatchId = $pre.batchId; AuthorMappings = @() } | ConvertTo-Json)
    if ($Media -and (Test-Path $Media)) {
        $files = Get-ChildItem $Media -Recurse -File -Include *.jpg, *.jpeg, *.png, *.mp4 | Sort-Object FullName
        for ($k = 0; $k -lt $files.Count; $k += 10) {
            $null = Invoke-RestMethod -SkipCertificateCheck -Method Post -Uri "$Api/Ingest/RejoinMedia" -Headers $h `
                -Form @{ Files = @($files[$k..([Math]::Min($k + 9, $files.Count - 1))]); ProjectId = $Project }
        }
    }
}

$from = [datetime]::ParseExact($Day, 'yyyy-MM-dd', $null)
$q = "ProjectId=$Project&From=$($from.ToString('s'))&To=$($from.AddDays(1).ToString('s'))&Take=1000"
$log = Invoke-RestMethod -SkipCertificateCheck -Uri "$Api/Ingest/GetMessages?$q" -Headers $h
$messages = @($log.items ?? $log.messages ?? $log) | Where-Object { $_.sentAt } | Sort-Object sentAt, sequenceNo
$brief = Invoke-RestMethod -SkipCertificateCheck -Uri "$Api/Brief/Day?projectId=$Project&day=$Day&days=1" -Headers $h

$thumbs = @{}
function Thumb([string] $artifactId) {
    if (-not $artifactId) { return $null }
    if ($thumbs.ContainsKey($artifactId)) { return $thumbs[$artifactId] }
    try {
        $tmp = [IO.Path]::GetTempFileName()
        Invoke-WebRequest -SkipCertificateCheck -Uri "$Api/Artifacts/$artifactId/thumbnail" -Headers $h -OutFile $tmp | Out-Null
        $thumbs[$artifactId] = 'data:image/jpeg;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes($tmp))
        Remove-Item $tmp
    } catch { $thumbs[$artifactId] = $null }
    return $thumbs[$artifactId]
}
function Esc([string] $s) { [Net.WebUtility]::HtmlEncode($s ?? '') }
function Img($id) { $src = Thumb $id; if ($src) { "<img src='$src' alt=''>" } else { "<span class='nofile'>file</span>" } }

$sb = [Text.StringBuilder]::new()
[void]$sb.Append(@"
<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Two-surface test $Day</title>
<style>
:root { --bg:#fafaf7; --paper:#fff; --text:#1a1d21; --muted:#6b6f76; --rule:rgba(26,29,33,.12); --accent:#c2542a; }
@media (prefers-color-scheme: dark) { :root { --bg:#15171a; --paper:#1c1f23; --text:#f0eee8; --muted:#a5a8af; --rule:rgba(255,255,255,.12); --accent:#d66948; } }
body { margin:0; background:var(--bg); color:var(--text); font:15px/1.5 Inter,"Segoe UI",sans-serif; }
header { padding:24px 16px 8px; max-width:1280px; margin:auto; }
h1 { font-family:Fraunces,Georgia,serif; font-weight:600; margin:0 0 4px; }
.cols { display:grid; grid-template-columns:1fr 1fr; gap:24px; max-width:1280px; margin:auto; padding:0 16px 48px; }
@media (max-width:800px) { .cols { grid-template-columns:1fr; } }
section { background:var(--paper); border:1px solid var(--rule); border-radius:10px; padding:16px; min-width:0; }
h2 { font-family:Fraunces,Georgia,serif; font-size:20px; margin:0 0 4px; } h3 { margin:16px 0 6px; font-size:15px; }
.sub { color:var(--muted); font-size:13px; margin:0 0 12px; }
.msg { display:grid; grid-template-columns:3.2rem 1fr; gap:8px; padding:6px 0; border-top:1px solid var(--rule); }
.t { color:var(--muted); font-variant-numeric:tabular-nums; font-size:13px; } .who { font-weight:600; font-size:13px; }
.body { overflow-wrap:anywhere; } img { width:120px; aspect-ratio:3/2; object-fit:cover; border-radius:4px; display:block; }
.frames { display:flex; flex-wrap:wrap; gap:8px; } .frame { display:flex; flex-direction:column; gap:4px; font-size:12px; }
.item { padding:6px 0; border-top:1px solid var(--rule); display:flex; gap:8px; align-items:flex-start; }
.drop { font-size:12px; color:var(--muted); white-space:nowrap; } .nofile { color:var(--muted); font-size:12px; }
.lead { border-left:3px solid var(--accent); padding-left:10px; } textarea { width:100%; min-height:5rem; font:inherit; }
@media print { .cols { grid-template-columns:1fr 1fr; } section { break-inside:auto; } }
</style></head><body>
<header><h1>One site day, two surfaces — $Day</h1>
<p class="sub">Left: everything that happened, as the Site Diary keeps it. Right: the brief Peter reads. Contractor: tick what you would have dropped. Peter: say whether this is what you wanted.</p></header>
<div class="cols"><section><h2>The Site Diary</h2><p class="sub">$(@($messages).Count) messages and files, nothing condensed, true authorship.</p>
"@)
foreach ($m in $messages) {
    $t = ([datetime]$m.sentAt).ToString('HH:mm')
    $who = Esc ($m.authorMemberName ?? $m.externalAuthor)
    $content = if ($m.artifactId) { (Img $m.artifactId) + "<div class='t'>$(Esc $m.mediaFileName)</div>" } else { Esc $m.body }
    if ($m.artifactId -and $m.body) { $content = (Esc $m.body) + $content }
    [void]$sb.Append("<div class='msg'><span class='t'>$t</span><div><div class='who'>$who</div><div class='body'>$content</div></div></div>")
}
[void]$sb.Append("</section><section><h2>The brief</h2><p class='sub'>Grouped by the work, not by time. The first three blocks lead; the truth floor cannot be dropped.</p>")
$i = 0
foreach ($b in @($brief.blocks)) {
    $i++
    $title = Esc ($b.deliverableTitle ?? $b.stageName ?? 'Not tied to a deliverable')
    $lead = if ($i -le 3) { " class='lead'" } else { '' }
    [void]$sb.Append("<div$lead><h3>$title$(if ($b.percent -ne $null) { " — $($b.percent)%" })</h3>")
    foreach ($n in @($b.notes)) {
        [void]$sb.Append("<div class='item'><label class='drop'><input type='checkbox'> drop</label><div><span class='t'>$(([datetime]$n.at).ToString('HH:mm'))</span> $(Esc $n.text)</div></div>")
    }
    $frames = @($b.pairs | ForEach-Object { $_.before; $_.after }) + @($b.frames) | Where-Object { $_ }
    if ($frames.Count) {
        [void]$sb.Append("<div class='frames'>")
        foreach ($f in $frames) { [void]$sb.Append("<div class='frame'>$(Img $f.artifactId)<label class='drop'><input type='checkbox'> drop</label></div>") }
        [void]$sb.Append("</div>")
    }
    [void]$sb.Append("</div>")
}
[void]$sb.Append("<h3>The truth floor</h3>")
foreach ($s in @($brief.truthFloor)) {
    foreach ($it in @($s.items)) {
        [void]$sb.Append("<div class='item'><span class='t'>$(Esc $s.label)</span><div>$(Esc $it.title)$(if ($it.detail) { ' — ' + (Esc $it.detail) })</div></div>")
    }
}
[void]$sb.Append(@"
<h3>What would you have dropped, and why?</h3><textarea></textarea>
<h3>Peter: is this what you wanted?</h3><textarea></textarea>
</section></div></body></html>
"@)
[IO.File]::WriteAllText($outFull, $sb.ToString())
Write-Host "Diary: $(@($messages).Count) items · Brief: $(@($brief.blocks).Count) blocks, $(@($brief.truthFloor.items).Count) floor items"
Write-Host "Page:  $outFull  (outside the repository — keep it there)"
