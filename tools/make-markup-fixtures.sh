#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the P8 markup suite.
#
# THE REAL EXPORT IS NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). The receipt below is invented: an invented
# vendor, round figures, no names.
#
# The exit criterion of plan.md P8 is written into its shape: the register says
# 36 bags of cement at UGX 4,320,000, the receipt says 40 bags at 4,800,000.
# Peter circles the cement line, asks, and the answer changes the figure.
#
# Usage:  bash tools/make-markup-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/p8}"
rm -rf "$OUT"
mkdir -p "$OUT"

WIN_OUT=$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")

pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  \$b = New-Object System.Drawing.Bitmap 1200,900
  \$g = [System.Drawing.Graphics]::FromImage(\$b)
  \$g.Clear([System.Drawing.Color]::White)
  \$big = New-Object System.Drawing.Font('Arial', 44, [System.Drawing.FontStyle]::Bold)
  \$f = New-Object System.Drawing.Font('Arial', 36)
  \$g.DrawString('HARDSTONE SUPPLIES', \$big, [System.Drawing.Brushes]::Black, 60, 60)
  \$g.DrawString('CASH SALE 2231', \$f, [System.Drawing.Brushes]::Black, 60, 170)
  \$g.DrawString('CEMENT CEM II 50KG  x40', \$f, [System.Drawing.Brushes]::Black, 60, 330)
  \$g.DrawString('@ 120,000     4,800,000', \$f, [System.Drawing.Brushes]::Black, 60, 400)
  \$g.DrawString('SAND 1 TRIP        650,000', \$f, [System.Drawing.Brushes]::Black, 60, 520)
  \$g.DrawString('TOTAL         5,450,000', \$big, [System.Drawing.Brushes]::Black, 60, 700)
  \$b.Save('$WIN_OUT\\IMG-20260806-WA0007.png', [System.Drawing.Imaging.ImageFormat]::Png)
  \$g.Dispose(); \$b.Dispose()
" >/dev/null

cat > "$OUT/fixtures.env" <<EOF
P8_RECEIPT=IMG-20260806-WA0007.png
EOF

echo "P8 fixtures written to $OUT"
