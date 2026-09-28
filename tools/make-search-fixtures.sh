#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the P6 retrieval suite.
#
# THE REAL EXPORT IS NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). Everything here is synthesised with the
# repo's pseudonyms — Peter, Nalan, Dinah, Kato — and invented vendors.
#
# The exit criterion of plan.md P6 is written into the shape of this fixture:
# a receipt that only ever existed as a photo inside a WhatsApp export. The
# vendor's name, ZENTARA TILES, is drawn into the pixels of the JPEG and
# appears nowhere else — not in the transcript, not in the file name. If a
# search for it answers, it answered from OCR.
#
# A second export is the delivery side's own record, with a receipt from a
# second invented vendor. Peter, once he has stood down as mediator, must not
# be able to find it (D5).
#
# Usage:  bash tools/make-search-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing for the JPEGs, Compress-Archive for the zips).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/p6}"
rm -rf "$OUT"
mkdir -p "$OUT/client" "$OUT/crew"

# US-dialect Android export with media — "<name> (file attached)" on its own line.
# Each thread carries one date whose day is over 12 (8/13, 8/14): with only
# ambiguous dates the importer rightly assumes day-first and 8/5 becomes 8 May.
cat > "$OUT/client/WhatsApp Chat with Guest wing.txt" <<'EOF'
8/1/26, 9:00 AM - Messages and calls are end-to-end encrypted. Only people in this chat can read, listen to, or share them.
8/3/26, 10:02 AM - Nalan: For the terrace balustrade we propose brushed stainless steel posts with 10mm toughened glass, 1.1m high
8/3/26, 10:30 AM - Peter: Go ahead with the stainless balustrade
8/5/26, 4:14 PM - Nalan: IMG-20260805-WA0012.jpg (file attached)
8/5/26, 4:15 PM - Nalan: Paid for the floor tiles today
8/6/26, 9:00 AM - Peter: Okay
8/7/26, 11:20 AM - Dinah: The laundry tiles should match the terrace
8/13/26, 9:00 AM - Peter: Thank you for the update
EOF

cat > "$OUT/crew/WhatsApp Chat with Site crew.txt" <<'EOF'
8/8/26, 7:40 AM - Nalan: IMG-20260808-WA0003.jpg (file attached)
8/8/26, 7:41 AM - Kato: Steel for the balustrade posts collected, crew rate agreed on site
8/14/26, 6:30 PM - Kato: Noted
EOF

WIN_OUT=$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")

pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  function Receipt([string]\$path, [string[]]\$lines) {
    # Large type on white so any OCR engine reads it; a real receipt photo is worse, which is the point of R1's validation.
    \$b = New-Object System.Drawing.Bitmap 1400,640
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.Clear([System.Drawing.Color]::White)
    \$f = New-Object System.Drawing.Font('Arial', 48, [System.Drawing.FontStyle]::Bold)
    \$y = 50
    foreach (\$l in \$lines) { \$g.DrawString(\$l, \$f, [System.Drawing.Brushes]::Black, 40, \$y); \$y += 140 }
    \$enc = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { \$_.MimeType -eq 'image/jpeg' }
    \$p = New-Object System.Drawing.Imaging.EncoderParameters 1
    \$p.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality, [long]92)
    \$b.Save(\$path, \$enc, \$p)
    \$g.Dispose(); \$b.Dispose()
  }
  Receipt '$WIN_OUT\\client\\IMG-20260805-WA0012.jpg' @('ZENTARA TILES', 'CASH RECEIPT 0457', 'FLOOR TILES 40 BOXES')
  Receipt '$WIN_OUT\\crew\\IMG-20260808-WA0003.jpg' @('QUILLON STEEL', 'CASH RECEIPT 1188', 'BALUSTRADE POSTS 24')

  Compress-Archive -Path '$WIN_OUT\\client\\*' -DestinationPath '$WIN_OUT\\whatsapp-p6-client.zip' -Force
  Compress-Archive -Path '$WIN_OUT\\crew\\*' -DestinationPath '$WIN_OUT\\whatsapp-p6-crew.zip' -Force
" >/dev/null

cat > "$OUT/fixtures.env" <<EOF
P6_CLIENT_ZIP=whatsapp-p6-client.zip
P6_CREW_ZIP=whatsapp-p6-crew.zip
P6_VENDOR=ZENTARA
P6_CREW_VENDOR=QUILLON
EOF

echo "P6 fixtures written to $OUT"
