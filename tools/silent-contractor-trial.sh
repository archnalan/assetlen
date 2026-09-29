#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the silent-contractor trial (assetlen.md §11, test 3; plan.md P7).
#
#   "Build tier 1 from a real WhatsApp export with no contractor involvement at
#    all, and give it to Peter for three weeks. Would he pay for this alone?"
#
# This script builds the thing Peter is given. It signs in as Peter, creates a
# project that only he is on, imports his export exactly as he would, re-joins
# the loose footage, and then reads back the daily brief for every day of the
# three-week window — writing a per-day account of what the brief could and
# could not make of it. Nobody from the delivery side is invited, maps an
# author, curates a frame or confirms anything. That is the test.
#
# THE REAL EXPORT NEVER ENTERS THIS REPOSITORY. Point the script at it where it
# lives, and keep the report out of the repository too — it quotes nothing, but
# its project name and counts describe a real engagement. The default report
# path is the system temp directory for exactly that reason.
#
# Usage:
#   bash tools/silent-contractor-trial.sh --email peter@example --password '…' \
#        --export "/path/to/WhatsApp Chat with X.zip" \
#        [--media "/path/to/loose footage folder"] [--api https://localhost:7264/api] \
#        [--name "Silent-contractor trial"] [--end 2026-09-28] [--days 21] \
#        [--stages "Stage A;Stage B"] [--report /tmp/trial.tsv] [--project <existing id>]
#
# --stages is optional on purpose: Peter may never set any up. Without them the
# brief groups by the stage catalogue's names for the work, and says so.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="https://localhost:7264/api"; EMAIL=""; PASSWORD=""; EXPORT=""; MEDIA=""
NAME="Silent-contractor trial"; END=""; DAYS=21; STAGES=""; PID=""
REPORT="${TMPDIR:-/tmp}/assetlen-silent-contractor-trial.tsv"

while [ $# -gt 0 ]; do
  case "$1" in
    --api) API="$2"; shift 2 ;;
    --email) EMAIL="$2"; shift 2 ;;
    --password) PASSWORD="$2"; shift 2 ;;
    --export) EXPORT="$2"; shift 2 ;;
    --media) MEDIA="$2"; shift 2 ;;
    --name) NAME="$2"; shift 2 ;;
    --end) END="$2"; shift 2 ;;
    --days) DAYS="$2"; shift 2 ;;
    --stages) STAGES="$2"; shift 2 ;;
    --report) REPORT="$2"; shift 2 ;;
    --project) PID="$2"; shift 2 ;;
    *) echo "Unknown argument: $1"; exit 2 ;;
  esac
done

[ -z "$EMAIL" ] || [ -z "$PASSWORD" ] && { echo "--email and --password are Peter's own sign-in."; exit 2; }
[ -z "$PID" ] && [ ! -f "$EXPORT" ] && { echo "--export must point at the export (.txt or .zip)."; exit 2; }
case "$(cd "$(dirname "$REPORT")" 2>/dev/null && pwd)" in
  "$(pwd)"*) echo "Refusing to write the report inside the repository: $REPORT"; exit 2 ;;
esac

CURL=(curl -sk --max-time 900)
jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }

