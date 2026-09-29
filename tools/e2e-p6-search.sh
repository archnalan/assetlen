#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P6 end-to-end suite: retrieval, Peter's four searches.
#
# Every assertion below is asked from Peter's chair (plan.md P6):
#
#   Is a receipt that only ever existed as a photo inside an export findable
#   by its vendor's name?                                   ← the exit criterion
#   Does "what did I approve on the balustrade?" answer with the commitment,
#   not a list of messages?
#   Does every result carry where it came from and where it has got to —
#   agreed → evidence → invoiced → cleared → queried → resolved?
#   Is a file tied to nothing said to be tied to nothing?
#   Is every result exactly as private as the thing it points at — by side,
#   by seat, by money, and to a stranger not at all?
#
# Usage:  bash tools/e2e-p6-search.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running with an OCR engine (Windows OCR or Tesseract), pwsh
#         for the fixtures and for reading the grouped JSON. Run from the repo
#         root. Idempotent — each run builds its own project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/p6"

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
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

# The answer is grouped and nested — provenance inside hits inside groups — so
# it is read with a real JSON parser rather than grep. $r is the parsed result;
# g KIND is the group of that kind, or an empty group when it is absent.
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    function g(\$k) { \$x = @(\$r.groups | Where-Object { \$_.kind -eq \$k }); if (\$x.Count) { \$x[0] } else { [pscustomobject]@{ total = 0; hits = @() } } }
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } else { \$out }"
}

# search TOKEN QUERY → the JSON answer, scoped to this run's project so that
# earlier runs — the same pseudonymous people on older projects — cannot answer.
# search_all TOKEN QUERY asks across every project, for the stranger.
enc() { printf '%s' "$1" | sed 's/ /%20/g;s/?/%3F/g'; }
search()     { req GET "/Search/Query?q=$(enc "$2")&projectId=$PID" "$1"; }
search_all() { req GET "/Search/Query?q=$(enc "$2")" "$1"; }

echo "ASSETLEN P6 — retrieval, Peter's four searches — $API"

# ── Fixtures ─────────────────────────────────────────────────────────────────
bash tools/make-search-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
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

PETER=$(tok peter.buyer@assetlen.test password)
NALAN=$(tok nalan.arch@assetlen.test password)
KATO=$(tok  kato.foreman@assetlen.test password)
DINAH=$(tok dinah.principal@assetlen.test password)
MARA=$(tok  mara.stranger@assetlen.test password)
for t in PETER NALAN KATO DINAH MARA; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Peter Search $STAMP\",\"Description\":\"P6 subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Terrace finishes\",\"DisplayOrder\":1}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }

# Nalan on the delivery side, Kato on his bench, Dinah on Peter's side but
# deliberately off the money (HandlesMoney=false). Mara is on no project at all.
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"IsMediator\":false,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.principal@assetlen.test\",\"Specialization\":9,\"Side\":0,\"HandlesMoney\":false,\"Title\":\"Representative\"}" >/dev/null

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
NALAN_MID=$(member_id '"userFullName":"Nalan Architect"')
PETER_MID=$(member_id '"userFullName":"Peter Developer"')
KATO_MID=$(member_id '"userFullName":"Kato Foreman"')
DINAH_MID=$(member_id '"userFullName":"Dinah Principal"')

import_zip() { # import_zip TOKEN ZIP MAPPINGS-JSON → final status
  local pre batch
  pre=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $1" -F "file=@$2" -F "projectId=$PID")
  batch=$(jget "$pre" batchId)
  [ -z "$batch" ] && { echo "preview failed: $(printf '%s' "$pre" | head -c 300)"; return; }
  jget "$(req POST /Ingest/CommitImport "$1" "{\"BatchId\":\"$batch\",\"AuthorMappings\":$3}")" status
}

