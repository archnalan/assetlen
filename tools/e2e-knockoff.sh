#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — knock-off: the work plan made real (works-report.md §4.5).
#
# "Assetlen should allow for the knockoff of these items" — and the issued plan's
# rule is "No photo, no tick". Asked from both sides of the table:
#
#   Is a line ticked only on a photo, stored once in the artifact store, dated?
#   Does a second tick change nothing, and does reopening keep the record?
#   Does the delivery side tick — the bench that captures included — while the
#   client side reads, a read-only seat cannot, and a stranger learns nothing?
#   Does the photo reach the client side under the mediator's name, with none of
#   the bench's words?
#   Do planned dates, trade and area round-trip as written, in UTC?
#   And does the demo house carry the issued plan — wardrobes included?
#
# Usage:  bash tools/e2e-knockoff.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running (Development), pwsh. Run from the repo root.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 120)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/knockoff"
DEMO=de300000-0000-4000-8000-000000000010
DEMO_PASS='Assetlen#2026'

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-62s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
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
oid()  { printf '%s' "$1" | pwsh -NoProfile -Command 'try { ([Console]::In.ReadToEnd() | ConvertFrom-Json).id } catch { }'; }
jx() {
  pwsh -NoProfile -Command "\$r = [Console]::In.ReadToEnd() | ConvertFrom-Json
    \$out = & { $1 }
    if (\$out -is [bool]) { \$out.ToString().ToLower() } elseif (\$null -eq \$out) { 'null' } else { \$out }"
}

