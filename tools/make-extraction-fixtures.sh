#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the P5 extraction suite.
#
# THE REAL EXPORT AND FOOTAGE ARE NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). Everything here is synthesised with the
# repo's pseudonyms — Peter, Nalan, Dinah — and the *shape* of the real thread:
# a numbered material schedule, a figure pushed back on, relative dates
# ("tomorrow", "by Tuesday", "this week"), a bulleted "Summary update" with
# percentages, a recommendation answered "yes", and a pile of "Okay" / "Noted"
# that must produce nothing.
#
# It also writes the loose footage the media re-join is tested against: files
# named the way WhatsApp names saved media, one misfiled into the wrong day's
# folder, one saved twice under "Image2", one with a human name, and one whose
# minute has no line in the transcript.
#
# Usage:  bash tools/make-extraction-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing for real JPEGs, Compress-Archive for the zip).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/p5}"
rm -rf "$OUT"
mkdir -p "$OUT"

TX="$OUT/whatsapp-p5-thread.txt"

# US-dialect Android export: M/D/YY, 12-hour clock, " - " separator — the
# dialect of the real thread. Continuation lines carry no stamp.
cat > "$TX" <<'EOF'
6/10/26, 9:00 AM - Messages and calls are end-to-end encrypted. Only people in this chat can read, listen to, or share them.
6/11/26, 3:56 PM - Peter: How long will the excavation of the retaining wall take?
6/11/26, 3:58 PM - Peter: Okay
6/11/26, 5:35 PM - Nalan: We can phase it... by end of tomorrow we shall have the first half ready
6/11/26, 5:39 PM - Nalan: Notes on the material schedule
6/11/26, 5:39 PM - Nalan: 1. Kindly dont procure brand X pipes, we need at least grade B
2. For cement we need CEM II 42.5
3. The 6 inch drainage pipe is heavy duty
4. Aggregate should be machine crushed, not hand crushed
5. Sand, we use washed lake sand
Labour shall be 12M (includes formwork, earthworks and concrete works)
6/11/26, 6:11 PM - Peter: UGX 12M is too high for labour
6/11/26, 6:12 PM - Peter: Noted
6/11/26, 7:00 PM - Nalan: Good progress today
6/13/26, 11:06 PM - Nalan: We need 150 bags of cement
6/13/26, 11:10 PM - Nalan: The hardcore should come first 5 trucks
6/13/26, 11:11 PM - Peter: Okay
6/13/26, 11:16 PM - Nalan: 1 sinotruck of sand
1 sinotruck of aggregate
20 T16 and 160 T12
6/17/26, 7:18 PM - Nalan: The materials have been confirmed on site.. hardcore still remaining
6/22/26, 2:34 PM - Peter: When do you intend to cast?
6/22/26, 2:34 PM - Nalan: Tomorrow
6/25/26, 1:59 PM - Nalan: You can let us know if you want us to have power points on the new wall
6/29/26, 4:00 PM - Nalan: Exterior plastering will be done by Tuesday
7/1/26, 9:53 AM - Peter: I need this closed. It is dangerous for the neighbours
7/2/26, 8:26 PM - Nalan: Summary update today
- Guest wing plastering is currently at 80% both in and out
- Epoxy primer was laid... completion is for 2morrow
- Stonework continues
7/3/26, 10:00 AM - Nalan: Guest wing plaster is in progress. It will be complete this week. 90% is already done
7/4/26, 9:39 AM - Peter: I want terrazzo on the terrace space
7/4/26, 4:20 PM - Nalan: We recommend terrazzo in the laundry too
7/4/26, 4:28 PM - Peter: Yes, all the space up there
7/5/26, 1:45 PM - Dinah: The epoxy team didn't show up
7/5/26, 2:00 PM - Nalan: <Media omitted>
7/5/26, 2:00 PM - Nalan: <Media omitted>
7/5/26, 2:00 PM - Nalan: <Media omitted>
7/5/26, 2:01 PM - Nalan: <Media omitted>
7/5/26, 3:10 PM - Nalan:
7/5/26, 3:10 PM - Nalan: <Media omitted>
7/5/26, 3:10 PM - Nalan: <Media omitted>
7/6/26, 10:00 AM - Peter: Thank you for the update
7/6/26, 10:05 AM - Peter: Tomorrow, you will receive UGX 50M for the next stage
EOF

