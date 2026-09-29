#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the works report suite (works-report.md §10).
#
# THE REAL EXPORT AND FOOTAGE ARE NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). This synthesises the *shape* of the
# 5 Aug – 28 Sep window with the repo's pseudonyms — Peter, Nalan, Dinah — and
# generic third parties ("the window team", "the aluminium team"): a completion
# date set and restated, a plaster reading that stalls at 90% while the thread
# says work continues, a "complete this week" that lapses, readings that give a
# pace, a UGX 100M release sent through an agent, and footage exported
# *without media* — every photo a <Media omitted> line — with the loose files
# arriving separately, one of them in the wrong day's folder.
#
# Usage:  bash tools/make-report-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing, Compress-Archive). ffmpeg is optional: with it a
#         short video is added so the poster job has something to read.
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/report}"
rm -rf "$OUT"
mkdir -p "$OUT/thread" "$OUT/media/24th august" "$OUT/media/22nd september" "$OUT/media/21st september" "$OUT/media/stray"

# US-dialect Android export; 8/13 settles the dialect. Photos are <Media omitted>.
cat > "$OUT/thread/WhatsApp Chat with Riverside house works.txt" <<'EOF'
8/5/26, 9:00 AM - Messages and calls are end-to-end encrypted. Only people in this chat can read, listen to, or share them.
8/5/26, 10:15 AM - Nalan: Window design as in that design, no changes.
8/5/26, 10:20 AM - Peter: Okay
8/13/26, 4:30 PM - Peter: I want everything complete by the end of September.
8/13/26, 5:05 PM - Nalan: Noted. We target completion of the whole project by 30 September.
8/13/26, 6:40 PM - Peter: Sent UGX 100M through the agent today for the windows and the next phase.
8/17/26, 8:10 AM - Nalan: Cast postponed, the machinery did not come.
8/22/26, 7:30 PM - Nalan: We are on track for the end of September.
8/24/26, 5:08 PM - Nalan: Summary update. Main house undercoat is at 70%.
8/24/26, 5:10 PM - Nalan: Guest wing plaster is currently at 80%.
8/24/26, 5:10 PM - Nalan: <Media omitted>
8/24/26, 5:12 PM - Peter: Noted
9/2/26, 6:00 PM - Nalan: Guest wing plaster is at 90%, complete this week.
9/4/26, 6:30 PM - Nalan: Guest wing plaster is at 90%.
9/4/26, 6:35 PM - Nalan: Power cuts slowed the stonework today.
9/7/26, 9:00 AM - Nalan: The epoxy team did not show up. They say they will be back tomorrow. Site levelling started.
9/8/26, 4:00 PM - Nalan: Epoxy sample approved, grey. The window team is slow to respond.
9/14/26, 5:00 PM - Nalan: Bathroom wall tiling started. Bathroom wall tiling is at 20%.
9/15/26, 5:30 PM - Dinah: Terrazzo on the terrace, one colour, no black.
9/15/26, 5:45 PM - Nalan: Terrazzo is at 10%.
9/17/26, 11:00 AM - Peter: Why was the parapet raised?
9/20/26, 10:00 AM - Nalan: Guest wing maintained as planned.
9/21/26, 6:00 PM - Nalan: Bathroom wall tiling is at 50%. Terrazzo is at 45%.
9/21/26, 6:05 PM - Peter: Thank you
9/22/26, 5:58 PM - Nalan: Windows fixed on the main house except 4 openings.
9/22/26, 5:58 PM - Nalan: <Media omitted>
9/22/26, 6:00 PM - Nalan: Guest wing plaster works continue on the front.
9/22/26, 6:00 PM - Nalan: <Media omitted>
9/22/26, 6:00 PM - Nalan: <Media omitted>
9/23/26, 9:30 AM - Nalan: Aluminium for the guest wing windows is not fabricated yet.
9/25/26, 5:00 PM - Nalan: Doors started. Screeding is at 60%.
9/28/26, 8:15 PM - Peter: Send me a full works report and let me know if you need more time and how long?
EOF

# The delivery side's own group — Site Diary material, never the client's.
mkdir -p "$OUT/crew"
cat > "$OUT/crew/WhatsApp Chat with Site crew.txt" <<'EOF'
9/27/26, 7:00 AM - Messages and calls are end-to-end encrypted. Only people in this chat can read, listen to, or share them.
9/27/26, 7:10 AM - Nalan: Crew only: scaffold moved to the rear, hirer wants cash.
9/27/26, 7:11 AM - Nalan: IMG-20260927-WA0001.jpg (file attached)
9/27/26, 7:20 AM - Kato: Okay
EOF

WIN_OUT=$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")

pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  function Save([System.Drawing.Bitmap]\$b, [string]\$path) {
    \$enc = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { \$_.MimeType -eq 'image/jpeg' }
    \$p = New-Object System.Drawing.Imaging.EncoderParameters 1
    \$p.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality, [long]90)
    \$b.Save(\$path, \$enc, \$p)
  }
  function Brush([int]\$r, [int]\$g, [int]\$bl) { New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(\$r, \$g, \$bl)) }

  # The guest wing from the gate: sky, roof line, wall with openings, ground.
  # Only the wall's finish changes between the two visits.
  function GuestWing([string]\$path, [int[]]\$wall) {
    \$b = New-Object System.Drawing.Bitmap 1200,800
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.FillRectangle((Brush 200 214 228), 0, 0, 1200, 540)
    \$g.FillRectangle((Brush 110 90 70), 0, 540, 1200, 260)
    \$g.FillRectangle((Brush 60 60 66), 110, 220, 980, 44)
    \$g.FillRectangle((Brush \$wall[0] \$wall[1] \$wall[2]), 130, 264, 940, 400)
    \$g.FillRectangle((Brush 34 34 38), 220, 340, 170, 150)
    \$g.FillRectangle((Brush 34 34 38), 810, 340, 170, 150)
    \$g.FillRectangle((Brush 34 34 38), 540, 420, 120, 244)
    Save \$b \$path
    \$g.Dispose(); \$b.Dispose()
  }

  # The main house windows: a different view entirely.
  function Windows([string]\$path, [int]\$shade = 70) {
    \$b = New-Object System.Drawing.Bitmap 1200,800
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.FillRectangle((Brush 236 232 222), 0, 0, 1200, 800)
    for (\$i = 0; \$i -lt 4; \$i++) { \$g.FillRectangle((Brush \$shade 96 120), 80 + \$i * 280, 120, 200, 420) }
    \$g.FillRectangle((Brush 40 40 44), 0, 700, 1200, 100)
    Save \$b \$path
    \$g.Dispose(); \$b.Dispose()
  }

  GuestWing '$WIN_OUT\\media\\24th august\\WhatsApp Image 2026-08-24 at 5.10.31 PM.jpeg' @(150, 84, 64)
  # Misfiled: the after-frame of 22 Sep sits in the 21 Sep folder. Its stamp is right.
  GuestWing '$WIN_OUT\\media\\21st september\\WhatsApp Image 2026-09-22 at 6.00.12 PM.jpeg' @(192, 188, 180)
  Windows '$WIN_OUT\\media\\22nd september\\WhatsApp Image 2026-09-22 at 5.58.40 PM.jpeg'
  # A stamp with no line at its minute: kept, reported, never dropped.
  Windows '$WIN_OUT\\media\\stray\\WhatsApp Image 2026-09-26 at 7.00.00 AM.jpeg' 150
  # The crew's own thread: one photo that belongs to the delivery side alone.
  Windows '$WIN_OUT\\crew\\IMG-20260927-WA0001.jpg' 230

  Compress-Archive -Path '$WIN_OUT\\thread\\*' -DestinationPath '$WIN_OUT\\whatsapp-report-thread.zip' -Force
  Compress-Archive -Path '$WIN_OUT\\crew\\*' -DestinationPath '$WIN_OUT\\whatsapp-report-crew.zip' -Force
" >/dev/null

HAS_VIDEO=0
if command -v ffmpeg >/dev/null 2>&1; then
  # A three-second clip for the second 9/22 6:00 PM line, through the same re-join.
  ffmpeg -hide_banner -loglevel error -f lavfi -i "testsrc=duration=3:size=320x240:rate=10" -pix_fmt yuv420p \
    -y "$OUT/media/22nd september/WhatsApp Video 2026-09-22 at 6.00.55 PM.mp4" && HAS_VIDEO=1
fi

pwsh -NoProfile -Command "Compress-Archive -Path '$WIN_OUT\\media\\*' -DestinationPath '$WIN_OUT\\report-loose-media.zip' -Force" >/dev/null

cat > "$OUT/fixtures.env" <<EOF
R_ZIP="whatsapp-report-thread.zip"
R_MEDIA_ZIP="report-loose-media.zip"
R_CREW_ZIP="whatsapp-report-crew.zip"
R_BEFORE="WhatsApp Image 2026-08-24 at 5.10.31 PM.jpeg"
R_AFTER="WhatsApp Image 2026-09-22 at 6.00.12 PM.jpeg"
R_WINDOWS="WhatsApp Image 2026-09-22 at 5.58.40 PM.jpeg"
R_STRAY="WhatsApp Image 2026-09-26 at 7.00.00 AM.jpeg"
R_VIDEO="WhatsApp Video 2026-09-22 at 6.00.55 PM.mp4"
R_HAS_VIDEO="$HAS_VIDEO"
EOF

echo "Works report fixtures written to $OUT"
