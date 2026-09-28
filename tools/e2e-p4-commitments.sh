#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN P4 — the commitment model and the money ledger (assetlen.md §3, §6).
#
# Three questions, all from the vision rather than from the code:
#
#   Can Peter hold a past commitment against present reality? The sixteen
#   retaining-wall items from whatsapp-evidence.md §7 are on the register, each
#   with who agreed, when, where it was said, and the accountable face — and
#   the bench that was brought on for one job cannot read any of it.
#
#   Does a spoken agreement ever silently become truth? A decision logged from
#   a call lands at Agreed, awaits the other side, and "that's not what we
#   said" flips it to a query whose answer lands on the item, never in a
#   message. The old figure survives beside the new one.
#
#   Can two people reconcile a stage without a 9 AM meeting? Per stage:
#   funded → claimed → cleared → carried forward, with every extra on the
#   variation register, costed or visibly not.
#
# The fixture checks read the seeded demo project. Everything that writes runs
# on a throwaway project that is binned and emptied at the end, so the page
# Peter is shown for §11 test 1 stays exactly the sixteen.
#
# Usage:  bash tools/e2e-p4-commitments.sh [api-base]
# Needs:  the API running in Development (POST /api/Dev/SeedDemo is called here).
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"

PROJ=de300000-0000-4000-8000-000000000010
WALL=de300000-0000-4000-8000-000000000104
SUBSTRUCTURE=de300000-0000-4000-8000-000000000102
SUPERSTRUCTURE=de300000-0000-4000-8000-000000000103
C1=de300000-0000-4000-8000-000000000901
C7=de300000-0000-4000-8000-000000000907
C14=de300000-0000-4000-8000-000000000914
C18=de300000-0000-4000-8000-000000000918
BLOCKER=de300000-0000-4000-8000-000000000507
DEMO_PASS='Assetlen#2026'
STAMP=$(date +%H%M%S)

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 40)

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s want=%s got=%s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }
ne()   { if [ -n "$2" ] && [ "$2" != "$3" ]; then ok "$1" "$2"; else bad "$1" "not '$3'" "$2"; fi; }
head_() { printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1@assetlen.dev\",\"Password\":\"$DEMO_PASS\"}" \
    | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
}

