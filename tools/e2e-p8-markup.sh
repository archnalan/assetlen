#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN P8 — markup, query state, parked ideas (plan.md P8).
#
# Three questions, from the vision rather than from the code:
#
#   Can Peter circle a line on a receipt, ask, and have the answer change the
#   commitment? (The exit.) The circle is a layer on the original — versioned,
#   attributed, never a new image — and the answer lands on the item as a new
#   statement beside the questioned one, not in a message (assetlen.md §3, Law 2).
#
#   Is "cleared" not closed? A paid item can be queried without it reading as
#   an accusation, and the earlier clearing stays on record.
#
#   Does a parked idea stay silent until waiting costs something? It gathers
#   references and estimates without asking anyone, and speaks only when a lead
#   time, a dependency or its stage starting gives it a deadline (Law 4).
#
# Everything runs on a throwaway project that is binned and emptied at the end.
#
# Usage:  bash tools/e2e-p8-markup.sh [api-base]
# Needs:  the API running in Development, and pwsh for the receipt fixture.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
DEMO_PASS='Assetlen#2026'
STAMP=$(date +%H%M%S)
FIXTURES="tools/fixtures/p8"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 60)

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s want=%s got=%s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }
ne()   { if [ -n "$2" ] && [ "$2" != "$3" ]; then ok "$1" "$2"; else bad "$1" "not '$3'" "$2"; fi; }
has()  { if printf '%s' "$2" | grep -q -- "$3"; then ok "$1" "$3"; else bad "$1" "contains '$3'" "$(printf '%s' "$2" | cut -c1-120)"; fi; }
hasnt(){ if printf '%s' "$2" | grep -q -- "$3"; then bad "$1" "no '$3'" "present"; else ok "$1" "absent"; fi; }
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

f() {
  printf '%s' "$1" | grep -o "\"$2\":[^,}]*" | head -1 | cut -d: -f2- | tr -d '"' \
    | sed -E 's/^(-?[0-9]+)\.0+$/\1/'
}
count() { printf '%s' "$1" | grep -o "$2" | wc -l | tr -d ' '; }
pick()  { printf '%s' "$1" | sed 's/},{/}\n{/g' | grep -- "$2" | head -1; }
idof()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
day()   { printf '%s' "$1" | cut -c1-10; }
iso()   { date -u -d "$1" +%Y-%m-%dT00:00:00Z; }

# A layer carries nested shapes, so flat slicing cannot find its later fields —
# read the annotated-artifact answer as JSON. $r is the parsed body.
jx() {
  printf '%s' "$1" | pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    function L(\$id) { @(\$r.layers) | Where-Object { \$_.layerId -eq \$id } | Select-Object -First 1 }
    \$o = & { $2 }
    if (\$o -is [bool]) { \$o.ToString().ToLower() } elseif (\$null -eq \$o) { 'null' } else { \$o }"
}
ymd()   { date -u -d "$1" +%Y-%m-%d; }
# Owed is across every project the reader stands on; these checks are about this one.
owed_on() { req GET /Brief/Owed "$1" | sed 's/},{/}\n{/g' | grep -- "\"projectId\":\"$PID\""; }

echo "ASSETLEN P8 — markup, query state, parked ideas — $API"

bash tools/make-markup-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
  || { echo "FATAL: could not build the receipt fixture (needs pwsh)"; exit 1; }
# shellcheck disable=SC1091
. "$FIXTURES/fixtures.env"
RECEIPT="$FIXTURES/$P8_RECEIPT"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"

PETER=$(tok peter); NALAN=$(tok nalan); DINAH=$(tok dinah); MUSA=$(tok musa); GRACE=$(tok grace)
if [ -z "$PETER" ] || [ -z "$NALAN" ] || [ -z "$DINAH" ] || [ -z "$MUSA" ] || [ -z "$GRACE" ]; then
  echo "FATAL: demo personas could not sign in. Is the API up in Development?"
  exit 1
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "A fresh engagement: Peter owns it, Nalan mediates and staffs the bench"

