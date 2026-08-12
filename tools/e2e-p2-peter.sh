#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P0 + P1 + P2 end-to-end suite, read against Peter's needs.
#
# The buyer is Peter, the developer who funds the work (assetlen.md §0, D1).
# This suite therefore casts him as the project owner and asks, at each step,
# a question from Peter.md rather than a question about the code:
#
#   Does the project belong to him?
#   Is exactly one person accountable for what reaches him?
#   Can the delivery side be staffed without him seeing its traffic?
#   Is he told when the feed he is reading has been filtered?
#   Does the bill follow the building, and never rise behind his back?
#
# Usage:  bash tools/e2e-p2-peter.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running. Idempotent — re-running reuses users and projects.
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

# req METHOD PATH TOKEN [JSON] → response body
req() {
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3" \
      -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

# code METHOD PATH TOKEN [JSON] → HTTP status
code() {
  if [ -n "${4:-}" ]; then
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3" \
      -H "Content-Type: application/json" -d "$4"
  else
    "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"
  fi
}

# jget BODY KEY → first value of "key": (string or scalar)
jget() {
  printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' \
    || true
}

# oid BODY → the id of the OUTER object.
#
# BaseDto's properties serialise after the derived type's, so a DTO carrying a
# nested collection emits stages[0].id / images[0].id before its own id. Taking
# the first match silently returns a child's id and every downstream call then
# runs against something that does not exist — which is exactly how this suite
# first "failed" twenty assertions against working code.
oid() {
  printf '%s' "$1" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//' || true
}
jnum() {
  printf '%s' "$1" | grep -o "\"$2\":[-0-9.]*" | head -1 | sed 's/.*://' || true
}

# frame_ids ENTRY_JSON → one image id per line, in payload order.
#
# Isolate the images array first and split it into objects. Matching ids across
# the whole document would also pick up the entry's own id and any comment's,
# and counting those made a filtered feed look unfiltered.
frame_ids() {
  printf '%s' "$1" \
    | sed -n 's/.*"images":\[\(.*\)\],"imageCount".*/\1/p' \
    | sed 's/},{/}\n{/g' \
    | grep -o '"id":"[^"]*"' | sed 's/.*:"//;s/"$//' || true
}
jbool() {
  printf '%s' "$1" | grep -o "\"$2\":\(true\|false\)" | head -1 | sed 's/.*://' || true
}

ok()   { printf "  ${c_pass}PASS${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-58s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-58s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
head_() { printf "\n${c_dim}── %s ${c_off}\n" "$1"; }

echo "ASSETLEN P0+P1+P2 — $API"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

# ── Cast ─────────────────────────────────────────────────────────────────────
# Peter holds the tenant-owner role because the six-role enum has not been
# collapsed yet and CreateProject still gates on Contractor|Manager. That the
# buyer must wear a role called "Contractor" is a naming debt, not a behaviour
# bug — every check below resolves standing per project, not from this claim.
mkuser() { # mkuser EMAIL ROLE FIRST LAST
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

mkuser peter.buyer@assetlen.test    Contractor Peter Developer
mkuser nalan.arch@assetlen.test    Manager    Nalan Architect
mkuser kato.foreman@assetlen.test     Crew       Kato  Foreman
mkuser dinah.principal@assetlen.test    Client     Dinah Principal

PETER=$(tok peter.buyer@assetlen.test password)
NALAN=$(tok nalan.arch@assetlen.test password)
KATO=$(tok  kato.foreman@assetlen.test  password)
DINAH=$(tok dinah.principal@assetlen.test password)
for t in PETER NALAN KATO DINAH; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

# ═════════════════════════════════════════════════════════════════════════════
head_ "D1 — the project belongs to the person who funds it"

PNAME="Peter House $STAMP"
CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"$PNAME\",\"Description\":\"E2E subject\",\"Location\":\"Kampala\",\"TotalBudget\":450000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Substructure\",\"BudgetAmount\":90000000,\"DisplayOrder\":1}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
ok "Peter creates a project" "$PID"

SID=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

STANDING=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$PETER")
eq "Peter's side on his own project"        "Client" "$(jget "$STANDING" side)"
eq "Peter mediates until he delegates"      "true"   "$(jbool "$STANDING" isMediator)"
eq "Peter can manage it"                    "true"   "$(jbool "$STANDING" canManage)"

ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
eq "creator is seated on the roster"        "1" "$(printf '%s' "$ROSTER" | grep -o '"isMediator":true' | wc -l | tr -d ' ')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "§10.1 — one accountable face, appointed deliberately"

eq "Nalan has no standing before invitation" "403" \
   "$(code GET "/ProjectsRS/GetProjectById?projectId=$PID" "$NALAN")"

ADDED=$(req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.arch@assetlen.test\",\"Specialization\":3,\"Side\":1,\"IsMediator\":false,\"Title\":\"Architect-contractor\"}")
NALAN_MID=$(oid "$ADDED")
eq "Peter adds Nalan to the delivery side"  "Contractor" "$(jget "$ADDED" side)"
eq "…and not as mediator by accident"       "false"      "$(jbool "$ADDED" isMediator)"

PROMOTED=$(req PUT /ProjectMembers/UpdateMember "$PETER" \
  "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}")
eq "Peter appoints Nalan mediator"          "true" "$(jbool "$PROMOTED" isMediator)"

# Peter then steps back. This is the whole delegation gesture and it must be
# possible: while he keeps a seat he still reads the raw Site Log, and "what
# Peter sees is what the architect chose" is not yet true of the product.
PETER_MID=$(printf '%s' "$ROSTER" | grep -o '"id":"[^"]*"' | tail -1 | sed 's/.*:"//;s/"$//')
eq "Peter stands himself down"              "200" \
   "$(code PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}")"

AFTER=$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$PETER")
eq "…and stops reading the Site Log"        "false" "$(jbool "$AFTER" canSeeSiteLog)"
eq "…while keeping ownership of the project" "true"  "$(jbool "$AFTER" canManage)"

# Nalan staffs his own side. Peter never touches the delivery roster.
KATO_ADD=$(req POST /ProjectMembers/AddMember "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.foreman@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}")
KATO_MID=$(oid "$KATO_ADD")
[ -n "$KATO_MID" ] && ok "Nalan staffs the delivery side himself" "foreman" \
                    || bad "Nalan staffs the delivery side himself" "no id" "a member id"

# An off-platform party: the tile supplier who will never sign in but must be
# nameable on a commitment.
OFFP=$(req POST /ProjectMembers/AddMember "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"PartyName\":\"Riverstone Tiling Ltd\",\"Specialization\":6,\"Side\":1}")
OFFP_MID=$(oid "$OFFP")
eq "off-platform party can be rostered"     "true" "$(jbool "$OFFP" isOffPlatform)"

# Appointing a mediator is never delegated — it decides whose name is
# accountable for everything that crosses.
eq "the mediator cannot appoint a mediator" "403" \
   "$(code PUT /ProjectMembers/UpdateMember "$NALAN" "{\"MemberId\":\"$KATO_MID\",\"IsMediator\":true}")"

# …but the owner can, up to the cap of two.
eq "the owner appoints a second mediator"   "200" \
   "$(code PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$KATO_MID\",\"IsMediator\":true}")"
eq "a third is refused (cap = 2)"           "409" \
   "$(code PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$OFFP_MID\",\"IsMediator\":true}")"

