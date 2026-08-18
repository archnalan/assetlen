#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the whole chain: P0 + P1 + P2 + P3, the arrangement and the bin,
# and P5 money and staging.
#
# Four suites, run in order, one exit code. They are kept separate because they
# answer different questions — e2e-p2-peter.sh asks whether the project belongs
# to the person who funds it, e2e-p3-ingest.sh asks whether he can get his year
# of history into it, e2e-p4-arrange.sh asks whether his screen is his own and
# whether deleting destroys the record — but a phase is only done when all of
# them are green together.
#
# Usage:  bash tools/e2e-all.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running, and pwsh for the P3 fixtures. Run from the repo root.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

c_head=$'\033[1m'; c_fail=$'\033[31m'; c_pass=$'\033[32m'; c_off=$'\033[0m'

FAILED=0
TOTAL_PASS=0; TOTAL_FAIL=0; TOTAL_SKIP=0

run() { # run LABEL SCRIPT
  printf "\n%s══ %s %s\n" "$c_head" "$1" "$c_off"

  local out
  out=$(bash "$2" "$API" "$ADMIN_EMAIL" "$ADMIN_PASS" 2>&1)
  local rc=$?
  printf '%s\n' "$out"

  # The per-suite tallies are re-read from the output rather than tracked here,
  # so this wrapper cannot drift out of step with what the suites report.
  local line
  line=$(printf '%s' "$out" | grep -oE '[0-9]+ passed, [0-9]+ failed, [0-9]+ skipped' | tail -1)
  if [ -n "$line" ]; then
    TOTAL_PASS=$((TOTAL_PASS + $(printf '%s' "$line" | awk '{print $1}')))
    TOTAL_FAIL=$((TOTAL_FAIL + $(printf '%s' "$line" | awk '{print $3}')))
    TOTAL_SKIP=$((TOTAL_SKIP + $(printf '%s' "$line" | awk '{print $5}')))
  fi

  [ "$rc" -eq 0 ] || FAILED=1
}

run "P0 + P1 + P2 — ownership, sides, the artifact store" tools/e2e-p2-peter.sh
run "P3 — ingest, the front door"                         tools/e2e-p3-ingest.sh
run "The reader's own screen, and the bin"                tools/e2e-p4-arrange.sh
run "P5 — the funding back-and-forth, and staging"        tools/e2e-p5-money-and-staging.sh

printf "\n%s══ Whole chain %s\n" "$c_head" "$c_off"
if [ "$FAILED" -eq 0 ]; then
  printf "  %s%d passed%s, %d failed, %d skipped — the chain is green\n\n" \
    "$c_pass" "$TOTAL_PASS" "$c_off" "$TOTAL_FAIL" "$TOTAL_SKIP"
else
  printf "  %s%d failed%s of %d — see above\n\n" \
    "$c_fail" "$TOTAL_FAIL" "$c_off" "$((TOTAL_PASS + TOTAL_FAIL))"
fi

exit "$FAILED"