LANDSCAPING=$(iso "+3 days"); DRIVEWAY=$(iso "+10 days"); EXTERNAL=$(iso "+40 days")
CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"P8 markup $STAMP\",\"Description\":\"E2E subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Finishes\",\"DisplayOrder\":1,\"StartDate\":\"$(iso "-20 days")\"},{\"StageName\":\"Landscaping\",\"DisplayOrder\":2,\"StartDate\":\"$LANDSCAPING\"},{\"StageName\":\"Driveway\",\"DisplayOrder\":3,\"StartDate\":\"$DRIVEWAY\"},{\"StageName\":\"External works\",\"DisplayOrder\":4,\"StartDate\":\"$EXTERNAL\"}]}")
PID=$(printf '%s' "$CREATED" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//')
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter opens a project" "$PID"

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
sid() { idof "$(pick "$STAGES" "\"stageName\":\"$1\"")"; }
S_FIN=$(sid Finishes); S_LAND=$(sid Landscaping); S_DRIVE=$(sid Driveway); S_EXT=$(sid "External works")

ADD_N=$(req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan@assetlen.dev\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}")
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(idof "$ADD_N")\",\"IsMediator\":true}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
PETER_MID=$(idof "$(pick "$ROSTER" '"userEmail":"peter@assetlen.dev"')")
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah@assetlen.dev\",\"Specialization\":9,\"Side\":0,\"HandlesMoney\":false}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"musa@assetlen.dev\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null
eq "Nalan is the accountable face" "$(f "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$NALAN")" isMediator)" "true"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Cleared is not closed — a paid item on the register"

CEM=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Kind\":\"Price\",\"Title\":\"Cement for the terrace screed: 36 bags\",\"Amount\":4320000}")
CEM_ID=$(f "$CEM" id)
eq "the cement is on the register at its agreed figure" "$(f "$CEM" amount)" "4320000"
CL=$(req PUT "/Commitments/Clear?commitmentId=$CEM_ID" "$PETER")
eq "Peter clears it — it was paid"                      "$(f "$CL" queryState)" "Cleared"
ne "…and it says when"                                "$(f "$CL" clearedAt)"  "null"

SHARE=$("${CURL[@]}" -X POST "$API/Ingest/CaptureShare" -H "Authorization: Bearer $PETER" \
  -F "projectId=$PID" -F "text=Receipt for the cement" -F "file=@$RECEIPT;type=image/png")
AID=$(f "$SHARE" artifactId)
ne "he forwards the receipt from his phone"             "$AID" "null"
BYTES_BEFORE=$("${CURL[@]}" "$API/Artifacts/$AID/content" -H "Authorization: Bearer $PETER" | sha256sum | cut -c1-64)
REFC_BEFORE=$(f "$(req GET "/Artifacts/Get?artifactId=$AID" "$PETER")" referenceCount)
req POST /Commitments/AddLink "$PETER" \
  "{\"CommitmentId\":\"$CEM_ID\",\"TargetType\":\"Artifact\",\"TargetId\":\"$AID\",\"Relation\":\"Invoice\"}" >/dev/null

ANN0=$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")
eq "the receipt opens with no layers yet"               "$(jx "$ANN0" '@($r.layers).Count')" "0"
eq "…and says what it is the invoice for"             "$(jx "$ANN0" '@($r.linkedCommitments)[0].commitmentId')" "$CEM_ID"
eq "Peter may ask about it"                             "$(f "$ANN0" canAsk)" "true"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Peter circles a line on the receipt and asks"

CIRCLE='{"Kind":"Ellipse","X":0.03,"Y":0.34,"W":0.82,"H":0.16}'
Q="The receipt says 40 bags at 4.8M. We agreed 36 - which is right?"
eq "a circle with no question is refused"               "$(code POST /Annotations/Ask "$PETER" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[$CIRCLE],\"Question\":\"\"}")" "400"
eq "a question with no circle is refused"               "$(code POST /Annotations/Ask "$PETER" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[],\"Question\":\"x\"}")" "400"
eq "a mark off the edge of the file is refused"         "$(code POST /Annotations/Ask "$PETER" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[{\"Kind\":\"Ellipse\",\"X\":0.9,\"Y\":0.3,\"W\":0.5,\"H\":0.1}],\"Question\":\"x\"}")" "400"