poll_text() { # poll_text TOKEN ARTIFACT → status once no longer Pending (90 s ceiling)
  local out
  for _ in $(seq 1 45); do
    out=$(req GET "/Extraction/GetArtifactText?artifactId=$2" "$1")
    [ "$(jget "$out" status)" != "Pending" ] && { jget "$out" status; return; }
    sleep 2
  done
  jget "$out" status
}

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 0 — Peter imports his own thread, photos and all"

ST=$(import_zip "$PETER" "$FIXTURES/$P6_CLIENT_ZIP" "[
      {\"ExternalAuthor\":\"Nalan\",\"MemberId\":\"$NALAN_MID\"},
      {\"ExternalAuthor\":\"Peter\",\"MemberId\":\"$PETER_MID\"},
      {\"ExternalAuthor\":\"Dinah\",\"MemberId\":\"$DINAH_MID\"}]")
eq "the export with its photo imports"            "Completed" "$ST"

MEDIA=$(req GET "/Ingest/GetMessages?ProjectId=$PID&MediaOnly=true&Take=10" "$PETER")
RECEIPT=$(jget "$MEDIA" artifactId)
[ -n "$RECEIPT" ] && ok "the receipt photo is stored as an artifact" "$RECEIPT" || bad "the receipt photo is stored as an artifact" "none" "an id"
eq "…and the vendor's name is nowhere in the thread" "0" \
   "$(req GET "/Ingest/GetMessages?ProjectId=$PID&Search=$P6_VENDOR" "$PETER" | grep -o '"id":"' | wc -l | tr -d ' ')"

OCR=$(poll_text "$PETER" "$RECEIPT")

# ═════════════════════════════════════════════════════════════════════════════
head_ "Exit — a receipt that only existed as a photo is findable by its vendor"

if [ "$OCR" = "Done" ]; then
  ok "the photo was read by OCR" "$OCR"
  S=$(search "$PETER" "$P6_VENDOR")
  eq "searching the vendor finds one file"         "1" "$(printf '%s' "$S" | jx '(g File).total')"
  eq "…and it is that receipt"                     "$RECEIPT" "$(printf '%s' "$S" | jx '(g File).hits[0].artifactId')"
  eq "…found in the text read from the photo"      "the text read from the photo" "$(printf '%s' "$S" | jx '(g File).hits[0].matchedIn')"
  eq "…with the vendor in the passage shown"       "true" "$(printf '%s' "$S" | jx "(g File).hits[0].snippet -match '$P6_VENDOR'")"
  eq "no message matched — only the pixels did"    "0" "$(printf '%s' "$S" | jx '(g Message).total')"
  eq "its source chip: from the thread"            "Thread" "$(printf '%s' "$S" | jx '(g File).hits[0].provenance.origin')"
  eq "…sent by Nalan"                              "Nalan Architect" "$(printf '%s' "$S" | jx '(g File).hits[0].provenance.who')"
  eq "…on the day it was sent, not import day"     "2026-08-05 16:14" "$(printf '%s' "$S" | jx '([datetime](g File).hits[0].provenance.at).ToString("yyyy-MM-dd HH:mm")')"
  eq "…and it opens at that day of the thread"     "/project/$PID/history?day=2026-08-05" "$(printf '%s' "$S" | jx '(g File).hits[0].href')"
  eq "a file tied to nothing says so"              "Not tied to any commitment yet" "$(printf '%s' "$S" | jx '(g File).hits[0].provenance.gap')"
  eq "lower case, two words, still found"          "1" "$(search "$PETER" "zentara tiles" | jx '(g File).total')"
  eq "asked as a question, still found"            "1" "$(search "$PETER" "where is the receipt from Zentara?" | jx '(g File).total')"
  eq "the server says how it matched"              "true" "$(printf '%s' "$S" | jx '$r.backend -in @("substring","full-text")')"
  # Postgres (CLAUDE.md §5.1.1): pg_trgm word similarity forgives one misread letter.
  if [ "$(printf '%s' "$S" | jx '$r.backend')" = "full-text" ]; then
    eq "an OCR misread still finds it: ZENTAHA"    "$RECEIPT" "$(search "$PETER" "zentaha" | jx '(g File).hits[0].artifactId')"
  else
    skip "an OCR misread still finds it" "substring backend has no fuzzy matching"
  fi
else
  skip "the photo was read by OCR" "status $OCR — no OCR engine on this host (Ocr:TesseractPath)"
  skip "the exit criterion" "cannot be shown without an OCR engine"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "What did I approve on the balustrade? — the commitment, not the messages"

GO=$(req GET "/Ingest/GetMessages?ProjectId=$PID&Search=Go%20ahead" "$PETER")
GO_ID=$(printf '%s' "$GO" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

BAL=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"Kind\":4,\"Title\":\"Terrace balustrade in brushed stainless steel with toughened glass\",\"Body\":\"1.1m high, 10mm glass\",\"SourceChannel\":1,\"IngestedMessageId\":\"$GO_ID\",\"AgreedAt\":\"2026-08-03T10:30:00\",\"Amount\":4200000,\"Currency\":\"UGX\",\"Maturity\":2}")
BAL_ID=$(jget "$BAL" id)
TILES=$(req POST /Commitments/AddCommitment "$PETER" \
  "{\"ProjectId\":\"$PID\",\"Kind\":3,\"Title\":\"Floor tiles, forty boxes\",\"AgreedAt\":\"2026-08-01T09:00:00\",\"Amount\":3600000,\"Currency\":\"UGX\",\"Maturity\":2}")
TILES_ID=$(jget "$TILES" id)
[ -n "$BAL_ID" ] && [ -n "$TILES_ID" ] && ok "two commitments on the register" "" || bad "two commitments on the register" "$BAL / $TILES" "two ids"

A=$(search "$PETER" "What did I approve on the balustrade?")
eq "the question is reduced to what to look for"  "balustrade" "$(printf '%s' "$A" | jx '$r.terms -join ","')"
eq "…and 'approve' is set aside, not dropped silently" "true" "$(printf '%s' "$A" | jx '$r.setAside -contains "approve"')"
eq "commitments come first"                        "Commitment" "$(printf '%s' "$A" | jx '$r.groups[0].kind')"
eq "…and the answer is the balustrade commitment"  "$BAL_ID" "$(printf '%s' "$A" | jx '(g Commitment).hits[0].id')"
eq "the message it was agreed in is folded into it" "0" "$(printf '%s' "$A" | jx '@((g Message).hits | Where-Object { $_.title -match "Go ahead" }).Count')"
eq "the proposal is still found in the thread"     "1" "$(printf '%s' "$A" | jx '@((g Message).hits | Where-Object { $_.title -match "we propose" }).Count')"
eq "the strip starts at agreed"                    "Agreed:true" "$(printf '%s' "$A" | jx '$s = (g Commitment).hits[0].provenance.steps[0]; "$($s.label):$($s.reached.ToString().ToLower())"')"
eq "…with six steps"                               "Agreed,Evidence,Invoiced,Cleared,Queried,Resolved" "$(printf '%s' "$A" | jx '((g Commitment).hits[0].provenance.steps | ForEach-Object step) -join ","')"
eq "…and nothing invoiced yet"                     "false" "$(printf '%s' "$A" | jx '((g Commitment).hits[0].provenance.steps | Where-Object step -eq "Invoiced").reached')"
eq "its source chip says it came from the thread"  "Thread" "$(printf '%s' "$A" | jx '(g Commitment).hits[0].provenance.origin')"
eq "it opens on the register, at that commitment"  "/project/$PID/register?commitment=$BAL_ID" "$(printf '%s' "$A" | jx '(g Commitment).hits[0].href')"
eq "Peter reads the figure"                        "4200000" "$(printf '%s' "$A" | jx '[int](g Commitment).hits[0].amount')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Provenance — the receipt is the invoiced step of the tiles"

req POST /Commitments/AddLink "$PETER" \
  "{\"CommitmentId\":\"$TILES_ID\",\"TargetType\":1,\"TargetId\":\"$RECEIPT\",\"Relation\":2,\"Note\":\"Receipt from the thread\"}" >/dev/null
eq "Peter clears the tiles"                        "200" "$(code PUT "/Commitments/Clear?commitmentId=$TILES_ID" "$PETER")"

if [ "$OCR" = "Done" ]; then
  Z=$(search "$PETER" "$P6_VENDOR")
  eq "the receipt now names what it bills"         "Floor tiles, forty boxes" "$(printf '%s' "$Z" | jx '(g File).hits[0].provenance.commitmentTitle')"
  eq "…as its invoice"                             "Invoice for" "$(printf '%s' "$Z" | jx '(g File).hits[0].provenance.role')"
  eq "…the strip marks this file as 'invoiced'"    "Invoiced" "$(printf '%s' "$Z" | jx '((g File).hits[0].provenance.steps | Where-Object isThis).step')"
  eq "…and shows the tiles cleared"                "true" "$(printf '%s' "$Z" | jx '((g File).hits[0].provenance.steps | Where-Object step -eq "Cleared").reached')"
  eq "…and no gap is claimed any more"             "" "$(printf '%s' "$Z" | jx '(g File).hits[0].provenance.gap')"
else
  skip "the receipt's provenance" "no OCR engine"
fi

T=$(search "$PETER" "floor tiles")
eq "the tiles commitment shows invoiced"           "true" "$(printf '%s' "$T" | jx '(((g Commitment).hits | Where-Object id -eq "'"$TILES_ID"'").provenance.steps | Where-Object step -eq "Invoiced").reached')"
eq "…and cleared"                                  "true" "$(printf '%s' "$T" | jx '(((g Commitment).hits | Where-Object id -eq "'"$TILES_ID"'").provenance.steps | Where-Object step -eq "Cleared").reached')"

req PUT /Commitments/RaiseQuery "$PETER" "{\"CommitmentId\":\"$BAL_ID\",\"Note\":\"Is the glass 10mm or 8mm?\"}" >/dev/null
req PUT /Commitments/ResolveQuery "$NALAN" "{\"CommitmentId\":\"$BAL_ID\",\"Note\":\"10mm, as agreed on 3 Aug\"}" >/dev/null
Q=$(search "$PETER" "balustrade")
eq "a query on it lights 'queried'"                "true" "$(printf '%s' "$Q" | jx '((g Commitment).hits[0].provenance.steps | Where-Object step -eq "Queried").reached')"
eq "…and its answer lights 'resolved'"             "true" "$(printf '%s' "$Q" | jx '((g Commitment).hits[0].provenance.steps | Where-Object step -eq "Resolved").reached')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Seats — a result is exactly as private as the thing it points at"

D=$(search "$DINAH" "balustrade")
eq "Dinah, a principal, finds the commitment"      "$BAL_ID" "$(printf '%s' "$D" | jx '(g Commitment).hits[0].id')"
eq "…but she is off the money: the figure is hidden" "true" "$(printf '%s' "$D" | jx '(g Commitment).hits[0].amountHidden')"
eq "…and no amount travels"                        "" "$(printf '%s' "$D" | jx '(g Commitment).hits[0].amount')"

K=$(search "$KATO" "balustrade")
eq "the foreman has no register to search"         "0" "$(printf '%s' "$K" | jx '(g Commitment).total')"
eq "…nor Peter's thread"                           "0" "$(printf '%s' "$K" | jx '(g Message).total')"
if [ "$OCR" = "Done" ]; then
  eq "…nor the receipt photo in it"                "0" "$(search "$KATO" "$P6_VENDOR" | jx '$r.totalHits')"
fi

M=$(search_all "$MARA" "$P6_VENDOR")
eq "a stranger searches no project of Peter's"     "0" "$(printf '%s' "$M" | jx '$r.projectsSearched')"
eq "…and finds nothing"                            "0" "$(printf '%s' "$M" | jx '$r.totalHits')"
eq "…and naming the project gets 404, not 403"     "404" "$(code GET "/Search/Query?q=$P6_VENDOR&projectId=$PID" "$MARA")"
eq "question words alone are not a search (400)"   "400" "$(code GET "/Search/Query?q=what%20did%20the" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Sides — the crew's own record stays the crew's (D5)"

# Peter appoints Nalan and steps back (§10.1). The delivery side's export then
# comes in, with a receipt of its own.
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null

ST2=$(import_zip "$NALAN" "$FIXTURES/$P6_CREW_ZIP" "[
      {\"ExternalAuthor\":\"Nalan\",\"MemberId\":\"$NALAN_MID\"},
      {\"ExternalAuthor\":\"Kato\",\"MemberId\":\"$KATO_MID\"}]")
eq "the crew's export imports"                     "Completed" "$ST2"

CREW_MEDIA=$(req GET "/Ingest/GetMessages?ProjectId=$PID&MediaOnly=true&From=2026-08-08T00:00:00&Take=10" "$NALAN")
CREW_RECEIPT=$(jget "$CREW_MEDIA" artifactId)
CREW_OCR=$(poll_text "$NALAN" "$CREW_RECEIPT")

C_N=$(search "$NALAN" "balustrade posts")
C_P=$(search "$PETER" "balustrade posts")
eq "the mediator finds the crew's message"         "1" "$(printf '%s' "$C_N" | jx '@((g Message).hits | Where-Object { $_.title -match "posts collected" }).Count')"
eq "…Peter does not"                               "0" "$(printf '%s' "$C_P" | jx '@((g Message).hits | Where-Object { $_.title -match "posts collected" }).Count')"

if [ "$CREW_OCR" = "Done" ]; then
  eq "the mediator finds the crew's receipt"       "1" "$(search "$NALAN" "$P6_CREW_VENDOR" | jx '(g File).total')"
  eq "…Peter does not, by vendor"                  "0" "$(search "$PETER" "$P6_CREW_VENDOR" | jx '$r.totalHits')"
  eq "…nor by what is written on it"               "0" "$(printf '%s' "$C_P" | jx '@((g File).hits | Where-Object { $_.snippet -match "QUILLON" }).Count')"
  eq "…nor does Dinah"                             "0" "$(search "$DINAH" "$P6_CREW_VENDOR" | jx '$r.totalHits')"
else
  skip "the crew's receipt" "status $CREW_OCR"
fi
eq "Peter still finds his own thread"              "1" "$(search "$PETER" "laundry tiles" | jx '(g Message).total')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The Site Diary — the delivery side's, unless a mediator exposes it"

ENTRY=$(req POST /Progress/AddProgressUpdate "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"Description\":\"Balustrade posts fixed on the terrace, awaiting the glass\",\"Channel\":0}")
ENTRY_ID=$(oid "$ENTRY")
eq "the mediator finds his Diary entry"            "$ENTRY_ID" "$(search "$NALAN" "posts fixed" | jx '(g DiaryEntry).hits[0].id')"
eq "…the foreman, who works from it, finds it too" "1" "$(search "$KATO" "posts fixed" | jx '(g DiaryEntry).total')"
eq "…Peter does not — it is crew-only"             "0" "$(search "$PETER" "posts fixed" | jx '(g DiaryEntry).total')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Where it could not look is said, not hidden"

W=$(search "$PETER" "tiles")
eq "the answer counts files still awaiting text"   "true" "$(printf '%s' "$W" | jx '$r.filesAwaitingText -is [long] -or $r.filesAwaitingText -is [int]')"
eq "…and names the OCR engine"                     "true" "$(printf '%s' "$W" | jx '$r.PSObject.Properties.Name -contains "ocrEngine"')"
eq "one project searched when scoped to it"        "1" "$(search "$PETER" "tiles" | jx '$r.projectsSearched')"
eq "unscoped, every project he stands on is searched" "true" "$(search_all "$PETER" "tiles" | jx '$r.projectsSearched -ge 1')"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
