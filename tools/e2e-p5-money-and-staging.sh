#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the funding back-and-forth, and staging across the app.
#
# Two questions, both from the vision rather than from the code:
#
#   Do the two parties reach agreement on a release without a meeting? Money
#   crosses a border and arrives smaller than it left — bank charges, a rate, a
#   partial transfer. The funder declares what they sent, the delivery side says
#   what landed, and whoever is out of pocket decides what to do about the gap.
#   Neither side settles it alone, and neither side can act on the other's half.
#
#   Does anything float? Nothing on a project should (CLAUDE.md §1) — but asking
#   "which stage?" on every capture is the tax that sends people back to the
#   chat. So the active stage is filled in, a named one always wins, and the
#   catalogue is reachable for the life of the project rather than only on day
#   one.
#
# Usage:  bash tools/e2e-p5-money-and-staging.sh [api-base] [admin-email] [password]
# Needs:  the API running, and the demo seed (POST /api/Dev/SeedDemo).
#         Idempotent — re-running takes the next unused catalogue key.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"

PROJ=de300000-0000-4000-8000-000000000010
DEMO_PASS='Assetlen#2026'

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 40)

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s want=%s got=%s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }
head_() { printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1@assetlen.dev\",\"Password\":\"$DEMO_PASS\"}" \
    | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
}