ASK=$(req POST /Annotations/Ask "$PETER" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[$CIRCLE],\"Question\":\"$Q\"}")
LAYER=$(f "$ASK" layerId)
ne "the circle is a layer"                              "$LAYER" "null"
eq "…version 1"                                     "$(f "$ASK" version)" "1"
eq "…attributed to Peter"                           "$(f "$ASK" authorName)" "Peter Ssembatya"
eq "…on the client side, because a query crosses"   "$(f "$ASK" channel)" "Client"
eq "…holding the ellipse he drew"                   "$(f "$ASK" kind)" "Ellipse"
eq "the cleared item is now under query"                "$(f "$(printf '%s' "$ASK" | grep -o '"commitment":.*')" queryState)" "QueryRaised"

AFTER=$(req GET "/Commitments/GetCommitment?commitmentId=$CEM_ID" "$PETER")
eq "…and says where the question was asked"         "$(f "$AFTER" markupArtifactId)" "$AID"
eq "…with the question itself"                      "$(f "$AFTER" markupNote)" "$Q"
QFLAG=$(f "$AFTER" openQueryFlagId)
eq "the question is owed by the accountable face"       "$(f "$(req GET "/Flags/GetFlag?flagId=$QFLAG" "$PETER")" assignedToName)" "Nalan Kaggwa"
eq "it cannot be asked twice while open"                "$(code POST /Annotations/Ask "$PETER" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[$CIRCLE],\"Question\":\"again\"}")" "409"

LINKS=$(req GET "/Commitments/GetLinks?commitmentId=$CEM_ID" "$PETER")
has "the markup is a backlink on the item"              "$LINKS" '"targetType":"Annotation"'
has "…labelled with the file and its version"       "$LINKS" 'Marked up: IMG-20260806-WA0007.png (v1)'

BYTES_AFTER=$("${CURL[@]}" "$API/Artifacts/$AID/content" -H "Authorization: Bearer $PETER" | sha256sum | cut -c1-64)
eq "the original's bytes are untouched"                 "$BYTES_AFTER" "$BYTES_BEFORE"
REFC_AFTER=$(f "$(req GET "/Artifacts/Get?artifactId=$AID" "$PETER")" referenceCount)
eq "…and no second image or pointer was made"      "$REFC_AFTER" "$REFC_BEFORE"

OWED_N=$(owed_on "$NALAN")
has "Nalan sees the question on his list"               "$OWED_N" "Cement for the terrace screed"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A layer is versioned, never overwritten"

V2=$(req POST /Annotations/Save "$PETER" "{\"ArtifactId\":\"$AID\",\"LayerId\":\"$LAYER\",\"Note\":\"$Q\",\"Shapes\":[$CIRCLE,{\"Kind\":\"Arrow\",\"X\":0.9,\"Y\":0.2,\"W\":-0.05,\"H\":0.15}]}")
eq "Peter adds an arrow: version 2"                     "$(f "$V2" version)" "2"
eq "…of the same layer"                             "$(f "$V2" layerId)" "$LAYER"
eq "…still asking about the same item"              "$(f "$V2" commitmentId)" "$CEM_ID"
eq "somebody else cannot write on his layer"            "$(code POST /Annotations/Save "$NALAN" "{\"ArtifactId\":\"$AID\",\"LayerId\":\"$LAYER\",\"Shapes\":[$CIRCLE]}")" "403"
ANN=$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")
eq "one current layer"                                  "$(jx "$ANN" '@($r.layers).Count')" "1"
eq "…with version 1 kept in its history"            "$(jx "$ANN" '(@($r.history) | ForEach-Object { $_.version }) -join ","')" "1"
eq "…and a version count of two"                    "$(f "$ANN" versionCount)" "2"
eq "the link follows the layer's current version"       "$(count "$(req GET "/Commitments/GetLinks?commitmentId=$CEM_ID" "$PETER")" 'Marked up: IMG-20260806-WA0007.png (v2)')" "1"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Sides and seats — the file decides first, then the layer"

