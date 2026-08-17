#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the reader's own screen, and the bin.
#
# Two questions, both from the vision rather than from the code:
#
#   Is the arrangement Peter's own? Two people work one engagement and each
#   keeps their own order and their own pins — an order stored on the project
#   would have one of them rearranging the other's screen.
#
#   Does deleting destroy the record? It must not. The record is the product
#   (assetlen.md §8), so "delete" archives for thirty days and only an explicit
#   second act empties it.
#
# Usage:  bash tools/e2e-p4-arrange.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running. Idempotent — re-running reuses users.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 40)
STAMP="$(date +%H%M%S)"

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1\",\"Password\":\"$2\"}" | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
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

# BaseDto serialises its id after the derived type's nested collections, so the
# OUTER object's id is the last match, not the first. See e2e-p2-peter.sh.
oid() { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

# Top-level project names on the dashboard, in the order they are returned.
# Sub-projects are nested inside their parent, so isolating the parent's
# subProjects array first is what keeps a wing out of the order under test.
order() {
  printf '%s' "$1" | sed 's/"subProjects":\[[^]]*\]//g' \
    | grep -o '"projectName":"[^"]*"' | sed 's/.*:"//;s/"$//' || true
}

# position NAMES NEEDLE → 1-based index, or 0
position() {
  local i=1
  while IFS= read -r line; do
    [ "$line" = "$2" ] && { printf '%s' "$i"; return; }
    i=$((i+1))
  done <<< "$1"
  printf '0'
}

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }
head_() { printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

echo "ASSETLEN P4 — the reader's own screen, and the bin — $API"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

mkuser() {
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

# A fresh pair per run, unlike the other suites. Everything here is about state
# the reader accumulates — an order, three pins, a bin — so a principal carrying
# yesterday's pins would make the cap assertion pass or fail on history rather
# than on the rule.
PETER_EMAIL="peter.p4.$STAMP@assetlen.test"
NALAN_EMAIL="nalan.p4.$STAMP@assetlen.test"

mkuser "$PETER_EMAIL" Contractor Peter Developer
mkuser "$NALAN_EMAIL" Manager    Nalan Architect

PETER=$(tok "$PETER_EMAIL" password)
NALAN=$(tok "$NALAN_EMAIL" password)
for t in PETER NALAN; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

mkproject() { # mkproject TOKEN NAME [PARENT_ID]
  local body="{\"ProjectName\":\"$2\",\"Location\":\"Kampala\",\"TotalBudget\":100000000,\"Currency\":\"UGX\""
  [ -n "${3:-}" ] && body="$body,\"ParentProjectId\":\"$3\""
  body="$body}"
  oid "$(req POST /ProjectsRS/CreateProject "$1" "$body")"
}

A_NAME="Arrange A $STAMP"
B_NAME="Arrange B $STAMP"
C_NAME="Arrange C $STAMP"
D_NAME="Arrange D $STAMP"
W_NAME="Guest wing $STAMP"

A=$(mkproject "$PETER" "$A_NAME")
B=$(mkproject "$PETER" "$B_NAME")
C=$(mkproject "$PETER" "$C_NAME")
D=$(mkproject "$PETER" "$D_NAME")
W=$(mkproject "$PETER" "$W_NAME" "$A")

for p in A B C D W; do
  [ -z "${!p}" ] && { echo "FATAL: could not create project $p."; exit 1; }
done

# ═════════════════════════════════════════════════════════════════════════════
head_ "The arrangement is the reader's own, and it is remembered"

DASH=$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")
NAMES=$(order "$DASH")

eq "a wing is not a row of its own"  "0" "$(position "$NAMES" "$W_NAME")"

# Drop D to the top and A to the bottom. The whole unpinned list is sent, not
# just the one that moved — one index is ambiguous the moment a second device
# reorders at the same time.
REORDER="{\"Items\":[{\"ProjectId\":\"$D\",\"SortOrder\":0},{\"ProjectId\":\"$B\",\"SortOrder\":1},{\"ProjectId\":\"$C\",\"SortOrder\":2},{\"ProjectId\":\"$A\",\"SortOrder\":3}]}"
eq "an order can be recorded"        "200" "$(code PUT /ProjectsRS/ReorderProjects "$PETER" "$REORDER")"

NAMES=$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")
eq "…and the dropped project is first"  "1" "$(position "$NAMES" "$D_NAME")"
eq "…and the one moved down is last"    "4" "$(position "$NAMES" "$A_NAME")"

# The point of storing this per user rather than per project.
eq "…and it survives a re-read"         "1" "$(position "$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")" "$D_NAME")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Pins are capped, and a wing has no position to pin"

eq "a project can be pinned"   "200" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$A&pinned=true" "$PETER")"
eq "…and a second"             "200" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$B&pinned=true" "$PETER")"
eq "…and a third"              "200" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$C&pinned=true" "$PETER")"
eq "…but never a fourth"       "400" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$D&pinned=true" "$PETER")"

NAMES=$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")
eq "a pin lifts the project above the list" "1" "$(position "$NAMES" "$A_NAME")"
eq "…and the unpinned one falls behind"     "4" "$(position "$NAMES" "$D_NAME")"

eq "a sub-project cannot be pinned"  "400" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$W&pinned=true" "$PETER")"

eq "a pin can be released"     "200" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$C&pinned=false" "$PETER")"
eq "…freeing the cap"          "200" "$(code PUT "/ProjectsRS/SetProjectPinned?projectId=$D&pinned=true" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Two people, one engagement, two screens"

# Nalan has to be able to see the projects before his own order means anything.
for p in "$A" "$B"; do
  req POST /ProjectMembers/AddMember "$PETER" \
    "{\"ProjectId\":\"$p\",\"UserEmail\":\"$NALAN_EMAIL\",\"Specialization\":3}" > /dev/null
done

NALAN_DASH=$(req GET /ProjectsRS/GetPortfolioDashboard "$NALAN")

if [ "$(position "$(order "$NALAN_DASH")" "$A_NAME")" = "0" ]; then
  skip "the mediator's own order is independent" "not a member of these projects"
  skip "…and his pins are his own"               "not a member of these projects"
else
  # Peter has A pinned. Nalan has pinned nothing, so A must not be at his top
  # for Peter's reason — the preference row is keyed by user.
  eq "the mediator inherits no pin from the owner" \
     "false" "$(printf '%s' "$NALAN_DASH" | grep -o '"isPinned":true' | head -1 | grep -q . && echo true || echo false)"

  eq "…and can record an order of his own" "200" \
     "$(code PUT /ProjectsRS/ReorderProjects "$NALAN" "{\"Items\":[{\"ProjectId\":\"$B\",\"SortOrder\":0},{\"ProjectId\":\"$A\",\"SortOrder\":1}]}")"

  # The whole reason the order lives on tbl_ProjectPreference keyed by user.
  eq "…which does not move the owner's screen" "1" \
     "$(position "$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")" "$A_NAME")"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Delete does not delete — it archives for thirty days"