# tick TOKEN DELIVERABLE [curl -F args...] — the body and the status on the last line.
tick()     { local t="$1" d="$2"; shift 2; "${CURL[@]}" -w '\n%{http_code}' -X POST "$API/WorkPlan/Tick" -H "Authorization: Bearer $t" -F "deliverableId=$d" "$@"; }
tickcode() { tick "$@" | tail -1; }
tickbody() { tick "$@" | sed '$d'; }
photo()    { printf '%s' "photo=@$FIXTURES/$1;type=image/jpeg"; }
plan()     { req GET "/WorkPlan/GetPlan?projectId=$1${3:+&stageId=$3}" "$2"; }
line()     { jx "\$l = @(\$r.items | Where-Object { \$_.id -eq '$1' })[0]; $2"; }
# The raw string a line carries on the wire — dates unparsed, so a shifted zone would show.
rawline()  { pwsh -NoProfile -Command "\$d = [System.Text.Json.JsonDocument]::Parse([Console]::In.ReadToEnd()); foreach (\$i in \$d.RootElement.GetProperty('items').EnumerateArray()) { if (\$i.GetProperty('id').GetString() -eq '$1') { \$i.GetProperty('$2').ToString() } }"; }
# How many Site Diary entries the first line has — the delivery side counts them.
d1count() { req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$NALAN" | jx "@(\$r.data | Where-Object { \$_.deliverableId -eq '$D1ID' }).Count"; }
# A plain Diary capture by the foreman, in his own words, never exposed.
capturecrew() { "${CURL[@]}" -X POST "$API/Progress/Capture" -H "Authorization: Bearer $KATO" -F "projectId=$PID" -F "deliverableId=$D2ID" -F "description=Crew only remark $STAMP" -F "files=@$FIXTURES/done-2.jpg;type=image/jpeg"; }

echo "ASSETLEN knock-off — the work plan made real — $API"

# ── Fixtures: three machine-drawn site photos and a file that is not one ──────
rm -rf "$FIXTURES"; mkdir -p "$FIXTURES"
WIN_OUT=$(cygpath -w "$FIXTURES" 2>/dev/null || echo "$FIXTURES")
pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  \$font = New-Object System.Drawing.Font('Arial', 26, [System.Drawing.FontStyle]::Bold)
  foreach (\$i in 1..3) {
    \$b = New-Object System.Drawing.Bitmap 480,320
    \$g = [System.Drawing.Graphics]::FromImage(\$b)
    \$g.Clear([System.Drawing.Color]::FromArgb(170 + \$i * 20, 160 + \$i * 15, 140))
    \$g.DrawString(('Line done $STAMP ' + \$i), \$font, [System.Drawing.Brushes]::Black, 20, 130)
    \$b.Save(('$WIN_OUT\\done-{0}.jpg' -f \$i), [System.Drawing.Imaging.ImageFormat]::Jpeg)
    \$g.Dispose(); \$b.Dispose()
  }
" >/dev/null || { echo "FATAL: could not draw fixtures. Is pwsh on PATH?"; exit 1; }
printf 'not a photo %s\n' "$STAMP" > "$FIXTURES/note.txt"

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

mkuser() {
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

BUYER="peter.ko$STAMP@assetlen.test"
mkuser "$BUYER"                   Contractor Peter  Developer
mkuser nalan.ko@assetlen.test     Manager    Nalan  Architect
mkuser kato.ko@assetlen.test      Crew       Kato   Foreman
mkuser obi.ko@assetlen.test       Crew       Obi    Observer
mkuser dinah.ko@assetlen.test     Client     Dinah  Principal
mkuser mara.ko@assetlen.test      Client     Mara   Stranger

PETER=$(tok "$BUYER" password)
NALAN=$(tok nalan.ko@assetlen.test password)
KATO=$(tok  kato.ko@assetlen.test password)
OBI=$(tok   obi.ko@assetlen.test password)
DINAH=$(tok dinah.ko@assetlen.test password)
MARA=$(tok  mara.ko@assetlen.test password)
for t in PETER NALAN KATO OBI DINAH MARA; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Knock-off House $STAMP\",\"Description\":\"Knock-off subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Finishes\",\"DisplayOrder\":1,\"BudgetAmount\":90000000}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
S_FIN=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//')

req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.ko@assetlen.test\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.ko@assetlen.test\",\"Specialization\":9,\"Side\":0,\"Title\":\"Representative\"}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | tr '}' '\n' | grep -- "$1" | grep -o '"id":"[^"]*"' | head -1 | sed 's/.*:"//;s/"$//'; }
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id '"userFullName":"Nalan Architect"')\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id '"userFullName":"Peter Developer"')\",\"IsMediator\":false}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.ko@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Site foreman\"}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"obi.ko@assetlen.test\",\"Specialization\":7,\"Side\":1,\"Title\":\"Observer\"}" >/dev/null

# ═════════════════════════════════════════════════════════════════════════════
head_ "Planned dates, trade and area are written once and read back as written"

D1=$(req POST /Commitments/AddDeliverable "$PETER" \
  "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Terrazzo grinding\",\"Trade\":\"Terrazzo crew\",\"Area\":\"Main house\",\"PlannedStart\":\"2026-09-30T00:00:00Z\",\"PlannedEnd\":\"2026-10-16T00:00:00Z\",\"WorkDays\":14}")
D1ID=$(oid "$D1")
D2ID=$(oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Wardrobes\",\"Trade\":\"Joinery\",\"Area\":\"Main house\"}")")
D3ID=$(oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Paving\",\"Trade\":\"Paving crew\",\"Area\":\"External works\"}")")
ne "the developer plans a line"                          "" "$D1ID"
eq "…its trade is a role"                                "Terrazzo crew" "$(printf '%s' "$D1" | jx '$r.trade')"
UPD=$(req PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$D2ID\",\"PlannedStart\":\"2026-11-08T00:00:00Z\",\"PlannedEnd\":\"2026-11-13T00:00:00Z\",\"WorkDays\":6}")
eq "the dates move by edit"                              "6" "$(printf '%s' "$UPD" | jx '$r.workDays')"
P=$(plan "$PID" "$PETER")
eq "planned start reads back as the day written"         "2026-11-08T00:00:00" "$(printf '%s' "$P" | rawline "$D2ID" plannedStart)"
eq "…and planned end, at midnight UTC, unshifted"       "2026-11-13T00:00:00" "$(printf '%s' "$P" | rawline "$D2ID" plannedEnd)"
eq "…area kept"                                        "Main house" "$(printf '%s' "$P" | line "$D2ID" '$l.area')"
eq "a line cannot finish before it starts"               "400" "$(codeb PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$D2ID\",\"PlannedEnd\":\"2026-11-01T00:00:00Z\"}")"
eq "the plan runs in date order"                         "$D1ID" "$(printf '%s' "$P" | jx '$r.items[0].id')"
eq "the foreman does not edit the plan"                  "404" "$(codeb PUT /Commitments/UpdateDeliverable "$KATO" "{\"Id\":\"$D2ID\",\"Trade\":\"Somebody\"}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Who sees the Plan tab, and who ticks"

stand() { req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$1" | jx "\"{0}|{1}\" -f \$r.canSeePlan.ToString().ToLower(), \$r.canTick.ToString().ToLower()"; }
eq "the mediator: plan and tick"                         "true|true"   "$(stand "$NALAN")"
eq "the foreman (bench, captures): plan and tick"        "true|true"   "$(stand "$KATO")"
eq "the developer: plan, no tick"                        "true|false"  "$(stand "$PETER")"
eq "the representative: plan, no tick"                   "true|false"  "$(stand "$DINAH")"
eq "a read-only bench seat: no Plan tab"                 "false|false" "$(stand "$OBI")"
eq "…nor the project-wide plan"                        "404" "$(code GET "/WorkPlan/GetPlan?projectId=$PID" "$OBI")"
eq "…but it still reads its stage's checklist"         "200" "$(code GET "/WorkPlan/GetPlan?projectId=$PID&stageId=$S_FIN" "$OBI")"
eq "a stranger learns nothing"                           "404" "$(code GET "/WorkPlan/GetPlan?projectId=$PID" "$MARA")"
eq "each line says who may tick it"                      "true|false" "$(plan "$PID" "$KATO" | line "$D1ID" '$l.canTick.ToString().ToLower()')|$(plan "$PID" "$DINAH" | line "$D1ID" '$l.canTick.ToString().ToLower()')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "No photo, no tick"

NP=$(tick "$KATO" "$D1ID")
eq "a tick without a photo is refused"                   "400" "$(printf '%s' "$NP" | tail -1)"
eq "…and says why"                                     "true" "$(printf '%s' "$NP" | sed '$d' | grep -q 'No photo, no tick' && echo true || echo false)"
eq "a file that is not a photo is refused"               "400" "$(tickcode "$KATO" "$D1ID" -F "photo=@$FIXTURES/note.txt;type=text/plain")"
eq "two photos is not one"                               "400" "$(tickcode "$KATO" "$D1ID" -F "$(photo done-1.jpg)" -F "photo2=@$FIXTURES/done-2.jpg;type=image/jpeg")"
eq "…and the line is still open"                       "NotStarted" "$(plan "$PID" "$NALAN" | line "$D1ID" '$l.status')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The client side reads the plan; a read-only seat and a stranger cannot tick"

eq "the developer cannot tick"                           "403" "$(tickcode "$PETER" "$D1ID" -F "$(photo done-1.jpg)")"
eq "the representative cannot tick"                      "403" "$(tickcode "$DINAH" "$D1ID" -F "$(photo done-1.jpg)")"
eq "the read-only bench cannot tick"                     "403" "$(tickcode "$OBI" "$D1ID" -F "$(photo done-1.jpg)")"
eq "a stranger is told nothing"                          "404" "$(tickcode "$MARA" "$D1ID" -F "$(photo done-1.jpg)")"
eq "…nor can he reopen"                                "404" "$(code PUT "/WorkPlan/Reopen?deliverableId=$D1ID" "$MARA")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The foreman knocks a line off on one photo"

T1=$(tick "$KATO" "$D1ID" -F "$(photo done-1.jpg)")
eq "the tick lands"                                      "200" "$(printf '%s' "$T1" | tail -1)"
T1B=$(printf '%s' "$T1" | sed '$d')
eq "…the line is Done"                                 "Done" "$(printf '%s' "$T1B" | jx '$r.status')"
ART=$(printf '%s' "$T1B" | jx '$r.completionArtifactId')
ne "…on a stored artifact"                             "null" "$ART"
ne "…dated"                                            "null" "$(printf '%s' "$T1B" | jx '$r.completedAt')"
eq "…one Ticked event with its photo"                  "1|Ticked|$ART" "$(printf '%s' "$T1B" | jx '"{0}|{1}|{2}" -f @($r.history).Count, $r.history[0].kind, $r.history[0].artifactId')"
eq "the photo is in the artifact store"                  "200" "$(code GET "/Artifacts/Get?artifactId=$ART" "$NALAN")"
eq "…with a thumbnail"                                 "200" "$(code GET "/Artifacts/$ART/thumbnail" "$NALAN")"
DIARY=$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$NALAN")
eq "the tick is one Site Diary entry on the line"        "1|Terrazzo grinding" "$(printf '%s' "$DIARY" | jx '"{0}|{1}" -f @($r.data).Count, $r.data[0].deliverableTitle')"
EID=$(printf '%s' "$DIARY" | jx '$r.data[0].id')
eq "…in the foreman's own name there"                  "Kato Foreman" "$(printf '%s' "$DIARY" | jx '$r.data[0].createdByName')"

T2=$(tickbody "$KATO" "$D1ID" -F "$(photo done-2.jpg)")
eq "ticking a done line again changes nothing"           "Done|$ART|1" "$(printf '%s' "$T2" | jx '"{0}|{1}|{2}" -f $r.status, $r.completionArtifactId, @($r.history).Count')"
eq "…and posts nothing to the Diary"                   "1" "$(d1count)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The client side sees the photo under the mediator's name, and no crew words"

CREW=$(capturecrew)
ne "the foreman's own words sit in the Diary beside it"   "" "$(oid "$CREW")"
DP=$(plan "$PID" "$DINAH")
eq "the representative sees the line done"               "Done" "$(printf '%s' "$DP" | line "$D1ID" '$l.status')"
eq "…with its photo"                                   "$ART" "$(printf '%s' "$DP" | line "$D1ID" '$l.completionArtifactId')"
eq "…which opens for her"                              "200" "$(code GET "/Artifacts/$ART/thumbnail" "$DINAH")"
eq "…under the mediator's name"                        "Nalan Architect|Nalan Architect" "$(printf '%s' "$DP" | line "$D1ID" '"{0}|{1}" -f $l.completedByName, $l.history[0].byName')"
eq "…never the foreman's"                              "false" "$(printf '%s' "$DP" | grep -q 'Kato' && echo true || echo false)"
PD=$(req GET "/Progress/GetProgressUpdates?projectId=$PID&limit=50" "$PETER")
eq "the developer's view of the entry: the face"         "Nalan Architect" "$(printf '%s' "$PD" | jx '$r.data[0].createdByName')"
eq "…one photo, crossed"                               "1|Client" "$(printf '%s' "$PD" | jx '"{0}|{1}" -f @($r.data[0].images).Count, $r.data[0].images[0].channel')"
eq "…its words are the line's own, nobody else's"   "Done: Terrazzo grinding" "$(printf '%s' "$PD" | jx '$r.data[0].description')"
eq "…the foreman's own entry never reaches him"      "1" "$(printf '%s' "$PD" | jx '@($r.data).Count')"
eq "…and no crew words at all"                         "false" "$(printf '%s' "$PD" | grep -q "Crew only remark\|Kato" && echo true || echo false)"
eq "the checklist read the old way also shows the face"  "Nalan Architect" "$(req GET "/Commitments/GetDeliverables?projectId=$PID" "$PETER" | jx "(\$r | Where-Object { \$_.id -eq '$D1ID' }).completedByName")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Reopening takes the tick back and keeps the record"

eq "the client side does not reopen"                     "403" "$(code PUT "/WorkPlan/Reopen?deliverableId=$D1ID" "$DINAH")"
R1=$(req PUT "/WorkPlan/Reopen?deliverableId=$D1ID" "$NALAN")
eq "the mediator reopens"                                "InProgress|null" "$(printf '%s' "$R1" | jx '"{0}|{1}" -f $r.status, $(if ($r.completionArtifactId) { $r.completionArtifactId } else { "null" })')"
eq "…both events kept, oldest first"                   "Ticked,Reopened" "$(printf '%s' "$R1" | jx '(@($r.history).kind) -join ","')"
eq "…the first tick still carries its photo"           "$ART" "$(printf '%s' "$R1" | jx '$r.history[0].artifactId')"
eq "…and the reopening its hand"                       "Nalan Architect" "$(printf '%s' "$R1" | jx '$r.history[1].byName')"
eq "the photo is not deleted"                            "200|200" "$(code GET "/Artifacts/$ART/content" "$NALAN")|$(code GET "/Artifacts/$ART/thumbnail" "$DINAH")"
eq "…nor is its Diary entry"                           "1" "$(d1count)"
eq "reopening an open line changes nothing"              "2" "$(req PUT "/WorkPlan/Reopen?deliverableId=$D1ID" "$NALAN" | jx '@($r.history).Count')"
T3=$(tickbody "$NALAN" "$D1ID" -F "$(photo done-3.jpg)")
eq "ticked again, on a new photo"                        "Done|3" "$(printf '%s' "$T3" | jx '"{0}|{1}" -f $r.status, @($r.history).Count')"
ne "…a new artifact"                                   "$ART" "$(printf '%s' "$T3" | jx '$r.completionArtifactId')"
eq "…and a second Diary entry"                         "2" "$(d1count)"

# ═════════════════════════════════════════════════════════════════════════════
head_ "No other road to Done: the old checklist edit cannot tick, and the client side cannot move a line"

eq "the mediator cannot set Done by hand"                "400" "$(codeb PUT /Commitments/UpdateDeliverable "$NALAN" "{\"Id\":\"$D3ID\",\"Status\":\"Done\"}")"
eq "…nor can the developer"                            "400" "$(codeb PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$D3ID\",\"Status\":\"Done\"}")"
eq "…nor the representative"                           "400" "$(codeb PUT /Commitments/UpdateDeliverable "$DINAH" "{\"Id\":\"$D3ID\",\"Status\":\"Done\"}")"
eq "the client side does not report progress on a line"  "403" "$(codeb PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$D3ID\",\"Status\":\"InProgress\"}")"
eq "…nor reopen a ticked one by the old edit"          "403" "$(codeb PUT /Commitments/UpdateDeliverable "$DINAH" "{\"Id\":\"$D1ID\",\"Status\":\"NotStarted\"}")"
GP=$(plan "$PID" "$DINAH")
eq "the untouched line is still open, with no history"   "NotStarted|0" "$(printf '%s' "$GP" | line "$D3ID" '"{0}|{1}" -f $l.status, @($l.history).Count')"
eq "…and the ticked one still done on its photo"       "Done|3" "$(printf '%s' "$GP" | line "$D1ID" '"{0}|{1}" -f $l.status, @($l.history).Count')"
eq "the developer can still plan a line's dates"         "200" "$(codeb PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$D3ID\",\"WorkDays\":3}")"
HR=$(req PUT /Commitments/UpdateDeliverable "$NALAN" "{\"Id\":\"$D1ID\",\"Status\":\"InProgress\"}")
eq "the delivery side's old edit reopens, and records it" "InProgress|null" "$(printf '%s' "$HR" | jx '"{0}|{1}" -f $r.status, $(if ($r.completionArtifactId) { "photo" } else { "null" })')"
eq "…the ticks and their photos stay in the history"   "Ticked,Reopened,Ticked,Reopened|true" "$(plan "$PID" "$NALAN" | line "$D1ID" '"{0}|{1}" -f (@($l.history).kind -join ","), ($null -ne $l.history[2].artifactId).ToString().ToLower()')"