NL=$(req POST /Annotations/Save "$NALAN" "{\"ArtifactId\":\"$AID\",\"Note\":\"Four bags for the ring beam\",\"Shapes\":[{\"Kind\":\"Rect\",\"X\":0.05,\"Y\":0.55,\"W\":0.6,\"H\":0.08}]}")
NLAYER=$(f "$NL" layerId)
eq "the delivery side's layer lands crew-only"          "$(f "$NL" channel)" "Crew"
eq "Peter does not see it"                              "$(count "$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")" "\"layerId\":\"$NLAYER\"")" "0"
eq "Nalan sees both"                                    "$(jx "$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$NALAN")" '@($r.layers).Count')" "2"
eq "the foreman is not shown Peter's question"          "$(count "$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$MUSA")" "\"layerId\":\"$LAYER\"")" "0"
eq "…nor may he ask one"                            "$(code POST /Annotations/Ask "$MUSA" "{\"ArtifactId\":\"$AID\",\"CommitmentId\":\"$CEM_ID\",\"Shapes\":[$CIRCLE],\"Question\":\"x\"}")" "404"
ML=$(req POST /Annotations/Save "$MUSA" "{\"ArtifactId\":\"$AID\",\"Shapes\":[{\"Kind\":\"Path\",\"Points\":[0.1,0.8,0.2,0.82,0.3,0.8]}]}")
MLAYER=$(f "$ML" layerId)
eq "the foreman's marks are the delivery side's own"    "$(f "$ML" channel)" "Crew"
eq "only the mediator moves them across"                "$(code PUT /Annotations/Expose "$MUSA" "{\"LayerId\":\"$MLAYER\",\"Channel\":\"Client\"}")" "403"
EXP=$(req PUT /Annotations/Expose "$NALAN" "{\"LayerId\":\"$MLAYER\",\"Channel\":\"Client\"}")
eq "Nalan exposes the foreman's layer"                  "$(f "$EXP" channel)" "Client"
PV=$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")
eq "Peter now sees it"                                  "$(jx "$PV" "(L '$MLAYER').layerId")" "$MLAYER"
eq "…in the accountable face's name, not the bench's" "$(jx "$PV" "(L '$MLAYER').authorName")" "Nalan Kaggwa"
eq "Peter's own question cannot be pulled back to crew"  "$(code PUT /Annotations/Expose "$NALAN" "{\"LayerId\":\"$LAYER\",\"Channel\":\"Crew\"}")" "400"
eq "a stranger reads no layers"                         "$(code GET "/Annotations/GetForArtifact?artifactId=$AID" "$GRACE")" "404"
eq "…and draws none"                                "$(code POST /Annotations/Save "$GRACE" "{\"ArtifactId\":\"$AID\",\"Shapes\":[$CIRCLE]}")" "404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The answer changes the commitment — the exit"

RES=$(req PUT /Commitments/ResolveQuery "$NALAN" \
  "{\"CommitmentId\":\"$CEM_ID\",\"Note\":\"Revised to 40 bags, +UGX 480,000: four for the ring beam, agreed on site 6 Aug.\",\"Amount\":4800000}")
