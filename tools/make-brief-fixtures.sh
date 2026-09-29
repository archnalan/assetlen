#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the P7 suite: Peter's home and the daily brief.
#
# THE REAL EXPORT IS NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). Everything here is synthesised with the
# repo's pseudonyms — Peter, Nalan, Dinah — and invented work.
#
# The shape is the one plan.md P7 is written against: photos sent in a burst
# with one line of words, acknowledgements between them, and the same view
# photographed again two days later. Two frames are drawn from one vantage
# point — the rear wall before and after plastering — and one from somewhere
# else entirely, so the suite can show that the brief pairs the first two and
# leaves the third alone.
#
# Usage:  bash tools/make-brief-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing for the JPEGs, Compress-Archive for the zip).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/p7}"
rm -rf "$OUT"
mkdir -p "$OUT/thread"

# US-dialect Android export. One date's day is over 12 (7/13) so the importer
# settles the dialect instead of assuming day-first.
cat > "$OUT/thread/WhatsApp Chat with Guest wing works.txt" <<'EOF'
7/6/26, 9:00 AM - Messages and calls are end-to-end encrypted. Only people in this chat can read, listen to, or share them.
7/6/26, 5:10 PM - Nalan: Rear wall plaster started today, scratch coat on the east face. Rear wall plaster is at 20%
7/6/26, 5:11 PM - Nalan: IMG-20260706-WA0001.jpg (file attached)
7/6/26, 5:11 PM - Nalan: IMG-20260706-WA0002.jpg (file attached)
7/6/26, 6:02 PM - Peter: Okay
7/8/26, 4:40 PM - Nalan: Rear wall plaster is now at 70%
7/8/26, 4:41 PM - Nalan: IMG-20260708-WA0003.jpg (file attached)
7/8/26, 5:02 PM - Peter: Noted
7/8/26, 5:20 PM - Nalan: Parapet coping stones delivered, fixing them on Friday
7/8/26, 5:30 PM - Dinah: For the terrace floor tiles we still have to choose between grey and sand colour
7/8/26, 5:35 PM - Nalan: Labour for the terrace balustrade is UGX 3,400,000
7/8/26, 5:36 PM - Peter: Thanks
7/13/26, 9:00 AM - Peter: Thank you
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

  # The rear wall from the gate: sky, roof line, wall with two windows and a
  # door, ground. Only the wall's finish changes between the two visits.
  function RearWall([string]\$path, [int[]]\$wall) {
    \$b = New-Object System.Drawing.Bitmap 1200,800
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.FillRectangle((Brush 205 216 228), 0, 0, 1200, 560)
    \$g.FillRectangle((Brush 104 86 66), 0, 560, 1200, 240)
    \$g.FillRectangle((Brush 58 58 64), 130, 240, 940, 40)
    \$g.FillRectangle((Brush \$wall[0] \$wall[1] \$wall[2]), 150, 280, 900, 400)
    \$g.FillRectangle((Brush 36 36 40), 240, 360, 180, 150)
    \$g.FillRectangle((Brush 36 36 40), 780, 360, 180, 150)
    \$g.FillRectangle((Brush 36 36 40), 540, 440, 120, 240)
    Save \$b \$path
    \$g.Dispose(); \$b.Dispose()
  }

  # Somewhere else on site: a stockpile in front of a tree line.
  function Elsewhere([string]\$path) {
    \$b = New-Object System.Drawing.Bitmap 1200,800
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.FillRectangle((Brush 60 96 58), 0, 0, 1200, 800)
    for (\$i = 0; \$i -lt 6; \$i++) { \$g.FillRectangle((Brush 220 214 196), \$i * 200 + 100, 0, 60, 800) }
    \$g.FillEllipse((Brush 150 140 120), 200, 380, 800, 520)
    \$g.FillRectangle((Brush 20 20 22), 0, 0, 1200, 90)
    Save \$b \$path
    \$g.Dispose(); \$b.Dispose()
  }

  RearWall '$WIN_OUT\\thread\\IMG-20260706-WA0001.jpg' @(150, 82, 62)
  Elsewhere '$WIN_OUT\\thread\\IMG-20260706-WA0002.jpg'
  RearWall '$WIN_OUT\\thread\\IMG-20260708-WA0003.jpg' @(188, 184, 176)

  Compress-Archive -Path '$WIN_OUT\\thread\\*' -DestinationPath '$WIN_OUT\\whatsapp-p7-thread.zip' -Force
" >/dev/null

cat > "$OUT/fixtures.env" <<EOF
P7_ZIP=whatsapp-p7-thread.zip
P7_BEFORE=IMG-20260706-WA0001.jpg
P7_ELSEWHERE=IMG-20260706-WA0002.jpg
P7_AFTER=IMG-20260708-WA0003.jpg
EOF

echo "P7 fixtures written to $OUT"