head_ "A photo is the bytes, not the label, and a double tap is one tick"

printf 'not a photo either %s\n' "$STAMP" > "$FIXTURES/fake.jpg"
eq "a note renamed .jpg and typed image/jpeg is refused" "400" "$(tickcode "$KATO" "$D3ID" -F "photo=@$FIXTURES/fake.jpg;type=image/jpeg")"
eq "…and the line is untouched"                        "NotStarted|0" "$(plan "$PID" "$NALAN" | line "$D3ID" '"{0}|{1}" -f $l.status, @($l.history).Count')"
for i in 1 2 3; do tickcode "$KATO" "$D3ID" -F "$(photo done-1.jpg)" >/dev/null & done; wait
eq "three taps at once: one tick in the history"         "Done|1" "$(plan "$PID" "$NALAN" | line "$D3ID" '"{0}|{1}" -f $l.status, @($l.history).Count')"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The demo house carries the issued plan"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"
D_PETER=$(tok peter@assetlen.dev "$DEMO_PASS"); D_MUSA=$(tok musa@assetlen.dev "$DEMO_PASS")
if [ -n "$D_PETER" ] && [ -n "$D_MUSA" ]; then
  DEMOPLAN=$(plan "$DEMO" "$D_PETER")
  eq "thirty-one planned lines, the guest wing's among them" "31|9" "$(printf '%s' "$DEMOPLAN" | jx '"{0}|{1}" -f @($r.items | Where-Object { $_.plannedStart }).Count, @($r.items | Where-Object { $_.area -eq "Guest wing" }).Count')"
  eq "wardrobes are in, made by the joinery"             "Joinery|Main house" "$(printf '%s' "$DEMOPLAN" | jx '$w = @($r.items | Where-Object { $_.title -eq "Wardrobes" })[0]; "{0}|{1}" -f $w.trade, $w.area')"
  eq "door adjustments take the rest of this week"       "2026-09-29|2026-10-02" "$(printf '%s' "$DEMOPLAN" | jx '$w = @($r.items | Where-Object { $_.title -eq "Door opening adjustments" })[0]; "{0}|{1}" -f $w.plannedStart.ToString("yyyy-MM-dd"), $w.plannedEnd.ToString("yyyy-MM-dd")')"
  eq "the floors are under way"                          "InProgress" "$(printf '%s' "$DEMOPLAN" | jx '@($r.items | Where-Object { $_.title -eq "Terrazzo grinding & polishing" })[0].status')"
  eq "handover work is last"                             "Snagging & cleaning" "$(printf '%s' "$DEMOPLAN" | jx '@($r.items | Where-Object { $_.plannedStart })[-1].title')"
  eq "no line names a person"                            "0" "$(printf '%s' "$DEMOPLAN" | jx '@($r.items | Where-Object { $_.trade -match "Nalan|Musa|Peter|Dinah|Grace" }).Count')"
  eq "the foreman ticks on the demo house"               "true" "$(plan "$DEMO" "$D_MUSA" | jx '$r.canTick')"
  eq "the developer reads it and does not"               "false" "$(printf '%s' "$DEMOPLAN" | jx '$r.canTick')"
else
  bad "demo personas sign in" "no token" "a token"
fi

printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
