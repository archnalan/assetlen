#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — WhatsApp export fixtures for the P3 ingest suite.
#
# THE REAL CORPUS IS NOT IN THIS REPOSITORY, and must not be added to it:
# whatsapp-evidence.md says so explicitly, and the export carries real names,
# banks, account numbers and a site location. So this generates a stand-in with
# the same *shape* as the real thing — the profile documented in
# whatsapp-evidence.md §2:
#
#   1,529 messages · 47% media · 3 participants · 18 Sep 2025 → 4 Aug 2026
#   largest single-minute dump: 18 photos
#
# To run the exit criterion against the genuine export instead, drop it in as
#   tools/fixtures/whatsapp-android-nomedia.txt
# and re-run the suite with SKIP_FIXTURES=1. The assertions read the manifest
# this script emits, so replace that too — or just read the numbers off the
# preview, which is the point of the preview step.
#
# Ground truth is written to fixtures.env: the generator counts what it wrote,
# and the parser reads the file back independently. Agreement between the two
# is therefore evidence, not a shared assumption.
#
# Usage:  bash tools/make-ingest-fixtures.sh [output-dir]
# Needs:  pwsh (for Compress-Archive — Git Bash on Windows has no zip).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

OUT="${1:-tools/fixtures}"
mkdir -p "$OUT"

# ── Deterministic pseudo-randomness ──────────────────────────────────────────
# A fixture that changes between runs turns a real regression into "it passed
# yesterday". Same seed, same file, byte for byte.
SEED=20260812
RND=0
# Sets $RND rather than echoing it. Command substitution runs a function in a
# subshell, so every advance of SEED would be thrown away and the "random"
# sequence would return the same number 1,529 times — which is exactly the
# fixture this generator produced on its first run: one author, all media.
rnd() {                       # rnd MAX → $RND in 0..MAX-1
  SEED=$(( (SEED * 1103515245 + 12345) % 2147483648 ))
  RND=$(( SEED % $1 ))
}

# ── The corpus profile ───────────────────────────────────────────────────────
# 1,528 from the loop plus the one multi-line message appended after it — the
# corpus headline of 1,529 (whatsapp-evidence.md §2).
TOTAL=1528
START_DATE="2025-09-18"
DAYS=321                      # 18 Sep 2025 → 4 Aug 2026

BIG="$OUT/whatsapp-android-nomedia.txt"

# Real material from whatsapp-evidence.md §7 — the sixteen retaining-wall
# commitments, in the plain text they actually arrived as. Present so P5 has
# something with genuine extractable content to be measured against, rather
# than lorem ipsum that would make any extractor look good.
COMMITMENTS=(
"Do not procure YOGI, minimum Plumol"
"Cement to use is Tororo CEM II"
"6 inch drainage pipe, heavy duty Gentex"
"Aggregate must be machine crushed, not hand crushed"
"Sand is lake sand from the usual pit"
"Labour for the retaining wall comes to UGX 12M covering formwork, earthworks and concrete"
"UGX 12M is too high for Labour"
"Excavation is phased, first half ready by 12 Jun"
"We need 150 bags of cement for the excavated section"
"5 trucks of hardcore first"
"1 sinotruck sand and 1 sinotruck aggregate"
"20 pieces T16, 160 pieces T12, 30 pieces R8"
"The open excavation is a danger to the neighbours and the children. I need this closed"
"Where do you want the electrical points on the interior face?"
"Materials confirmed on site, hardcore still outstanding"
"First half is cast, formwork struck to place the inch pipes"
"Reinforced concrete retaining wall instead of stone pitching, it conserves compound space"
"We have saved approximately 15M on the balconies"
"Let me know the additional cost for the extra floor"
"Issue a receipt and carry the balance towards the next stage"
)

# Filler — the ambient noise Law 3 says extraction must yield nothing from.
# Hundreds of "Okay" and "Noted" is what the real thread is made of, and an
# extractor that fires on them is worse than no extractor at all.
CHATTER=(
"Okay" "Noted" "Good progress" "Thanks" "Received" "Will check" "Yes"
"Sure, let me confirm" "On site now" "Tomorrow morning" "Understood"
"How is today's progress?" "Any updates?" "Alright" "Perfect" "Noted, thanks"
)

SYSTEM_LINES=(
"Messages and calls are end-to-end encrypted. No one outside of this chat, not even WhatsApp, can read or listen to them. Tap to learn more."
"Nalan created group \"HOUSE PLAN\""
"Peter changed the subject from \"House\" to \"HOUSE PLAN\""
"Dinah joined using this group's invite link"
)

# ── Generate the big transcript ──────────────────────────────────────────────
: > "$BIG"

n_nalan=0; n_peter=0; n_dinah=0; n_system=0; n_media=0; n_written=0

# Precompute the calendar once. 1,529 calls to date(1) is slow enough to notice.
mapfile -t CAL < <(for d in $(seq 0 $((DAYS - 1))); do
  date -d "$START_DATE +$d days" "+%d/%m/%Y"
done)

