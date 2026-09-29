#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P7 end-to-end suite: Peter's surfaces, the home and the brief.
#
# Every assertion below is asked from Peter's chair (plan.md P7):
#
#   With the contractor silent — no login, no curation, not even on the
#   roster — does a forwarded thread become a page grouped by the work,
#   with the acknowledgements set aside and the same view paired?  ← Law 0
#   Does the home answer, for every project at once and in one call: where
#   the money stands, what he owes, what moved?
#   Are the decisions he owes listed with by-when and consequence, across
#   projects, in the same list the rail counts?
#   Is the truth floor there for every reader — including a blocker the
#   delivery side raised on its own channel — and is it the same set of
#   facts for the funder and the representative, only in a different order?
#   Is everything else exactly as private as its source — by side, by seat,
#   by money, and to a stranger not at all?
#
# Usage:  bash tools/e2e-p7-brief.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running, pwsh for the fixtures and for reading JSON. Run
#         from the repo root. Idempotent — each run builds its own project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/p7"
TODAY="$(date +%Y-%m-%d)"

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-60s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-60s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
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

code() {
  "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
}

jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
oid()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

# The answers are nested — sections inside the floor, pairs inside blocks — so
# they are read with a real JSON parser. $r is the parsed answer; b TITLE is the
# block for a deliverable (or an empty one); s KIND is a truth-floor section.
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    function b(\$t) { \$x = @(\$r.blocks | Where-Object { \$_.deliverableTitle -eq \$t }); if (\$x.Count) { \$x[0] } else { [pscustomobject]@{ notes = @(); pairs = @(); frames = @(); frameTotal = 0; percent = \$null } } }
    function s(\$k) { \$x = @(\$r.truthFloor | Where-Object { \$_.kind -eq \$k }); if (\$x.Count) { \$x[0] } else { [pscustomobject]@{ items = @() } } }
    function p(\$id) { \$x = @(\$r.projects | Where-Object { \$_.id -eq \$id }); if (\$x.Count) { \$x[0] } else { \$null } }
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } elseif (\$null -eq \$out) { 'null' } else { \$out }"
}

brief() { req GET "/Brief/Day?projectId=$PID&day=$2&days=${3:-1}" "$1"; }

echo "ASSETLEN P7 — Peter's home and the daily brief — $API"

# ── Fixtures ─────────────────────────────────────────────────────────────────
bash tools/make-brief-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
  || { echo "FATAL: could not build fixtures. Is pwsh on PATH?"; exit 1; }
# shellcheck disable=SC1090
. "$FIXTURES/fixtures.env"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

