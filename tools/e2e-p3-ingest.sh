#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P3 end-to-end suite: the front door.
#
# P3 is the whole tier-1 thesis (assetlen.md D2, D3, Law 0). Peter's year of
# history already exists and already reached him; he cannot read it. Every
# assertion below is asked from that position:
#
#   Can he get a year of history in without anyone else logging in?
#   If he imports the same export twice, does he get it twice?
#   Do eighteen photos posted in one minute survive as eighteen?
#   Does the same receipt sent five times become one file?
#   Does a wrong day/month reading get stated rather than hidden?
#   Is the delivery side's own record kept out of his view, and his out of theirs?
#
# Usage:  bash tools/e2e-p3-ingest.sh [api-base] [tenant-admin-email] [password]
#         SKIP_FIXTURES=1 bash tools/e2e-p3-ingest.sh     # keep existing fixtures
# Needs:  the API running, and pwsh for fixture generation. Run from the repo root.
#         Idempotent — each run builds its own project.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 600)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures"

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

jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
jnum() { printf '%s' "$1" | grep -o "\"$2\":[-0-9.]*" | head -1 | sed 's/.*://' || true; }

# oid BODY → the id of the OUTER object. BaseDto's properties serialise after
# the derived type's, so a DTO carrying a nested collection emits its children's
# ids first; taking the first match runs every later call against a child.
oid()  { printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true; }

echo "ASSETLEN P3 — the front door — $API"

# ── Fixtures ─────────────────────────────────────────────────────────────────
if [ "${SKIP_FIXTURES:-0}" != "1" ]; then
  bash tools/make-ingest-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
    || { echo "FATAL: could not build fixtures. Is pwsh on PATH?"; exit 1; }
fi
[ -f "$FIXTURES/fixtures.env" ] || { echo "FATAL: $FIXTURES/fixtures.env missing."; exit 1; }
# shellcheck disable=SC1090
. "$FIXTURES/fixtures.env"

BIG_PATH="$FIXTURES/$BIG_FILE"
IOS_PATH="$FIXTURES/$IOS_FILE"

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
  "{\"ProjectName\":\"Peter Ingest $STAMP\",\"Description\":\"P3 subject\",\"Location\":\"Kampala\",\"TotalBudget\":450000000,\"Currency\":\"UGX\",\"Stages\":[]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }

up() { # up TOKEN FILE → preview JSON
  "${CURL[@]}" -X POST "$API/Ingest/UploadArchive" -H "Authorization: Bearer $1" \
    -F "file=@$2" -F "projectId=$PID"
}

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 0 — a year of history, with nobody else logged in"

# Peter is alone on this project. No contractor has been invited, nobody has
# captured anything. This is the silent-contractor case and it is the only one
# tier 1 is allowed to depend on.
PRE=$(up "$PETER" "$BIG_PATH")
BATCH=$(jget "$PRE" batchId)
if [ -z "$BATCH" ]; then
  echo "FATAL: preview failed: $(printf '%s' "$PRE" | head -c 300)"
  exit 1
fi
ok "Peter uploads his own export, alone" "$BATCH"