eq "deleting outright is refused"  "400" "$(code DELETE "/ProjectsRS/DeleteProject?projectId=$C" "$PETER")"
eq "…and the project is still there" "1" \
   "$(position "$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")" "$C_NAME" | grep -q '^0$' && echo 0 || echo 1)"

eq "a project can be binned"       "200" "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$C" "$PETER")"
eq "…and leaves the dashboard"     "0"   "$(position "$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")" "$C_NAME")"

BIN=$(req GET /ProjectsRS/GetArchivedProjects "$PETER")
eq "…and appears in the bin"       "1"   "$(position "$(order "$BIN")" "$C_NAME")"
eq "…with its thirty days on it"   "30"  "$(printf '%s' "$BIN" | grep -o '"daysUntilPurge":[0-9]*' | head -1 | sed 's/.*://')"
eq "…and who put it there"         "Peter Developer" \
   "$(printf '%s' "$BIN" | grep -o '"archivedByName":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')"

eq "it can be taken back out"      "200" "$(code PUT "/ProjectsRS/RestoreProject?projectId=$C" "$PETER")"
eq "…and is on the dashboard again" "0" \
   "$(position "$(order "$(req GET /ProjectsRS/GetPortfolioDashboard "$PETER")")" "$C_NAME" | grep -q '^0$' && echo 1 || echo 0)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A wing goes to the bin with its house, and comes back with it"

eq "binning the house takes the wing" "200" "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$A" "$PETER")"

BIN=$(req GET /ProjectsRS/GetArchivedProjects "$PETER")
eq "the wing is in the bin"           "1" \
   "$(printf '%s' "$BIN" | grep -c "$W_NAME")"
eq "…nested under its house, not loose" "0" "$(position "$(order "$BIN")" "$W_NAME")"

eq "restoring the wing alone is refused" "400" "$(code PUT "/ProjectsRS/RestoreProject?projectId=$W" "$PETER")"
eq "restoring the house brings it back"  "200" "$(code PUT "/ProjectsRS/RestoreProject?projectId=$A" "$PETER")"
eq "…and the wing is readable again"     "200" "$(code GET "/ProjectsRS/GetProjectById?projectId=$W" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Emptying the bin early is a second, explicit act"

eq "the project goes to the bin first" "200" "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$D" "$PETER")"
eq "…and only then can it be emptied"  "200" "$(code DELETE "/ProjectsRS/DeleteProject?projectId=$D" "$PETER")"
eq "…after which it is gone"           "404" "$(code GET "/ProjectsRS/GetProjectById?projectId=$D" "$PETER")"
eq "…and out of the bin too"           "0"   "$(position "$(order "$(req GET /ProjectsRS/GetArchivedProjects "$PETER")")" "$D_NAME")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Only the owning account may bin an engagement"

eq "the mediator cannot bin the owner's project" "403" \
   "$(code PUT "/ProjectsRS/ArchiveProject?projectId=$B" "$NALAN")"
eq "…and the project is untouched"               "200" \
   "$(code GET "/ProjectsRS/GetProjectById?projectId=$B" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A cover is an artifact address, checked against the project"

eq "a cover cannot point at a file that is not stored" "404" \
   "$(code PUT /ProjectsRS/SetProjectCover "$PETER" "{\"ProjectId\":\"$B\",\"ArtifactId\":\"00000000-0000-0000-0000-000000000000\"}")"

eq "a cover can be cleared"  "200" \
   "$(code PUT /ProjectsRS/SetProjectCover "$PETER" "{\"ProjectId\":\"$B\",\"ArtifactId\":null}")"

eq "a stranger cannot set a cover" "403" \
   "$(code PUT /ProjectsRS/SetProjectCover "$NALAN" "{\"ProjectId\":\"$C\",\"ArtifactId\":null}")"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