NEW_ID=$(f "$RES" id)
ne "resolution writes a new statement"                  "$NEW_ID" "$CEM_ID"
eq "…at the answered figure"                        "$(f "$RES" amount)" "4800000"
eq "…saying what it was"                            "$(f "$RES" previousAmount)" "4320000"
ne "…and that the old figure had been cleared"      "$(f "$RES" previousClearedAt)" "null"
eq "…resolved, with the answer on the item"         "$(f "$RES" queryState)" "Resolved"
has "…in words"                                     "$(f "$RES" resolutionNote)" "Revised to 40 bags"
eq "…still pointing at the circled receipt"         "$(f "$RES" markupArtifactId)" "$AID"
OLD=$(req GET "/Commitments/GetCommitment?commitmentId=$CEM_ID" "$PETER")
eq "the questioned figure stays on record"              "$(f "$OLD" amount)" "4320000"
eq "…superseded, not deleted"                       "$(f "$OLD" supersededById)" "$NEW_ID"
eq "the question is closed"                             "$(f "$(req GET "/Flags/GetFlag?flagId=$QFLAG" "$PETER")" status)" "Resolved"
LY=$(req GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")
eq "the layer points at the item's current statement"    "$(jx "$LY" "(L '$LAYER').currentCommitmentId")" "$NEW_ID"
eq "…and knows it was resolved"                     "$(jx "$LY" "(L '$LAYER').commitmentQueryState")" "Resolved"
eq "the new figure can be cleared in its turn"          "$(f "$(req GET "/Commitments/GetCommitment?commitmentId=$NEW_ID" "$PETER")" canClear)" "true"
DV=$(req GET "/Commitments/GetCommitment?commitmentId=$NEW_ID" "$DINAH")
eq "Dinah, off the money, sees a figure exists"          "$(f "$DV" amountHidden)" "true"
eq "…but neither figure"                            "$(f "$DV" previousAmount)" "null"
eq "…and still sees what was circled"               "$(f "$DV" markupArtifactId)" "$AID"
HIST=$(req GET "/Commitments/GetChain?commitmentId=$NEW_ID" "$PETER")
eq "the history reads as two statements"                "$(count "$HIST" '"kind":')" "2"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Parked ideas accumulate silently"

GATE=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_EXT\",\"Kind\":\"Choice\",\"Maturity\":\"Idea\",\"Title\":\"Boundary wall with a sliding gate\"}")
GATE_ID=$(f "$GATE" id)
eq "an idea is parked against External works"           "$(f "$GATE" stageName)" "External works"
eq "…as an idea"                                    "$(f "$GATE" maturity)" "Idea"
eq "…with no deadline yet"                          "$(f "$GATE" isSurfaced)" "false"
PERG=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_EXT\",\"Kind\":\"Spec\",\"Maturity\":\"Idea\",\"Title\":\"Pergola over the terrace\",\"LeadTimeDays\":7}")
PERG_ID=$(f "$PERG" id)
eq "a second idea, seven days' lead time"               "$(day "$(f "$PERG" decideBy)")" "$(ymd "+33 days")"
eq "…forty days out: silent"                        "$(f "$PERG" isSurfaced)" "false"

E1=$(req POST /Commitments/AddEstimate "$PETER" "{\"CommitmentId\":\"$PERG_ID\",\"Amount\":6500000,\"Note\":\"Timber, a guess from the yard\"}")
eq "an estimate is added"                               "$(f "$E1" amount)" "6500000"
req POST /Commitments/AddEstimate "$DINAH" "{\"CommitmentId\":\"$PERG_ID\",\"Note\":\"Saw one in steel at a neighbour's\"}" >/dev/null
eq "Dinah, off the money, may not put a figure on it"   "$(code POST /Commitments/AddEstimate "$DINAH" "{\"CommitmentId\":\"$PERG_ID\",\"Amount\":1}")" "403"
req POST /Commitments/AddEstimate "$NALAN" "{\"CommitmentId\":\"$PERG_ID\",\"Amount\":8200000,\"Note\":\"Steel, fabricator's quote\",\"ArtifactId\":\"$AID\"}" >/dev/null
PG=$(req GET "/Commitments/GetCommitment?commitmentId=$PERG_ID" "$PETER")
eq "three estimates gathered"                           "$(f "$PG" estimateCount)" "3"
eq "…from the lowest"                               "$(f "$PG" estimateLow)" "6500000"
eq "…to the highest"                                "$(f "$PG" estimateHigh)" "8200000"
eq "…and the reference they cited is linked"        "$(f "$PG" linkCount)" "1"
eq "…and still nobody is asked anything"            "$(f "$PG" isSurfaced)" "false"
OWED_P=$(owed_on "$PETER")
hasnt "Peter's list does not carry the pergola"          "$OWED_P" "Pergola over the terrace"
hasnt "…nor the gate"                               "$OWED_P" "Boundary wall with a sliding gate"
eq "an agreed figure takes no estimates"                "$(code POST /Commitments/AddEstimate "$PETER" "{\"CommitmentId\":\"$NEW_ID\",\"Amount\":1}")" "400"
eq "the bench cannot see the idea's estimates"          "$(code GET "/Commitments/GetEstimates?commitmentId=$PERG_ID" "$MUSA")" "404"
EST=$(req GET "/Commitments/GetEstimates?commitmentId=$PERG_ID" "$DINAH")
eq "Dinah reads the estimates, figures masked"          "$(count "$EST" '"amountHidden":true')" "2"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 4 — it speaks only when waiting costs something"