burst_start=700               # where the 18-photo minute goes
burst_len=18
# Pinned to one day, computed once. Letting the burst take the loop's own
# day_index spread it across two dates and quietly destroyed the property being
# tested: eighteen messages identical in author, timestamp and body.
burst_day="${CAL[$(( burst_start * (DAYS - 1) / (TOTAL - 1) ))]}"

i=0
while [ "$i" -lt "$TOTAL" ]; do
  day_index=$(( i * (DAYS - 1) / (TOTAL - 1) ))
  day="${CAL[$day_index]}"

  # The single-minute dump: eighteen photos, one author, one timestamp. This is
  # the case the dedupe key's occurrence ordinal exists for — without it these
  # eighteen hash identically and an import silently keeps one.
  if [ "$i" -ge "$burst_start" ] && [ "$i" -lt $((burst_start + burst_len)) ]; then
    printf '%s, 21:56 - Nalan: <Media omitted>\n' "$burst_day" >> "$BIG"
    n_nalan=$((n_nalan + 1)); n_media=$((n_media + 1)); n_written=$((n_written + 1))
    i=$((i + 1))
    continue
  fi

  rnd 24; hour=$RND; rnd 60; minute=$RND
  stamp=$(printf '%s, %02d:%02d' "$day" "$hour" "$minute")

  rnd 100; roll=$RND
  if [ "$roll" -lt 66 ]; then
    author="Nalan"; media_chance=68; n_nalan=$((n_nalan + 1))
  elif [ "$roll" -lt 84 ]; then
    author="Peter"; media_chance=8; n_peter=$((n_peter + 1))
  elif [ "$roll" -lt 91 ]; then
    author="Dinah"; media_chance=14; n_dinah=$((n_dinah + 1))
  else
    author=""; media_chance=0; n_system=$((n_system + 1))
  fi

  if [ -z "$author" ]; then
    rnd ${#SYSTEM_LINES[@]}; idx=$RND
    printf '%s - %s\n' "$stamp" "${SYSTEM_LINES[$idx]}" >> "$BIG"
    n_written=$((n_written + 1)); i=$((i + 1))
    continue
  fi

  rnd 100
  if [ "$RND" -lt "$media_chance" ]; then
    printf '%s - %s: <Media omitted>\n' "$stamp" "$author" >> "$BIG"
    n_media=$((n_media + 1))
  else
    # One message in six carries real commitment material; the rest is chatter.
    rnd 6
    if [ "$RND" -eq 0 ]; then
      rnd ${#COMMITMENTS[@]}; idx=$RND
      printf '%s - %s: %s\n' "$stamp" "$author" "${COMMITMENTS[$idx]}" >> "$BIG"
    else
      rnd ${#CHATTER[@]}; idx=$RND
      printf '%s - %s: %s\n' "$stamp" "$author" "${CHATTER[$idx]}" >> "$BIG"
    fi
  fi

  n_written=$((n_written + 1)); i=$((i + 1))
done

# A multi-line message — a materials list arrives as one message with hard line
# breaks, and those continuation lines carry no timestamp. Appended after the
# loop so the counted total stays exact: this is ONE message, five lines.
last_day="${CAL[$((DAYS - 1))]}"
{
  printf '%s, 08:15 - Nalan: Materials for the retaining wall:\n' "$last_day"
  printf 'Cement: Tororo CEM II, 150 bags\n'
  printf 'Aggregate: machine crushed, 1 sinotruck\n'
  printf 'Sand: lake sand, 1 sinotruck\n'
  printf 'Steel: 20 x T16, 160 x T12, 30 x R8\n'
} >> "$BIG"
n_nalan=$((n_nalan + 1)); n_written=$((n_written + 1))

BIG_TOTAL=$n_written

# ── The iOS fixture, with media ──────────────────────────────────────────────
#
# A different platform dialect on purpose. iOS brackets the stamp, uses a
# 12-hour clock, names attachments with <attached: …>, and wraps lines in
# direction marks that render as nothing and defeat every regex that has not
# been told about them. If the parser only ever sees Android output, the first
# real iPhone export produces zero messages and looks like an empty chat.
IOS_DIR="$OUT/ios-media"
rm -rf "$IOS_DIR"; mkdir -p "$IOS_DIR"

# The known-good 32x24 RGB PNG from the P2 suite. A previous fixture was
# structurally a PNG with an invalid filter byte, so ImageSharp refused it and
# no thumbnail was ever produced — assertions passed over a decode failure.
PNG='iVBORw0KGgoAAAANSUhEUgAAACAAAAAYCAIAAAAUMWhjAAAAJElEQVR42mM4FKJFU8QwasGoBaMWjFowasGoBaMWjFowNCwAAND5wC6OsU0VAAAAAElFTkSuQmCC'
b64d() { base64 -d 2>/dev/null || base64 --decode; }

UNIQUE_PHOTOS=12
DUP_RECEIPTS=5

# Twelve distinct photos: same base image, distinct trailing bytes, so twelve
# distinct hashes. Trailing bytes after IEND are ignored by decoders.
for k in $(seq 1 $UNIQUE_PHOTOS); do
  { printf '%s' "$PNG" | b64d; printf 'assetlen-fixture-photo-%02d' "$k"; } \
    > "$IOS_DIR/$(printf 'photo-%02d.jpg' "$k")"
done

# Five *byte-identical* files under five different names — the receipt sent
# five times (assetlen.md Law 2). Content-addressing must collapse these to one
# artifact with five pointers; filenames must not be what decides it.
for k in $(seq 1 $DUP_RECEIPTS); do
  { printf '%s' "$PNG" | b64d; printf 'assetlen-fixture-receipt-R014'; } \
    > "$IOS_DIR/$(printf 'receipt-%d.jpg' "$k")"
done

IOS_UNIQUE_CONTENT=$((UNIQUE_PHOTOS + 1))          # 12 photos + 1 receipt image
IOS_DUP_CONTENT=$((DUP_RECEIPTS - 1))              # 4 re-sends of that receipt
IOS_MEDIA_MSGS=$((UNIQUE_PHOTOS + DUP_RECEIPTS))

CHAT="$IOS_DIR/_chat.txt"
LRM=$'‎'                 # left-to-right mark — invisible, and fatal to a naive ^\[
NNBSP=$' '               # narrow no-break space — iOS puts it before AM/PM

{
  printf '%s[18/09/2025, 2:32:10%sPM] Nalan: Starting the retaining wall setting out today\n' "$LRM" "$NNBSP"
  printf '[18/09/2025, 2:33:01%sPM] Peter: Good. Send pictures when you can\n' "$NNBSP"

  m=0
  for k in $(seq 1 $UNIQUE_PHOTOS); do
    m=$((m + 1))
    printf '%s[16/07/2026, 9:%02d:%02d%sPM] Nalan: <attached: photo-%02d.jpg>\n' \
      "$LRM" $((50 + m % 10)) $((m * 3 % 60)) "$NNBSP" "$k"
  done

  for k in $(seq 1 $DUP_RECEIPTS); do
    printf '[29/07/2026, 11:%02d:00%sAM] Nalan: <attached: receipt-%d.jpg>\n' \
      $((10 + k)) "$NNBSP" "$k"
  done

  printf '[29/07/2026, 11:30:00%sAM] Peter: That is confusing me. We need to go step by step. Too many stages combined. I want to know if they were cleared or not.\n' "$NNBSP"
  printf '[04/08/2026, 9:00:00%sAM] Dinah: Can we confirm the epoxy finish before the tiles are ordered?\n' "$NNBSP"
} > "$CHAT"

IOS_TOTAL=$((2 + IOS_MEDIA_MSGS + 2))

# Compress-Archive rather than zip(1): Git Bash on Windows ships no zip, and a
# fixture the suite cannot build is a fixture nobody runs.
IOS_ZIP="$OUT/whatsapp-ios-media.zip"
rm -f "$IOS_ZIP"
pwsh -NoProfile -Command "Compress-Archive -Path '$(cygpath -w "$IOS_DIR")\\*' -DestinationPath '$(cygpath -w "$IOS_ZIP")' -Force" >/dev/null

# ── Manifest ─────────────────────────────────────────────────────────────────
cat > "$OUT/fixtures.env" <<EOF
# Generated by tools/make-ingest-fixtures.sh — ground truth for the P3 suite.
# The generator counted what it wrote; the parser reads the files back on its
# own. The two agreeing is evidence, not a shared assumption.
BIG_FILE="whatsapp-android-nomedia.txt"
BIG_TOTAL=$BIG_TOTAL
BIG_MEDIA=$n_media
BIG_NALAN=$n_nalan
BIG_PETER=$n_peter
BIG_DINAH=$n_dinah
BIG_SYSTEM=$n_system
BIG_PARTICIPANTS=3
BIG_BURST=$burst_len

IOS_FILE="whatsapp-ios-media.zip"
IOS_TOTAL=$IOS_TOTAL
IOS_MEDIA_MSGS=$IOS_MEDIA_MSGS
IOS_UNIQUE_CONTENT=$IOS_UNIQUE_CONTENT
IOS_DUP_CONTENT=$IOS_DUP_CONTENT
EOF

printf 'Fixtures written to %s\n' "$OUT"
printf '  %-34s %5d messages, %4d media, %d participants\n' \
  "whatsapp-android-nomedia.txt" "$BIG_TOTAL" "$n_media" 3
printf '     Nalan %d · Peter %d · Dinah %d · system %d\n' \
  "$n_nalan" "$n_peter" "$n_dinah" "$n_system"
printf '  %-34s %5d messages, %4d attachments -> %d unique, %d duplicate\n' \
  "whatsapp-ios-media.zip" "$IOS_TOTAL" "$IOS_MEDIA_MSGS" "$IOS_UNIQUE_CONTENT" "$IOS_DUP_CONTENT"