req() {
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3" -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

code() {
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3" \
      -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

# First value of a JSON field, with decimal(18,4) zeros trimmed.
f() {
  printf '%s' "$1" | grep -o "\"$2\":[^,}]*" | head -1 | cut -d: -f2- | tr -d '"' \
    | sed -E 's/^(-?[0-9]+)\.0+$/\1/'
}
count() { printf '%s' "$1" | grep -o "$2" | wc -l | tr -d ' '; }

# One object out of a flat JSON array, by a field it contains.
pick() { printf '%s' "$1" | sed 's/},{/}\n{/g' | grep -- "$2" | head -1; }
day()  { printf '%s' "$1" | cut -c1-10; }

echo "ASSETLEN P4 — commitments and the money ledger — $API"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"

PETER=$(tok peter); NALAN=$(tok nalan); DINAH=$(tok dinah); MUSA=$(tok musa); GRACE=$(tok grace)
if [ -z "$PETER" ] || [ -z "$NALAN" ] || [ -z "$DINAH" ] || [ -z "$MUSA" ]; then
  echo "FATAL: demo personas could not sign in. Is the API up in Development?"
  exit 1
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "§11 test 1 — the sixteen retaining-wall commitments are on the register"

REG=$(req GET "/Commitments/GetCommitments?projectId=$PROJ&stageId=$WALL" "$PETER")
eq "fifteen commitments on the retaining wall"        "$(count "$REG" '"kind":')"                        "15"
eq "…every one under the accountable face"          "$(count "$REG" '"accountableName":"Nalan Kaggwa"')" "15"
eq "…every one filed from Peter's own import"       "$(count "$REG" '"sourceChannel":"Ingested"')"      "15"

BLK=$(req GET "/Flags/GetFlag?flagId=$BLOCKER" "$PETER")
eq "the sixteenth is a blocker"                       "$(f "$BLK" isBlocker)"                        "true"
eq "…owned by whoever has to move"                  "$(f "$BLK" ownerName)"                        "Nalan Kaggwa"

HIST=$(req GET "/Commitments/GetCommitments?projectId=$PROJ&stageId=$WALL&includeSuperseded=true" "$PETER")
eq "the stone-pitching statement it replaced is kept" "$(count "$HIST" '"kind":')"                       "16"

ONE=$(req GET "/Commitments/GetCommitment?commitmentId=$C1" "$PETER")
eq "the wall spec shows it was restated"              "$(f "$ONE" restatementCount)"                 "1"
eq "…and carries its variation"                     "$(f "$ONE" variationStatus)"                  "Approved"

SEVEN=$(req GET "/Commitments/GetCommitment?commitmentId=$C7" "$PETER")
eq "the disputed labour price is under query"         "$(f "$SEVEN" queryState)"                     "QueryRaised"
ne "…and the query is one tap away"                 "$(f "$SEVEN" openQueryFlagId)"                "null"

OWED=$(req GET "/Commitments/GetCommitment?commitmentId=$C14" "$PETER")
eq "the electrical-points choice is Peter's to make"  "$(f "$OWED" owedBySide)"                      "Client"
eq "…and still in discussion"                       "$(f "$OWED" maturity)"                        "InDiscussion"

SPOKEN=$(req GET "/Commitments/GetCommitment?commitmentId=$C18" "$PETER")
eq "a decision from site waits on Peter"              "$(f "$SPOKEN" canConfirm)"                    "true"
eq "…agreed with a party who has no login"          "$(f "$SPOKEN" agreedWithName)"                "Sunrise Aluminium Ltd"

eq "the foreman is not shown the register"            "$(code GET "/Commitments/GetCommitments?projectId=$PROJ" "$MUSA")"  "404"
eq "nor is the photographer"                          "$(code GET "/Commitments/GetCommitments?projectId=$PROJ" "$GRACE")" "404"
eq "the representative reads it"                      "$(code GET "/Commitments/GetCommitments?projectId=$PROJ" "$DINAH")" "200"
eq "the bench still reads the checklist it works to"  "$(code GET "/Commitments/GetDeliverables?projectId=$PROJ&stageId=$WALL" "$MUSA")" "200"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The seeded ledger reads stage by stage, and carries a balance forward"

LEDGER=$(req GET "/Ledger/GetStageLedger?projectId=$PROJ" "$PETER")
ROW=$(pick "$LEDGER" "\"stageId\":\"$WALL\"")
# Whether the second release has been acknowledged depends on who has clicked
# through the demo since it was seeded, so the two columns are read together.
eq "retaining wall: released, acknowledged or not"   "$(( $(f "$ROW" funded) + $(f "$ROW" pendingFunding) ))" "62000000"
eq "retaining wall: claimed"                          "$(f "$ROW" claimed)"                          "58000000"
eq "…cleared"                                       "$(f "$ROW" cleared)"                          "38000000"
eq "…a claim awaits the funder"                     "$(f "$ROW" awaitingClearance)"                "20000000"
eq "…an open stage carries nothing forward"         "$(f "$ROW" carriedForward)"                   "null"

SUB=$(pick "$LEDGER" "\"stageId\":\"$SUBSTRUCTURE\"")
SUP=$(pick "$LEDGER" "\"stageId\":\"$SUPERSTRUCTURE\"")
eq "substructure closed with a balance"               "$(f "$SUB" carriedForward)"                   "1500000"
eq "…and the next stage received it"                "$(f "$SUP" carriedIn)"                        "1500000"
eq "the added floor and the deleted balconies net out" "$(f "$SUP" variationsApproved)"              "70000000"
eq "the ledger is not the foreman's"                  "$(code GET "/Ledger/GetStageLedger?projectId=$PROJ" "$MUSA")" "404"

# ═════════════════════════════════════════════════════════════════════════════
# A throwaway engagement for everything that writes.
# ═════════════════════════════════════════════════════════════════════════════
head_ "A fresh engagement: Peter owns it, Nalan mediates, Nalan staffs the bench"

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"P4 register $STAMP\",\"Description\":\"E2E subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Retaining wall\",\"BudgetAmount\":74000000,\"DisplayOrder\":1,\"StartDate\":\"2026-06-04T00:00:00Z\",\"ExpectedEndDate\":\"2026-08-29T00:00:00Z\"},{\"StageName\":\"Roofing\",\"BudgetAmount\":96000000,\"DisplayOrder\":2}]}")
PID=$(printf '%s' "$CREATED" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//')
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter opens a project" "$PID"

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
S1=$(pick "$STAGES" '"stageName":"Retaining wall"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
S2=$(pick "$STAGES" '"stageName":"Roofing"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

ADD_N=$(req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan@assetlen.dev\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}")
NALAN_MID=$(printf '%s' "$ADD_N" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null

# He also runs the programme, as on the real job — the manager seat is what
# lets him acknowledge a release and move a stage's forecast.
NALAN_UID=$(f "$ADD_N" userId)
req PUT "/ProjectsRS/AssignProjectManager?projectId=$PID&managerId=$NALAN_UID" "$PETER" >/dev/null

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
PETER_MID=$(pick "$ROSTER" '"userEmail":"peter@assetlen.dev"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null

# Dinah is a principal on this job but deliberately off the money — reading
# the register must not become a way to read the budget.
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah@assetlen.dev\",\"Specialization\":9,\"Side\":0,\"HandlesMoney\":false}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"musa@assetlen.dev\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null

STAND=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$NALAN")
eq "Nalan is the accountable face"                    "$(f "$STAND" isMediator)"                      "true"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The plan as first agreed is kept when the dates move"

ST=$(req GET "/Stages/GetStageById?stageId=$S1" "$PETER")
eq "a stage created with dates takes them as baseline" "$(day "$(f "$ST" baselineEndDate)")"          "2026-08-29"

MOVED=$(printf '%s' "$ST" | sed 's/"expectedEndDate":"[^"]*"/"expectedEndDate":"2026-09-30T00:00:00Z"/')
UPD=$(req PUT /Stages/UpdateStage "$NALAN" "$MOVED")
eq "the forecast moves"                               "$(day "$(f "$UPD" expectedEndDate)")"           "2026-09-30"
eq "…the baseline does not"                         "$(day "$(f "$UPD" baselineEndDate)")"           "2026-08-29"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Five to eight deliverables per stage — the checklist capture aims at"

D1=$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Title\":\"Materials specified and on site\"}")
D2=$(req POST /Commitments/AddDeliverable "$DINAH" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Title\":\"First half cast\"}")
D1ID=$(f "$D1" id); D2ID=$(f "$D2" id)
eq "the developer adds a line"                        "$(f "$D1" displayOrder)"                       "1"
eq "the representative adds the next"                 "$(f "$D2" displayOrder)"                       "2"
eq "the foreman cannot write the checklist"           "$(code POST /Commitments/AddDeliverable "$MUSA" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Title\":\"x\"}")" "404"
DONE=$(req PUT /Commitments/UpdateDeliverable "$NALAN" "{\"Id\":\"$D2ID\",\"Status\":\"Done\"}")
eq "a line is ticked off"                             "$(f "$DONE" status)"                           "Done"
ne "…and says when"                                 "$(f "$DONE" completedAt)"                      "null"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A commitment carries who agreed, when, and where it was said"

M=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"DeliverableId\":\"$D1ID\",\"Kind\":\"Material\",\"Title\":\"150 bags CEM II for the excavated section\"}")
MID=$(f "$M" id)
eq "it lands at Agreed"                               "$(f "$M" maturity)"                            "Agreed"
eq "…recorded in the app"                           "$(f "$M" sourceChannel)"                       "App"
eq "…on its deliverable's stage"                    "$(f "$M" stageName)"                           "Retaining wall"
eq "…under the mediator's name"                     "$(f "$M" accountableName)"                     "Nalan Kaggwa"

LOOSE=$(req POST /Commitments/AddCommitment "$PETER" "{\"ProjectId\":\"$PID\",\"Kind\":\"Spec\",\"Title\":\"Weep holes every two metres\"}")
ne "nothing floats: no stage named, one is filled in" "$(f "$LOOSE" stageId)"                       "null"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A spoken decision never silently becomes truth"

V=$(req POST /Commitments/LogDecision "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Kind\":\"Price\",\"Title\":\"Labour: formwork, earthworks and concrete\",\"Amount\":10500000}")
VID=$(f "$V" id)
eq "one tap from a call lands at Agreed"              "$(f "$V" maturity)"                            "Agreed"
eq "…marked as spoken"                              "$(f "$V" sourceChannel)"                       "Verbal"
eq "…attributed to both parties"                    "$(f "$V" agreedWithName)"                      "Peter Ssembatya"
eq "…and visibly unconfirmed"                       "$(f "$V" isAwaitingCounterparty)"              "true"
eq "whoever wrote it down cannot confirm it"          "$(f "$V" canConfirm)"                          "false"
eq "…and the server agrees"                         "$(code PUT "/Commitments/Confirm?commitmentId=$VID" "$NALAN")" "403"
eq "the bench cannot answer for the developer"        "$(code PUT "/Commitments/Confirm?commitmentId=$VID" "$MUSA")"  "404"

PV=$(req GET "/Commitments/GetCommitment?commitmentId=$VID" "$PETER")
eq "the other side is offered Confirm"                "$(f "$PV" canConfirm)"                         "true"

DIS=$(req PUT /Commitments/Dispute "$PETER" "{\"CommitmentId\":\"$VID\",\"Note\":\"We said 10M on the call, not 10.5M.\"}")
eq "\"That's not what we said\" raises a query"       "$(f "$DIS" queryState)"                        "QueryRaised"
eq "…attributed to whoever disputed it"             "$(f "$DIS" disputedByName)"                    "Peter Ssembatya"
QFLAG=$(f "$DIS" openQueryFlagId)
QF=$(req GET "/Flags/GetFlag?flagId=$QFLAG" "$NALAN")
eq "…and lands on the recorder's list"              "$(f "$QF" assignedToName)"                     "Nalan Kaggwa"
eq "…as a query on the item, not a blocker"         "$(f "$QF" isBlocker)"                          "false"
eq "it cannot be disputed twice"                      "$(code PUT /Commitments/Dispute "$PETER" "{\"CommitmentId\":\"$VID\",\"Note\":\"again\"}")" "403"

RES=$(req PUT /Commitments/ResolveQuery "$NALAN" \
  "{\"CommitmentId\":\"$VID\",\"Note\":\"Agreed at 10M on the second call, 12 Jun.\",\"Amount\":10000000}")
NEWID=$(f "$RES" id)
ne "resolution writes a new statement"                "$NEWID"                                        "$VID"
eq "…that supersedes the questioned one"            "$(f "$RES" supersedesId)"                      "$VID"
eq "…with the agreed figure"                        "$(f "$RES" amount)"                            "10000000"
eq "…and the answer on the item"                    "$(f "$RES" resolutionNote)"                    "Agreed at 10M on the second call"
eq "the question itself is closed"                    "$(f "$(req GET "/Flags/GetFlag?flagId=$QFLAG" "$PETER")" status)" "Resolved"
OLD=$(req GET "/Commitments/GetCommitment?commitmentId=$VID" "$PETER")
eq "the old figure is still on record"                "$(f "$OLD" amount)"                            "10500000"
eq "…marked superseded, not deleted"                "$(f "$OLD" supersededById)"                    "$NEWID"
eq "the history reads as two statements"              "$(count "$(req GET "/Commitments/GetChain?commitmentId=$NEWID" "$PETER")" '"kind":')" "2"

PD=$(req POST /Commitments/LogDecision "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Kind\":\"Spec\",\"Title\":\"Conduits on the interior face, before the second half\",\"SourceChannel\":\"Meeting\"}")
PDID=$(f "$PD" id)
eq "a decision from a meeting keeps its source"       "$(f "$PD" sourceChannel)"                      "Meeting"
eq "…and is owed to the accountable face"           "$(f "$PD" agreedWithName)"                     "Nalan Kaggwa"
eq "Peter cannot confirm his own note"                "$(code PUT "/Commitments/Confirm?commitmentId=$PDID" "$PETER")" "403"
CF=$(req PUT "/Commitments/Confirm?commitmentId=$PDID" "$NALAN")
ne "the other side confirms it"                       "$(f "$CF" counterpartyConfirmedAt)"             "null"
eq "…and it stops waiting"                          "$(f "$CF" isAwaitingCounterparty)"             "false"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Dates are restated, never overwritten"

DT=$(req POST /Commitments/AddCommitment "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Kind\":\"Date\",\"Title\":\"First half ready\",\"DueDate\":\"2026-06-12T00:00:00Z\"}")
DTID=$(f "$DT" id)
eq "a date passed with the work undelivered is overdue" "$(f "$DT" isOverdue)"                        "true"
RS=$(req POST /Commitments/Restate "$NALAN" "{\"CommitmentId\":\"$DTID\",\"DueDate\":\"2026-06-19T00:00:00Z\"}")
eq "a restated date counts its restatements"          "$(f "$RS" restatementCount)"                   "1"
eq "…and keeps the new promise"                     "$(day "$(f "$RS" dueDate)")"                   "2026-06-19"
HEADS=$(req GET "/Commitments/GetCommitments?projectId=$PID" "$PETER")
ALL=$(req GET "/Commitments/GetCommitments?projectId=$PID&includeSuperseded=true" "$PETER")
eq "the register shows the current promise once"      "$(count "$HEADS" '"title":"First half ready"')"  "1"
eq "…and the history keeps both"                    "$(count "$ALL" '"title":"First half ready"')"    "2"
eq "a superseded statement cannot be restated again"  "$(code POST /Commitments/Restate "$NALAN" "{\"CommitmentId\":\"$DTID\"}")" "409"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Idea → In discussion → Agreed → Delivered → Verified"

eq "delivered by the delivery side"                   "$(f "$(req PUT /Commitments/SetMaturity "$NALAN" "{\"CommitmentId\":\"$MID\",\"Maturity\":\"Delivered\"}")" maturity)" "Delivered"
eq "a commitment does not move backwards"             "$(code PUT /Commitments/SetMaturity "$NALAN" "{\"CommitmentId\":\"$MID\",\"Maturity\":\"Agreed\"}")" "400"
eq "the bench cannot verify"                          "$(code PUT /Commitments/SetMaturity "$MUSA" "{\"CommitmentId\":\"$MID\",\"Maturity\":\"Verified\"}")" "404"
VF=$(req PUT /Commitments/SetMaturity "$DINAH" "{\"CommitmentId\":\"$MID\",\"Maturity\":\"Verified\"}")
eq "the client side verifies against reality"         "$(f "$VF" maturity)"                           "Verified"
ne "…and it says when"                              "$(f "$VF" verifiedAt)"                         "null"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A flag is a query on a commitment, or a blocker — nothing else"

QR=$(req PUT /Commitments/RaiseQuery "$DINAH" "{\"CommitmentId\":\"$MID\",\"Note\":\"Were all 150 bags CEM II? Two looked different.\"}")
eq "a query moves the item to Query raised"           "$(f "$QR" queryState)"                         "QueryRaised"
QFL=$(f "$QR" openQueryFlagId)
eq "…and is owed by the accountable face"           "$(f "$(req GET "/Flags/GetFlag?flagId=$QFL" "$DINAH")" assignedToName)" "Nalan Kaggwa"
req PUT "/Flags/ResolveFlag?flagId=$QFL" "$NALAN" >/dev/null
eq "closing the question resolves the item"           "$(f "$(req GET "/Commitments/GetCommitment?commitmentId=$MID" "$PETER")" queryState)" "Resolved"

LOOSE_ID=$(f "$LOOSE" id)
FQ=$(req POST /Flags/AddFlag "$PETER" "{\"ProjectId\":\"$PID\",\"Title\":\"Weep holes: every two metres or three?\",\"CommitmentId\":\"$LOOSE_ID\"}")
eq "a flag raised on a commitment is that query"      "$(f "$FQ" commitmentId)"                       "$LOOSE_ID"
eq "…and the item moves with it"                    "$(f "$(req GET "/Commitments/GetCommitment?commitmentId=$LOOSE_ID" "$PETER")" queryState)" "QueryRaised"

BK=$(req POST /Flags/AddFlag "$NALAN" "{\"ProjectId\":\"$PID\",\"Title\":\"Epoxy team did not turn up\",\"OwnerPartyName\":\"the epoxy team\",\"Channel\":\"Client\"}")
eq "a blocker names who has to move"                  "$(f "$BK" ownerName)"                          "the epoxy team"
eq "…and is not a query"                            "$(f "$BK" isBlocker)"                          "true"
eq "a flag cannot question another project's item"    "$(code POST /Flags/AddFlag "$PETER" "{\"ProjectId\":\"$PID\",\"Title\":\"x\",\"CommitmentId\":\"$C7\"}")" "400"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Backlinks run both ways"

RSID=$(f "$RS" id)
LK=$(req POST /Commitments/AddLink "$NALAN" \
  "{\"CommitmentId\":\"$MID\",\"TargetType\":\"Commitment\",\"TargetId\":\"$RSID\",\"Relation\":\"Evidence\"}")
eq "a delivery points at the promise it keeps"        "$(f "$LK" targetLabel)"                        "First half ready"
eq "…and the promise lists it back"                 "$(count "$(req GET "/Commitments/GetLinks?commitmentId=$RSID" "$PETER")" "\"targetId\":\"$MID\"")" "1"
eq "…as does the backlink query"                    "$(count "$(req GET "/Commitments/GetBacklinks?projectId=$PID&targetType=Commitment&targetId=$RSID" "$PETER")" "\"commitmentId\":\"$MID\"")" "1"
eq "linking the same thing twice is one link"         "$(f "$(req POST /Commitments/AddLink "$NALAN" "{\"CommitmentId\":\"$MID\",\"TargetType\":\"Commitment\",\"TargetId\":\"$RSID\",\"Relation\":\"Evidence\"}")" id)" "$(f "$LK" id)"
eq "a link cannot reach off the project"              "$(code POST /Commitments/AddLink "$NALAN" "{\"CommitmentId\":\"$MID\",\"TargetType\":\"Commitment\",\"TargetId\":\"$C7\"}")" "404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Reading the register is not reading the money"

DV=$(req GET "/Commitments/GetCommitment?commitmentId=$NEWID" "$DINAH")
eq "a principal off the money sees no figure"         "$(f "$DV" amount)"                             "null"
eq "…and is told one exists"                        "$(f "$DV" amountHidden)"                       "true"
eq "…and the ledger is absent for her"              "$(code GET "/Ledger/GetStageLedger?projectId=$PID" "$DINAH")" "404"

eq "only the funder clears a priced item"             "$(code PUT "/Commitments/Clear?commitmentId=$NEWID" "$NALAN")" "403"
eq "the funder clears it"                             "$(f "$(req PUT "/Commitments/Clear?commitmentId=$NEWID" "$PETER")" queryState)" "Cleared"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Funded → claimed → cleared → carried forward, one row per stage"
# No release on this throwaway: a second project is not on the free tier, so
# funding is refused until it is subscribed. The funded column is proved on the
# seeded project above; here the ledger has to tell the truth about a stage
# that has been cleared beyond what was ever released.

CL=$(req POST /Ledger/AddClaim "$NALAN" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Amount\":30000000,\"Note\":\"First half cast\"}")
CLID=$(f "$CL" id)
eq "the contractor claims against the stage"          "$(f "$CL" status)"                             "Claimed"
eq "…and cannot clear his own claim"                "$(code PUT /Ledger/DecideClaim "$NALAN" "{\"ClaimId\":\"$CLID\",\"Clear\":true}")" "403"
eq "the bench cannot claim"                           "$(code POST /Ledger/AddClaim "$MUSA" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Amount\":1}")" "404"
CC=$(req PUT /Ledger/DecideClaim "$PETER" "{\"ClaimId\":\"$CLID\",\"Clear\":true,\"ClearedAmount\":29000000}")
eq "the funder clears it at a different figure"       "$(f "$CC" settledAmount)"                      "29000000"