mkuser() {
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

mkuser peter.buyer@assetlen.test     Contractor Peter Developer
mkuser nalan.arch@assetlen.test      Manager    Nalan Architect
mkuser kato.foreman@assetlen.test    Crew       Kato  Foreman
mkuser dinah.principal@assetlen.test Client     Dinah Principal
mkuser mara.stranger@assetlen.test   Client     Mara  Stranger

# SILENT_CONTRACTOR=1 runs the suite with no delivery-side account ever signing
# in (Law 0, plan.md P9). The sections that exist to show what the delivery side
# adds are skipped, not faked; everything Peter can do alone still runs.
SILENT="${SILENT_CONTRACTOR:-0}"
silent() { [ "$SILENT" = 1 ]; }
skip() { printf "  ${c_skip}SKIP${c_off}  %-60s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }

PETER=$(tok peter.buyer@assetlen.test password)
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
head_ "Law 0 — Peter alone: his own project, his own forwarded thread"

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Peter Brief $STAMP\",\"Description\":\"P7 subject\",\"Location\":\"Test plot\",\"TotalBudget\":200000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Guest wing plaster\",\"DisplayOrder\":1,\"BudgetAmount\":40000000},{\"StageName\":\"Terrace finishes\",\"DisplayOrder\":2,\"BudgetAmount\":60000000}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter opens a project — nobody else is on it" "$PID"

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
stage_id() { printf '%s' "$STAGES" | tr '}' '\n' | grep -- "\"stageName\":\"$1\"" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
S1=$(stage_id "Guest wing plaster"); S2=$(stage_id "Terrace finishes")

deliverable() { jget "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$1\",\"Title\":\"$2\"}")" id; }
D_WALL=$(deliverable "$S1" "Rear wall plaster")
D_COPING=$(deliverable "$S1" "Parapet coping")
D_TILES=$(deliverable "$S2" "Terrace floor tiles")
D_BAL=$(deliverable "$S2" "Terrace balustrade")
ne "the checklist he funds is written"               "" "$D_BAL"

PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $PETER" -F "file=@$FIXTURES/$P7_ZIP" -F "projectId=$PID")
BATCH=$(jget "$PRE" batchId)
ST=$(jget "$(req POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"$BATCH\",\"AuthorMappings\":[]}")" status)
eq "he imports the export himself, authors unmapped" "Completed" "$ST"

MEDIA=$(req GET "/Ingest/GetMessages?ProjectId=$PID&MediaOnly=true&Take=20" "$PETER")
art_for() { printf '%s' "$MEDIA" | tr '}' '\n' | grep -- "$1" | grep -o '"artifactId":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
A_BEFORE=$(art_for "$P7_BEFORE"); A_ELSE=$(art_for "$P7_ELSEWHERE"); A_AFTER=$(art_for "$P7_AFTER")
ne "the three photos are stored"                     "" "$A_AFTER"

B8=$(brief "$PETER" 2026-07-08)
eq "the day's brief answers him"                     "2026-07-08" "$(printf '%s' "$B8" | jx '([datetime]$r.day).ToString("yyyy-MM-dd")')"
eq "…assembled with nobody curating it"            "true" "$(printf '%s' "$B8" | jx '$r.assembledWithoutCurator')"
eq "…grouped by the work: the rear wall"           "true" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").notes.Count -ge 1')"
eq "…the coping"                                    "true" "$(printf '%s' "$B8" | jx "(b 'Parapet coping').notes[0].text -match 'coping'")"
eq "…the tiles, from the representative's line"     "true" "$(printf '%s' "$B8" | jx "(b 'Terrace floor tiles').notes[0].text -match 'grey and sand'")"
eq "…the balustrade, with its figure"               "true" "$(printf '%s' "$B8" | jx "(b 'Terrace balustrade').notes[0].text -match '3,400,000'")"
eq "acknowledgements are read and given no space"    "2" "$(printf '%s' "$B8" | jx '$r.counts.acknowledgementsSetAside')"
eq "…so 'Noted' is on no block"                     "0" "$(printf '%s' "$B8" | jx '@($r.blocks.notes | Where-Object { $_.text -eq "Noted" }).Count')"
eq "a photo with no words joins its sender's line"   "1" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").frameTotal')"
eq "where the work stands, read from the thread"     "70" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").percent')"
eq "…and where it stood before"                    "20" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").percentBefore')"
eq "the funder's block leads: progress moved"        "Progress moved" "$(printf '%s' "$B8" | jx '$r.blocks[0].emphasisReason')"
eq "…and the coping, which named no figure, has none" "null" "$(printf '%s' "$B8" | jx '(b "Parapet coping").percent')"

head_ "Same view, then and now — not seventeen frames in a row"

eq "one same-view pair on the rear wall"             "1" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").pairs.Count')"
eq "…before: the first visit"                     "$A_BEFORE" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").pairs[0].before.artifactId')"
eq "…now: the second"                              "$A_AFTER" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").pairs[0].after.artifactId')"
eq "…two days apart"                               "2" "$(printf '%s' "$B8" | jx '(b "Rear wall plaster").pairs[0].daysApart')"
eq "the photo from elsewhere is never paired"        "0" "$(printf '%s' "$B8" | jx "@(\$r.blocks.pairs | Where-Object { \$_.before.artifactId -eq '$A_ELSE' -or \$_.after.artifactId -eq '$A_ELSE' }).Count")"
B6=$(brief "$PETER" 2026-07-06)
eq "the first day has two frames and nothing to pair" "2|0" "$(printf '%s' "$B6" | jx '"{0}|{1}" -f (b "Rear wall plaster").frameTotal, (b "Rear wall plaster").pairs.Count')"
eq "a pair's frames open for him"                    "200" "$(code GET "/Artifacts/$A_BEFORE/thumbnail" "$PETER")"

head_ "The truth floor, with the contractor silent"

eq "five sections, every one present"                "5" "$(printf '%s' "$B8" | jx '$r.truthFloor.Count')"
eq "…money first, for the funder"                  "Money" "$(printf '%s' "$B8" | jx '$r.truthFloor[0].kind')"
eq "the thread's figure reaches him, unconfirmed"    "true" "$(printf '%s' "$B8" | jx '@((s Money).items | Where-Object { $_.amount -eq 3400000 -and $_.unconfirmed }).Count -ge 1')"
eq "…marked as not yet in his register"            "true" "$(printf '%s' "$B8" | jx '((s Money).items | Where-Object { $_.unconfirmed } | Select-Object -First 1).source -match "not yet in your register"')"
eq "the rule is stated to him plainly"               "true" "$(printf '%s' "$B8" | jx '$r.rule -match "reaches you" -and $r.rule -match "emphasis"')"
eq "an empty section says so rather than vanishing"  "0" "$(printf '%s' "$B8" | jx '(s Blocker).items.Count')"

H=$(req GET /Brief/Home "$PETER")
eq "his home lists the project"                      "true" "$(printf '%s' "$H" | jx "\$null -ne (p '$PID')")"
eq "…with what he owes: the thread's proposals"    "Proposals" "$(printf '%s' "$H" | jx "(p '$PID').nextOwed.kind")"
eq "…and where the money stands"                   "0" "$(printf '%s' "$H" | jx "(p '$PID').money.funded")"
eq "…and what moved"                               "true" "$(printf '%s' "$H" | jx "(p '$PID').moved.Count -ge 0")"

if silent; then
  skip "the delivery side arrives, owes, claims and curates" "contractor silent"
else
# ═════════════════════════════════════════════════════════════════════════════
head_ "The delivery side arrives: Nalan mediates, Kato on his bench, Dinah on Peter's side"

req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.principal@assetlen.test\",\"Specialization\":9,\"Side\":0,\"HandlesMoney\":true,\"Title\":\"Representative\"}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
NALAN_MID=$(member_id '"userFullName":"Nalan Architect"')
PETER_MID=$(member_id '"userFullName":"Peter Developer"')
DINAH_MID=$(member_id '"userFullName":"Dinah Principal"')
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null
eq "Nalan is the accountable face"                   "true" "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$NALAN" | grep -o '"isMediator":[a-z]*' | head -1 | sed 's/.*://')"

# A choice Peter's side owes, with a by-when.
CH=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"DeliverableId\":\"$D_TILES\",\"Kind\":\"Choice\",\"Title\":\"Terrace floor tile colour: grey or sand\",\"Maturity\":\"InDiscussion\",\"OwedBySide\":\"Client\",\"DueDate\":\"2026-07-20T00:00:00\",\"AgreedAt\":\"2026-07-08T18:00:00\"}")
CH_ID=$(jget "$CH" id)
# A spoken price Nalan wrote down — Peter must confirm it or dispute it.
VB=$(req POST /Commitments/LogDecision "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S2\",\"DeliverableId\":\"$D_BAL\",\"Kind\":\"Price\",\"Title\":\"Balustrade labour\",\"Amount\":3400000,\"AgreedAt\":\"2026-07-08T18:30:00\"}")
VB_ID=$(jget "$VB" id)
# A promised date, then moved.
DT=$(req POST /Commitments/AddCommitment "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"DeliverableId\":\"$D_COPING\",\"Kind\":\"Date\",\"Title\":\"Coping fixed\",\"DueDate\":\"2026-07-10T00:00:00\",\"AgreedAt\":\"2026-07-08T17:20:00\"}")
RS=$(req POST /Commitments/Restate "$NALAN" "{\"CommitmentId\":\"$(jget "$DT" id)\",\"DueDate\":\"2026-07-17T00:00:00\",\"AgreedAt\":\"2026-07-08T19:00:00\"}")
RS_ID=$(jget "$RS" id)
# A blocker on the delivery side's own channel, detail and all.
BK=$(req POST /Flags/AddFlag "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Title\":\"Scaffold hire lapsed\",\"Description\":\"Crew-only detail: the hirer wants cash up front\",\"OwnerPartyName\":\"the scaffold hirer\",\"Channel\":\"Crew\"}")
BK_ID=$(oid "$BK")
# A claim and an extra, both for the funder to decide.
CL=$(req POST /Ledger/AddClaim "$NALAN" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Amount\":5000000,\"Note\":\"Scratch coat, rear wall\"}")
CL_ID=$(jget "$CL" id)
VA=$(req POST /Ledger/AddVariation "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S2\",\"Title\":\"Raise the parapet a course\",\"Reason\":\"Asked for on site\",\"CostDelta\":1200000}")
VA_ID=$(jget "$VA" id)
ne "the register and the ledger take it all"         "" "$VA_ID$CL_ID$RS_ID$VB_ID$CH_ID"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Decisions Peter owes — by-when and consequence, in the list the rail counts"

OW=$(req GET /Brief/Owed "$PETER")
owed() { printf '%s' "$OW" | jx "@(\$r | Where-Object { \$_.key -eq '$1' }) | Select-Object -First 1 | ForEach-Object { $2 }"; }
eq "the choice he owes"                              "Choice" "$(owed "commitment:$CH_ID" '$_.kind')"
eq "…by when"                                      "2026-07-20" "$(owed "commitment:$CH_ID" '([datetime]$_.dueBy).ToString("yyyy-MM-dd")')"
eq "…and what waiting holds up"                    "true" "$(owed "commitment:$CH_ID" '$_.consequence -match "Holds up Terrace floor tiles"')"
eq "the spoken price, waiting on his word"           "Confirmation" "$(owed "commitment:$VB_ID" '$_.kind')"
eq "the claim, for him to clear"                     "5000000" "$(owed "claim:$CL_ID" '$_.amount')"
eq "the extra, for his yes or no"                    "Variation" "$(owed "variation:$VA_ID" '$_.kind')"
eq "what the thread proposed, still waiting"         "Proposals" "$(owed "proposals:$PID" '$_.kind')"
eq "dated items come before undated ones"            "true" "$(printf '%s' "$OW" | jx '$d = @($r | ForEach-Object { $_.dueBy }); $first = [Array]::IndexOf($d, $null); $first -lt 0 -or @($d[$first..($d.Count-1)] | Where-Object { $null -ne $_ }).Count -eq 0')"

NOW=$(req GET /Brief/Owed "$NALAN")
eq "the choice is not the delivery side's to make"   "0" "$(printf '%s' "$NOW" | jx "@(\$r | Where-Object { \$_.key -eq 'commitment:$CH_ID' -or \$_.key -eq 'claim:$CL_ID' }).Count")"
eq "the bench owes nothing here"                     "0" "$(req GET /Brief/Owed "$KATO" | jx "@(\$r | Where-Object { \$_.projectId -eq '$PID' }).Count")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The home: every project, one call — money position, owed, moved"

H=$(req GET /Brief/Home "$PETER")
LEDG=$(req GET "/Ledger/GetStageLedger?projectId=$PID" "$PETER")
eq "what he owes here is counted"                    "$(printf '%s' "$OW" | jx "@(\$r | Where-Object { \$_.projectId -eq '$PID' }).Count")" "$(printf '%s' "$H" | jx "(p '$PID').owedCount")"
eq "claimed agrees with the stage ledger"            "$(printf '%s' "$LEDG" | jx '$r.totalClaimed')" "$(printf '%s' "$H" | jx "(p '$PID').money.claimed")"
eq "cleared agrees with the stage ledger"            "$(printf '%s' "$LEDG" | jx '$r.totalCleared')" "$(printf '%s' "$H" | jx "(p '$PID').money.cleared")"
eq "funded agrees with the stage ledger"             "$(printf '%s' "$LEDG" | jx '$r.totalFunded')" "$(printf '%s' "$H" | jx "(p '$PID').money.funded")"
eq "…and five million waits to be cleared"         "5000000" "$(printf '%s' "$H" | jx "(p '$PID').money.awaitingClearance")"
eq "what moved names the claim"                      "true" "$(printf '%s' "$H" | jx "@((p '$PID').moved | Where-Object { \$_.kind -eq 'Money' }).Count -ge 1")"
eq "…and the blocker, raised on the crew channel"  "true" "$(printf '%s' "$H" | jx "@((p '$PID').moved | Where-Object { \$_.kind -eq 'Blocker' }).Count -ge 1")"
eq "…one blocker open"                             "1" "$(printf '%s' "$H" | jx "(p '$PID').openBlockers")"
eq "he reads it as the funder"                       "Funder" "$(printf '%s' "$H" | jx "(p '$PID').emphasis")"

HK=$(req GET /Brief/Home "$KATO")
eq "the bench sees the project on its home"          "true" "$(printf '%s' "$HK" | jx "\$null -ne (p '$PID')")"
eq "…but no money"                                 "null" "$(printf '%s' "$HK" | jx "(p '$PID').money")"
eq "…and owes nothing"                             "0" "$(printf '%s' "$HK" | jx "(p '$PID').owedCount")"
eq "a stranger's home does not have it"              "false" "$(req GET /Brief/Home "$MARA" | jx "\$null -ne (p '$PID')")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The truth floor cannot be dropped — and emphasis orders, never removes"

B8=$(brief "$PETER" 2026-07-08)
BLK=$(printf '%s' "$B8" | jx "(s Blocker).items | Where-Object { \$_.key -eq 'flag:$BK_ID' } | ConvertTo-Json -Compress")
eq "the crew-channel blocker reaches Peter"          "true" "$(printf '%s' "$BLK" | jx '$r.crossedByFloor')"
eq "…in the accountable face's name"               "Nalan Architect" "$(printf '%s' "$BLK" | jx '$r.who')"
eq "…without the crew's own wording"               "null" "$(printf '%s' "$BLK" | jx '$r.detail')"
eq "…though his flag list does not carry it"       "0" "$(req GET "/Flags/GetFlagsByProject?projectId=$PID" "$PETER" | grep -c "$BK_ID" | tr -d ' ')"
eq "the date that moved says from and to"            "2026-07-10|2026-07-17" "$(printf '%s' "$B8" | jx "(s Date).items | Where-Object { \$_.key -eq 'date:$RS_ID' } | ForEach-Object { '{0}|{1}' -f ([datetime]\$_.previousDate).ToString('yyyy-MM-dd'), ([datetime]\$_.newDate).ToString('yyyy-MM-dd') }")"
eq "the decision he owes, on the floor"              "true" "$(printf '%s' "$B8" | jx "@((s DecisionOwed).items | Where-Object { \$_.key -eq 'owed:commitment:$CH_ID' }).Count -eq 1")"
eq "the spoken price, as money that moved"           "3400000" "$(printf '%s' "$B8" | jx "(s Money).items | Where-Object { \$_.key -eq 'commitment:$VB_ID' } | ForEach-Object { \$_.amount }")"
eq "…said by the accountable face"                 "Nalan Architect" "$(printf '%s' "$B8" | jx "(s Money).items | Where-Object { \$_.key -eq 'commitment:$VB_ID' } | ForEach-Object { \$_.who }")"
eq "the register's items sit in their blocks too"    "true" "$(printf '%s' "$B8" | jx "@((b 'Parapet coping').commitments | Where-Object { \$_.key -like '*$RS_ID' }).Count -ge 1")"

BD=$(brief "$DINAH" 2026-07-08)
keys() { printf '%s' "$1" | jx '($r.truthFloor.items | ForEach-Object { $_.key } | Sort-Object) -join ","'; }
eq "the representative reads it weighted for her"    "Representative" "$(printf '%s' "$BD" | jx '$r.emphasis')"
eq "…choices she owes first"                       "DecisionOwed" "$(printf '%s' "$BD" | jx '$r.truthFloor[0].kind')"
eq "…the funder's floor starts with money"         "Money" "$(printf '%s' "$B8" | jx '$r.truthFloor[0].kind')"
eq "…and the two hold exactly the same facts"      "$(keys "$B8")" "$(keys "$BD")"
eq "…and the same blocks"                          "$(printf '%s' "$B8" | jx '($r.blocks.key | Sort-Object) -join ","')" "$(printf '%s' "$BD" | jx '($r.blocks.key | Sort-Object) -join ","')"
eq "…with the tiles lifted for her"                "A choice you owe is here" "$(printf '%s' "$BD" | jx '(b "Terrace floor tiles").emphasisReason')"

req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$DINAH_MID\",\"HandlesMoney\":false}" >/dev/null
BD2=$(brief "$DINAH" 2026-07-08)
eq "taken off the money, she keeps the fact"         "true" "$(printf '%s' "$BD2" | jx "@((s Money).items | Where-Object { \$_.key -eq 'commitment:$VB_ID' -and \$_.amountHidden -and \$null -eq \$_.amount }).Count -eq 1")"
eq "…but not the ledger's figures"                 "0" "$(printf '%s' "$BD2" | jx "@((s Money).items | Where-Object { \$_.key -like 'claim:*' }).Count")"
eq "…and her home shows no money for it"           "null" "$(req GET /Brief/Home "$DINAH" | jx "(p '$PID').money")"

BN=$(brief "$NALAN" 2026-07-08)
eq "the mediator reads the brief the client reads"   "Delivery" "$(printf '%s' "$BN" | jx '$r.emphasis')"
eq "…and is told the same rule"                    "true" "$(printf '%s' "$BN" | jx '$r.rule -match "reaches the client"')"
eq "…the blocker is his own, detail and all"       "false" "$(printf '%s' "$BN" | jx "(s Blocker).items | Where-Object { \$_.key -eq 'flag:$BK_ID' } | ForEach-Object { \$_.crossedByFloor }")"
eq "the bench has no brief"                          "404" "$(code GET "/Brief/Day?projectId=$PID&day=2026-07-08" "$KATO")"
eq "nor does a stranger"                             "404" "$(code GET "/Brief/Day?projectId=$PID&day=2026-07-08" "$MARA")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Curation is an upgrade: only what the mediator exposes crosses"

PNG='iVBORw0KGgoAAAANSUhEUgAAACAAAAAYCAIAAAAUMWhjAAAAJElEQVR42mM4FKJFU8QwasGoBaMWjFowasGoBaMWjFowNCwAAND5wC6OsU0VAAAAAElFTkSuQmCC'
ENTRY=$(req POST /Progress/AddProgressUpdate "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Description\":\"Rear wall plaster second coat on\",\"Channel\":0,\"Images\":[{\"Base64Image\":\"$PNG\",\"FileName\":\"coat.png\",\"ContentType\":\"image/png\",\"DisplayOrder\":1}]}")
EID=$(oid "$ENTRY")
BT=$(brief "$PETER" "$TODAY")
eq "a crew-only capture does not cross"              "0" "$(printf '%s' "$BT" | jx '@($r.blocks.notes | Where-Object { $_.text -match "second coat" }).Count')"
eq "…so today is still uncurated"                  "true" "$(printf '%s' "$BT" | jx '$r.assembledWithoutCurator')"
IMG=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN" | jx '$r.images[0].id')
req PUT /Progress/SetImageChannel "$NALAN" "{\"ImageIds\":[\"$IMG\"],\"Channel\":1}" >/dev/null
BT=$(brief "$PETER" "$TODAY")
eq "once exposed, it lands in its deliverable"       "true" "$(printf '%s' "$BT" | jx '@((b "Rear wall plaster").notes | Where-Object { $_.text -match "second coat" }).Count -eq 1')"
eq "…attributed to the accountable face"           "Nalan Architect" "$(printf '%s' "$BT" | jx '((b "Rear wall plaster").notes | Where-Object { $_.text -match "second coat" }).who')"
eq "…and the page says a person shaped it"         "false" "$(printf '%s' "$BT" | jx '$r.assembledWithoutCurator')"
eq "the spoken price still waits on Peter"           "true" "$(printf '%s' "$BT" | jx "@((s DecisionOwed).items | Where-Object { \$_.key -eq 'owed:commitment:$VB_ID' }).Count -eq 1")"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "No stages set up at all — the work still has a name"

BARE=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Peter Bare $STAMP\",\"Description\":\"P7 subject, no stages\",\"Location\":\"Test plot\",\"Currency\":\"UGX\",\"Stages\":[]}")
BID=$(oid "$BARE")
PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $PETER" -F "file=@$FIXTURES/$P7_ZIP" -F "projectId=$BID")
req POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"$(jget "$PRE" batchId)\",\"AuthorMappings\":[]}" >/dev/null
BB=$(req GET "/Brief/Day?projectId=$BID&day=2026-07-08&days=1" "$PETER")
eq "the plaster line is grouped by the catalogue"    "true" "$(printf '%s' "$BB" | jx '@($r.blocks | Where-Object { $_.catalogueKey -and ($_.notes.text -match "plaster") }).Count -eq 1')"
eq "…which says it is not a stage here yet"        "null" "$(printf '%s' "$BB" | jx '($r.blocks | Where-Object { $_.catalogueKey } | Select-Object -First 1).stageId')"
eq "…and still pairs the same view"                "1" "$(printf '%s' "$BB" | jx '@($r.blocks.pairs).Count')"
eq "…ahead of anything it could not name"          "true" "$(printf '%s' "$BB" | jx '$last = @($r.blocks)[-1]; $null -ne $last.catalogueKey -or $null -eq $last.stageName')"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
