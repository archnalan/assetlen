#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — fixtures for the P9 contractor-tier suite.
#
# THE REAL FOOTAGE IS NOT IN THIS REPOSITORY and must not be added
# (whatsapp-evidence.md, Anonymity). Everything here is drawn or spoken by the
# machine: numbered site frames and one invented voice note.
#
# The shape is the real one: a clerk's evening dump is thirteen to eighteen
# frames of the same piece of work (Nalan.md), so the batch is sixteen distinct
# frames plus one duplicate — the same photo picked twice from the camera roll.
#
# Usage:  bash tools/make-p9-fixtures.sh [output-dir]
# Needs:  pwsh (System.Drawing). The voice note needs Windows PowerShell's
#         System.Speech; without it the file is simply not written and the
#         suite skips the transcription assertions.
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures/p9}"
rm -rf "$OUT"
mkdir -p "$OUT"
WIN_OUT=$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")

pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  \$font = New-Object System.Drawing.Font('Arial', 28, [System.Drawing.FontStyle]::Bold)
  for (\$i = 1; \$i -le 25; \$i++) {
    \$b = New-Object System.Drawing.Bitmap 480,320
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.Clear([System.Drawing.Color]::FromArgb(150 + (\$i * 3) % 90, 140 + (\$i * 7) % 90, 120 + (\$i * 11) % 90))
    \$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(70, 60, 50), 6)
    \$g.DrawRectangle(\$pen, 40 + \$i * 4, 60, 240, 180)
    \$g.DrawLine(\$pen, 0, 280 - \$i * 3, 480, 250)
    \$g.DrawString(('Wall course ' + \$i), \$font, [System.Drawing.Brushes]::Black, 30, 20)
    \$b.Save(('$WIN_OUT\\frame-{0:D2}.jpg' -f \$i), [System.Drawing.Imaging.ImageFormat]::Jpeg)
    \$g.Dispose(); \$b.Dispose()
  }
" >/dev/null

# The voice note is spoken by the machine and read back by the server's
# transcriber, so it is invented words in a synthetic voice.
VOICE="$WIN_OUT\\voice-note.wav"
powershell.exe -NoProfile -NonInteractive -Command "
  try {
    Add-Type -AssemblyName System.Speech
    \$s = New-Object System.Speech.Synthesis.SpeechSynthesizer
    \$s.Rate = -1
    \$s.SetOutputToWaveFile('$VOICE')
    \$s.Speak('The steel team arrives tomorrow morning. We need forty bags of cement for the slab.')
    \$s.Dispose()
  } catch { }
" >/dev/null 2>&1 || true

cat > "$OUT/fixtures.env" <<EOF
P9_FRAMES=25
P9_VOICE=$( [ -f "$OUT/voice-note.wav" ] && echo voice-note.wav || echo "" )
EOF

echo "P9 fixtures written to $OUT"