req() {
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

# A decimal(18,4) column renders 15050000.0000 for a figure written as
# 15050000. Trailing zeros are stripped only after a decimal point, so ids and
# names survive untouched.
f() {
  printf '%s' "$1" | grep -o "\"$2\":[^,}]*" | head -1 | cut -d: -f2- | tr -d '"' \
    | sed -E 's/^([0-9]+)\.0+$/\1/'
}

echo "ASSETLEN P5 — money and staging — $API"

PETER=$(tok peter); NALAN=$(tok nalan); MUSA=$(tok musa)

if [ -z "$PETER" ] || [ -z "$NALAN" ]; then
  echo "FATAL: demo personas could not sign in. Run POST $API/Dev/SeedDemo first."
  exit 1
fi

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PROJ" "$NALAN")
STAGE=$(printf '%s' "$STAGES" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

# ═════════════════════════════════════════════════════════════════════════════
head_ "A release crosses a border, and the two figures are both kept"

NEW=$(req POST /Funding/AddFundingEntry "$PETER" \
  "{\"projectId\":\"$PROJ\",\"stageId\":\"$STAGE\",\"amount\":4000,\"currency\":\"USD\",\"exchangeRate\":3800,\"notes\":\"Chain test\"}")
ENTRY=$(f "$NEW" id)

eq "the ledger keeps one currency"            "$(f "$NEW" amount)"           "15200000"
eq "…and what he actually sent"             "$(f "$NEW" declaredAmount)"   "4000"
eq "…in the currency he sent it"            "$(f "$NEW" declaredCurrency)" "USD"
eq "it starts unacknowledged"                 "$(f "$NEW" status)"           "Pending"

eq "a foreign figure without a rate is refused" "400" \
   "$(code POST /Funding/AddFundingEntry "$PETER" \
      "{\"projectId\":\"$PROJ\",\"stageId\":\"$STAGE\",\"amount\":4000,\"currency\":\"USD\"}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Reading the money is not the same as moving it"

eq "the foreman cannot acknowledge a release" "403" \
   "$(code PUT /Funding/ConfirmFunding "$MUSA" "{\"fundingEntryId\":\"$ENTRY\",\"isConfirmed\":true}")"

CONF=$(req PUT /Funding/ConfirmFunding "$NALAN" \
  "{\"fundingEntryId\":\"$ENTRY\",\"isConfirmed\":true,\"receivedAmount\":15050000,\"notes\":\"Bank charges\"}")

eq "a different figure opens a query"         "$(f "$CONF" status)"          "AmountQueried"
eq "…and records what landed"               "$(f "$CONF" receivedAmount)"  "15050000"

eq "the contractor cannot write off the gap"  "403" \
   "$(code PUT /Funding/SettleFunding "$NALAN" "{\"fundingEntryId\":\"$ENTRY\"}")"

PQ=$(req GET /Funding/GetFundingNeedingMe "$PETER")
eq "the gap is queued for the funder"         "$(printf '%s' "$PQ" | grep -c "$ENTRY")" "1"

SET=$(req PUT /Funding/SettleFunding "$PETER" "{\"fundingEntryId\":\"$ENTRY\"}")
eq "the funder accepts it"                    "$(f "$SET" status)"           "Settled"
eq "…and it settles at what arrived"        "$(f "$SET" settledAmount)"    "15050000"

eq "a settled release cannot be settled again" "400" \
   "$(code PUT /Funding/SettleFunding "$PETER" "{\"fundingEntryId\":\"$ENTRY\"}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Every stage knows its phase, and so wears an accent"

SEEDED=$(printf '%s' "$STAGES" | tr '{' '\n' | grep '"id":"de300000')
eq "no seeded stage is left unphased"         "$(printf '%s' "$SEEDED" | grep -c '"phase":"Custom"')" "0"

DISTINCT=$(printf '%s' "$STAGES" | grep -o '"phase":"[^"]*"' | sort -u | wc -l | tr -d ' ')
eq "several phases are in use"                "$([ "$DISTINCT" -ge 5 ] && echo yes || echo "no($DISTINCT)")" "yes"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The catalogue is reachable for the life of the project"

# A successful run consumes its key, so take the first still unused. The phase
# travels with the key — that is the property under test.
KEY=""; NAME=""; PHASE=""
while IFS='|' read -r k n p; do
  if ! printf '%s' "$STAGES" | grep -q "\"$k\""; then KEY="$k"; NAME="$n"; PHASE="$p"; break; fi
done <<'POOL'
svc.electrical-first|Electrical first fix|Services
svc.electrical-second|Electrical second fix|Services
svc.plumbing-first|Plumbing first fix|Services
svc.plumbing-second|Plumbing second fix|Services
svc.drainage|Drainage|Services
svc.water-storage|Water storage|Services
fin.wall-tiling|Wall tiling|Finishes
fin.ceiling|Ceilings|Finishes
POOL

if [ -z "$KEY" ]; then
  skip "adding from the catalogue" "every pooled key is used — reseed"
  skip "a catalogue stage cannot be added twice"
  NEWSTAGE=""
else
  ADDED=$(req POST "/Stages/CreateStage?projectId=$PROJ" "$PETER" "{\"catalogueKey\":\"$KEY\"}")
  NEWSTAGE=$(f "$ADDED" id)

  eq "the name comes from the catalogue"      "$(f "$ADDED" stageName)"      "$NAME"
  eq "…and so does the phase"               "$(f "$ADDED" phase)"          "$PHASE"

  eq "the same stage cannot be added twice"   "409" \
     "$(code POST "/Stages/CreateStage?projectId=$PROJ" "$PETER" "{\"catalogueKey\":\"$KEY\"}")"
fi

eq "an unknown catalogue key is refused"      "400" \
   "$(code POST "/Stages/CreateStage?projectId=$PROJ" "$PETER" '{"catalogueKey":"not.a.real.stage"}')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A stage the catalogue does not have, under a major one"

if [ -n "$NEWSTAGE" ]; then
  CUSTOM=$(req POST "/Stages/CreateStage?projectId=$PROJ" "$PETER" \
    "{\"stageName\":\"Borehole and pump house\",\"phase\":\"Services\",\"parentStageId\":\"$NEWSTAGE\"}")
  CUSTOMID=$(f "$CUSTOM" id)

  eq "a custom stage takes the phase it is given" "$(f "$CUSTOM" phase)"     "Services"
  eq "…and sits under its major stage"        "$(f "$CUSTOM" parentStageId)" "$NEWSTAGE"

  # One level, as with sub-projects: a few majors with the detail folded under.
  eq "a sub-stage cannot take a sub-stage"    "400" \
     "$(code POST "/Stages/CreateStage?projectId=$PROJ" "$PETER" \
        "{\"stageName\":\"Too deep\",\"parentStageId\":\"$CUSTOMID\"}")"

  AFTER=$(req GET "/Stages/GetStagesByProjectId?projectId=$PROJ" "$NALAN")
  eq "it comes back nested as well as flat"   "$(printf '%s' "$AFTER" | grep -o "$CUSTOMID" | wc -l | tr -d ' ')" "2"
else
  skip "custom stages and nesting" "no parent stage was created"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Nothing floats, and nothing has to be filed by hand"

CAP=$(req POST /Progress/AddProgressUpdate "$NALAN" \
  "{\"projectId\":\"$PROJ\",\"description\":\"Chain auto-link\",\"completionPercentage\":0}")
eq "a capture with no stage lands on the live one" "$(f "$CAP" stageName)"   "Retaining wall"

FLAG=$(req POST /Flags/AddFlag "$NALAN" \
  "{\"projectId\":\"$PROJ\",\"title\":\"Chain auto-link question\",\"description\":\"which stage?\"}")
eq "so does a question raised on site"        "$(f "$FLAG" stageName)"       "Retaining wall"

if [ -n "$NEWSTAGE" ]; then
  CAP2=$(req POST /Progress/AddProgressUpdate "$NALAN" \
    "{\"projectId\":\"$PROJ\",\"stageId\":\"$NEWSTAGE\",\"description\":\"Chain deliberate\",\"completionPercentage\":0}")
  eq "a stage named deliberately still wins"  "$(f "$CAP2" stageId)"         "$NEWSTAGE"
else
  skip "a stage named deliberately still wins"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Clearing up after itself"

# Deduplication is the point of the catalogue key, which means a run that leaves
# its stage behind burns that key for every later run. Deleting archives rather
# than destroys, and the dedup check reads through the same filter — so the key
# comes back. Child first: a major stage should not be removed out from under one.
if [ -n "${CUSTOMID:-}" ]; then
  eq "the custom stage is removed"            "200" \
     "$(code DELETE "/Stages/DeleteStage?stageId=$CUSTOMID" "$PETER")"
fi

if [ -n "$NEWSTAGE" ]; then
  eq "…and the catalogue stage with it"     "200" \
     "$(code DELETE "/Stages/DeleteStage?stageId=$NEWSTAGE" "$PETER")"

  FREED=$(req GET "/Stages/GetStagesByProjectId?projectId=$PROJ" "$NALAN")
  eq "which frees its catalogue key again"    "$(printf '%s' "$FREED" | grep -c "\"$KEY\"")" "0"
fi

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
