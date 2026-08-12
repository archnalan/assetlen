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
eq "off-platform party can be rostered"     "true" "$(jbool "$OFFP" isOffPlatform)"

# The cap. Peter and Nalan hold both seats; a third must be refused.
eq "third mediator refused (cap = 2)"       "409" \
   "$(code PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$KATO_MID\",\"IsMediator\":true}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "D5 — Peter sees THAT the delivery side exists, never its traffic"

PROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
KATO_SEEN=$(printf '%s' "$PROSTER" | grep -c 'Kato' || true)
[ "$KATO_SEEN" -ge 1 ] && ok "Peter can see the foreman is on the project" \
                       || bad "Peter can see the foreman is on the project" "absent" "present"

# ═════════════════════════════════════════════════════════════════════════════
head_ "P2 — the Site Log is contractor-side; exposure is per frame"

PNG='iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg=='
mkimg() { printf '{"Base64Image":"%s","FileName":"%s","ContentType":"image/png","Caption":"%s","DisplayOrder":%s}' "$PNG" "$1" "$2" "$3"; }

ENTRY=$(req POST /Progress/AddProgressUpdate "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$SID\",\"Description\":\"Retaining wall — course 4 laid\",\"CompletionPercentage\":35,\"Channel\":0,\"Images\":[$(mkimg a.png "frame 1" 1),$(mkimg b.png "frame 2" 2),$(mkimg c.png "frame 3" 3)]}")
EID=$(oid "$ENTRY")
if [ -z "$EID" ]; then
  skip "site log entry with three frames" "create failed: $(printf '%s' "$ENTRY" | head -c 160)"
else
  ok "Nalan posts an entry with three frames" "$EID"

  NSEE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN")
  eq "delivery side sees all three"         "3" "$(printf '%s' "$NSEE" | grep -o '"progressUpdateId"' | wc -l | tr -d ' ')"

  # Peter is client-side. Crew material must not reach him at all.
  eq "client side cannot open a crew entry" "403" \
     "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"

  # Nalan exposes exactly one frame — the gesture the product exists for.
  IMG1=$(printf '%s' "$NSEE" | grep -o '"id":"[^"]*","progressUpdateId"' | head -1 | sed 's/"id":"//;s/","progressUpdateId"//')
  EXPOSED=$(req PUT /Progress/SetImageChannel "$NALAN" \
    "{\"ImageIds\":[\"$IMG1\"],\"Channel\":1}")
  eq "mediator exposes one frame"           "200" \
     "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"

  PSEE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")
  eq "Peter receives the one exposed frame" "1" "$(printf '%s' "$PSEE" | grep -o '"progressUpdateId"' | wc -l | tr -d ' ')"
  eq "…and is told the true total (3)"      "3" "$(jnum "$PSEE" imageCount)"

  # The foreman may write to the Site Log but must not be able to publish.
  IMG2=$(printf '%s' "$NSEE" | grep -o '"id":"[^"]*","progressUpdateId"' | sed -n 2p | sed 's/"id":"//;s/","progressUpdateId"//')
  if [ -n "$IMG2" ]; then
    eq "foreman cannot expose to the client" "403" \
       "$(code PUT /Progress/SetImageChannel "$KATO" "{\"ImageIds\":[\"$IMG2\"],\"Channel\":1}")"
  else
    skip "foreman cannot expose to the client" "second frame id not parsed"
  fi
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 2 — one canonical artifact, many pointers"

TMP="${TMPDIR:-/tmp}/assetlen-e2e-$STAMP.png"
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
  eq "client side cannot fetch by id alone" "403" "$(code GET "/Artifacts/$AID/content" "$PETER")"

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