eq "every message is read"                  "$BIG_TOTAL"        "$(jnum "$PRE" messageCount)"
eq "…all of them new"                       "$BIG_TOTAL"        "$(jnum "$PRE" newMessageCount)"
eq "attachments are counted, not dropped"   "$BIG_MEDIA"        "$(jnum "$PRE" mediaMessageCount)"
eq "the three participants are found"       "$BIG_PARTICIPANTS" \
   "$(printf '%s' "$PRE" | grep -o '"externalAuthor":"[^"]*"' | wc -l | tr -d ' ')"

# "Export without media" is the likelier file and the one the corpus is. Saying
# so is the difference between a thin import and one that looks broken.
eq "media with no bytes is reported"        "$BIG_MEDIA"        "$(jnum "$PRE" mediaFilesMissing)"

# A wrong day/month reading shifts a year by months and shows no error, so the
# parser must *prove* the order rather than assume it. 18/09 proves day-first.
eq "day-first is proven, not assumed"       "DayFirst"          "$(jget "$PRE" dateOrder)"
eq "…and the range starts on the real date" "2025-09-18"        "$(jget "$PRE" firstMessageAt | cut -c1-10)"
eq "…and ends on the real date"             "2026-08-04"        "$(jget "$PRE" lastMessageAt | cut -c1-10)"

# Preview writes nothing. Attribution is the expensive thing to get wrong, so
# nothing lands until a person has seen who is who.
eq "preview has written no messages"        "0" \
   "$(jnum "$(req GET "/Ingest/GetMessages?ProjectId=$PID" "$PETER")" totalCount)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Attribution survives import (whatsapp-evidence.md §1)"

# Nalan holds a login; the export names him "Nalan". Peter maps that himself —
# the contractor need not be present, or even exist yet.
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"IsMediator\":false,\"Title\":\"Architect-contractor\"}" >/dev/null

PRE2=$(up "$PETER" "$BIG_PATH")

# The export is itself an artifact (Law 2 one level up), so re-uploading the
# same bytes stores no second copy — a run is a new batch, but never a new file.
eq "the export itself is stored once"       "$(jget "$PRE" archiveArtifactId)" \
                                            "$(jget "$PRE2" archiveArtifactId)"
eq "an import lands on the uploader's side" "Client" "$(jget "$PRE2" importedSide)"

NALAN_MID=$(printf '%s' "$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")" \
  | tr '}' '\n' | grep -i 'Nalan' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

BATCH2=$(jget "$PRE2" batchId)
[ -n "$BATCH2" ] && BATCH="$BATCH2"

# Nalan maps onto his real membership; the other two become off-platform
# parties. A thread names people who may never sign in, and an unattributable
# record is the failure this mapping exists to prevent.
COMMIT=$(req POST /Ingest/CommitImport "$PETER" \
  "{\"BatchId\":\"$BATCH\",\"AuthorMappings\":[
      {\"ExternalAuthor\":\"Nalan\",\"MemberId\":\"$NALAN_MID\"},
      {\"ExternalAuthor\":\"Peter\",\"CreateAsPartyName\":\"Peter (thread)\",\"Side\":0,\"Specialization\":8},
      {\"ExternalAuthor\":\"Dinah\",\"CreateAsPartyName\":\"Dinah (thread)\",\"Side\":0,\"Specialization\":9}]}")

eq "the import completes"                   "Completed"  "$(jget "$COMMIT" status)"
eq "every message lands"                    "$BIG_TOTAL" "$(jnum "$COMMIT" importedMessageCount)"
eq "…and is readable back"                  "$BIG_TOTAL" \
   "$(jnum "$(req GET "/Ingest/GetMessages?ProjectId=$PID" "$PETER")" totalCount)"

# An off-platform party is a real row, so "agreed with the windows contractor"
# has something to point at.
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
eq "off-platform parties were created"      "2" \
   "$(printf '%s' "$ROSTER" | grep -o '"partyName":"[^"]*(thread)"' | wc -l | tr -d ' ')"

MAPPED=$(req GET "/Ingest/GetMessages?ProjectId=$PID&Search=Tororo&Take=5" "$PETER")
eq "a mapped message names its member"      "Nalan Architect" "$(jget "$MAPPED" authorMemberName)"
eq "…while keeping the export's own name"   "Nalan"           "$(jget "$MAPPED" externalAuthor)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Re-import is safe — the P3 exit criterion"

# The single most important property here. Peter will re-export and re-upload,
# because that is what people do when they are not sure it worked. If that
# doubles his register, he never trusts it again.
PRE3=$(up "$PETER" "$BIG_PATH")
BATCH3=$(jget "$PRE3" batchId)

eq "a re-upload knows what is already here" "$BIG_TOTAL" "$(jnum "$PRE3" alreadyImportedCount)"
eq "…and offers to add nothing"             "0"          "$(jnum "$PRE3" newMessageCount)"

RECOMMIT=$(req POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"$BATCH3\",\"AuthorMappings\":[]}")
eq "re-importing adds no message"           "0"          "$(jnum "$RECOMMIT" importedMessageCount)"
eq "…and says how many it skipped"          "$BIG_TOTAL" "$(jnum "$RECOMMIT" duplicateMessageCount)"
eq "the record is still exactly one copy"   "$BIG_TOTAL" \
   "$(jnum "$(req GET "/Ingest/GetMessages?ProjectId=$PID" "$PETER")" totalCount)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "F2 — eighteen photos in one minute survive as eighteen"

# The corpus's normal posting pattern (whatsapp-evidence.md §2): a batch of 13
# to 18 photos inside a single minute, same author, all bodied "<Media
# omitted>". They are byte-identical as text. Hashing them without an occurrence
# ordinal collapses all eighteen into one, and the loss is invisible — the
# import reports success and 17 photos are simply gone.
BURST=$(req GET "/Ingest/GetMessages?ProjectId=$PID&From=2026-02-11T21:56:00&To=2026-02-11T21:56:59&Take=50" "$PETER")
eq "the whole burst is kept"                "$BIG_BURST" "$(jnum "$BURST" totalCount)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 2 — one canonical artifact, many pointers"

# A second dialect on purpose: iOS brackets its stamps, uses a 12-hour clock,
# and wraps lines in direction marks that render as nothing. A parser taught
# only Android returns zero messages here and looks like an empty chat.
IPRE=$(up "$PETER" "$IOS_PATH")
IBATCH=$(jget "$IPRE" batchId)
if [ -z "$IBATCH" ]; then
  skip "iOS export with media" "preview failed: $(printf '%s' "$IPRE" | head -c 200)"
else
  ok "an iOS export parses too" "$IBATCH"
  eq "…every message"                       "$IOS_TOTAL"      "$(jnum "$IPRE" messageCount)"
  eq "…and the archive's media is found"    "$IOS_MEDIA_MSGS" "$(jnum "$IPRE" mediaFilesAvailable)"
  eq "…with nothing missing"                "0"               "$(jnum "$IPRE" mediaFilesMissing)"

  ICOMMIT=$(req POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"$IBATCH\",\"AuthorMappings\":[]}")

  # The receipt sent five times. Five messages, five pointers, ONE file —
  # content-addressed, so five different names cannot make five copies.
  eq "distinct files are stored once each"  "$IOS_UNIQUE_CONTENT" "$(jnum "$ICOMMIT" newArtifactCount)"
  eq "identical bytes are recognised"       "$IOS_DUP_CONTENT"    "$(jnum "$ICOMMIT" duplicateArtifactCount)"

  MEDIA=$(req GET "/Ingest/GetMessages?ProjectId=$PID&BatchId=$IBATCH&MediaOnly=true&Take=50" "$PETER")
  eq "every attachment reaches its message" "$IOS_MEDIA_MSGS" "$(jnum "$MEDIA" totalCount)"

  AID=$(printf '%s' "$MEDIA" | grep -o '"artifactId":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')
  if [ -n "$AID" ]; then
    eq "…and its bytes stream to the owner"  "200" "$(code GET "/Artifacts/$AID/content" "$PETER")"
    eq "…with a thumbnail generated"         "200" "$(code GET "/Artifacts/$AID/thumbnail" "$PETER")"
  else
    skip "imported artifact streams" "no artifactId on any message"
  fi
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "D5 — the delivery side's record is not Peter's to read"

# Peter appoints Nalan and steps back. This is the delegation gesture from
# assetlen.md §10.1, and it is what makes the next assertion mean anything.
PETER_MID=$(printf '%s' "$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")" \
  | tr '}' '\n' | grep -i '"userFullName":"Peter Developer"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null

req POST /ProjectMembers/AddMember "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}" >/dev/null

STANDING=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$PETER")
eq "Peter is client-side once he delegates" "Client" "$(jget "$STANDING" side)"

# Nalan imports the crew's own thread. It lands on the delivery side.
NPRE=$(up "$NALAN" "$IOS_PATH")
NBATCH=$(jget "$NPRE" batchId)
if [ -z "$NBATCH" ]; then
  skip "a delivery-side import" "preview failed"
else
  eq "a delivery-side import is marked so"  "Contractor" "$(jget "$NPRE" importedSide)"
  req POST /Ingest/CommitImport "$NALAN" "{\"BatchId\":\"$NBATCH\",\"AuthorMappings\":[]}" >/dev/null

  # 404 rather than 403: a refusal would confirm the crew's record exists, and
  # its existence is itself delivery-side.
  eq "Peter cannot read the crew's import"  "404" "$(code GET "/Ingest/GetBatch?batchId=$NBATCH" "$PETER")"
  eq "…but the mediator can"                "200" "$(code GET "/Ingest/GetBatch?batchId=$NBATCH" "$NALAN")"
  eq "…and so can the foreman on that side" "200" "$(code GET "/Ingest/GetBatch?batchId=$NBATCH" "$KATO")"

  # He keeps his own, though — standing down is not forfeiting what he put in.
  eq "Peter still reads his own import"     "200" "$(code GET "/Ingest/GetBatch?batchId=$BATCH" "$PETER")"
fi

eq "a stranger sees nothing"                "403" \
   "$(code GET "/Ingest/GetMessages?ProjectId=$PID" "$DINAH")"
eq "…and cannot upload into the project"    "403" \
   "$( "${CURL[@]}" -o /dev/null -w '%{http_code}' -X POST "$API/Ingest/UploadArchive" \
        -H "Authorization: Bearer $DINAH" -F "file=@$BIG_PATH" -F "projectId=$PID" )"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The ongoing trickle — share sheet and email-in"

SHARE=$("${CURL[@]}" -X POST "$API/Ingest/CaptureShare" -H "Authorization: Bearer $NALAN" \
  -F "projectId=$PID" -F "text=Agreed on the call: retaining wall labour at UGX 12M, 11 Jun")
eq "a shared note lands on the project"     "ShareSheet" "$(jget "$SHARE" sourceType)"

share_file() {
  "${CURL[@]}" -X POST "$API/Ingest/CaptureShare" -H "Authorization: Bearer $NALAN" \
    -F "projectId=$PID" -F "text=$1" -F "file=@$FIXTURES/ios-media/photo-01.jpg;type=image/jpeg"
}

# The receipt sent twice, through the door people actually use. Content
# addressing has to hold on the trickle path too, or Law 2 only works for
# imports and the day-to-day forwarding quietly accumulates copies.
SHARE_AID=$(jget "$(share_file 'Receipt for the cement')" artifactId)
SHARE_AID2=$(jget "$(share_file 'Sending again in case you missed it')" artifactId)

if [ -z "$SHARE_AID" ]; then
  bad "a shared photo is stored" "no artifactId" "an artifact id"
else
  ok "a shared photo is stored" "$SHARE_AID"
  eq "…and re-sharing it makes no second copy" "$SHARE_AID" "$SHARE_AID2"
fi

INBOX=$(req GET "/Ingest/GetInbox?projectId=$PID" "$PETER")
ADDR=$(jget "$INBOX" emailAddress)
printf '%s' "$ADDR" | grep -q '^in+[a-f0-9]\{16\}@' \
  && ok "the project has an inbound address" "$ADDR" \
  || bad "the project has an inbound address" "$ADDR" "in+<key>@<domain>"

# appsettings.json is gitignored, so Ingest:InboundSecret does not survive a
# fresh clone and the endpoint correctly answers 503 rather than standing open.
# Probe for that first: a spurious red here would teach the reader to ignore
# this suite, which is worse than the coverage it would buy.
SECRET="${INGEST_SECRET:-dev-inbound-secret-not-for-deployment}"
mail_code() { # mail_code SECRET TO → HTTP status
  "${CURL[@]}" -o /dev/null -w '%{http_code}' -X POST "$API/Ingest/InboundEmail" \
    -H "Content-Type: application/json" -H "X-Assetlen-Ingest-Secret: $1" \
    -d "{\"To\":\"$2\",\"From\":\"peter.buyer@assetlen.test\",\"Subject\":\"Deposit slip\",\"TextBody\":\"Sent UGX 71,000,000 today\"}"
}

if [ "$(mail_code "$SECRET" "$ADDR")" = "503" ]; then
  skip "inbound mail" "Ingest:InboundSecret unset — add it to appsettings.json (untracked) or set INGEST_SECRET"
else
  # The relay is not a signed-in person, so the secret is the whole gate.
  eq "inbound mail without the secret is refused" "401" "$(mail_code wrong "$ADDR")"

  MAILED=$("${CURL[@]}" -X POST "$API/Ingest/InboundEmail" \
    -H "Content-Type: application/json" -H "X-Assetlen-Ingest-Secret: $SECRET" \
    -d "{\"To\":\"$ADDR\",\"From\":\"peter.buyer@assetlen.test\",\"Subject\":\"Deposit slip\",\"TextBody\":\"Sent UGX 71,000,000 today\"}")
  eq "…and with it, the mail lands"         "Email" "$(jget "$MAILED" sourceType)"

  # The address is a capability: anyone holding it can post, so it must be
  # revocable without touching the project.
  RESET=$(req PUT "/Ingest/ResetInbox?projectId=$PID" "$PETER")
  NEW_ADDR=$(jget "$RESET" emailAddress)
  if [ -n "$NEW_ADDR" ] && [ "$NEW_ADDR" != "$ADDR" ]; then
    ok "reissuing revokes the old address" "$NEW_ADDR"
  else
    bad "reissuing revokes the old address" "$NEW_ADDR" "a different address"
  fi

  eq "…and the old one no longer resolves"  "404" "$(mail_code "$SECRET" "$ADDR")"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Refusing what is not an export"

printf 'this is not a chat transcript at all\n' > "assetlen-junk-$STAMP.txt"
eq "a file that is not an export is refused" "400" \
   "$( "${CURL[@]}" -o /dev/null -w '%{http_code}' -X POST "$API/Ingest/UploadArchive" \
        -H "Authorization: Bearer $PETER" -F "file=@assetlen-junk-$STAMP.txt" -F "projectId=$PID" )"
rm -f "assetlen-junk-$STAMP.txt"

eq "committing an unknown batch is refused"  "404" \
   "$(code POST /Ingest/CommitImport "$PETER" "{\"BatchId\":\"no-such-batch\",\"AuthorMappings\":[]}")"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ] || exit 1