# A second, delivery-side thread: the crew's own record, which Peter must not
# see proposals from once he has stood down as mediator (D5).
cat > "$OUT/whatsapp-p5-crew.txt" <<'EOF'
7/7/26, 8:00 AM - Nalan: Steel team labour will cost UGX 3,500,000 for the slab
7/7/26, 8:05 AM - Kato: Noted
EOF

# ── Loose footage for the re-join ────────────────────────────────────────────
MEDIA="$OUT/site-activities"
mkdir -p "$MEDIA/5th july" "$MEDIA/4th july" "$MEDIA/9th july"

WIN_MEDIA=$(cygpath -w "$MEDIA" 2>/dev/null || echo "$MEDIA")
WIN_OUT=$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")

pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  function Frame([string]\$path, [string]\$label, [int]\$shade) {
    \$b = New-Object System.Drawing.Bitmap 480,320
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.Clear([System.Drawing.Color]::FromArgb(\$shade, 200, 180))
    \$f = New-Object System.Drawing.Font('Arial', 18)
    \$g.DrawString(\$label, \$f, [System.Drawing.Brushes]::Black, 12, 12)
    \$b.Save(\$path, [System.Drawing.Imaging.ImageFormat]::Jpeg)
    \$g.Dispose(); \$b.Dispose()
  }
  \$m = '$WIN_MEDIA'
  Frame \"\$m\\5th july\\WhatsApp Image 2026-07-05 at 2.00.11 PM.jpeg\" 'frame one' 10
  Frame \"\$m\\5th july\\WhatsApp Image 2026-07-05 at 2.00.12 PM.jpeg\" 'frame two' 40
  Copy-Item \"\$m\\5th july\\WhatsApp Image 2026-07-05 at 2.00.12 PM.jpeg\" \"\$m\\5th july\\WhatsApp Image2 2026-07-05 at 2.00.12 PM.jpeg\"
  Frame \"\$m\\4th july\\WhatsApp Image 2026-07-05 at 2.00.40 PM.jpeg\" 'misfiled frame' 70
  Frame \"\$m\\5th july\\WhatsApp Image 2026-07-05 at 3.10.20 PM.jpeg\" 'afternoon frame' 100
  Frame \"\$m\\5th july\\intended stoppage line.jpeg\" 'stoppage line' 130
  Frame \"\$m\\9th july\\WhatsApp Image 2026-07-09 at 9.00.00 AM.jpeg\" 'no line at this minute' 160
  [IO.File]::WriteAllBytes(\"\$m\\5th july\\WhatsApp Video 2026-07-05 at 2.01.05 PM.mp4\", [byte[]](0,0,0,24,102,116,121,112,109,112,52,50) + [Text.Encoding]::ASCII.GetBytes('synthetic video bytes'))

  # A receipt photographed, for OCR. Large type so any engine can read it.
  \$r = New-Object System.Drawing.Bitmap 1400,500
  \$g = [System.Drawing.Graphics]::FromImage(\$r)
  \$g.Clear([System.Drawing.Color]::White)
  \$f = New-Object System.Drawing.Font('Arial', 48, [System.Drawing.FontStyle]::Bold)
  \$g.DrawString('HILLSIDE HARDWARE RECEIPT', \$f, [System.Drawing.Brushes]::Black, 30, 60)
  \$g.DrawString('CEMENT 150 BAGS', \$f, [System.Drawing.Brushes]::Black, 30, 220)
  \$r.Save('$WIN_OUT\\receipt.png', [System.Drawing.Imaging.ImageFormat]::Png)

  Compress-Archive -Path \"\$m\\*\" -DestinationPath '$WIN_OUT\\site-activities.zip' -Force
" >/dev/null

printf 'Delivery note: 150 bags cement received at site, signed by the storekeeper.\n' > "$OUT/delivery-note.txt"

cat > "$OUT/fixtures.env" <<EOF
P5_THREAD=whatsapp-p5-thread.txt
P5_CREW=whatsapp-p5-crew.txt
P5_MEDIA_ZIP=site-activities.zip
P5_RECEIPT=receipt.png
P5_NOTE=delivery-note.txt
EOF

echo "P5 fixtures written to $OUT"