Q2=$(req POST /Ledger/AddClaim "$NALAN" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S1\",\"Amount\":5000000,\"Note\":\"Transport\"}")
Q2ID=$(f "$Q2" id)
eq "a claim can be queried"                           "$(f "$(req PUT /Ledger/DecideClaim "$PETER" "{\"ClaimId\":\"$Q2ID\",\"Clear\":false,\"Note\":\"Transport was in the rate\"}")" status)" "Queried"
eq "…and withdrawn by whoever made it"              "$(f "$(req PUT "/Ledger/WithdrawClaim?claimId=$Q2ID" "$NALAN")" status)" "Withdrawn"

L1=$(pick "$(req GET "/Ledger/GetStageLedger?projectId=$PID" "$PETER")" "\"stageId\":\"$S1\"")
eq "funded (nothing released on this one)"          "$(f "$L1" funded)"                             "0"
eq "claimed (a withdrawn claim is not)"               "$(f "$L1" claimed)"                            "30000000"
eq "cleared"                                          "$(f "$L1" cleared)"                            "29000000"
eq "cleared beyond funding reads as owed, not hidden" "$(f "$L1" inHand)"                            "-29000000"
eq "an open stage carries nothing forward yet"        "$(f "$L1" carriedForward)"                     "null"

DONE_ST=$(printf '%s' "$(req GET "/Stages/GetStageById?stageId=$S1" "$PETER")" | sed 's/"completionPercentage":[^,]*/"completionPercentage":100/')
req PUT /Stages/UpdateStage "$PETER" "$DONE_ST" >/dev/null
LEDG=$(req GET "/Ledger/GetStageLedger?projectId=$PID" "$PETER")
eq "a closed stage carries its balance forward"       "$(f "$(pick "$LEDG" "\"stageId\":\"$S1\"")" carriedForward)" "-29000000"
eq "…into the next stage"                           "$(f "$(pick "$LEDG" "\"stageId\":\"$S2\"")" carriedIn)"      "-29000000"
eq "the totals conserve money"                        "$(f "$LEDG" totalInHand)"                      "-29000000"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Every extra is on the variation register — costed, or visibly not"

