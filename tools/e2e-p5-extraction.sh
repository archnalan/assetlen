#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P5 end-to-end suite: extraction, pile into register.
#
# Under Law 0 extraction is the only path from Peter's forwarded pile to a
# register (plan.md P5, assetlen.md Law 3). Every assertion below is asked from
# that position:
#
#   Does "Okay" / "Noted" / "Good progress" produce nothing?
#   Does a material schedule become one proposal per material?
#   Is a figure pushed back on held as contested, not agreed?
#   Are "tomorrow", "by Tuesday", "this week" pinned to the right calendar day?
#   Do the contractor's "Summary update" percentages become progress readings?
#   Can Peter clear the queue in bulk, and is the accept rate measured?
#   Does a re-run propose nothing twice — not even what he rejected?
#   Is a proposal exactly as private as the message it came from?
#   Do loose photos find their <Media omitted> line by the stamp in their name,
#   whatever folder they were filed in?
#   Is every new file read for text (OCR), off the request path?
#
# Usage:  bash tools/e2e-p5-extraction.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running, pwsh for the fixtures. Run from the repo root.
#         Idempotent — each run builds its own project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/p5"

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
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3" \
      -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
jnum() { printf '%s' "$1" | grep -o "\"$2\":[-0-9.]*" | head -1 | sed 's/.*://' || true; }
jbool() { printf '%s' "$1" | grep -o "\"$2\":\(true\|false\)" | head -1 | sed 's/.*://' || true; }
oid()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

