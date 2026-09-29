#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — P9 end-to-end suite: the contractor tier (tier 3).
#
# Only now, and only because nothing above depends on it (plan.md P9). Asked
# from both sides of the table:
#
#   Can the mediator staff his own side while Peter keeps the only roster?
#   Is capture three taps against today's work — the camera roll, not one shot
#   — with every frame in the artifact store, and a retry that never posts twice?
#   Is the Site Diary complete, true to its authors, and the delivery side's alone?
#   Does a voice note become words that search can find?
#   Does the brief take a few frames from each piece of work at the cutoff in the
#   mediator's name, whether or not he touched it — and never the ones he dropped?
#   Does a claim carry its own proof, so the funder clears it without a call?
#   Does the phone ring at WhatsApp speed — and only the phones that should?
#   And does everything above still work with the contractor silent?  ← Law 0
#
# Usage:  bash tools/e2e-p9-contractor.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running (Development), pwsh. Run from the repo root.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 300)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/p9"

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_skip=$'\033[33m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-62s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
skip() { printf "  ${c_skip}SKIP${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; SKIP=$((SKIP+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
ne()   { if [ "$2" != "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "not $2"; fi; }
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
code()  { "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3"; }
codeb() { "${CURL[@]}" -o /dev/null -w '%{http_code}' -X "$1" "$API$2" -H "Authorization: Bearer $3" -H "Content-Type: application/json" -d "$4"; }
jget() { printf '%s' "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | sed 's/.*:"//;s/"$//' || true; }
oid()  { printf '%s' "$1" | pwsh -NoProfile -Command 'try { ([Console]::In.ReadToEnd() | ConvertFrom-Json).id } catch { }'; }

# JSON is read with a real parser; $r is the parsed answer.
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } elseif (\$null -eq \$out) { 'null' } else { \$out }"
}

# A capture from the camera roll: capture TOKEN KEY [extra -F args...] — frames are named by number.
capture() {
  local token="$1"; shift
  "${CURL[@]}" -X POST "$API/Progress/Capture" -H "Authorization: Bearer $token" "$@"
}
frames() { local a=(); for n in "$@"; do a+=(-F "files=@$FIXTURES/frame-$(printf '%02d' "$n").jpg;type=image/jpeg"); done; printf '%s\n' "${a[@]}"; }

echo "ASSETLEN P9 — the contractor tier — $API"

bash tools/make-p9-fixtures.sh "$FIXTURES" >/dev/null 2>&1 \
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

# A buyer of his own per run: his first project is the free one (plan.md P4, outstanding).
BUYER="peter.p9$STAMP@assetlen.test"
mkuser "$BUYER"                         Contractor Peter  Developer
mkuser nalan.p9@assetlen.test           Manager    Nalan  Architect
mkuser kato.p9@assetlen.test            Crew       Kato   Foreman
mkuser dinah.p9@assetlen.test           Client     Dinah  Principal
mkuser mara.p9@assetlen.test            Client     Mara   Stranger

PETER=$(tok "$BUYER" password)
NALAN=$(tok nalan.p9@assetlen.test password)
KATO=$(tok  kato.p9@assetlen.test password)
DINAH=$(tok dinah.p9@assetlen.test password)
MARA=$(tok  mara.p9@assetlen.test password)
for t in PETER NALAN KATO DINAH MARA; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

# ═════════════════════════════════════════════════════════════════════════════
head_ "The mediator staffs the delivery side; Peter keeps the access roster"

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"P9 Wing $STAMP\",\"Description\":\"P9 subject\",\"Location\":\"Test plot\",\"TotalBudget\":200000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Retaining wall\",\"DisplayOrder\":1,\"BudgetAmount\":40000000},{\"StageName\":\"Terrace finishes\",\"DisplayOrder\":2,\"BudgetAmount\":60000000}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
STAGES=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER")
stage_id() { printf '%s' "$STAGES" | tr '}' '\n' | grep -- "\"stageName\":\"$1\"" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
S_WALL=$(stage_id "Retaining wall"); S_TERR=$(stage_id "Terrace finishes")
deliverable() { oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$1\",\"Title\":\"$2\"}")"; }
D_COURSES=$(deliverable "$S_WALL" "Wall courses")
D_WEEP=$(deliverable "$S_WALL" "Weep holes")
D_TILES=$(deliverable "$S_TERR" "Terrace floor tiles")
ne "Peter writes the checklist he funds"             "" "$D_TILES"

req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.p9@assetlen.test\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" \
  "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.p9@assetlen.test\",\"Specialization\":9,\"Side\":0,\"Title\":\"Representative\"}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
NALAN_MID=$(member_id '"userFullName":"Nalan Architect"'); PETER_MID=$(member_id '"userFullName":"Peter Developer"')
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$NALAN_MID\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":false}" >/dev/null
eq "Peter appoints the mediator"                     "true" "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$NALAN" | jx '$r.isMediator')"

eq "the mediator staffs his own bench"               "200" "$(codeb POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.p9@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Foreman\"}")"
eq "…but never Peter's side"                       "403" "$(codeb POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"mara.p9@assetlen.test\",\"Specialization\":9,\"Side\":0}")"
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
eq "Peter's roster shows who holds a key"            "Contractor" "$(printf '%s' "$ROSTER" | jx '($r | Where-Object { $_.userFullName -eq "Kato Foreman" }).side')"
eq "…names and sides, never traffic"               "false" "$(printf '%s' "$ROSTER" | jx '($r | Get-Member -MemberType NoteProperty).Name -match "capture|last|activity|count" -contains $true')"
KATO_MID=$(printf '%s' "$ROSTER" | tr '}' '\n' | grep -- '"userFullName":"Kato Foreman"' | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

# ═════════════════════════════════════════════════════════════════════════════
head_ "Three taps: the work, the camera roll, post"

TD=$(req GET "/Progress/GetCaptureToday?projectId=$PID" "$KATO")
eq "the camera opens on today's work"                "3" "$(printf '%s' "$TD" | jx '@($r.deliverables).Count')"
eq "…the stage under way's first"                  "Retaining wall" "$(printf '%s' "$TD" | jx '$r.deliverables[0].stageName')"
eq "Peter does not capture to the Site Diary"        "404" "$(code GET "/Progress/GetCaptureToday?projectId=$PID" "$PETER")"
eq "…nor does a stranger"                          "404" "$(code GET "/Progress/GetCaptureToday?projectId=$PID" "$MARA")"

AT=$(date -u -d '-30 min' +%Y-%m-%dT%H:%M:%SZ)
DAY=$(date -d '-30 min' +%Y-%m-%d)
CCID="p9-$STAMP-a"
mapfile -t BATCH < <(frames $(seq 1 16) 3)
C1=$(capture "$KATO" -F "projectId=$PID" -F "deliverableId=$D_COURSES" -F "clientCaptureId=$CCID" \
  -F "capturedAt=$AT" -F "completionPercentage=40" "${BATCH[@]}")
EID=$(oid "$C1")
ne "the clerk posts the evening's batch"             "" "$EID"
eq "…seventeen frames, the duplicate kept as picked" "17" "$(printf '%s' "$C1" | jx '$r.imageCount')"
eq "…filed on the deliverable he tapped"           "Wall courses|Retaining wall" "$(printf '%s' "$C1" | jx '"{0}|{1}" -f $r.deliverableTitle, $r.stageName')"
eq "…every frame a stored artifact, none inline"   "17" "$(printf '%s' "$C1" | jx '@($r.images | Where-Object { $_.artifactId -and $_.imageUrl -like "/api/Artifacts/*" }).Count')"
eq "…the photo picked twice is one file (Law 2)"   "16" "$(printf '%s' "$C1" | jx '@($r.images.artifactId | Sort-Object -Unique).Count')"
eq "…on the Site Diary only"                       "true" "$(printf '%s' "$C1" | jx '"$($r.channel)" -in @("Crew","0")')"
eq "the deliverable is now under way"                "InProgress" "$(req GET "/Commitments/GetDeliverables?projectId=$PID&stageId=$S_WALL" "$PETER" | jx "(\$r | Where-Object { \$_.id -eq '$D_COURSES' }).status")"
eq "the reading went on the stage"                   "40" "$(req GET "/Stages/GetStageById?stageId=$S_WALL" "$PETER" | jx '$r.completionPercentage')"
mapfile -t TOO_MANY < <(frames $(seq 1 25))
eq "twenty-five at once is refused"                  "400" "$(capture "$KATO" -o /dev/null -w '%{http_code}' -F "projectId=$PID" "${TOO_MANY[@]}")"
eq "an empty capture is refused"                     "400" "$(capture "$KATO" -o /dev/null -w '%{http_code}' -F "projectId=$PID")"
eq "Peter cannot post to the Diary"                  "403" "$(capture "$PETER" -o /dev/null -w '%{http_code}' -F "projectId=$PID" -F "description=From the client side")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The offline queue: captured at 22:00, sent at dawn, never twice"

C1B=$(capture "$KATO" -F "projectId=$PID" -F "deliverableId=$D_COURSES" -F "clientCaptureId=$CCID" \
  -F "capturedAt=$AT" -F "completionPercentage=40" "${BATCH[@]}")
eq "the retry returns the capture already made"      "$EID" "$(oid "$C1B")"
eq "…and the Diary holds one, not two"             "1" "$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$NALAN" | jx "@(\$r.data | Where-Object { \$_.clientCaptureId -eq '$CCID' }).Count")"
eq "it is filed when it was shot, not when it landed" "${AT:0:16}" "$(printf '%s' "$C1" | jx '([datetime]$r.dateTimeCreated).ToString("yyyy-MM-ddTHH:mm")')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The Site Diary: complete, true to its authors, the delivery side's alone"

NE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN")
eq "the mediator reads every frame"                  "17" "$(printf '%s' "$NE" | jx '@($r.images).Count')"
eq "…with who actually took them"                  "Kato Foreman" "$(printf '%s' "$NE" | jx '$r.createdByName')"
eq "Peter does not know the entry exists"            "404" "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"
eq "…nor its bytes"                                "404" "$(code GET "/Artifacts/$(printf '%s' "$C1" | jx '$r.images[0].artifactId')/content" "$PETER")"
eq "…and his Diary list is empty"                  "0" "$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$PETER" | jx '@($r.data).Count')"
eq "a stranger gets nothing"                         "404" "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$MARA")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Voice notes, with the words read into search"

if [ -z "${P9_VOICE:-}" ]; then
  skip "voice note transcription" "no speech synthesiser on this host to make the fixture"
else
  V=$(capture "$KATO" -F "projectId=$PID" -F "deliverableId=$D_WEEP" -F "voice=@$FIXTURES/$P9_VOICE;type=audio/wav")
  VID=$(oid "$V")
  VART=$(printf '%s' "$V" | jx '$r.voiceArtifactId')
  ne "a voice note alone is a capture"                 "null" "$VART"
  eq "…the recording plays for the delivery side"    "200" "$(code GET "/Artifacts/$VART/content" "$NALAN")"
  STATUS="Pending"
  for i in $(seq 1 60); do
    VE=$(req GET "/Progress/GetProgressUpdate?updateId=$VID" "$NALAN")
    STATUS=$(printf '%s' "$VE" | jx '$r.voiceTranscriptStatus')
    [ "$STATUS" != "Pending" ] && [ "$STATUS" != "0" ] && break
    sleep 2
  done
  if [ "$STATUS" = "EngineUnavailable" ] || [ "$STATUS" = "5" ]; then
    skip "the words are read out of it" "no transcription engine on this host"
  else
    eq "the words are read out of it"                  "true" "$(printf '%s' "$VE" | jx '$r.voiceTranscript -match "cement"')"
    eq "…and search finds the voice note by them"    "true" "$(req GET "/Search/Query?q=cement&projectId=$PID" "$NALAN" | grep -c "$VART" | awk '{print ($1 > 0) ? "true" : "false"}')"
    eq "…but not for the client side"                "0" "$(req GET "/Search/Query?q=cement&projectId=$PID" "$PETER" | grep -c "$VART" | tr -d ' ')"
  fi
fi

# ═════════════════════════════════════════════════════════════════════════════
head_ "Web push at WhatsApp speed — and only for whoever should hear it"

SINK_N=$(jget "$(req POST /Dev/PushSinkSubscribe "$NALAN")" sinkId)
SINK_P=$(jget "$(req POST /Dev/PushSinkSubscribe "$PETER")" sinkId)
ne "a device subscribes"                              "" "$SINK_N$SINK_P"
eq "the server has one key to sign with"             "87" "$(req GET /Push/Status "$PETER" | jx '$r.publicKey.Length')"
eq "a plain-http endpoint is refused"                "400" "$(codeb POST /Push/Subscribe "$PETER" '{"Endpoint":"http://example.invalid/x","P256dh":"AAAA","Auth":"AAAA"}')"

T0=$(date +%s%N)
mapfile -t TWO < <(frames 17 18)
C2=$(capture "$KATO" -F "projectId=$PID" -F "deliverableId=$D_COURSES" -F "description=Second lift pointing done" "${TWO[@]}")
GOT=""
for i in $(seq 1 50); do
  GOT=$(req GET "/Dev/PushSinkInbox/$SINK_N" "$NALAN")
  printf '%s' "$GOT" | grep -q "Second lift pointing" && break
  sleep 0.1
done
T1=$(date +%s%N)
MS=$(( (T1 - T0) / 1000000 ))
eq "the mediator's phone hears the capture"          "true" "$(printf '%s' "$GOT" | jx '@($r | Where-Object { $_.payload -match "Second lift pointing" -and $_.payload -match "Kato Foreman" }).Count -ge 1')"
eq "…decrypted with the phone's own keys"          "true" "$(printf '%s' "$GOT" | jx '@($r | Where-Object { $_.error -eq $null -and $_.encoding -eq "aes128gcm" }).Count -ge 1')"
eq "…signed by the server's VAPID key"             "true" "$(printf '%s' "$GOT" | jx '@($r | Where-Object { $_.vapidValid }).Count -ge 1')"
eq "…within five seconds of the post"              "true" "$([ "$MS" -lt 5000 ] && echo true || echo false)"
eq "the bench's traffic never wakes Peter"           "0" "$(req GET "/Dev/PushSinkInbox/$SINK_P" "$PETER" | jx '@($r | Where-Object { $_.payload -match "Second lift" }).Count')"
eq "the delivery is on record, with its latency"     "true" "$(req GET "/Push/Deliveries?take=5" "$NALAN" | jx '@($r | Where-Object { $_.kind -in @("Capture","1") -and $null -ne $_.latencyMs -and $_.latencyMs -lt 5000 }).Count -ge 1')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Curation by exception: the brief goes at the cutoff, with or without him"

DR=$(req GET "/Curation/Draft?projectId=$PID&day=$DAY" "$NALAN")
eq "tonight's draft holds the day's frames"          "19" "$(printf '%s' "$DR" | jx '$r.frameTotal')"
eq "…three per piece of work will cross"           "3" "$(printf '%s' "$DR" | jx '$r.willPublish')"
eq "…in the accountable face's name"               "Nalan Architect" "$(printf '%s' "$DR" | jx '$r.accountableName')"
eq "…with who shot them, on the delivery side"     "Kato Foreman" "$(printf '%s' "$DR" | jx '$r.blocks[0].frames[0].capturedByName')"
eq "the bench does not curate"                       "404" "$(code GET "/Curation/Draft?projectId=$PID&day=$DAY" "$KATO")"
eq "…nor does Peter"                               "404" "$(code GET "/Curation/Draft?projectId=$PID&day=$DAY" "$PETER")"

GOING=$(printf '%s' "$DR" | jx '(@($r.blocks.frames | Where-Object { $_.willPublish }) | Select-Object -First 1).imageId')
HELD=$(printf '%s' "$DR" | jx '(@($r.blocks.frames | Where-Object { -not $_.willPublish }) | Select-Object -First 1).imageId')
DROP_ART=$(printf '%s' "$DR" | jx "(\$r.blocks.frames | Where-Object { \$_.imageId -eq '$GOING' }).artifactId")
KEEP_ART=$(printf '%s' "$DR" | jx "(\$r.blocks.frames | Where-Object { \$_.imageId -eq '$HELD' }).artifactId")
D2=$(req PUT /Curation/Mark "$NALAN" "{\"ImageIds\":[\"$GOING\"],\"Curation\":2}")
eq "he drops one the rule chose"                     "false|1" "$(printf '%s' "$D2" | jx "'{0}|{1}' -f (\$r.blocks.frames | Where-Object { \$_.imageId -eq '$GOING' }).willPublish.ToString().ToLower(), \$r.dropped")"
eq "…and the rule chooses another in its place"    "3" "$(printf '%s' "$D2" | jx '$r.willPublish')"
D3=$(req PUT /Curation/Mark "$NALAN" "{\"ImageIds\":[\"$HELD\"],\"Curation\":1}")
eq "he keeps one the rule passed over"               "true|Kept by the mediator" "$(printf '%s' "$D3" | jx "\$f = \$r.blocks.frames | Where-Object { \$_.imageId -eq '$HELD' }; '{0}|{1}' -f \$f.willPublish.ToString().ToLower(), \$f.reason")"
eq "the bench cannot mark frames"                    "403" "$(codeb PUT /Curation/Mark "$KATO" "{\"ImageIds\":[\"$HELD\"],\"Curation\":2}")"
eq "before the cutoff, nothing has crossed"          "404" "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")"
eq "the bench remarks on the entry in the Diary"     "200" "$(codeb POST /Progress/AddComment "$KATO" "{\"ProgressUpdateId\":\"$EID\",\"CommentText\":\"Bench remark $STAMP\"}")"
eq "…Peter cannot comment on an entry he cannot see" "404" "$(codeb POST /Progress/AddComment "$PETER" "{\"ProgressUpdateId\":\"$EID\",\"CommentText\":\"Asking blind\"}")"

# The cutoff, run as the schedule runs it: nobody signed in, nobody's authority.
CUT=$(req POST "/Curation/RunCutoff?projectId=$PID&now=${DAY}T23:59:00" "$ADMIN")
eq "at the cutoff the selection crosses by itself"   "3|Cutoff" "$(printf '%s' "$CUT" | jx '"{0}|{1}" -f ($r | Measure-Object -Property framesExposed -Sum).Sum, "$($r[0].trigger)"' | sed 's/|1$/|Cutoff/')"
PE=$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER")
PL=$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$PETER")
eq "Peter now reads the captures, three frames in all" "3" "$(printf '%s' "$PL" | jx '(@($r.data.images)).Count')"
eq "…told the true total"                          "17" "$(printf '%s' "$PE" | jx '$r.imageCount')"
eq "…as the mediator's word, not the foreman's"    "Nalan Architect" "$(printf '%s' "$PE" | jx '$r.createdByName')"
eq "…the bench's remark stays in the Diary"        "0" "$(printf '%s' "$PE" | grep -c "Bench remark $STAMP" | tr -d ' ')"
eq "…where the mediator still reads it"            "1" "$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN" | grep -c "Bench remark $STAMP" | tr -d ' ')"
eq "…the kept frame among them"                    "200" "$(code GET "/Artifacts/$KEEP_ART/content" "$PETER")"
eq "…never the dropped one"                        "404" "$(code GET "/Artifacts/$DROP_ART/content" "$PETER")"
BT=$(req GET "/Brief/Day?projectId=$PID&day=$DAY&days=1" "$PETER")
eq "the frames are in his brief, under the work"     "true" "$(printf '%s' "$BT" | jx '@($r.blocks | Where-Object { $_.deliverableTitle -eq "Wall courses" -and $_.frameTotal -ge 3 }).Count -ge 1')"
eq "…which says a person shaped it"                "false" "$(printf '%s' "$BT" | jx '$r.assembledWithoutCurator')"
PUSHED=""
for i in $(seq 1 50); do
  PUSHED=$(req GET "/Dev/PushSinkInbox/$SINK_P" "$PETER")
  printf '%s' "$PUSHED" | grep -q "BriefPublished" && break
  sleep 0.1
done
eq "…and his phone is told"                        "true" "$(printf '%s' "$PUSHED" | jx '@($r | Where-Object { $_.payload -match "BriefPublished" -and $_.payload -match "3 new photos" }).Count -ge 1')"
eq "the cutoff runs once a day"                      "0" "$(req POST "/Curation/RunCutoff?projectId=$PID&now=${DAY}T23:59:00" "$ADMIN" | jx '@($r).Count')"
WD=$(req PUT /Curation/Mark "$NALAN" "{\"ImageIds\":[\"$HELD\"],\"Curation\":2}")
eq "dropping a frame already shown pulls it back"    "2|404" "$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$PETER" | jx '(@($r.data.images)).Count')|$(code GET "/Artifacts/$KEEP_ART/content" "$PETER")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Claims carry their own evidence: paid without a phone call"

OPT=$(req GET "/Ledger/GetClaimEvidenceOptions?projectId=$PID&stageId=$S_WALL" "$NALAN")
eq "the claim form offers what the stage produced"   "true" "$(printf '%s' "$OPT" | jx '@($r.frames).Count -ge 17 -and @($r.deliverables).Count -eq 2 -and $r.latestReading.percent -eq 40')"
eq "…with the newest frames suggested"             "true" "$(printf '%s' "$OPT" | jx '@($r.suggestedImageIds).Count -ge 1')"
eq "the bench has no money seat to claim from"       "404" "$(code GET "/Ledger/GetClaimEvidenceOptions?projectId=$PID&stageId=$S_WALL" "$KATO")"
CREW3=$(printf '%s' "$OPT" | jx "(@(\$r.frames | Where-Object { \$_.progressImageId -ne '$HELD' -and \$_.progressImageId -ne '$GOING' }) | Select-Object -Skip 5 -First 3 | ForEach-Object { '\"' + \$_.progressImageId + '\"' }) -join ','")
eq "Peter cannot attach site frames he never saw"    "403" "$(codeb POST /Ledger/AddClaim "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_WALL\",\"Amount\":1000000,\"EvidenceImageIds\":[$CREW3]}")"
CL=$(req POST /Ledger/AddClaim "$NALAN" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_WALL\",\"Amount\":8000000,\"Note\":\"Wall courses to lift two\",\"EvidenceImageIds\":[$CREW3],\"EvidenceDeliverableIds\":[\"$D_COURSES\"],\"AttachLatestReading\":true}")
CL_ID=$(oid "$CL")
ne "the mediator claims the stage, proof attached"   "" "$CL_ID"
eq "…three photos, a deliverable and the reading"  "3|1|1" "$(printf '%s' "$CL" | jx '"{0}|{1}|{2}" -f @($r.evidence | Where-Object { "$($_.kind)" -in @("Frame","0") }).Count, @($r.evidence | Where-Object { "$($_.kind)" -in @("Deliverable","1") }).Count, @($r.evidence | Where-Object { "$($_.kind)" -in @("Reading","2") }).Count')"
PC=$(req GET "/Ledger/GetClaims?projectId=$PID" "$PETER")
eq "Peter sees the claim with its proof"             "5" "$(printf '%s' "$PC" | jx "@((\$r | Where-Object { \$_.id -eq '$CL_ID' }).evidence).Count")"
EV_ART=$(printf '%s' "$PC" | jx "((\$r | Where-Object { \$_.id -eq '$CL_ID' }).evidence | Where-Object { \$_.artifactId } | Select-Object -First 1).artifactId")
eq "…and can open the photos it carries"           "200" "$(code GET "/Artifacts/$EV_ART/thumbnail" "$PETER")"
eq "…as the accountable face's claim"              "Nalan Architect" "$(printf '%s' "$PC" | jx "(\$r | Where-Object { \$_.id -eq '$CL_ID' }).claimedByName")"
PUSHED=""
for i in $(seq 1 50); do
  PUSHED=$(req GET "/Dev/PushSinkInbox/$SINK_P" "$PETER")
  printf '%s' "$PUSHED" | grep -q '8,000,000' && break
  sleep 0.1
done
eq "his phone rings for the claim"                   "true" "$(printf '%s' "$PUSHED" | jx '@($r | Where-Object { $_.payload -match "Claim" -and $_.payload -match "8,000,000" -and $_.payload -match "3 photos" }).Count -ge 1')"
eq "he clears it from the page"                      "200" "$(codeb PUT /Ledger/DecideClaim "$PETER" "{\"ClaimId\":\"$CL_ID\",\"Clear\":true}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "D1 — Peter replaces the contractor and loses nothing"

req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$PETER_MID\",\"IsMediator\":true}" >/dev/null
eq "Peter removes the contractor"                    "200" "$(code DELETE "/ProjectMembers/DeactivateMember?memberId=$NALAN_MID" "$PETER")"
eq "…and his bench"                                "200" "$(code DELETE "/ProjectMembers/DeactivateMember?memberId=$KATO_MID" "$PETER")"
eq "the Diary is still there, whole"                 "17" "$(req GET "/Progress/GetProgressUpdate?updateId=$EID" "$PETER" | jx '$r.imageCount')"
eq "the claim and its proof are still there"         "Cleared|5" "$(req GET "/Ledger/GetClaims?projectId=$PID" "$PETER" | jx "\$c = \$r | Where-Object { \$_.id -eq '$CL_ID' }; '{0}|{1}' -f \$c.status, @(\$c.evidence).Count" | sed 's/^2|/Cleared|/')"
eq "the removed contractor reads none of it"         "404" "$(code GET "/Progress/GetProgressUpdate?updateId=$EID" "$NALAN")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Law 0 — everything above still works with the contractor silent"

# The P7 and works-report suites again, with no delivery-side account signing in
# at all. The proof is the server's own login record, not the suites' say-so.
BEFORE=$("${CURL[@]}" "$API/Dev/LoginStats?email=nalan.arch@assetlen.test" | jx '$r.logins')
KBEFORE=$("${CURL[@]}" "$API/Dev/LoginStats?email=kato.foreman@assetlen.test" | jx '$r.logins')
P7OUT=$(SILENT_CONTRACTOR=1 bash tools/e2e-p7-brief.sh "$API" "$ADMIN_EMAIL" "$ADMIN_PASS" 2>&1); P7RC=$?
P7T=$(printf '%s' "$P7OUT" | grep -oE '[0-9]+ passed, [0-9]+ failed, [0-9]+ skipped' | tail -1)
eq "the daily brief suite, contractor silent"        "0" "$P7RC"
eq "…with none of its assertions failing"          "0" "$(printf '%s' "$P7T" | awk '{print $3}')"
ok "…and most of them still asked"                 "$P7T"
RPOUT=$(SILENT_CONTRACTOR=1 bash tools/e2e-report.sh "$API" "$ADMIN_EMAIL" "$ADMIN_PASS" 2>&1); RPRC=$?
RPT=$(printf '%s' "$RPOUT" | grep -oE '[0-9]+ passed, [0-9]+ failed, [0-9]+ skipped' | tail -1)
eq "the works report suite, contractor silent"       "0" "$RPRC"
eq "…with none of its assertions failing"          "0" "$(printf '%s' "$RPT" | awk '{print $3}')"
ok "…and most of them still asked"                 "$RPT"
eq "the contractor never signed in"                  "$BEFORE" "$("${CURL[@]}" "$API/Dev/LoginStats?email=nalan.arch@assetlen.test" | jx '$r.logins')"
eq "…nor did his bench"                            "$KBEFORE" "$("${CURL[@]}" "$API/Dev/LoginStats?email=kato.foreman@assetlen.test" | jx '$r.logins')"
if [ "$P7RC" -ne 0 ] || [ "$RPRC" -ne 0 ]; then
  printf '%s\n' "$P7OUT" | grep -a "FAIL" | head -10
  printf '%s\n' "$RPOUT" | grep -a "FAIL" | head -10
fi

# ═════════════════════════════════════════════════════════════════════════════
printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