VA=$(req POST /Ledger/AddVariation "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S2\",\"Title\":\"Parapet raised a course, plus coping\",\"Reason\":\"Queried on site; answered with a marked-up photo\"}")
VAID=$(f "$VA" id)
eq "an extra is proposed"                             "$(f "$VA" status)"                             "Proposed"
eq "…uncosted is a gap, not a zero"                 "$(f "$VA" costDelta)"                          "null"
VAC=$(f "$VA" commitmentId)
eq "…and sits on the register as an item"           "$(f "$(req GET "/Commitments/GetCommitment?commitmentId=$VAC" "$PETER")" variationStatus)" "Proposed"
eq "the side proposing it never approves it"          "$(code PUT /Ledger/DecideVariation "$NALAN" "{\"VariationId\":\"$VAID\",\"Approve\":true}")" "403"
AP=$(req PUT /Ledger/DecideVariation "$PETER" "{\"VariationId\":\"$VAID\",\"Approve\":true,\"CostDelta\":2400000}")
eq "the funder approves it with a figure"             "$(f "$AP" status)"                             "Approved"
VCM=$(req GET "/Commitments/GetCommitment?commitmentId=$VAC" "$PETER")
eq "…and the commitment agrees"                     "$(f "$VCM" maturity)"                          "Agreed"
eq "…at the approved figure"                        "$(f "$VCM" amount)"                            "2400000"
req POST /Ledger/AddVariation "$NALAN" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S2\",\"Title\":\"Terrazzo to the laundry\"}" >/dev/null
L2=$(pick "$(req GET "/Ledger/GetStageLedger?projectId=$PID" "$PETER")" "\"stageId\":\"$S2\"")
eq "the stage shows the approved extra"               "$(f "$L2" variationsApproved)"                 "2400000"
eq "…and counts the one nobody has priced"          "$(f "$L2" variationsUncosted)"                 "1"
eq "the variation register is not the bench's"        "$(code GET "/Ledger/GetVariations?projectId=$PID" "$MUSA")" "404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Accountability is a query, not a feature"

ACC=$(req GET "/Commitments/GetAccountability?projectId=$PID" "$PETER")
HEADS=$(req GET "/Commitments/GetCommitments?projectId=$PID" "$PETER")
eq "every current commitment is under one name"       "$(f "$ACC" total)"                             "$(count "$HEADS" '"kind":')"
eq "…the mediator's"                                "$(f "$ACC" name)"                              "Nalan Kaggwa"
eq "blockers are grouped by who has to move"          "$(count "$ACC" '"ownerName":"the epoxy team"')" "2"
eq "the bench has no accountability view"             "$(code GET "/Commitments/GetAccountability?projectId=$PID" "$MUSA")" "404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Clearing up after itself"

eq "the throwaway project is binned"                  "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$PID" "$PETER")" "200"
eq "…and emptied"                                   "$(code DELETE "/ProjectsRS/DeleteProject?projectId=$PID" "$PETER")" "200"
eq "its register goes with it"                        "$(code GET "/Commitments/GetCommitments?projectId=$PID" "$PETER")" "404"

printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