PK=$(req PUT /Commitments/Park "$PETER" "{\"CommitmentId\":\"$GATE_ID\",\"DependsOnStageId\":\"$S_DRIVE\"}")
eq "the gate's power duct must be in before the driveway" "$(f "$PK" dependsOnStageName)" "Driveway"
eq "…so it must be decided by the driveway's start"  "$(day "$(f "$PK" decideBy)")" "$(ymd "+10 days")"
eq "…which is close enough to say so"                "$(f "$PK" isSurfaced)" "true"
has "…and says why"                                  "$(f "$PK" decideByReason)" "before Driveway starts"
OWED_P=$(owed_on "$PETER")
GATE_OWED=$(pick "$OWED_P" "Boundary wall with a sliding gate")
eq "it is on Peter's list now"                          "$(f "$GATE_OWED" kind)" "ParkedIdea"
eq "…by the driveway's start"                       "$(day "$(f "$GATE_OWED" dueBy)")" "$(ymd "+10 days")"
has "…with the consequence in words"                "$(f "$GATE_OWED" consequence)" "Driveway"
hasnt "…while the pergola stays silent"             "$OWED_P" "Pergola over the terrace"
hasnt "the delivery side is not nagged with Peter's idea" "$(owed_on "$NALAN")" "Boundary wall with a sliding gate"

PAVE=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_LAND\",\"Kind\":\"Material\",\"Maturity\":\"Idea\",\"Title\":\"Cabro paving in a herringbone\"}")
eq "an idea parked for a stage about to start"          "$(f "$PAVE" isSurfaced)" "true"
has "…is handed back at kickoff"                    "$(f "$PAVE" decideByReason)" "parked for it"
has "…on Peter's list"                              "$(owed_on "$PETER")" "Parked: Cabro paving in a herringbone"

MOVED=$(printf '%s' "$(req GET "/Stages/GetStageById?stageId=$S_DRIVE" "$PETER")" | sed "s/\"startDate\":\"[^\"]*\"/\"startDate\":\"$(iso "+60 days")\"/")
req PUT /Stages/UpdateStage "$PETER" "$MOVED" >/dev/null
GM=$(req GET "/Commitments/GetCommitment?commitmentId=$GATE_ID" "$PETER")
eq "the driveway slips two months — the date moves"     "$(day "$(f "$GM" decideBy)")" "$(ymd "+40 days")"
eq "…and the gate goes quiet again"                 "$(f "$GM" isSurfaced)" "false"
hasnt "…off Peter's list"                           "$(owed_on "$PETER")" "Boundary wall with a sliding gate"

PAVE_ID=$(f "$PAVE" id)
req PUT /Commitments/SetMaturity "$PETER" "{\"CommitmentId\":\"$PAVE_ID\",\"Maturity\":\"InDiscussion\"}" >/dev/null
req PUT /Commitments/SetMaturity "$PETER" "{\"CommitmentId\":\"$PAVE_ID\",\"Maturity\":\"Agreed\"}" >/dev/null
hasnt "once decided, it stops asking"                   "$(owed_on "$PETER")" "Cabro paving in a herringbone"
eq "an agreed item cannot be re-parked"                 "$(code PUT /Commitments/Park "$PETER" "{\"CommitmentId\":\"$PAVE_ID\",\"StageId\":\"$S_EXT\"}")" "400"
eq "a stage from another project is refused"            "$(code PUT /Commitments/Park "$PETER" "{\"CommitmentId\":\"$PERG_ID\",\"DependsOnStageId\":\"de300000-0000-4000-8000-000000000104\"}")" "400"
eq "the bench cannot re-file an idea"                   "$(code PUT /Commitments/Park "$MUSA" "{\"CommitmentId\":\"$PERG_ID\",\"LeadTimeDays\":3}")" "404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Clearing up after itself"

eq "the throwaway project is binned"                    "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$PID" "$PETER")" "200"
eq "…and emptied"                                   "$(code DELETE "/ProjectsRS/DeleteProject?projectId=$PID" "$PETER")" "200"
eq "its layers go with it"                              "$(code GET "/Annotations/GetForArtifact?artifactId=$AID" "$PETER")" "404"

printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
