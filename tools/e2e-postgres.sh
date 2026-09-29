#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — what moving to PostgreSQL could quietly break (CLAUDE.md §5.1.1).
#
# SQL Server compared text case-insensitively, stored any character, and
# translated a few LINQ shapes Npgsql does not. Every assertion below is a
# defect that was found on Postgres after the rest of the chain was green, or a
# lookup that used to work only because the old collation forgave it:
#
#   Signing in       an address typed in capitals, or with a stray space
#   User search      a first name in the other case; a keyword that matches nobody
#   The front door   a mailed-in address in capitals; a NUL pasted from a PDF
#   The clock        a row written now reads back as now, not three hours off
#   Nobody signed in the cutoff and the milestone run, called as the jobs call them
#
# Usage:  bash tools/e2e-postgres.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running in Development.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-https://localhost:7264/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"
DEMO_PASS="Assetlen#2026"
DEMO_PID="de300000-0000-4000-8000-000000000010"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 60)

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-60s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-60s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-60s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
has()  { if printf '%s' "$3" | grep -qF -- "$2"; then ok "$1" "$2"; else bad "$1" "$(printf '%s' "$3" | head -c 160)" "…$2…"; fi; }
head_(){ printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

tok() {
  "${CURL[@]}" -X POST "$API/Authorization/Login" -H "Content-Type: application/json" \
    -d "{\"Email\":\"$1\",\"Password\":\"$2\"}" \
    | grep -o '"token":"[^"]*"' | sed 's/.*:"//;s/"$//'
}
req() {  # req METHOD PATH TOKEN
  "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3"
}
code() { # code METHOD PATH TOKEN
  "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
}
jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
count(){ printf '%s' "$1" | grep -o "$2" | wc -l | tr -d ' '; }

echo "ASSETLEN — PostgreSQL regressions — $API"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"
ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
PETER=$(tok peter@assetlen.dev "$DEMO_PASS")
[ -z "$ADMIN" ] || [ -z "$PETER" ] && { echo "FATAL: sign-in failed. Is the API up in Development?"; exit 1; }

# ═════════════════════════════════════════════════════════════════════════════
head_ "Signing in is not case-sensitive"

[ -n "$(tok '  PETER@AssetLen.DEV ' "$DEMO_PASS")" ] \
  && ok "an address in capitals, with stray spaces, signs in" "PETER@AssetLen.DEV" \
  || bad "an address in capitals, with stray spaces, signs in" "no token" "a token"
[ -n "$(tok 'PETER' "$DEMO_PASS")" ] \
  && ok "…and so does the user name in capitals" "PETER" \
  || bad "…and so does the user name in capitals" "no token" "a token"

# ═════════════════════════════════════════════════════════════════════════════
head_ "User search"

# FirstName.ToString() translated on SQL Server and threw on Npgsql, so both lists 500'd.
R=$(req GET "/Users/SearchUsersForComboBoxes?keywords=SYSTEM" "$ADMIN")
has "the picker finds a first name typed in the other case"       "System Admin" "$R"
eq  "…and the employee list answers too"  "200" "$(code GET "/Users/SearchForEmployees?keywords=system" "$ADMIN")"
has "…with the same person in it"          "userone" "$(req GET "/Users/SearchForEmployees?keywords=system" "$ADMIN")"

# The keyword test was inverted: a search for anyone returned everyone.
R=$(req GET "/Users/SearchUserByKeywords?keywords=zzqqnobodyzz" "$ADMIN")
eq  "a keyword that matches nobody returns nobody"  "0" "$(count "$R" '"userName":')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The front door"

INBOX=$(req GET "/Ingest/GetInbox?projectId=$DEMO_PID" "$PETER")
ADDR=$(jget "$INBOX" emailAddress)
SECRET="${INGEST_SECRET:-dev-inbound-secret-not-for-deployment}"
mail() { # mail TO BODY → response body + status on the last line
  "${CURL[@]}" -w '\n%{http_code}' -X POST "$API/Ingest/InboundEmail" \
    -H "Content-Type: application/json" -H "X-Assetlen-Ingest-Secret: $SECRET" \
    -d "{\"To\":\"$1\",\"From\":\"PETER@AssetLen.dev\",\"Subject\":\"Postgres probe\",\"TextBody\":\"$2\"}"
}

if [ -z "$ADDR" ]; then
  bad "the demo project has an inbound address" "$INBOX" "in+<key>@<domain>"
elif [ "$(mail "$ADDR" "probe" | tail -1)" = "503" ]; then
  skip "inbound mail" "Ingest:InboundSecret unset — add it to appsettings.json (untracked) or set INGEST_SECRET"
else
  UPPER=$(printf '%s' "$ADDR" | tr '[:lower:]' '[:upper:]')
  OUT=$(mail "$UPPER" "Addressed in capitals")
  eq "mail to the address typed in capitals still lands" "200" "$(printf '%s' "$OUT" | tail -1)"

  # Postgres text cannot hold U+0000; SQL Server stored it, so one pasted from a PDF 500'd the save.
  STAMP=$(date +%s)
  OUT=$(mail "$ADDR" "Deposit slip $STAMP pasted from a PDF\\u0000 with a NUL")
  eq "a body carrying a NUL character is accepted" "200" "$(printf '%s' "$OUT" | tail -1)"
  BATCH=$(jget "$(printf '%s' "$OUT" | sed '$d')" id)
  MSGS=$(req GET "/Ingest/GetMessages?ProjectId=$DEMO_PID&BatchId=$BATCH" "$PETER")
  # Mail joins the day's mailbox batch, so pick this message out by its stamp.
  STARTED=$(printf '%s' "$MSGS" | grep -o "\"sentAt\":\"[^\"]*\",\"body\":\"[^\"]*Deposit slip $STAMP" \
            | head -1 | sed 's/^"sentAt":"//;s/".*//')
  has "…and reads back with only the NUL gone"  "Deposit slip $STAMP pasted from a PDF with a NUL" "$MSGS"

  FOUND=$(req GET "/Ingest/GetMessages?ProjectId=$DEMO_PID&Search=DEPOSIT%20SLIP%20$STAMP" "$PETER")
  has "the thread filter ignores case"  "Deposit slip $STAMP" "$FOUND"

  # ── The clock ──────────────────────────────────────────────────────────────
  # A mail with no Date header is stamped DateTime.UtcNow and read back from the
  # database here; a Kind slip in the timestamptz converter shows up as the zone's offset.
  NOW_H=$(date -u +%Y-%m-%dT%H); PREV_H=$(date -u -d '-1 hour' +%Y-%m-%dT%H 2>/dev/null || echo none)
  case "$STARTED" in
    "$NOW_H"*|"$PREV_H"*) ok "an instant written now reads back as now (UTC)" "$STARTED" ;;
    *) bad "an instant written now reads back as now (UTC)" "$STARTED" "$NOW_H:…" ;;
  esac
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "The scheduled jobs, with nobody signed in"

# Exactly what the brief-cutoff job runs: no clock override, no project, no caller.
eq "the brief cutoff runs anonymously, as its job does"  "200" \
   "$("${CURL[@]}" -o /dev/null -w '%{http_code}' -X POST "$API/Curation/RunCutoff")"
# Exactly what works-report-milestones runs every hour: every project, now.
eq "the milestone run covers every project"             "200" \
   "$(code POST "/WorksReport/RunSchedule?weekly=false" "$ADMIN")"

printf "\n  %d passed, %d failed, %d skipped\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
