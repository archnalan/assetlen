#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the works report suite (works-report.md §10, slices R1–R5).
#
# Asked from the chair of the person who wrote, two days before his deadline,
# "Send me a full works report and let me know if you need more time and how
# long?":
#
#   With the contractor silent — not even on the roster — does a report issue
#   from Peter's own forwarded thread, his loose footage and his declarations,
#   and does a re-issue a week later say what changed?            ← R1, Law 0
#   Does the deadline strip show the date set, restated, the short promises
#   that lapsed, and the contractor's date beside the pace forecast?     ← R2
#   Does every change of scope appear as a variation or as a gap?        ← R3
#   Is every sentence tied to a source in the snapshot, and does the report
#   issue with the model switched off?                                   ← R4
#   Does a report issue with nobody logged in, and reach its readers?    ← R5
#   Is an issued report frozen — the same bytes every time it is read — and
#   exactly as private as its sources: by side, by seat, by money?
#
# Usage:  bash tools/e2e-report.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running (Development, for the schedule and validator
#         endpoints), pwsh for fixtures and JSON. ffmpeg optional (posters).
#         Idempotent — each run builds its own buyer and project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)$RANDOM"
FIXTURES="tools/fixtures/report"
SCRATCH="$FIXTURES/out"

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-62s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "$2"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
ne()   { if [ "$2" != "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "not $2"; fi; }
head_() { printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1\",\"Password\":\"$2\"}" | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
}
req() {
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3" -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}
code() { "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"; }
codeb() { "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3" -H "Content-Type: application/json" -d "$4"; }
jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
jnum() { printf '%s' "$1" | grep -o "\"$2\":[-0-9.]*" | head -1 | sed 's/.*://' || true; }
oid()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

# Reports are nested, so they are read with a real JSON parser. $r is the answer;
# st NAME is a stage card, mark KIND the first deadline mark of that kind.
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    function st(\$n) { @(\$r.stageGroups.stages | Where-Object { \$_.name -eq \$n }) | Select-Object -First 1 }
    function marks(\$k) { @(\$r.deadline.marks | Where-Object { \$_.kind -eq \$k }) }
    function lane(\$o) { @(\$r.blockers | Where-Object { \$_.owner -eq \$o }) | Select-Object -First 1 }
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } elseif (\$null -eq \$out) { 'null' } else { \$out }"
}

echo "ASSETLEN works report (R1–R5) — $API"

# ── Fixtures ─────────────────────────────────────────────────────────────────
bash tools/make-report-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
  || { echo "FATAL: could not build fixtures. Is pwsh on PATH?"; exit 1; }
# shellcheck disable=SC1090
. "$FIXTURES/fixtures.env"
mkdir -p "$SCRATCH"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

mkuser() {
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

# A buyer of his own for every run: his first project is the free one, so his
# releases are accepted (plan.md P4, outstanding).
BUYER="peter.report$STAMP@assetlen.test"
mkuser "$BUYER"                       Contractor Peter Developer
mkuser nalan.arch@assetlen.test       Manager    Nalan Architect
mkuser kato.foreman@assetlen.test     Crew       Kato  Foreman
mkuser dinah.principal@assetlen.test  Client     Dinah Principal
mkuser mara.stranger@assetlen.test    Client     Mara  Stranger

# SILENT_CONTRACTOR=1 runs the suite with no delivery-side account ever signing
# in (Law 0, plan.md P9): Nalan and his bench are on the roster, never logged in,
# and every assertion that needs one of them to act is skipped, not faked.
SILENT="${SILENT_CONTRACTOR:-0}"
silent() { [ "$SILENT" = 1 ]; }

PETER=$(tok "$BUYER" password)
if silent; then
  NALAN=""; KATO=""; LOGINS="PETER DINAH MARA"
  echo "  (contractor silent: no delivery-side sign-in)"
else
  NALAN=$(tok nalan.arch@assetlen.test password)
  KATO=$(tok  kato.foreman@assetlen.test password)
  LOGINS="PETER NALAN KATO DINAH MARA"
fi
DINAH=$(tok dinah.principal@assetlen.test password)
MARA=$(tok  mara.stranger@assetlen.test password)
for t in $LOGINS; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 0 — Peter alone: his project, his thread, his footage, his declarations"

stage() { printf '{"StageName":"%s","Phase":"%s","DisplayOrder":%s,"BudgetAmount":%s,"StartDate":"%sT00:00:00","ExpectedEndDate":"%sT00:00:00"}' "$@"; }
STAGES_JSON="[$(stage 'Guest wing plaster' Finishes 1 20000000 2026-08-01 2026-09-10),$(stage 'Main house undercoat' Finishes 2 15000000 2026-08-15 2026-09-20),$(stage 'Windows main house' Envelope 3 30000000 2026-08-20 2026-09-25),$(stage 'Windows guest wing' Envelope 4 15000000 2026-09-01 2026-09-30),$(stage 'Doors' Finishes 5 10000000 2026-09-20 2026-09-30),$(stage 'Screeding' Finishes 6 10000000 2026-09-01 2026-09-25),$(stage 'Terrazzo' Finishes 7 12000000 2026-09-10 2026-09-30),$(stage 'Epoxy floor' Finishes 8 8000000 2026-09-05 2026-09-28),$(stage 'Bathroom wall tiling' Finishes 9 6000000 2026-09-10 2026-09-30),$(stage 'Septic tank' Services 10 5000000 2026-08-20 2026-09-30),$(stage 'Stonework' Envelope 11 9000000 2026-08-01 2026-09-30),$(stage 'External works' ExternalWorks 12 20000000 2026-10-01 2026-10-30)]"
CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Riverside works $STAMP\",\"Description\":\"Works report subject\",\"Location\":\"Test plot\",\"TotalBudget\":160000000,\"Currency\":\"UGX\",\"ExpectedCompletionDate\":\"2026-09-30T00:00:00\",\"Stages\":$STAGES_JSON}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter opens a project with twelve stages" "$PID"

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
stage_id() { printf '%s' "$STAGES" | tr '}' '\n' | grep -- "\"stageName\":\"$1\"" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
S_PLASTER=$(stage_id "Guest wing plaster"); S_WIN=$(stage_id "Windows main house"); S_WING=$(stage_id "Windows guest wing")
S_DOORS=$(stage_id "Doors"); S_TERRAZZO=$(stage_id "Terrazzo"); S_EPOXY=$(stage_id "Epoxy floor")
S_TILING=$(stage_id "Bathroom wall tiling"); S_SEPTIC=$(stage_id "Septic tank"); S_STONE=$(stage_id "Stonework")
ne "…each stage with its planned dates"            "" "$S_SEPTIC"

deliverable() { jget "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$1\",\"Title\":\"$2\",\"DueDate\":\"$3T00:00:00\"}")" id; }
D_DOORS=$(deliverable "$S_DOORS" "First-floor doors hung" 2026-10-08)
D_WING=$(deliverable "$S_WING" "Guest wing windows fixed" 2026-10-02)

PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $PETER" -F "file=@$FIXTURES/$R_ZIP" -F "projectId=$PID")
ST=$(jget "$(req POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"$(jget "$PRE" batchId)\",\"AuthorMappings\":[]}")" status)
eq "he imports the export himself, authors unmapped" "Completed" "$ST"

head_ "Footage re-joined to its moment (works-report §5)"
RJ=$("${CURL[@]}" -X POST "$API/Ingest/RejoinMedia" -H "Authorization: Bearer $PETER" -F "files=@$FIXTURES/$R_MEDIA_ZIP" -F "projectId=$PID")
WANT_BOUND=$((3 + R_HAS_VIDEO))
eq "the loose files bind to their <Media omitted> lines" "$WANT_BOUND" "$(jnum "$RJ" bound)"
AFTER_ROW=$(printf '%s' "$RJ" | sed 's/},{/}\n{/g' | grep '6.00.12 PM')
eq "the frame in the wrong day's folder binds by its stamp" "Bound" "$(jget "$AFTER_ROW" outcome)"
eq "…to the 22 Sep 6:00 PM line"                    "2026-09-22T18:00:00" "$(jget "$AFTER_ROW" messageSentAt | cut -c1-19)"
eq "the stamp with no line is kept and reported"     "1" "$(jnum "$RJ" unbound)"
eq "…as unbound"                                    "Unbound" "$(jget "$(printf '%s' "$RJ" | sed 's/},{/}\n{/g' | grep '7.00.00 AM')" outcome)"

MEDIA=$(req GET "/Ingest/GetMessages?ProjectId=$PID&MediaOnly=true&Take=50" "$PETER")
A_AFTER=$(printf '%s' "$(req GET "/Ingest/GetMessages?ProjectId=$PID&From=2026-09-22T18:00:00&To=2026-09-22T18:01:00&Take=20" "$PETER")" | grep -o '"artifactId":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
A_BEFORE=$(printf '%s' "$(req GET "/Ingest/GetMessages?ProjectId=$PID&From=2026-08-24T17:10:00&To=2026-08-24T17:11:00&Take=20" "$PETER")" | grep -o '"artifactId":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
ne "the before and after frames are stored"          "" "$A_BEFORE$A_AFTER"

# What Peter writes down from his own thread — the contractor is nowhere near it.
com() { req POST /Commitments/AddCommitment "$PETER" "$1"; }
C_DONE=$(jget "$(com "{\"ProjectId\":\"$PID\",\"Kind\":\"Date\",\"Title\":\"Complete the whole project\",\"DueDate\":\"2026-09-30T00:00:00\",\"AgreedAt\":\"2026-08-13T17:05:00\",\"AgreedWithPartyName\":\"Nalan, on the thread\"}")" id)
R_DONE=$(jget "$(req POST /Commitments/Restate "$PETER" "{\"CommitmentId\":\"$C_DONE\",\"DueDate\":\"2026-09-30T00:00:00\",\"AgreedAt\":\"2026-08-22T19:30:00\"}")" id)
C_WEEK=$(jget "$(com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_PLASTER\",\"Kind\":\"Date\",\"Title\":\"Guest wing plaster complete this week\",\"DueDate\":\"2026-09-06T00:00:00\",\"AgreedAt\":\"2026-09-02T18:00:00\"}")" id)
C_EPOXYBACK=$(jget "$(com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_EPOXY\",\"Kind\":\"Date\",\"Title\":\"Epoxy team back on site\",\"DueDate\":\"2026-09-08T00:00:00\",\"AgreedAt\":\"2026-09-07T09:00:00\"}")" id)
C_DOORSBY=$(jget "$(com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_DOORS\",\"Kind\":\"Date\",\"Title\":\"Doors fitted\",\"DueDate\":\"2026-10-10T00:00:00\",\"AgreedAt\":\"2026-09-25T17:00:00\"}")" id)
com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_WIN\",\"Kind\":\"Spec\",\"Title\":\"Window design as drawn, no changes\",\"AgreedAt\":\"2026-08-05T10:15:00\"}" >/dev/null
com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_EPOXY\",\"Kind\":\"Material\",\"Title\":\"Epoxy colour grey\",\"AgreedAt\":\"2026-09-08T16:00:00\"}" >/dev/null
com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_TERRAZZO\",\"Kind\":\"Spec\",\"Title\":\"Terrazzo on the terrace, one colour, no black\",\"AgreedAt\":\"2026-09-15T17:30:00\"}" >/dev/null
com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_PLASTER\",\"Kind\":\"Choice\",\"Title\":\"Guest wing maintained as planned\",\"AgreedAt\":\"2026-09-20T10:00:00\"}" >/dev/null
C_OWED=$(jget "$(com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_TILING\",\"Kind\":\"Choice\",\"Title\":\"Bathroom tile colour to match the epoxy\",\"Maturity\":\"InDiscussion\",\"OwedBySide\":\"Client\",\"DueDate\":\"2026-10-05T00:00:00\",\"AgreedAt\":\"2026-09-14T17:00:00\"}")" id)
C_PARAPET=$(jget "$(com "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_STONE\",\"Kind\":\"Spec\",\"Title\":\"Parapet height as drawn\",\"AgreedAt\":\"2026-08-10T12:00:00\"}")" id)
R_PARAPET=$(jget "$(req POST /Commitments/Restate "$PETER" "{\"CommitmentId\":\"$C_PARAPET\",\"Title\":\"Parapet raised one course plus coping\",\"AgreedAt\":\"2026-09-17T11:00:00\"}")" id)
ne "the register takes his dates, decisions and the owed choice" "" "$C_DONE$R_DONE$C_WEEK$C_OWED$R_PARAPET"

var() { jget "$(req POST /Ledger/AddVariation "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$1\",\"Title\":\"$2\",\"RaisedAt\":\"$3T12:00:00\"${4:+,\"CostDelta\":$4}}")" id; }
V1=$(var "$S_PLASTER" "Pantry turned into dining" 2026-08-12)
V2=$(var "$S_TERRAZZO" "Terrazzo extended to the laundry" 2026-09-17)
V3=$(var "$S_EPOXY" "Epoxy over the whole main house floor" 2026-09-12)
V4=$(var "$S_DOORS" "First-floor door openings cut higher" 2026-09-25)
V5=$(var "$S_TILING" "Bathroom tiles changed to match the epoxy" 2026-09-24)
V6=$(var "$S_SEPTIC" "Septic tank top slab thickened" 2026-09-10 1200000)
ne "six variations, five of them never costed"      "" "$V6"

blk() { oid "$(req POST /Flags/AddFlag "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$1\",\"Title\":\"$2\",\"OwnerPartyName\":\"$3\",\"RaisedAt\":\"$4\",\"Channel\":\"Client\"}")"; }
B1=$(blk "$S_WIN" "Window team slow to respond" "the window team" 2026-09-08T16:00:00)
B2=$(blk "$S_EPOXY" "Epoxy team did not show up" "the epoxy team" 2026-09-07T09:00:00)
B3=$(blk "$S_WING" "Guest wing aluminium not fabricated" "the aluminium team" 2026-09-23T09:30:00)
B4=$(blk "$S_SEPTIC" "Cast postponed, no machinery" "the machinery hire" 2026-08-17T08:10:00)
B5=$(blk "$S_STONE" "Power cuts stop the stonework" "the power utility" 2026-09-04T18:30:00)
ne "five blockers, each owned by a named third party" "" "$B5"

F=$(req POST /Funding/AddFundingEntry "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_WIN\",\"Amount\":100000000,\"Currency\":\"UGX\",\"PaymentDate\":\"2026-08-13T18:40:00\",\"Notes\":\"Sent through the agent\"}")
F_ID=$(oid "$F")
ne "UGX 100M declared, sent through an agent"        "" "$F_ID"
eq "nobody but Peter stands on this project"         "1" "$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER" | grep -o '"userId":"[^"]*"' | sort -u | wc -l | tr -d ' ')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "R1 — a report issues with the contractor silent, frozen and hashed"

issue() { req POST /WorksReport/Issue "$1" "{\"ProjectId\":\"$PID\",\"AsAt\":\"$2T00:00:00\"${3:+,\"Audience\":\"$3\"}}"; }
I14=$(issue "$PETER" 2026-09-14)
R14=$(printf '%s' "$I14" | jx '$r.id')
ne "Peter issues the report as at 14 Sep"            "null" "$R14"
eq "…for his own side"                             "Client" "$(printf '%s' "$I14" | jx '$r.audience')"
eq "…in his own name"                              "Peter Developer" "$(printf '%s' "$I14" | jx '$r.issuedByName')"
eq "…for the seven days before, as its first"      "true" "$(printf '%s' "$I14" | jx '$r.windowLabel -match "7 days" -and $null -eq $r.previousReportId')"
eq "the stall surfaces itself: plaster at 90% since 2 Sep" "90|12" "$(printf '%s' "$I14" | jx '$s = st "Guest wing plaster"; "{0}|{1}" -f $s.percent, $s.stalledDays')"
eq "…the thread still says it continues"           "true" "$(printf '%s' "$I14" | jx '$null -ne (st "Guest wing plaster").stillReportedAt')"
eq "readings over time, for the sparkline"           "3" "$(printf '%s' "$I14" | jx '@((st "Guest wing plaster").readings).Count')"
eq "a stage not yet begun reads as not started"      "NotStarted" "$(printf '%s' "$I14" | jx '(st "External works").status')"
eq "the undercoat has gone quiet — a gap, not a blank" "true" "$(printf '%s' "$I14" | jx '@((st "Main house undercoat").gaps | Where-Object { $_ -match "No reading since 24 Aug" }).Count -eq 1')"
eq "the board is grouped by phase"                     "4" "$(printf '%s' "$I14" | jx '@($r.stageGroups).Count')"
eq "overall progress says how it is weighted"        "budget" "$(printf '%s' "$I14" | jx '$r.progress.weighting')"

A=$(req GET "/WorksReport/Issued?reportId=$R14" "$PETER"); B=$(req GET "/WorksReport/Issued?reportId=$R14" "$PETER")
eq "an issued snapshot reads back byte for byte"     "$(printf '%s' "$A" | sha256sum | cut -c1-64)" "$(printf '%s' "$B" | sha256sum | cut -c1-64)"
eq "…and still hashes to its content hash"         "true" "$(printf '%s' "$A" | jx '$r.hashVerified')"
eq "…the hash it was issued with"                  "$(printf '%s' "$I14" | jx '$r.contentSha256')" "$(printf '%s' "$A" | jx '$r.contentSha256')"

I21=$(issue "$PETER" 2026-09-21)
R21=$(printf '%s' "$I21" | jx '$r.id')
eq "a week later, the re-issue links the last one"   "$R14" "$(printf '%s' "$I21" | jx '$r.previousReportId')"
eq "…its window starts where that one ended"       "2026-09-14" "$(printf '%s' "$I21" | jx '([datetime]$r.windowFrom).ToString("yyyy-MM-dd")')"
eq "…and says what moved: tiling 20% → 50%"        "20|50" "$(printf '%s' "$I21" | jx '$m = @($r.sinceLast.stageMoves | Where-Object { $_.stageName -eq "Bathroom wall tiling" })[0]; "{0}|{1}" -f $m.from, $m.to')"
eq "…terrazzo, from nothing to 45%"               "|45" "$(printf '%s' "$I21" | jx '$m = @($r.sinceLast.stageMoves | Where-Object { $_.stageName -eq "Terrazzo" })[0]; "{0}|{1}" -f $m.from, $m.to')"
eq "…the decisions that are new since"            "true" "$(printf '%s' "$I21" | jx '@($r.sinceLast.newDecisions | Where-Object { $_ -match "Terrazzo on the terrace" }).Count -eq 1')"
eq "the earlier report is untouched by the later"   "$(printf '%s' "$A" | sha256sum | cut -c1-64)" "$(req GET "/WorksReport/Issued?reportId=$R14" "$PETER" | sha256sum | cut -c1-64)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "R2 — the date: set, restated, lapsed, and two answers to 'how long'"

I28=$(issue "$PETER" 2026-09-28)
R28=$(printf '%s' "$I28" | jx '$r.id')
printf '%s' "$I28" > "$SCRATCH/report-28-sep.json"
eq "since the last report: the aluminium blocker opened" "true" "$(printf '%s' "$I28" | jx '@($r.sinceLast.blockersOpened) -contains "Guest wing aluminium not fabricated"')"
eq "the strip marks the date set on 13 Aug"          "2026-08-13" "$(printf '%s' "$I28" | jx '([datetime](marks Set)[0].at).ToString("yyyy-MM-dd")')"
eq "…restated 'on track' on 22 Aug"               "2026-08-22" "$(printf '%s' "$I28" | jx '([datetime](marks Restated)[0].at).ToString("yyyy-MM-dd")')"
eq "…promised for 30 Sep"                          "2026-09-30" "$(printf '%s' "$I28" | jx '([datetime]$r.deadline.promised).ToString("yyyy-MM-dd")')"
eq "…two short promises lapsed"                    "2" "$(printf '%s' "$I28" | jx '$r.deadline.lapsedCount')"
eq "…'complete this week' among them"             "true" "$(printf '%s' "$I28" | jx '@(marks Lapsed | Where-Object { $_.label -match "complete this week" }).Count -eq 1')"
eq "…the doors' promise is still ahead, not lapsed" "true" "$(printf '%s' "$I28" | jx '@($r.ahead.items | Where-Object { $_.title -eq "Doors fitted" }).Count -eq 1')"
eq "the pace forecast: 10 Oct, from the readings"    "2026-10-10" "$(printf '%s' "$I28" | jx '([datetime]$r.deadline.forecast).ToString("yyyy-MM-dd")')"
eq "…and it names what it cannot speak for"        "true" "$(printf '%s' "$I28" | jx '@($r.deadline.forecastExcludes) -contains "Guest wing plaster"')"
eq "at risk, two days out"                           "At risk" "$(printf '%s' "$I28" | jx '$r.deadline.stateWord')"
eq "the answer he asked for, first"                  "Not on course for 30 Sep." "$(printf '%s' "$I28" | jx '(@($r.narrative.targets | Where-Object { $_.targetId -eq "answer" })[0].sentences[0]).text')"
eq "…and how much: ten days after it"             "true" "$(printf '%s' "$I28" | jx '(@($r.narrative.targets | Where-Object { $_.targetId -eq "answer" })[0].sentences | Where-Object { $_.text -match "10 days after" }).Count -eq 1')"
eq "…and that no revised date has been given"     "true" "$(printf '%s' "$I28" | jx '@(@($r.narrative.targets | Where-Object { $_.targetId -eq "answer" })[0].sentences | Where-Object { $_.text -match "has not yet given a revised date" }).Count -eq 1')"
eq "the revised date is listed as owed to him"       "Contractor" "$(printf '%s' "$I28" | jx '@($r.ahead.items | Where-Object { $_.key -eq "owed:revised-date" })[0].owedBy')"
eq "the decision he owes, with its by-when"          "2026-10-05" "$(printf '%s' "$I28" | jx "([datetime]@(\$r.ahead.items | Where-Object { \$_.key -eq 'owed:$C_OWED' })[0].dueBy).ToString('yyyy-MM-dd')")"
eq "a stage's plan and forecast sit side by side"    "2026-09-30|2026-10-08" "$(printf '%s' "$I28" | jx '$s = st "Terrazzo"; "{0}|{1}" -f ([datetime]$s.plannedEnd).ToString("yyyy-MM-dd"), ([datetime]$s.forecast).ToString("yyyy-MM-dd")')"

req POST /Commitments/Restate "$PETER" "{\"CommitmentId\":\"$R_DONE\",\"DueDate\":\"2026-10-20T00:00:00\",\"AgreedAt\":\"2026-09-28T21:00:00\"}" >/dev/null
L=$(req GET "/WorksReport/Live?projectId=$PID&asAt=2026-09-28" "$PETER")
eq "once a new date is given, the contractor's ○ appears" "2026-10-20" "$(printf '%s' "$L" | jx '([datetime]$r.deadline.contractorDate).ToString("yyyy-MM-dd")')"
eq "…beside the pace forecast ◌, neither winning"  "2026-10-10" "$(printf '%s' "$L" | jx '([datetime]$r.deadline.forecast).ToString("yyyy-MM-dd")')"
eq "…and it is no longer owed"                     "0" "$(printf '%s' "$L" | jx '@($r.ahead.items | Where-Object { $_.key -eq "owed:revised-date" }).Count')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "R3 — agreed and stuck: variations, gaps, blockers by owner"

eq "all six variations are on the report"            "6" "$(printf '%s' "$I28" | jx '@($r.variations).Count')"
eq "…five say 'not costed' rather than zero"       "5" "$(printf '%s' "$I28" | jx '@($r.variations | Where-Object { $_.gaps -contains "Not costed" -and $null -eq $_.costDelta }).Count')"
eq "…none has an approval on record"               "6" "$(printf '%s' "$I28" | jx '@($r.variations | Where-Object { $_.gaps -contains "No approval on record" }).Count')"
eq "the parapet change with no variation is a gap"   "true" "$(printf '%s' "$I21" | jx '@($r.scopeGaps | Where-Object { $_.text -match "Parapet raised" }).Count -eq 1')"
eq "blockers in five lanes, one per owner"           "5" "$(printf '%s' "$I28" | jx '@($r.blockers).Count')"
eq "…the window team's, open 20 days"              "20" "$(printf '%s' "$I28" | jx '(lane "the window team").items[0].daysOpen')"
eq "…the oldest lane first: the machinery hire"    "the machinery hire" "$(printf '%s' "$I28" | jx '$r.blockers[0].owner')"
eq "decisions carry who agreed and when"             "true" "$(printf '%s' "$I28" | jx '$d = @($r.decisions | Where-Object { $_.title -eq "Epoxy colour grey" })[0]; $null -ne $d.agreedAt -and $d.sourceIds.Count -ge 1')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Every figure reconciles to its source table"

LIVE=$(req GET "/WorksReport/Live?projectId=$PID" "$PETER")
LEDG=$(req GET "/Ledger/GetStageLedger?projectId=$PID" "$PETER")
eq "funded agrees with the stage ledger"             "$(printf '%s' "$LEDG" | jx '$r.totalFunded')" "$(printf '%s' "$LIVE" | jx '$r.money.totalFunded')"
eq "sent-not-acknowledged agrees with it"            "$(printf '%s' "$LEDG" | jx '$r.totalPending')" "$(printf '%s' "$LIVE" | jx '$r.money.totalPending')"
eq "claimed and cleared agree with it"               "$(printf '%s' "$LEDG" | jx '"{0}|{1}" -f $r.totalClaimed, $r.totalCleared')" "$(printf '%s' "$LIVE" | jx '"{0}|{1}" -f $r.money.totalClaimed, $r.money.totalCleared')"
eq "…each stage's row too"                         "$(printf '%s' "$LEDG" | jx "@(\$r.rows | Where-Object { \$_.stageId -eq '$S_WIN' })[0].pendingFunding")" "$(printf '%s' "$LIVE" | jx "@(\$r.money.rows | Where-Object { \$_.stageId -eq '$S_WIN' })[0].pending")"
eq "the release is a gap: receipt not confirmed"     "true" "$(printf '%s' "$LIVE" | jx "@(\$r.money.releases | Where-Object { \$_.id -eq '$F_ID' -and \$_.gap -match 'not confirmed' }).Count -eq 1")"
RD=$(req GET "/Extraction/GetReadings?projectId=$PID&stageId=$S_TILING" "$PETER")
eq "a stage's figure is its latest reading"          "$(printf '%s' "$RD" | jx '($r | Sort-Object { [datetime]$_.observedAt } | Select-Object -Last 1).percent')" "$(printf '%s' "$LIVE" | jx '(st "Bathroom wall tiling").percent')"
eq "…and the readings on the card are the table's" "$(printf '%s' "$RD" | jx '@($r).Count')" "$(printf '%s' "$LIVE" | jx '@((st "Bathroom wall tiling").readings).Count')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "R4 — every sentence cited; the report issues with the model off"

cited() { printf '%s' "$1" | jx '$ids = @($r.sources.id); @($r.narrative.targets.sentences | Where-Object { @($_.sourceIds).Count -lt 1 -or @($_.sourceIds | Where-Object { $ids -notcontains $_ }).Count -gt 0 }).Count'; }
eq "every sentence of the 28 Sep report cites the snapshot" "0" "$(cited "$I28")"
eq "…every sentence of the live page too"          "0" "$(cited "$LIVE")"
eq "…there are sentences to check"                 "true" "$(printf '%s' "$I28" | jx '@($r.narrative.targets.sentences).Count -gt 20')"
eq "with drafting off, the report is fully templated" "template|true" "$(printf '%s' "$I28" | jx '"{0}|{1}" -f $r.narrative.engine, (@($r.narrative.targets | Where-Object { -not $_.templated }).Count -eq 0).ToString().ToLower()')"
SET=$(req GET "/WorksReport/Settings?projectId=$PID" "$PETER")
eq "drafting is off until the owner turns it on"     "false" "$(printf '%s' "$SET" | jx '$r.draftingEnabled')"
eq "…and the setting says plainly what it sends"   "true" "$(printf '%s' "$SET" | jx '$r.statement -match "Anthropic" -and $r.statement -match "Numbers never come from the model"')"
req PUT /WorksReport/Settings "$PETER" "{\"ProjectId\":\"$PID\",\"DraftingEnabled\":true}" >/dev/null
IM=$(issue "$PETER" 2026-09-28)
if [ "$(printf '%s' "$SET" | jx '$r.draftingAvailable')" = "false" ]; then
  eq "drafting on but no model here: it still issues, templated" "0" "$(printf '%s' "$IM" | jx '@($r.narrative.targets | Where-Object { -not $_.templated }).Count')"
  eq "…and says the model was unavailable"         "true" "$(printf '%s' "$IM" | jx '$r.narrative.engine -match "template"')"
else
  eq "drafting on: it issues, every accepted sentence cited" "0" "$(cited "$IM")"
  eq "…the validator reports what it refused"      "true" "$(printf '%s' "$IM" | jx '$r.narrative.rejected -ge 0')"
fi
req PUT /WorksReport/Settings "$PETER" "{\"ProjectId\":\"$PID\",\"DraftingEnabled\":false}" >/dev/null

check() { req POST /WorksReport/CheckNarrative "$PETER" "{\"ProjectId\":\"$PID\",\"Draft\":{\"TargetId\":\"blocker:$B1\",\"Sentences\":[{\"Text\":\"$1\",\"SourceIds\":[$2]}]}}"; }
GOOD_DAYS=$(printf '%s' "$LIVE" | jx '(lane "the window team").items[0].daysOpen')
eq "the validator accepts a sentence built from the facts" "1" "$(jnum "$(check "Open $GOOD_DAYS days, waiting on the window team." "\"blocker:$B1\"")" accepted)"
eq "…refuses a figure the facts do not carry"      "0" "$(jnum "$(check "Open 45 days, waiting on the window team." "\"blocker:$B1\"")" accepted)"
eq "…refuses a figure written out in words"       "0" "$(jnum "$(check "Open three weeks, waiting on the window team." "\"blocker:$B1\"")" accepted)"
eq "…refuses a sentence citing nothing"           "0" "$(jnum "$(check "Waiting on the window team." "")" accepted)"
eq "…refuses a source it was not given"            "0" "$(jnum "$(check "Waiting on the window team." "\"release:$F_ID\"")" accepted)"
eq "…refuses selling"                              "0" "$(jnum "$(check "Great progress with the window team." "\"blocker:$B1\"")" accepted)"
eq "…refuses a promise the record does not make"   "0" "$(jnum "$(check "The window team will finish soon." "\"blocker:$B1\"")" accepted)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Video posters — the footage is not blank tiles"

if [ "$R_HAS_VIDEO" = "1" ]; then
  POSTER="Pending"
  for _ in $(seq 1 30); do
    LV=$(req GET "/WorksReport/Live?projectId=$PID&asAt=2026-09-28" "$PETER")
    POSTER=$(printf '%s' "$LV" | jx '$v = @($r.footage.frames | Where-Object { $_.isVideo })[0]; if ($v) { $v.posterStatus } else { "none" }')
    [ "$POSTER" = "Done" ] || [ "$POSTER" = "EngineUnavailable" ] || [ "$POSTER" = "Failed" ] && break
    sleep 2
  done
  if [ "$POSTER" = "EngineUnavailable" ]; then
    skip "the video gets a poster frame" "the API process has no ffmpeg; its tile says length unknown"
  else
    eq "the video gets a poster frame"               "Done" "$POSTER"
    eq "…and its length, three seconds"            "3" "$(printf '%s' "$LV" | jx '[math]::Round(@($r.footage.frames | Where-Object { $_.isVideo })[0].durationSeconds)')"
    VID=$(printf '%s' "$LV" | jx '@($r.footage.frames | Where-Object { $_.isVideo })[0].artifactId')
    eq "…which opens as the video's thumbnail"     "200" "$(code GET "/Artifacts/$VID/thumbnail" "$PETER")"
  fi
else
  skip "the video gets a poster frame" "no ffmpeg on this host to make the fixture video"
fi

eq "the same view, then and now, is paired"          "true" "$(printf '%s' "$I28" | jx "@(\$r.changed | Where-Object { \$_.before.artifactId -eq '$A_BEFORE' -and \$_.after.artifactId -eq '$A_AFTER' }).Count -eq 1")"
eq "…under the stage it shows"                     "Guest wing plaster" "$(printf '%s' "$I28" | jx '@($r.changed)[0].stageName')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Sides and seats — exactly as private as the sources"

req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.principal@assetlen.test\",\"Specialization\":9,\"Side\":0,\"HandlesMoney\":false,\"Title\":\"Representative\"}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id '"userFullName":"Nalan Architect"')\",\"IsMediator\":true}" >/dev/null
# Peter stands down as mediator once Nalan holds the seat: from here he reads as the client side.
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id '"userFullName":"Peter Developer"')\",\"IsMediator\":false}" >/dev/null

if silent; then
  skip "the delivery side's own report, crew thread and crew blocker" "contractor silent"
  skip "the bench is refused the report"                              "contractor silent"
else
# The delivery side forwards its own crew group: Site Diary material, on its side of the line.
CPRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $NALAN" -F "file=@$FIXTURES/$R_CREW_ZIP" -F "projectId=$PID")
req POST /Ingest/CommitImport "$NALAN" "{\"BatchId\":\"$(jget "$CPRE" batchId)\",\"AuthorMappings\":[]}" >/dev/null
CB=$(oid "$(req POST /Flags/AddFlag "$NALAN" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_PLASTER\",\"Title\":\"Scaffold hire lapsed\",\"Description\":\"crew wording: the hirer is owed\",\"OwnerPartyName\":\"the scaffold hirer\",\"Channel\":\"Crew\"}")")
IN=$(issue "$NALAN" "$(date +%Y-%m-%d)" Contractor)
RN=$(printf '%s' "$IN" | jx '$r.id')
printf "%s" "$IN" > "$SCRATCH/contractor-issued.json"
CREW_ART=$(printf '%s' "$IN" | jx '@($r.footage.frames | Where-Object { ([datetime]$_.at).ToString("yyyy-MM-dd") -eq "2026-09-27" })[0].artifactId')
eq "the mediator may issue his side's report"        "Contractor" "$(printf '%s' "$IN" | jx '$r.audience')"
ne "…which carries the crew's own photo"           "null" "$CREW_ART"

PL=$(req GET "/WorksReport/Live?projectId=$PID" "$PETER")
printf "%s" "$PL" > "$SCRATCH/client-live.json"
eq "the client-side report carries no crew photo"    "0" "$(printf '%s' "$PL" | grep -c "$CREW_ART" | tr -d ' ')"
eq "…nor the crew thread's words"                 "0" "$(printf '%s' "$PL" | grep -c "scaffold moved" | tr -d ' ')"
eq "…nor the crew blocker's wording"              "0" "$(printf '%s' "$PL" | grep -c "the hirer is owed" | tr -d ' ')"
eq "…but the blocker crosses on the truth floor"   "true|null" "$(printf '%s' "$PL" | jx "\$b = @(\$r.blockers.items | Where-Object { \$_.id -eq '$CB' })[0]; '{0}|{1}' -f \$b.crossed.ToString().ToLower(), \$(if (\$null -eq \$b.detail) { 'null' } else { \$b.detail })")"
eq "the delivery side's report is not the client's to read"                  "404" "$(code GET "/WorksReport/Issued?reportId=$RN" "$DINAH")"
eq "the bench has no report: live"                   "404" "$(code GET "/WorksReport/Live?projectId=$PID" "$KATO")"
eq "…issued"                                       "404" "$(code GET "/WorksReport/Issued?reportId=$R28" "$KATO")"
eq "…nor may it issue one"                         "404" "$(codeb POST /WorksReport/Issue "$KATO" "{\"ProjectId\":\"$PID\"}")"
fi
eq "a stranger has none either"                     "404" "$(code GET "/WorksReport/Live?projectId=$PID" "$MARA")"

DL=$(req GET "/WorksReport/Live?projectId=$PID" "$DINAH")
DI=$(req GET "/WorksReport/Issued?reportId=$R28" "$DINAH")
eq "the representative, off the money: no money section at all" "0" "$(printf '%s' "$DL" | grep -c '"money":' | tr -d ' ')"
eq "…nor in the issued snapshot"                   "0" "$(printf '%s' "$DI" | grep -c '"money":' | tr -d ' ')"
eq "…no money card"                                "0" "$(printf '%s' "$DI" | jx '@($r.headlines | Where-Object { $_.key -eq "money" }).Count')"
eq "…no release in the sources"                    "0" "$(printf '%s' "$DI" | grep -c "release:$F_ID" | tr -d ' ')"
eq "…the costed variation says a figure exists"    "true|null" "$(printf '%s' "$DI" | jx "\$v = @(\$r.variations | Where-Object { \$_.id -eq '$V6' })[0]; '{0}|{1}' -f \$v.amountHidden.ToString().ToLower(), \$(if (\$null -eq \$v.costDelta) { 'null' } else { \$v.costDelta })")"
eq "…while Peter's copy of the same report has it" "true" "$(printf '%s' "$(req GET "/WorksReport/Issued?reportId=$R28" "$PETER")" | jx '$null -ne $r.money')"
eq "the representative may not switch drafting on"   "403" "$(codeb PUT /WorksReport/Settings "$DINAH" "{\"ProjectId\":\"$PID\",\"DraftingEnabled\":true}")"
if silent; then skip "…nor may the mediator" "contractor silent"; else
eq "…nor may the mediator"                         "403" "$(codeb PUT /WorksReport/Settings "$NALAN" "{\"ProjectId\":\"$PID\",\"DraftingEnabled\":true}")"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "R5 — a week with nobody logged in, and a report still issues"

SC=$(req POST "/WorksReport/RunSchedule?projectId=$PID&milestones=false" "$ADMIN")
eq "the weekly job issues on its own"                "1" "$(jnum "$SC" weekly)"
SID=$(printf '%s' "$SC" | jx '$r.reportIds[0]')
SR=$(req GET "/WorksReport/Issued?reportId=$SID" "$PETER")
eq "…by nobody"                                    "Scheduled|Issued on schedule" "$(printf '%s' "$SR" | jx '"{0}|{1}" -f $r.issueKind, $r.issuedByName')"
eq "…and delivered to the client principals"       "true" "$(printf '%s' "$SR" | jx '$r.deliveryNote -match "Peter Developer" -and $r.deliveryNote -match "Dinah Principal" -and $r.deliveryNote -notmatch "Kato"')"
eq "…never twice in one week"                      "0" "$(jnum "$(req POST "/WorksReport/RunSchedule?projectId=$PID&milestones=false" "$ADMIN")" weekly)"

SEPTIC=$(req GET "/Stages/GetStageById?stageId=$S_SEPTIC" "$PETER")
DONE_BODY=$(printf '%s' "$SEPTIC" | pwsh -NoProfile -Command '$s = [Console]::In.ReadToEnd() | ConvertFrom-Json; $s.completionPercentage = 100; $s.status = "Completed"; $s.actualEndDate = (Get-Date).ToString("s"); $s | ConvertTo-Json -Compress -Depth 5')
req PUT /Stages/UpdateStage "$PETER" "$DONE_BODY" >/dev/null
MS=$(req POST "/WorksReport/RunSchedule?projectId=$PID&weekly=false" "$ADMIN")
eq "a stage completing issues a milestone report"    "1" "$(jnum "$MS" milestones)"
eq "…saying why"                                   "true" "$(printf '%s' "$(req GET "/WorksReport/Issued?reportId=$(printf '%s' "$MS" | jx '$r.reportIds[0]')" "$PETER")" | jx '$r.issueKind -eq "Milestone" -and $r.issueReason -match "Septic tank complete"')"
eq "…once"                                         "0" "$(jnum "$(req POST "/WorksReport/RunSchedule?projectId=$PID&weekly=false" "$ADMIN")" milestones)"

H=$(req GET "/WorksReport/History?projectId=$PID" "$PETER")
eq "his history lists every client report, newest first" "true" "$(printf '%s' "$H" | jx '$k = @($r.issueKind); ($k -contains "Manual") -and ($k -contains "Scheduled") -and ($k -contains "Milestone")')"
if silent; then
  skip "…but not the delivery side's own" "contractor silent: there is none"
  skip "…the mediator sees both"          "contractor silent"
else
eq "…but not the delivery side's own"              "0" "$(printf '%s' "$H" | jx "@(\$r | Where-Object { \$_.id -eq '$RN' }).Count")"
eq "…the mediator sees both"                       "1" "$(req GET "/WorksReport/History?projectId=$PID" "$NALAN" | jx "@(\$r | Where-Object { \$_.id -eq '$RN' }).Count")"
fi
eq "a stranger reads no history"                     "404" "$(code GET "/WorksReport/History?projectId=$PID" "$MARA")"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
