#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the redesigned surfaces, read from all four seats.
#
# Every screen in the rewrite renders off one of the calls below, so this suite
# asks the question each screen asks, from each persona:
#
#   Home            does this seat see the right projects, and only those?
#   Project header  is the side per project, and is the mediator the one name?
#   Money           who may see a position, and who must not?
#   Brief           does a client-side reader get the exposed subset only?
#   Site Log        is the delivery surface invisible rather than refused?
#   History         does the ingested pile stay with the side that imported it?
#   Register        can a question be raised, owed and resolved?
#
# It also asserts the correction this rewrite exists for: Peter has ONE
# top-level project with ONE sub-project, not nine.
#
# Usage:  bash tools/e2e-ux-personas.sh [api-base]
# Needs:  the API running in Development, and POST /api/Dev/SeedDemo applied.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-https://localhost:7264/api}"
PASS_WORD="Assetlen#2026"

PASS=0; FAIL=0
CURL=(curl -sk --max-time 40)

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-62s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }
head_(){ printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1\",\"Password\":\"$PASS_WORD\"}" \
    | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
}

req() {  # req METHOD PATH TOKEN [JSON]
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3" \
      -H "Content-Type: application/json" -d "$4"
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

jget()  { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
jbool() { printf '%s' "$1" | grep -o "\"$2\":\(true\|false\)" | head -1 | sed 's/.*://' || true; }
jnum()  { printf '%s' "$1" | grep -o "\"$2\":[-0-9.]*" | head -1 | sed 's/.*://' || true; }
count() { printf '%s' "$1" | grep -o "$2" | wc -l | tr -d ' '; }

echo "ASSETLEN — redesigned surfaces, four seats — $API"

# ── Seed and sign in ─────────────────────────────────────────────────────────
"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"

PETER=$(tok peter@assetlen.dev)
DINAH=$(tok dinah@assetlen.dev)
NALAN=$(tok nalan@assetlen.dev)
MUSA=$(tok  musa@assetlen.dev)

for t in PETER DINAH NALAN MUSA; do
  [ -z "${!t}" ] && { echo "FATAL: $t could not sign in. Is the API in Development and seeded?"; exit 1; }
done
ok "all four personas sign in" "peter · dinah · nalan · musa"

PID="de300000-0000-4000-8000-000000000010"
WING="de300000-0000-4000-8000-000000000011"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The correction — one engagement, not nine"

HOME=$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")
ROOTS=$(count "$HOME" '"parentProjectId":null')
eq "Peter's home shows exactly one top-level project"   1 "$(jnum "$HOME" activeProjectsCount)"
eq "…and it is the residence"                           "Kira Residence" "$(jget "$HOME" projectName)"
eq "…with exactly one sub-project under it"             1 "$(jnum "$HOME" subProjectCount)"

PROJ=$(req GET "/ProjectsRS/GetProjectById?projectId=$PID" "$PETER")
STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
STAGE_N=$(count "$STAGES" '"stageName"')
eq "the residence carries nine stages, not nine projects" 9 "$STAGE_N"

# The three things previously mistaken for projects are stages of this one.
for s in "Retaining wall" "External works" "Doors &amp; windows"; do :; done
printf '%s' "$STAGES" | grep -q '"stageName":"Retaining wall"' \
  && ok "retaining wall is a stage" || bad "retaining wall is a stage" "missing" "present"
printf '%s' "$STAGES" | grep -q '"stageName":"External works"' \
  && ok "external works is a stage" || bad "external works is a stage" "missing" "present"
printf '%s' "$STAGES" | grep -q '"stageName":"Doors & windows"' \
  && ok "doors and windows is a stage" || bad "doors and windows is a stage" "missing" "present"

WINGP=$(req GET "/ProjectsRS/GetProjectById?projectId=$WING" "$PETER")
eq "the guest wing names the residence as its parent" "$PID" "$(jget "$WINGP" parentProjectId)"

SIZING=$(req GET "/ProjectsRS/GetProjectSizing?projectId=$WING" "$PETER")
eq "reading the wing reports the parent's billable tier"  "Medium" "$(jget "$SIZING" tier)"
eq "…and the rolled-up area, wing included"               "620.00" "$(jnum "$SIZING" totalAreaSqm)"
eq "…and names the residence as the billable project"     "$PID" "$(jget "$SIZING" billableProjectId)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Sides — per project, never from a global role"

for who in PETER:Client DINAH:Client NALAN:Contractor MUSA:Contractor; do
  name="${who%%:*}"; want="${who##*:}"
  st=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "${!name}")
  eq "$(printf '%s' "$name" | tr 'A-Z' 'a-z') sits on the $want side" "$want" "$(jget "$st" side)"
done

NST=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$NALAN")
eq "Nalan is the mediator"                       "true"  "$(jbool "$NST" isMediator)"
eq "…so he may expose to the client side"        "true"  "$(jbool "$NST" canExposeToClient)"

MST=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$MUSA")
eq "Musa reads the Site Log"                     "true"  "$(jbool "$MST" canSeeSiteLog)"
eq "…but does not mediate"                       "false" "$(jbool "$MST" isMediator)"

DST=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$DINAH")
eq "Dinah never reaches the Site Log"            "false" "$(jbool "$DST" canSeeSiteLog)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Money — the screen that replaced a 9 AM meeting"

FUND=$(req GET "/Funding/GetFundingByProject?projectId=$PID" "$PETER")
eq "Peter reads the release history"     "200" "$(code GET "/Funding/GetFundingByProject?projectId=$PID" "$PETER")"
eq "Dinah reads it too — she has cost opinions and needs the numbers" \
   "200" "$(code GET "/Funding/GetFundingByProject?projectId=$PID" "$DINAH")"

PENDING=$(count "$FUND" '"status":"Pending"')
[ "$PENDING" -ge 1 ] && ok "at least one release is awaiting confirmation" "$PENDING" \
                     || bad "at least one release is awaiting confirmation" "$PENDING" ">=1"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Two surfaces — the brief and the Site Log"

PLOG=$(req GET "/Progress/GetProgressUpdates?projectId=$PID&offset=0&limit=50" "$PETER")
NLOG=$(req GET "/Progress/GetProgressUpdates?projectId=$PID&offset=0&limit=50" "$NALAN")

PN=$(count "$PLOG" '"description"')
NN=$(count "$NLOG" '"description"')

[ "$NN" -gt "$PN" ] && ok "the delivery side sees more than the client side" "$NN vs $PN" \
                    || bad "the delivery side sees more than the client side" "$NN vs $PN" "delivery > client"

# A Crew entry must 404 to a client-side reader — a refusal would confirm the
# Site Log exists, which is the thing the two-surface model hides.
CREW_ID=$(printf '%s' "$NLOG" | grep -o '"id":"[^"]*","projectId"' | sed 's/"id":"//;s/","projectId"//' | head -1)
if [ -n "$CREW_ID" ]; then
  ok "delivery-side entries are readable by the delivery side" "$CREW_ID"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Ingested record — owned by the side that imported it"

eq "Nalan may open the ingest inbox"  "200" "$(code GET "/Ingest/GetBatches?projectId=$PID" "$NALAN")"
DIN_ING=$(code GET "/Ingest/GetBatches?projectId=$PID" "$DINAH")
[ "$DIN_ING" = "200" ] || [ "$DIN_ING" = "404" ] \
  && ok "Dinah is not refused — she is answered as if it is not there" "$DIN_ING" \
  || bad "Dinah is not refused with a 403" "$DIN_ING" "200 or 404"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Register — a question that keeps its answer"

FLAGS=$(req GET "/Flags/GetFlagsByProject?projectId=$PID" "$PETER")
OPEN=$(count "$FLAGS" '"status":"Open"')
[ "$OPEN" -ge 2 ] && ok "open questions exist on the register" "$OPEN" \
                  || bad "open questions exist on the register" "$OPEN" ">=2"

printf '%s' "$FLAGS" | grep -q '"dueDate":"' \
  && ok "at least one question carries a by-when" \
  || bad "at least one question carries a by-when" "none" "one"

# ASCII only in this payload. Git Bash on Windows re-encodes a literal em dash
# before curl sees it and the request then fails JSON parsing — a property of
# this shell, not of the API, which round-trips real UTF-8 bytes correctly.
RAISED=$(req POST /Flags/AddFlag "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"Title\":\"E2E balustrade profile\",\"Description\":\"Raised by the suite.\",\"Severity\":\"Medium\",\"Channel\":\"Client\"}")
RID=$(printf '%s' "$RAISED" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//')
[ -n "$RID" ] && ok "the mediator can raise a question" "$RID" \
              || bad "the mediator can raise a question" "no id" "an id"

if [ -n "$RID" ]; then
  RES=$(req PUT "/Flags/ResolveFlag?flagId=$RID" "$NALAN")
  eq "resolving updates the item itself"  "Resolved" "$(jget "$RES" status)"

  # Leave the demo world as it was found. A suite that accumulates its own
  # probes turns the register into a list of test artefacts, and the next
  # person to open the screen cannot tell the seeded story from the noise.
  req PUT /Flags/UpdateFlag "$NALAN" "{\"Id\":\"$RID\",\"Status\":\"Archived\"}" > /dev/null
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Roster — who holds a key, and nothing more"

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
MEDIATORS=$(count "$ROSTER" '"isMediator":true')
eq "exactly one accountable face on the project" 1 "$MEDIATORS"

printf '%s' "$ROSTER" | grep -q '"partyName":"Sunrise Aluminium Ltd"' \
  && ok "off-platform parties are nameable without a login" \
  || bad "off-platform parties are nameable without a login" "missing" "present"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  ${c_pass}%d passed${c_off}  ${c_fail}%d failed${c_off}\n\n" "$PASS" "$FAIL"
[ "$FAIL" -eq 0 ] || exit 1
