#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — three-user access regression suite (plan.md P0 exit criterion).
#
# Walks the three real personas of the vision — David (contractor/owner),
# Peter (developer/client) and Colin (clerk of works) — through the paths that
# finding A1 broke. Every one of these returned 403 before P0.
#
#   Usage:  bash tools/e2e-access-audit.sh [api-base] [owner-email] [owner-password]
#   Needs:  the API running on the https profile.
#
# Idempotent: re-running reuses the existing test users and memberships.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-https://localhost:7264/api}"
OWNER_EMAIL="${2:-1b1euserone@mowt.com}"
OWNER_PASS="${3:-password}"

PASS=0; FAIL=0
CURL=(curl -sk --max-time 30)

tok() { "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
        -d "{\"Email\":\"$1\",\"Password\":\"$2\"}" | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'; }

# status METHOD PATH TOKEN [JSON]
status() {
  if [ $# -ge 4 ] && [ -n "${4:-}" ]; then
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" \
      -H "Authorization: Bearer $3" -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

# expect LABEL EXPECTED ACTUAL
expect() {
  if [ "$2" = "$3" ]; then printf '  \033[32mPASS\033[0m  %-52s %s\n' "$1" "$3"; PASS=$((PASS+1))
  else printf '  \033[31mFAIL\033[0m  %-52s got %s, want %s\n' "$1" "$3" "$2"; FAIL=$((FAIL+1)); fi
}

echo "ASSETLEN access regression — $API"
echo

DAVID=$(tok "$OWNER_EMAIL" "$OWNER_PASS")
[ -z "$DAVID" ] && { echo "FATAL: owner login failed. Is the API up on the https profile?"; exit 1; }

# ── Fixtures: two teammates, both active members of David's first project ────
for U in "peter:Client:Peter:Developer:8:Developer" "clerk:Crew:Colin:Clerk:5:Clerk of works"; do
  IFS=: read -r name role fn ln spec title <<<"$U"
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" -H "Authorization: Bearer $DAVID" \
    -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$name@assetlen.test\",\"UserName\":\"$name\",\"FirstName\":\"$fn\",\"LastName\":\"$ln\",\"UserRolesDto\":{\"Roles\":[\"$role\"]},\"defaultRole\":[\"$role\"]}"
done

FIXTURE_NAME="AccessIsolationFixture"
SUBJECT_NAME="AccessAuditSubject"

# Subject = the owner's newest real project. Skip our own isolation fixture,
# which would otherwise be picked (the dashboard is newest-first) and has no stages.
find_subject() {
  "${CURL[@]}" "$API/ProjectsRS/GetPortfolioDashboard" -H "Authorization: Bearer $DAVID" \
    | tr '{' '\n' | grep '"projectName"' | grep -v "\"$FIXTURE_NAME\"" \
    | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'
}

PID=$(find_subject)
if [ -z "$PID" ]; then
  # A freshly migrated database has no projects (P1 dropped the dev DB and
  # rebaselined). Seed our own subject so the suite runs from zero.
  "${CURL[@]}" -o /dev/null -X POST "$API/ProjectsRS/CreateProject" -H "Authorization: Bearer $DAVID" \
    -H "Content-Type: application/json" \
    -d "{\"ProjectName\":\"$SUBJECT_NAME\",\"Description\":\"created by the access audit\",\"Location\":\"Kampala\",\"TotalBudget\":1000000,\"Currency\":\"UGX\",\"Stages\":[]}"
  PID=$(find_subject)
fi
[ -z "$PID" ] && { echo "FATAL: could not obtain a project to test against."; exit 1; }
echo "project under test: $PID"

for M in "peter@assetlen.test:8:Developer" "clerk@assetlen.test:5:Clerk of works"; do
  IFS=: read -r em spec title <<<"$M"
  "${CURL[@]}" -o /dev/null -X POST "$API/ProjectMembers/AddMember" -H "Authorization: Bearer $DAVID" \
    -H "Content-Type: application/json" \
    -d "{\"ProjectId\":\"$PID\",\"UserEmail\":\"$em\",\"Specialization\":$spec,\"Title\":\"$title\"}"
done

PETER=$(tok peter@assetlen.test password)
CLERK=$(tok clerk@assetlen.test password)
[ -z "$PETER" ] || [ -z "$CLERK" ] && { echo "FATAL: teammate login failed."; exit 1; }

SID=$("${CURL[@]}" "$API/Stages/GetStagesByProjectId?projectId=$PID" -H "Authorization: Bearer $DAVID" \
      | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
if [ -z "$SID" ]; then
  "${CURL[@]}" -o /dev/null -X POST "$API/Stages/CreateStage?projectId=$PID" -H "Authorization: Bearer $DAVID" \
    -H "Content-Type: application/json" \
    -d '{"StageName":"Regression stage","Description":"created by the access audit","BudgetAmount":1}'
  SID=$("${CURL[@]}" "$API/Stages/GetStagesByProjectId?projectId=$PID" -H "Authorization: Bearer $DAVID" \
        | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
fi
[ -z "$SID" ] && { echo "FATAL: no stage to capture against."; exit 1; }

# A 1x1 JPEG. Its base64 starts "/9j/" — the exact payload that finding A2 corrupted.
PIX="/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/wAALCAABAAEBAREA/8QAFAABAAAAAAAAAAAAAAAAAAAACf/EABQQAQAAAAAAAAAAAAAAAAAAAAD/2gAIAQEAAD8AKp//2Q=="
CAP="{\"ProjectId\":\"$PID\",\"StageId\":\"$SID\",\"Description\":\"Regression capture\",\"CompletionPercentage\":40,\"HasIssues\":false,\"Channel\":0,\"Images\":[{\"FileName\":\"a.jpg\",\"ContentType\":\"image/jpeg\",\"Base64Image\":\"$PIX\",\"DisplayOrder\":1}]}"

echo
echo "The clerk of works can do the job (David.md: capture must be dumber than WhatsApp)"
expect "clerk opens the project"        200 "$(status GET  "/ProjectsRS/GetProjectById?projectId=$PID" "$CLERK")"
expect "clerk reads the stage list"     200 "$(status GET  "/Stages/GetStagesByProjectId?projectId=$PID" "$CLERK")"
expect "clerk reads the site log"       200 "$(status GET  "/Progress/GetProgressUpdates?projectId=$PID" "$CLERK")"
expect "clerk captures a photo"         200 "$(status POST "/Progress/AddProgressUpdate" "$CLERK" "$CAP")"

echo
echo "Peter can hold a commitment against reality (Peter.md: the daily loop)"
expect "peter opens the project"        200 "$(status GET  "/ProjectsRS/GetProjectById?projectId=$PID" "$PETER")"
expect "peter reads the budget"         200 "$(status GET  "/Budget/GetSummary?projectId=$PID" "$PETER")"
expect "peter reads the journal"        200 "$(status GET  "/Progress/GetProgressUpdates?projectId=$PID" "$PETER")"
expect "peter raises a question"        200 "$(status POST "/Flags/AddFlag" "$PETER" "{\"ProjectId\":\"$PID\",\"Title\":\"Which tile?\",\"Description\":\"Need the spec\",\"Severity\":2}")"
expect "peter lists open questions"     200 "$(status GET  "/Flags/GetFlagsByProject?projectId=$PID" "$PETER")"
expect "peter sees funding vs progress" 200 "$(status GET  "/Funding/GetFundingByProject?projectId=$PID" "$PETER")"
expect "peter sees stage funding"       200 "$(status GET  "/Funding/GetFundingByStage?stageId=$SID" "$PETER")"
expect "peter sees project analytics"   200 "$(status GET  "/ProjectsRS/GetProjectAnalytics?projectId=$PID" "$PETER")"
expect "peter's search finds his job"   200 "$(status GET  "/ProjectsRS/SearchProjects?keywords=" "$PETER")"

echo
echo "Curation still holds (assetlen.md §5: David controls emphasis, not truth)"
expect "peter cannot edit the budget"   403 "$(status POST "/Budget/AddLineItem" "$PETER" "{\"ProjectId\":\"$PID\",\"Title\":\"x\",\"PlannedAmount\":1,\"Category\":0}")"
expect "clerk cannot publish to client" 403 "$(status PUT  "/Progress/SetChannel?updateId=00000000-0000-0000-0000-000000000000&channel=Client" "$CLERK")"

echo
echo "Membership is per-project, not global"
# A second project David owns and Peter is NOT a member of. Access must not
# leak sideways just because Peter is a member of the first one.
find_fixture() {
  "${CURL[@]}" "$API/ProjectsRS/SearchProjects?keywords=AccessIsolationFixture" \
    -H "Authorization: Bearer $DAVID" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'
}

OTHER=$(find_fixture)
if [ -z "$OTHER" ]; then
  # Create once, then re-read the id from search — the create response orders
  # its fields differently and cannot be scraped the same way.
  "${CURL[@]}" -o /dev/null -X POST "$API/ProjectsRS/CreateProject" -H "Authorization: Bearer $DAVID" \
    -H "Content-Type: application/json" \
    -d '{"ProjectName":"AccessIsolationFixture","Description":"regression fixture, not a real project","Location":"n/a","TotalBudget":1,"Currency":"UGX","Stages":[]}'
  OTHER=$(find_fixture)
fi

if [ -n "$OTHER" ] && [ "$OTHER" != "$PID" ]; then
  expect "peter denied a project he isn't on"  403 "$(status GET "/ProjectsRS/GetProjectById?projectId=$OTHER" "$PETER")"
  expect "clerk denied a project he isn't on"  403 "$(status GET "/ProjectsRS/GetProjectById?projectId=$OTHER" "$CLERK")"
else
  printf '  \033[33mSKIP\033[0m  isolation fixture unavailable\n'
fi

# Existence is not leaked: an id nobody owns is 404, never 403.
expect "unknown project reads as absent"       404 "$(status GET "/ProjectsRS/GetProjectById?projectId=00000000-0000-0000-0000-000000000000" "$PETER")"

echo
printf 'passed %d, failed %d\n' "$PASS" "$FAIL"
[ "$FAIL" -eq 0 ] || exit 1
