#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — Law 0 end to end: "Does it still work when the contractor is
# silent?" (assetlen.md §4).
#
# The other suites prove Law 0 surface by surface. This one does it in one
# sitting, the way Peter would on day one: he opens a project nobody else is
# on, imports his forwarded thread, clears the proposals, and then reads his
# home, the register, the daily brief and the works report — issued, frozen and
# hashed. Nobody from the delivery side is invited, maps an author or signs in,
# and the server's own login record is the proof of that.
#
# Also pinned here, because they were found in the full test pass (plan.md,
# "Full pass"): the seat rules Peter's front door depends on — History is his,
# the Site Diary and Capture are not — and the search footnote that must not
# name the Site Diary to the client side.
#
# Usage:  bash tools/e2e-law0.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running in Development, pwsh for reading JSON. Run from the
#         repo root. Each run builds its own project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/p5"
DEMO=de300000-0000-4000-8000-000000000010
DEMO_PASS='Assetlen#2026'

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_dim=$'\033[2m'; c_off=$'\033[0m'
ok()   { printf "  ${c_pass}PASS${c_off}  %-60s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-60s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
ge()   { if [ "${3:-0}" -ge "$2" ] 2>/dev/null; then ok "$1" "$3"; else bad "$1" "${3:-}" ">= $2"; fi; }
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
jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
jraw() { printf '%s' "$1" | grep -o "\"$2\":[^,}]*" | head -1 | cut -d: -f2- | tr -d '"' || true; }
oid()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } elseif (\$null -eq \$out) { 'null' } else { \$out }"
}
logins() { "${CURL[@]}" "$API/Dev/LoginStats?email=$1" | jx '$r.logins'; }

echo "ASSETLEN Law 0 — the contractor silent, end to end — $API"

bash tools/make-extraction-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
  || { echo "FATAL: could not build fixtures. Is pwsh on PATH?"; exit 1; }
# shellcheck disable=SC1090
. "$FIXTURES/fixtures.env"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }
"${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"Password":"password","Email":"peter.buyer@assetlen.test","UserName":"peter.buyer","FirstName":"Peter","LastName":"Developer","UserRolesDto":{"Roles":["Contractor"]},"defaultRole":["Contractor"]}'

# The delivery-side accounts the other suites use. They are read, never signed in.
N_BEFORE=$(logins nalan.arch@assetlen.test)
K_BEFORE=$(logins kato.foreman@assetlen.test)

PETER=$(tok peter.buyer@assetlen.test password)
[ -z "$PETER" ] && { echo "FATAL: Peter could not sign in."; exit 1; }

# ═════════════════════════════════════════════════════════════════════════════
head_ "Peter alone: a project nobody else is on, and his forwarded thread"

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Law 0 $STAMP\",\"Description\":\"Silent contractor\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Retaining wall\",\"DisplayOrder\":1},{\"StageName\":\"Guest wing plaster\",\"DisplayOrder\":2}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter opens a project" "$PID"

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
eq "…and is the only person on it"             "1" "$(printf '%s' "$ROSTER" | jx '@($r).Count')"

STAND=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$PETER")
eq "his History tab is there — the front door"  "true" "$(jraw "$STAND" canSeeHistory)"

PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $PETER" \
  -F "file=@$FIXTURES/$P5_THREAD" -F "projectId=$PID")