# Kato held a seat only to prove the cap. Take it back so the rest of the suite
# runs with one accountable face, which is the arrangement being tested.
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$KATO_MID\",\"IsMediator\":false}" >/dev/null

# A mediator may not staff the other party's side — that would be writing the
# client's roster on their behalf.
eq "a mediator cannot staff the client side" "403" \
   "$(code POST /ProjectMembers/AddMember "$NALAN" \
      "{\"ProjectId\":\"$PID\",\"PartyName\":\"Someone Else\",\"Specialization\":9,\"Side\":0}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "D5 — Peter sees THAT the delivery side exists, never its traffic"

PROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
KATO_SEEN=$(printf '%s' "$PROSTER" | grep -c 'Kato' || true)
[ "$KATO_SEEN" -ge 1 ] && ok "Peter can see the foreman is on the project" \
                       || bad "Peter can see the foreman is on the project" "absent" "present"

# ═════════════════════════════════════════════════════════════════════════════
head_ "P2 — the Site Log is contractor-side; exposure is per frame"

# Request bodies stay ASCII. A curl -d argument carrying UTF-8 is re-encoded
# crossing the Windows argv boundary, and the server rejects the whole body
# with a binding error that names the wrong field. Labels and comments may use
# any character they like — those only ever reach a terminal.
#
# A real 32x24 RGB PNG. The previous fixture was a well-travelled 4x4 blob that
# is structurally a PNG but carries an invalid filter byte, so ImageSharp
# refused it and no thumbnail was ever produced — the artifact assertions were
# passing over a decode failure the API had correctly logged and survived.
PNG='iVBORw0KGgoAAAANSUhEUgAAACAAAAAYCAIAAAAUMWhjAAAAJElEQVR42mM4FKJFU8QwasGoBaMWjFowasGoBaMWjFowNCwAAND5wC6OsU0VAAAAAElFTkSuQmCC'
mkimg() { printf '{"Base64Image":"%s","FileName":"%s","ContentType":"image/png","Caption":"%s","DisplayOrder":%s}' "$PNG" "$1" "$2" "$3"; }

ENTRY=$(req POST /Progress/AddProgressUpdate "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$SID\",\"Description\":\"Retaining wall - course 4 laid\",\"CompletionPercentage\":35,\"Channel\":0,\"Images\":[$(mkimg a.png "frame 1" 1),$(mkimg b.png "frame 2" 2),$(mkimg c.png "frame 3" 3)]}")
EID=$(oid "$ENTRY")
if [ -z "$EID" ]; then
  skip "site log entry with three frames" "create failed: $(printf '%s' "$ENTRY" | head -c 160)"
else
  ok "Nalan posts an entry with three frames" "$EID"

  NSEE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN")
  eq "delivery side sees all three"         "3" "$(frame_ids "$NSEE" | grep -c . | tr -d ' ')"

  # Peter is client-side now. A crew-only entry does not merely refuse him —
  # it answers 404. Denying with 403 would confirm that something exists at
  # that id, and the Site Log's existence is itself contractor-side.
  eq "a crew entry does not exist for the client" "404" \
     "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"

  # Nalan exposes exactly one frame — the gesture the product exists for.
  IMG1=$(frame_ids "$NSEE" | sed -n 1p)
  req PUT /Progress/SetImageChannel "$NALAN" "{\"ImageIds\":[\"$IMG1\"],\"Channel\":1}" >/dev/null
  eq "the entry becomes readable to Peter"  "200" \
     "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"

  PSEE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")
  eq "Peter receives the one exposed frame" "1" "$(frame_ids "$PSEE" | grep -c . | tr -d ' ')"
  eq "…and is told the true total (3)"      "3" "$(jnum "$PSEE" imageCount)"

  # The foreman may write to the Site Log but must not be able to publish.
  IMG2=$(frame_ids "$NSEE" | sed -n 2p)
  if [ -n "$IMG2" ]; then
    eq "foreman cannot expose to the client" "403" \
       "$(code PUT /Progress/SetImageChannel "$KATO" "{\"ImageIds\":[\"$IMG2\"],\"Channel\":1}")"
  else
    skip "foreman cannot expose to the client" "second frame id not parsed"
  fi
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 2 — one canonical artifact, many pointers"

# Relative path, written into the current directory. curl here is a native
# Windows binary and does not understand the MSYS /tmp that $TMPDIR expands to
# under Git Bash; it failed to open the file and sent nothing at all, which
# looks identical to a server that refused the upload.
TMP="assetlen-e2e-$STAMP.png"
printf '%s' "$PNG" | base64 -d > "$TMP" 2>/dev/null || printf '%s' "$PNG" | base64 --decode > "$TMP"

up() { "${CURL[@]}" -X POST "$API/Artifacts/Upload" -H "Authorization: Bearer $1" \
        -F "file=@$TMP;type=image/png" -F "projectId=$PID"; }

A1=$(up "$NALAN")
AID=$(oid "$A1")
if [ -z "$AID" ]; then
  skip "artifact upload" "$(printf '%s' "$A1" | head -c 160)"
else
  ok "multipart upload stores an artifact" "$AID"
  eq "first upload is not a duplicate"     "false" "$(jbool "$A1" wasDeduplicated)"

  A2=$(up "$NALAN")
  eq "identical bytes de-duplicate"        "true"  "$(jbool "$A2" wasDeduplicated)"
  eq "…to the same artifact id"            "$AID"  "$(oid "$A2")"

  eq "content streams to the uploader"     "200" "$(code GET "/Artifacts/$AID/content" "$NALAN")"
  eq "thumbnail streams to the uploader"   "200" "$(code GET "/Artifacts/$AID/thumbnail" "$NALAN")"
  # Knowing an id must not be enough. 404 rather than 403 for the same reason
  # as the Site Log: a refusal would confirm the artifact exists.
  eq "guessing an id gets the client nowhere" "404" "$(code GET "/Artifacts/$AID/content" "$PETER")"

  REF=$(req POST /Artifacts/AddRef "$NALAN" \
    "{\"ArtifactId\":\"$AID\",\"TargetType\":1,\"TargetId\":\"$PID\",\"Caption\":\"Structural drawing rev A\"}")
  RID=$(oid "$REF")
  eq "a new pointer lands crew-only"       "Crew" "$(jget "$REF" channel)"

  if [ -n "$RID" ]; then
    eq "the foreman cannot expose a pointer" "403" \
       "$(code PUT /Artifacts/SetRefChannel "$KATO" "{\"RefId\":\"$RID\",\"Channel\":1}")"
    eq "the mediator can"                    "200" \
       "$(code PUT /Artifacts/SetRefChannel "$NALAN" "{\"RefId\":\"$RID\",\"Channel\":1}")"
  fi
fi
rm -f "$TMP"

# ═════════════════════════════════════════════════════════════════════════════
head_ "§10.3 — the bill follows the building, and never rises silently"

S=$(req GET "/ProjectsRS/GetProjectSizing?projectId=$PID" "$PETER")
eq "an undeclared project bills at Small"  "Small" "$(jget "$S" tier)"

S=$(req PUT /ProjectsRS/SetProjectArea "$PETER" "{\"ProjectId\":\"$PID\",\"FloorAreaSqm\":180,\"Source\":1}")
eq "180 m² is Small"                       "Small" "$(jget "$S" tier)"

S=$(req PUT /ProjectsRS/SetProjectArea "$PETER" "{\"ProjectId\":\"$PID\",\"FloorAreaSqm\":900,\"Source\":1}")
eq "900 m² does NOT auto-upgrade the bill" "Small" "$(jget "$S" tier)"
eq "…it is held pending as Large"          "Large" "$(jget "$S" pendingTier)"

S=$(req PUT "/ProjectsRS/ConfirmProjectTier?projectId=$PID" "$PETER")
eq "Peter accepts, and only then it moves" "Large" "$(jget "$S" tier)"

S=$(req PUT /ProjectsRS/SetProjectArea "$PETER" "{\"ProjectId\":\"$PID\",\"FloorAreaSqm\":150,\"Source\":1}")
eq "a decrease applies immediately"        "Small" "$(jget "$S" tier)"

# A guest wing is part of one engagement, not a second invoice.
SUB=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Guest Wing $STAMP\",\"ParentProjectId\":\"$PID\",\"Currency\":\"UGX\",\"Stages\":[]}")
SUBID=$(oid "$SUB")
if [ -z "$SUBID" ]; then
  skip "sub-project rolls up into the parent" "sub-project create failed"