TOKEN=$("${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
  -d "{\"Email\":\"$EMAIL\",\"Password\":\"$PASSWORD\"}" | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//')
[ -z "$TOKEN" ] && { echo "Sign-in failed for $EMAIL. Is the API up on $API?"; exit 1; }
auth=(-H "Authorization: Bearer $TOKEN")

if [ -z "$PID" ]; then
  stages_json=""
  if [ -n "$STAGES" ]; then
    i=0
    IFS=';' read -ra parts <<< "$STAGES"
    for s in "${parts[@]}"; do
      i=$((i+1)); [ -n "$stages_json" ] && stages_json+=","
      stages_json+="{\"StageName\":\"$(printf '%s' "$s" | sed 's/^ *//;s/ *$//')\",\"DisplayOrder\":$i}"
    done
  fi
  CREATED=$("${CURL[@]}" -X POST "$API/ProjectsRS/CreateProject" "${auth[@]}" -H "Content-Type: application/json" \
    -d "{\"ProjectName\":\"$NAME\",\"Description\":\"Silent-contractor trial: built from the export alone\",\"Currency\":\"UGX\",\"Stages\":[${stages_json}]}")
  PID=$(printf '%s' "$CREATED" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//')
  [ -z "$PID" ] && { echo "Project create failed: $(printf '%s' "$CREATED" | head -c 300)"; exit 1; }
  echo "Project: $PID (only $EMAIL is on it)"

  PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" "${auth[@]}" -F "file=@$EXPORT" -F "projectId=$PID")
  BATCH=$(jget "$PRE" batchId)
  [ -z "$BATCH" ] && { echo "Preview failed: $(printf '%s' "$PRE" | head -c 300)"; exit 1; }
  echo "Preview: $(printf '%s' "$PRE" | grep -o '"messageCount":[0-9]*' | head -1) — read that number before trusting anything below."

  # No author mappings: nobody from the delivery side is on the project to map to.
  DONE=$("${CURL[@]}" -X POST "$API/Ingest/CommitImport" "${auth[@]}" -H "Content-Type: application/json" \
    -d "{\"BatchId\":\"$BATCH\",\"AuthorMappings\":[]}")
  echo "Import: $(jget "$DONE" status)"

  if [ -n "$MEDIA" ] && [ -d "$MEDIA" ]; then
    # Loose footage in handfuls, so no single request carries the whole folder.
    mapfile -t files < <(find "$MEDIA" -type f \( -iname '*.jpg' -o -iname '*.jpeg' -o -iname '*.png' -o -iname '*.mp4' \) | sort)
    bound=0
    for ((k=0; k<${#files[@]}; k+=10)); do
      form=()
      for f in "${files[@]:k:10}"; do form+=(-F "Files=@$f"); done
      out=$("${CURL[@]}" -X POST "$API/Ingest/RejoinMedia" "${auth[@]}" "${form[@]}" -F "ProjectId=$PID")
      n=$(printf '%s' "$out" | grep -o '"bound":[0-9]*' | head -1 | sed 's/.*://')
      bound=$((bound + ${n:-0}))
    done
    echo "Footage: ${#files[@]} files offered, $bound bound to their messages"
  fi
fi

if [ -z "$END" ]; then
  LAST=$("${CURL[@]}" "$API/Ingest/GetBatches?projectId=$PID" "${auth[@]}" | grep -o '"lastMessageAt":"[^"]*"' | sed 's/.*:"//;s/"$//' | sort | tail -1)
  END="${LAST:0:10}"
fi
[ -z "$END" ] && END="$(date +%Y-%m-%d)"

printf "day\tmessages\tacks_set_aside\tblocks\tunfiled\tframes\tpairs\tmoney\tdates\tspecs\tblockers\towed\n" > "$REPORT"
for ((d=DAYS-1; d>=0; d--)); do
  DAY=$(date -d "$END -$d day" +%Y-%m-%d)
  "${CURL[@]}" "$API/Brief/Day?projectId=$PID&day=$DAY&days=1" "${auth[@]}" | pwsh -NoProfile -Command "
    \$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    function n(\$k) { @(\$r.truthFloor | Where-Object { \$_.kind -eq \$k } | ForEach-Object { \$_.items }).Count }
    '{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}' -f '$DAY', \$r.counts.messagesRead, \$r.counts.acknowledgementsSetAside,
      @(\$r.blocks).Count, \$r.counts.unfiled, \$r.counts.frames, \$r.counts.paired,
      (n 'Money'), (n 'Date'), (n 'Spec'), (n 'Blocker'), (n 'DecisionOwed')" | tr '|' '\t' >> "$REPORT"
done

echo
column -t -s $'\t' "$REPORT" 2>/dev/null || cat "$REPORT"
echo
echo "Report:  $REPORT  (outside the repository — keep it there)"
echo "Home:    https://localhost:7025/"
echo "Brief:   https://localhost:7025/project/$PID/brief?day=$END"
echo
echo "Now the part no script can do: give Peter this project for three weeks and ask"
echo "whether he would pay for it alone (assetlen.md §11, test 3)."