BATCH=$(jget "$PRE" batchId)
[ -z "$BATCH" ] && { echo "FATAL: preview failed: $(printf '%s' "$PRE" | head -c 300)"; exit 1; }
PETER_MID=$(printf '%s' "$PRE" | grep -o '"suggestedMemberId":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

# The contractor exists only as a name in the thread — a party with no login.
COMMIT=$(req POST /Ingest/CommitImport "$PETER" \
  "{\"BatchId\":\"$BATCH\",\"AuthorMappings\":[
      {\"ExternalAuthor\":\"Nalan\",\"CreateAsPartyName\":\"Nalan (thread)\",\"Side\":1,\"Specialization\":3},
      {\"ExternalAuthor\":\"Peter\",\"MemberId\":\"$PETER_MID\"},
      {\"ExternalAuthor\":\"Dinah\",\"CreateAsPartyName\":\"Dinah (thread)\",\"Side\":0,\"Specialization\":9}]}")
eq "the thread imports"                           "Completed" "$(jget "$COMMIT" status)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The register fills from the pile, cleared by Peter alone"

Q=$(req GET "/Extraction/GetQueue?projectId=$PID" "$PETER")
IDS=$(printf '%s' "$Q" | sed 's/\],"pendingCount".*//' | sed 's/},{/}\n{/g' | grep '"ingestedMessageId"' \
  | grep '"status":"Pending"' | grep -o '"id":"[^"]*"' | sed 's/.*:"//;s/"$//' | sed 's/.*/"&"/' | paste -sd, -)
N_PENDING=$(printf '%s' "$IDS" | tr ',' '\n' | grep -c . || true)
ge "extraction proposed items with nobody asking"  15 "$N_PENDING"

DEC=$(req POST /Extraction/Decide "$PETER" "{\"ProposalIds\":[$IDS],\"Accept\":true}")
eq "Peter clears them in one pass"               "$N_PENDING" "$(jraw "$DEC" accepted)"

REG=$(req GET "/Commitments/GetCommitments?projectId=$PID" "$PETER")
ge "the register has commitments on it"           15 "$(printf '%s' "$REG" | grep -o '"kind":' | wc -l | tr -d ' ')"
ge "…money among them"                            1 "$(printf '%s' "$REG" | grep -o '"kind":"Price"' | wc -l | tr -d ' ')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "His home and the daily brief, assembled with no curator"

HOME_=$(req GET "/Brief/Home?days=7" "$PETER")
eq "the project is on his home"                   "true" "$(printf '%s' "$HOME_" | jx "@(\$r.projects | Where-Object { \$_.id -eq '$PID' }).Count -eq 1")"

BRIEF=$(req GET "/Brief/Day?projectId=$PID&day=2026-07-02&days=7" "$PETER")
eq "the brief says nobody curated it"             "true" "$(printf '%s' "$BRIEF" | jx '$r.assembledWithoutCurator')"
ge "…and still groups the week by the work"      1 "$(printf '%s' "$BRIEF" | jx '@($r.blocks).Count')"
ge "…with the truth floor on it"                 3 "$(printf '%s' "$BRIEF" | jx '(@($r.truthFloor | ForEach-Object { @($_.items).Count }) | Measure-Object -Sum).Sum')"
ge "…the blocker he raised among it"             1 "$(printf '%s' "$BRIEF" | jx '@(($r.truthFloor | Where-Object { $_.kind -eq "Blocker" }).items).Count')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The works report — live, then issued, frozen and hashed"

LIVE=$(req GET "/WorksReport/Live?projectId=$PID&asAt=2026-07-06T23:00:00" "$PETER")
ge "the live report has headlines"               1 "$(printf '%s' "$LIVE" | jx '@($r.headlines).Count')"
ge "…the decisions on it"                        1 "$(printf '%s' "$LIVE" | jx '@($r.decisions).Count')"
ge "…the blockers by owner"                      1 "$(printf '%s' "$LIVE" | jx '@($r.blockers).Count')"
eq "…drafted from the template, not a model"    "template" "$(printf '%s' "$LIVE" | jx '$r.narrative.engine')"

ISSUED=$(req POST /WorksReport/Issue "$PETER" "{\"ProjectId\":\"$PID\",\"AsAt\":\"2026-07-06T23:00:00\"}")
RID=$(jget "$ISSUED" id)
[ -n "$RID" ] && ok "Peter issues it himself" "$RID" || bad "Peter issues it himself" "$(printf '%s' "$ISSUED" | head -c 120)" "an id"
BACK=$(req GET "/WorksReport/Issued?reportId=$RID" "$PETER")
eq "…and it reads back with its hash verified"  "true" "$(jraw "$BACK" hashVerified)"
eq "…in his name"                               "Peter Developer" "$(jget "$BACK" issuedByName)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Nobody from the delivery side signed in — the server's record"

eq "the contractor's account was not used"       "$N_BEFORE" "$(logins nalan.arch@assetlen.test)"
eq "nor the foreman's"                           "$K_BEFORE" "$(logins kato.foreman@assetlen.test)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Money follows the seat, not the tenant role"

# A site engineer whose tenant role reads finance, seated on this project as a
# foreman. The role gate lets him in; the seat must still keep the releases
# from him, as the Money tab already does.
"${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"Password":"password","Email":"site.eng@assetlen.test","UserName":"site.eng","FirstName":"Site","LastName":"Engineer","UserRolesDto":{"Roles":["Manager"]},"defaultRole":["Manager"]}'
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"site.eng@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null
ENG=$(tok site.eng@assetlen.test password)
STAGE1=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
fcode() { "${CURL[@]}" -o /dev/null -w '%{http_code}' "$API/Funding/$1" -H "Authorization: Bearer $2"; }

eq "the engineer's seat carries no money"         "false" "$(jraw "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$ENG")" canSeeMoney)"
eq "…and the releases are not there for him"    "404" "$(fcode "GetFundingByProject?projectId=$PID" "$ENG")"
eq "…stage by stage either"                     "404" "$(fcode "GetFundingByStage?stageId=$STAGE1" "$ENG")"
eq "Peter reads his own"                          "200" "$(fcode "GetFundingByProject?projectId=$PID" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Seats on the demo project — what each chair is shown"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"
D_PETER=$(tok peter@assetlen.dev "$DEMO_PASS"); D_DINAH=$(tok dinah@assetlen.dev "$DEMO_PASS")
D_NALAN=$(tok nalan@assetlen.dev "$DEMO_PASS"); D_MUSA=$(tok musa@assetlen.dev "$DEMO_PASS")
cap() { jraw "$(req GET "/ProjectMembers/GetMyStanding?projectId=$DEMO" "$1")" "$2"; }

eq "the funder has History"                       "true"  "$(cap "$D_PETER" canSeeHistory)"
eq "…and so does the representative"            "true"  "$(cap "$D_DINAH" canSeeHistory)"
eq "…and the mediator"                          "true"  "$(cap "$D_NALAN" canSeeHistory)"
eq "the foreman does not"                         "false" "$(cap "$D_MUSA" canSeeHistory)"
eq "the funder has no Site Diary"                 "false" "$(cap "$D_PETER" canSeeSiteLog)"
eq "…and no Capture"                            "false" "$(cap "$D_PETER" canCapture)"
eq "the foreman has no money"                     "false" "$(cap "$D_MUSA" canSeeMoney)"
eq "…his crew role is refused the releases"     "403" "$(fcode "GetFundingByProject?projectId=$DEMO" "$D_MUSA")"
eq "…stage by stage too"                        "403" "$(fcode "GetFundingByStage?stageId=de300000-0000-4000-8000-000000000104" "$D_MUSA")"
eq "the funder still reads his releases"          "200" "$(fcode "GetFundingByProject?projectId=$DEMO" "$D_PETER")"
eq "…and so does the representative"            "200" "$(fcode "GetFundingByProject?projectId=$DEMO" "$D_DINAH")"

S_DINAH=$(req GET "/Search/Query?q=cement" "$D_DINAH")
S_NALAN=$(req GET "/Search/Query?q=cement" "$D_NALAN")
eq "search does not name the Diary to the client" "false" "$(jraw "$S_DINAH" searchedSiteDiary)"
eq "…and does to the mediator"                  "true"  "$(jraw "$S_NALAN" searchedSiteDiary)"

LEDGER=$(req GET "/Ledger/GetStageLedger?projectId=$DEMO" "$D_PETER")
eq "a re-seed restores the demo ledger"          "58000000" \
   "$(printf '%s' "$LEDGER" | jx '[decimal](@($r.rows | Where-Object { $_.stageName -eq "Retaining wall" })[0].claimed)')"

printf "\n  %d passed, %d failed, %d skipped\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