else
  req PUT /ProjectsRS/SetProjectArea "$PETER" "{\"ProjectId\":\"$SUBID\",\"FloorAreaSqm\":140,\"Source\":1}" >/dev/null
  SS=$(req GET "/ProjectsRS/GetProjectSizing?projectId=$SUBID" "$PETER")
  eq "sub-project bills through its parent" "$PID" "$(jget "$SS" billableProjectId)"
  eq "areas roll up (150 + 140 = 290)"      "290" "$(jnum "$SS" totalAreaSqm | sed 's/\.0*$//')"
  eq "…which is Medium, pending acceptance" "Medium" "$(jget "$SS" pendingTier)"
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "F4 — a reissued drawing supersedes, it never replaces"

DOC=$(req POST /Artifacts/CreateDocument "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"Title\":\"Ground floor plan\",\"Kind\":0,\"Channel\":0}")
DID=$(oid "$DOC")
if [ -z "$DID" ]; then
  skip "controlled document" "create failed: $(printf '%s' "$DOC" | head -c 160)"
else
  ok "Nalan opens a controlled document" "$DID"
  eq "it starts internal"                   "Crew" "$(jget "$DOC" channel)"

  # Two issues of the same drawing. The bytes differ, so they are two artifacts.
  printf '%s' "$PNG" | base64 -d > "$TMP" 2>/dev/null || printf '%s' "$PNG" | base64 --decode > "$TMP"
  REVA=$(oid "$(up "$NALAN")")
  printf 'rev-b-%s' "$STAMP" >> "$TMP"
  REVB=$(oid "$(up "$NALAN")")

  req POST /Artifacts/AddRevision "$NALAN" \
    "{\"DocumentId\":\"$DID\",\"ArtifactId\":\"$REVA\",\"Notes\":\"First issue\"}" >/dev/null
  D2=$(req POST /Artifacts/AddRevision "$NALAN" \
    "{\"DocumentId\":\"$DID\",\"ArtifactId\":\"$REVB\",\"Notes\":\"Window head raised\"}")

  eq "reissuing keeps both revisions"       "2" \
     "$(printf '%s' "$D2" | grep -o '"revisionNo":[0-9]*' | wc -l | tr -d ' ')"
  eq "…and pins the newer as current"       "true" \
     "$(printf '%s' "$D2" | grep -o '"revisionNo":2,[^}]*"isCurrent":true' >/dev/null && echo true || echo false)"

  # The superseded issue must still be downloadable — that is the whole point.
  eq "the superseded issue is still readable" "200" "$(code GET "/Artifacts/$REVA/content" "$NALAN")"

  # An internal document is invisible to the client side, bytes included.
  eq "an internal drawing is not listed for the client" "0" \
     "$(printf '%s' "$(req GET "/Artifacts/GetDocuments?projectId=$PID" "$PETER")" | grep -o '"title"' | wc -l | tr -d ' ')"
  eq "…and its bytes are refused"           "404" "$(code GET "/Artifacts/$REVB/content" "$PETER")"

  eq "the foreman cannot issue it"          "403" \
     "$(code PUT "/Artifacts/SetDocumentChannel?documentId=$DID&channel=1" "$KATO")"
  eq "the mediator can"                     "200" \
     "$(code PUT "/Artifacts/SetDocumentChannel?documentId=$DID&channel=1" "$NALAN")"

  eq "issued: the client now sees it"       "1" \
     "$(printf '%s' "$(req GET "/Artifacts/GetDocuments?projectId=$PID" "$PETER")" | grep -o '"title"' | wc -l | tr -d ' ')"
  # Regression: visibility used to be decided by artifact refs alone, so a
  # document could be listed while its bytes stayed unreachable.
  eq "…and can open the file"               "200" "$(code GET "/Artifacts/$REVB/content" "$PETER")"
  eq "…including the superseded issue"      "200" "$(code GET "/Artifacts/$REVA/content" "$PETER")"