# The queue's proposals, one JSON object per line. They carry no nested objects,
# so splitting on "},{" is exact; the per-rule stats are told apart by "kind".
# The array is cut off before the per-rule stats, whose first object would
# otherwise ride along on the last proposal's line.
props() { printf '%s' "$1" | sed 's/\],"pendingCount".*//' | sed 's/},{/}\n{/g' | grep '"ingestedMessageId"' || true; }
pcount() { props "$1" | grep -c -- "$2" || true; }
# Matched on the title only: every proposal also carries its whole source
# message, so a looser match finds the wrong item in a multi-line message.
pline() { props "$1" | grep -- "\"title\":\"[^\"]*$2" | head -1; }
pid_of() { pline "$1" "$2" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

echo "ASSETLEN P5 — extraction, pile into register — $API"

# ── Fixtures ─────────────────────────────────────────────────────────────────
bash tools/make-extraction-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
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

PETER=$(tok peter.buyer@assetlen.test password)
NALAN=$(tok nalan.arch@assetlen.test password)
KATO=$(tok  kato.foreman@assetlen.test password)
DINAH=$(tok dinah.principal@assetlen.test password)
for t in PETER NALAN KATO DINAH; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Peter Extraction $STAMP\",\"Description\":\"P5 subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Retaining wall\",\"DisplayOrder\":1},{\"StageName\":\"Guest wing plaster\",\"DisplayOrder\":2}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }

STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
S_PLASTER=$(printf '%s' "$STAGES" | sed 's/},{/}\n{/g' | grep '"stageName":"Guest wing plaster"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

# Nalan on the delivery side, Kato on his bench. Peter created the project, so
# he is its mediator for now.
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"IsMediator\":false,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
NALAN_MID=$(member_id '"userFullName":"Nalan Architect"')
PETER_MID=$(member_id '"userFullName":"Peter Developer"')

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 0 — Peter imports his own thread; extraction runs on its own"

PRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $PETER" \
  -F "file=@$FIXTURES/$P5_THREAD" -F "projectId=$PID")
BATCH=$(jget "$PRE" batchId)
[ -z "$BATCH" ] && { echo "FATAL: preview failed: $(printf '%s' "$PRE" | head -c 300)"; exit 1; }

COMMIT=$(req POST /Ingest/CommitImport "$PETER" \
  "{\"BatchId\":\"$BATCH\",\"AuthorMappings\":[
      {\"ExternalAuthor\":\"Nalan\",\"MemberId\":\"$NALAN_MID\"},
      {\"ExternalAuthor\":\"Peter\",\"MemberId\":\"$PETER_MID\"},
      {\"ExternalAuthor\":\"Dinah\",\"CreateAsPartyName\":\"Dinah (thread)\",\"Side\":0,\"Specialization\":9}]}")
eq "the thread imports"                          "Completed" "$(jget "$COMMIT" status)"

# No "run" call: the import itself queues the reading of the record, so the
# register is waiting the next time Peter looks.
Q=$(req GET "/Extraction/GetQueue?projectId=$PID" "$PETER")
eq "extraction ran on import, unasked"           "rules-v1" "$(jget "$Q" engine)"
eq "twenty-one proposals from 34 messages"       "21" "$(jnum "$Q" pendingCount)"
eq "every one waits for a person"                "21" "$(pcount "$Q" '"status":"Pending"')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 3 — money, materials, dates, decisions; acknowledgements yield nothing"

ACKS=$(props "$Q" | grep -c -E '"sourceExcerpt":"(Okay|Noted|Good progress today|Thank you for the update)"' || true)
eq "'Okay', 'Noted', 'Good progress' propose nothing" "0" "$ACKS"
eq "a question commits nobody"                   "0" "$(pcount "$Q" '"title":"How long will the excavation')"

eq "the schedule is one proposal per material"   "5" "$(pcount "$Q" '"rule":"material-spec"')"
eq "quantities become material proposals"        "3" "$(pcount "$Q" '"rule":"material-quantity"')"
eq "three lines of one order are one proposal"   "1" "$(pcount "$Q" '1 sinotruck of sand; 1 sinotruck of aggregate; 20 T16 and 160 T12')"
eq "'confirmed on site' is a delivery"           "Delivered" "$(jget "$(pline "$Q" 'confirmed on site')" maturity)"

LABOUR=$(pline "$Q" 'Labour shall be 12M')
eq "12M is read as UGX 12,000,000"               "12000000" "$(jnum "$LABOUR" amount | sed 's/\.0*$//')"
eq "'too high for labour' marks it contested"    "true" "$(jbool "$LABOUR" contested)"
eq "…so it lands In discussion, not Agreed"      "InDiscussion" "$(jget "$LABOUR" maturity)"
eq "the pushback adds no second proposal"        "0" "$(pcount "$Q" '"title":"UGX 12M is too high')"

eq "a release announced is money with a date"    "2026-07-07" "$(jget "$(pline "$Q" 'UGX 50M')" dueDate | cut -c1-10)"

eq "the decision owed is Peter's to make"        "Client" "$(jget "$(pline "$Q" 'power points')" owedBySide)"
eq "a directive from Peter is a decision"        "decision-directive" "$(jget "$(pline "$Q" 'I want terrazzo')" rule)"
eq "'yes' to a recommendation is one decision"   "decision-affirmed" "$(jget "$(pline "$Q" 'Yes, all the space up there')" rule)"
eq "…and the bare recommendation is not queued"  "0" "$(pcount "$Q" '"rule":"recommendation"')"
eq "a no-show is a blocker with an owner"        "Epoxy team" "$(jget "$(pline "$Q" 'show up')" partyName)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Relative dates — pinned to the day they were said, never to import day"

eq "'by end of tomorrow' said 11 Jun → 12 Jun"   "2026-06-12" "$(jget "$(pline "$Q" 'first half ready')" dueDate | cut -c1-10)"
eq "'Tomorrow' answering 'when?' → 23 Jun"       "2026-06-23" "$(jget "$(pline "$Q" 'When do you intend to cast')" dueDate | cut -c1-10)"
eq "'by Tuesday' said Mon 29 Jun → 30 Jun"       "2026-06-30" "$(jget "$(pline "$Q" 'by Tuesday')" dueDate | cut -c1-10)"
eq "'2morrow' in a summary bullet → 3 Jul"       "2026-07-03" "$(jget "$(pline "$Q" '2morrow')" dueDate | cut -c1-10)"
eq "'this week' said Fri 3 Jul → Sun 5 Jul"      "2026-07-05" "$(jget "$(pline "$Q" 'this week')" dueDate | cut -c1-10)"
eq "…and the words it was read from are kept"    "this week" "$(jget "$(pline "$Q" 'this week')" dateText)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Summary updates — percentages become progress readings (works-report §4.2)"

R=$(req GET "/Extraction/GetReadings?projectId=$PID" "$PETER")
eq "two readings read from the thread"           "2" "$(printf '%s' "$R" | grep -o '"sourceKind":"Ingested"' | wc -l | tr -d ' ')"
eq "the summary bullet's 80% is kept"            "1" "$(printf '%s' "$R" | sed 's/},{/}\n{/g' | grep '"percent":80' | grep -c '"subject":"Guest wing plastering"')"
eq "'90% is already done' borrows its subject"   "1" "$(printf '%s' "$R" | sed 's/},{/}\n{/g' | grep '"percent":90' | grep -c '"subject":"Guest wing plaster"')"
eq "…and both file against the stage they name"  "2" "$(printf '%s' "$R" | grep -o "\"stageId\":\"$S_PLASTER\"" | wc -l | tr -d ' ')"
eq "a reading is never a proposal"               "0" "$(pcount "$Q" '"title":"Guest wing plastering is currently at 80%')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Idempotent — a re-run proposes nothing twice"

RUN=$(req POST /Extraction/Run "$PETER" "{\"ProjectId\":\"$PID\"}")
eq "a second run reads the whole record again"   "34" "$(jnum "$RUN" messagesRead)"
eq "…and proposes nothing new"                   "0" "$(jnum "$RUN" proposalsCreated)"
eq "…nor re-records a reading"                   "0" "$(jnum "$RUN" readingsCreated)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The review queue — cleared in bulk, instrumented"

ids_json() { props "$1" | grep -E -- "$2" | grep -o '"id":"[^"]*"' | sed 's/"id"://' | paste -sd, -; }

ACCEPT=$(ids_json "$Q" '"kind":"Date"')
N_ACCEPT=$(printf '%s' "$ACCEPT" | tr ',' '\n' | grep -c . || true)
D1=$(req POST /Extraction/Decide "$PETER" "{\"ProposalIds\":[$ACCEPT],\"Accept\":true}")
eq "Peter accepts all five dates at once"        "5" "$(jnum "$D1" accepted)"
eq "…each becomes a commitment"                  "$N_ACCEPT" "$(printf '%s' "$D1" | grep -o '"commitmentIds":\[[^]]*\]' | grep -o '"[0-9a-f-]\{36\}"' | wc -l | tr -d ' ')"

Q=$(req GET "/Extraction/GetQueue?projectId=$PID" "$PETER")
eq "all accepted so far — 100%"                  "1" "$(jnum "$Q" acceptRate)"
eq "…nowhere near the two-thirds tripwire"       "false" "$(jbool "$Q" belowThreshold)"

# Rejecting most of the rest drives the rate under two-thirds — the state in
# which the plan says to narrow the trigger rather than trust the register.
REJECT=$(ids_json "$Q" '"kind":"(Material|Choice|Spec)"')
N_REJECT=$(printf '%s' "$REJECT" | tr ',' '\n' | grep -c . || true)
D2=$(req POST /Extraction/Decide "$PETER" "{\"ProposalIds\":[$REJECT],\"Accept\":false}")
eq "he rejects twelve in bulk too"               "12" "$(jnum "$D2" rejected)"

Q=$(req GET "/Extraction/GetQueue?projectId=$PID" "$PETER")
DECIDED=$((N_ACCEPT + N_REJECT))
if [ "$DECIDED" -ge 10 ]; then
  eq "the accept rate is measured"               "$N_ACCEPT" "$(jnum "$Q" acceptedCount)"
  eq "…and trips below two-thirds"               "true" "$(jbool "$Q" belowThreshold)"
else
  skip "the tripwire" "only $DECIDED decided"
fi
eq "the rule stats name the weak trigger first"  "material-spec" \
   "$(printf '%s' "$Q" | grep -o '"rules":\[{"rule":"[^"]*"' | sed 's/.*"rule":"//;s/"$//')"

# An accepted proposal is a real commitment, sourced from its message.
C_ID=$(printf '%s' "$D1" | grep -o '"commitmentIds":\["[^"]*"' | sed 's/.*\["//;s/"$//')
COMMITMENT=$(req GET "/Commitments/GetCommitment?commitmentId=$C_ID" "$PETER")
eq "the commitment records it was ingested"      "Ingested" "$(jget "$COMMITMENT" sourceChannel)"
LINKS=$(req GET "/Commitments/GetLinks?commitmentId=$C_ID" "$PETER")
eq "…and links back to the message it came from" "IngestedMessage" "$(jget "$LINKS" targetType)"

eq "a decided proposal cannot be decided again"  "1" \
   "$(jnum "$(req POST /Extraction/Decide "$PETER" "{\"ProposalIds\":[\"$(pid_of "$Q" 'first half ready')\"],\"Accept\":true}")" skipped)"

RUN2=$(req POST /Extraction/Run "$PETER" "{\"ProjectId\":\"$PID\"}")
eq "a rejected item is never proposed again"     "0" "$(jnum "$RUN2" proposalsCreated)"

# The blocker becomes a flag with a named owner, and the contested price lands In discussion.
BLOCK_ID=$(pid_of "$Q" 'show up')
LABOUR_ID=$(pid_of "$Q" 'Labour shall be 12M')
EDIT=$(req PUT /Extraction/Edit "$PETER" "{\"ProposalId\":\"$LABOUR_ID\",\"Title\":\"Retaining wall labour, formwork to concrete\",\"StageId\":\"$S_PLASTER\"}")
eq "a pending proposal can be corrected first"   "Retaining wall labour, formwork to concrete" "$(jget "$EDIT" title)"
D3=$(req POST /Extraction/Decide "$PETER" "{\"ProposalIds\":[\"$BLOCK_ID\",\"$LABOUR_ID\"],\"Accept\":true}")
eq "blocker and price accepted together"         "2" "$(jnum "$D3" accepted)"
FLAG_ID=$(printf '%s' "$D3" | grep -o '"flagIds":\["[^"]*"' | sed 's/.*\["//;s/"$//')
eq "the blocker is a flag owned by the epoxy team" "Epoxy team" "$(jget "$(req GET "/Flags/GetFlag?flagId=$FLAG_ID" "$PETER")" ownerName)"
LABOUR_C=$(printf '%s' "$D3" | grep -o '"commitmentIds":\["[^"]*"' | sed 's/.*\["//;s/"$//')
LC=$(req GET "/Commitments/GetCommitment?commitmentId=$LABOUR_C" "$PETER")
eq "the contested figure is In discussion"       "InDiscussion" "$(jget "$LC" maturity)"
eq "…at the figure the thread named"             "12000000" "$(jnum "$LC" amount | sed 's/\.0*$//')"
eq "an edit on a decided proposal is refused"    "409" \
   "$(code PUT /Extraction/Edit "$PETER" "{\"ProposalId\":\"$LABOUR_ID\",\"Title\":\"x\"}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Seats and sides — a proposal is exactly as private as its message"

eq "the foreman has no queue (404, not 403)"     "404" "$(code GET "/Extraction/GetQueue?projectId=$PID" "$KATO")"
eq "…and cannot run extraction"                  "404" "$(code POST /Extraction/Run "$KATO" "{\"ProjectId\":\"$PID\"}")"
eq "a stranger has no queue"                     "404" "$(code GET "/Extraction/GetQueue?projectId=$PID" "$DINAH")"
eq "…nor readings"                               "404" "$(code GET "/Extraction/GetReadings?projectId=$PID" "$DINAH")"

# Peter appoints Nalan and steps back (§10.1). The crew's own thread then comes
# in on the delivery side; its figure must not reach Peter's queue.
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null

CPRE=$("${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $NALAN" \
  -F "file=@$FIXTURES/$P5_CREW" -F "projectId=$PID")
CBATCH=$(jget "$CPRE" batchId)
req POST /Ingest/CommitImport "$NALAN" "{\"BatchId\":\"$CBATCH\",\"AuthorMappings\":[]}" >/dev/null

NQ=$(req GET "/Extraction/GetQueue?projectId=$PID" "$NALAN")
PQ=$(req GET "/Extraction/GetQueue?projectId=$PID" "$PETER")
eq "the crew's figure reaches the mediator"      "1" "$(pcount "$NQ" 'UGX 3,500,000')"
eq "…and not Peter"                              "0" "$(pcount "$PQ" 'UGX 3,500,000')"
eq "Peter still sees what came from his own thread" "1" "$(pcount "$PQ" 'UGX 50M')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Loose footage re-joined by the stamp in its name (works-report §5)"

RJ=$("${CURL[@]}" -X POST "$API/Ingest/RejoinMedia" -H "Authorization: Bearer $PETER" \
  -F "files=@$FIXTURES/$P5_MEDIA_ZIP" -F "projectId=$PID")
eq "eight files arrive in one zip"               "8" "$(jnum "$RJ" filesReceived)"
eq "five bind to their <Media omitted> lines"    "5" "$(jnum "$RJ" bound)"
eq "'Image2' is the same bytes — one duplicate"  "1" "$(jnum "$RJ" duplicates)"
eq "two bind to nothing and are kept, reported"  "2" "$(jnum "$RJ" unbound)"
eq "the human-named file becomes its caption"    "Intended stoppage line" "$(jget "$(printf '%s' "$RJ" | sed 's/},{/}\n{/g' | grep 'intended stoppage')" caption)"
eq "one line is still waiting for its file"      "1" "$(jnum "$RJ" linesStillOpen)"

MISFILED=$(printf '%s' "$RJ" | sed 's/},{/}\n{/g' | grep '2.00.40 PM')
eq "the misfiled frame binds by stamp, not folder" "Bound" "$(jget "$MISFILED" outcome)"
eq "…to the 7/5 2:00 PM line"                    "2026-07-05T14:00:00" "$(jget "$MISFILED" messageSentAt | cut -c1-19)"
eq "a stamp with no line at its minute is unbound" "Unbound" \
   "$(jget "$(printf '%s' "$RJ" | sed 's/},{/}\n{/g' | grep '9.00.00 AM')" outcome)"

WIN=$(req GET "/Ingest/GetMessages?ProjectId=$PID&From=2026-07-05T14:00:00&To=2026-07-05T14:02:00&Take=20" "$PETER")
eq "the thread now shows the photos on their lines" "4" "$(printf '%s' "$WIN" | grep -o '"artifactId":"[^"]*"' | wc -l | tr -d ' ')"

RJ2=$("${CURL[@]}" -X POST "$API/Ingest/RejoinMedia" -H "Authorization: Bearer $PETER" \
  -F "files=@$FIXTURES/$P5_MEDIA_ZIP" -F "projectId=$PID")
eq "the same zip again binds nothing new"        "0" "$(jnum "$RJ2" bound)"
eq "…and says the six are already on their lines" "6" "$(jnum "$RJ2" alreadyBound)"
eq "a stranger cannot re-join into the project"  "403" "$("${CURL[@]}" -o /dev/null -w '%{http_code}' -X POST "$API/Ingest/RejoinMedia" \
  -H "Authorization: Bearer $DINAH" -F "files=@$FIXTURES/$P5_MEDIA_ZIP" -F "projectId=$PID")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "OCR — every new file is read for text, off the request path"

share() { "${CURL[@]}" -X POST "$API/Ingest/CaptureShare" -H "Authorization: Bearer $PETER" -F "projectId=$PID" -F "file=@$1"; }
RECEIPT_ART=$(jget "$(share "$FIXTURES/$P5_RECEIPT")" artifactId)
NOTE_ART=$(jget "$(share "$FIXTURES/$P5_NOTE")" artifactId)

poll_text() { # poll_text ARTIFACT → final JSON once no longer Pending (60 s ceiling)
  local out
  for _ in $(seq 1 30); do
    out=$(req GET "/Extraction/GetArtifactText?artifactId=$1" "$PETER")
    [ "$(jget "$out" status)" != "Pending" ] && { printf '%s' "$out"; return; }
    sleep 2
  done
  printf '%s' "$out"
}

NOTE_T=$(poll_text "$NOTE_ART")
eq "a forwarded text file is read"               "Done" "$(jget "$NOTE_T" status)"
eq "…word for word"                              "1" "$(printf '%s' "$NOTE_T" | grep -c '150 bags cement received')"

RT=$(poll_text "$RECEIPT_ART")
case "$(jget "$RT" status)" in
  Done)
    ok "the receipt photo is read by OCR" "$(jget "$RT" engine)"
    eq "…and its vendor and material are findable" "2" \
       "$(printf '%s' "$RT" | grep -o -i -E 'HARDWARE|CEMENT' | sort -u -f | wc -l | tr -d ' ')"
    ;;
  EngineUnavailable)
    skip "the receipt photo is read by OCR" "no OCR engine on this host — set Ocr:TesseractPath"
    skip "…and its vendor and material are findable" "no OCR engine"
    ;;
  *)
    bad "the receipt photo is read by OCR" "$(jget "$RT" status) $(jget "$RT" error)" "Done"
    bad "…and its vendor and material are findable" "-" "2"
    ;;
esac

eq "a stranger cannot learn a file exists (404)"  "404" "$(code GET "/Extraction/GetArtifactText?artifactId=$RECEIPT_ART" "$DINAH")"
QO=$(req POST "/Extraction/QueueOcr?projectId=$PID" "$PETER")
eq "a backfill reports what it queued"           "true" "$(printf '%s' "$QO" | grep -q '"queued":' && echo true || echo false)"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