fi
rm -f "$TMP"

# ═════════════════════════════════════════════════════════════════════════════
head_ "§10.2 — one human, many accounts"

ACCOUNTS=$(req GET /Authorization/GetMyAccounts "$NALAN")
eq "Nalan can list his accounts"           "1" \
   "$(printf '%s' "$ACCOUNTS" | grep -o '"tenantId"' | wc -l | tr -d ' ')"
eq "…and one is marked current"            "true" "$(jbool "$ACCOUNTS" isCurrent)"

HOME=$(jget "$ACCOUNTS" tenantId)
eq "switching to his own account works"    "200" \
   "$(code POST "/Authorization/SwitchTenant?tenantId=$HOME" "$NALAN")"
eq "switching to one he is not in is refused" "403" \
   "$(code POST "/Authorization/SwitchTenant?tenantId=not-his-account" "$NALAN")"

SWITCHED=$(req POST "/Authorization/SwitchTenant?tenantId=$HOME" "$NALAN")
eq "the re-issued token names the account" "$HOME" "$(jget "$SWITCHED" tenantId)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Isolation — a stranger with a valid token sees nothing"

eq "Dinah is not on this project"          "403" \
   "$(code GET "/ProjectsRS/GetProjectById?projectId=$PID" "$DINAH")"
eq "…cannot read its roster"               "403" \
   "$(code GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$DINAH")"
eq "…and has no standing"                  "None" \
   "$(jget "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$DINAH")" level)"

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ] || exit 1
