#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — the scheduler: how the contractor drives the plan (works-report.md §4.6).
#
# "Every date follows from durations, waits and order, and every change shows its
# effect on handover before it is saved." Asked of the API:
#
#   Fed the 30 Sep inputs, does the demo house compute the issued plan — works
#   complete Wed 2 Dec, one working day of reserve, the aluminium team setting it?
#   Does a preview say what a change does, and save nothing? Does it set two
#   sequences side by side, the S1/S2 table of 30 Sep?
#   Does a save persist and re-date what follows? Does a tick pin the actual
#   finish and move the successors, with nobody editing?
#   Is the handover a Date commitment whose restatements are kept?
#   Does the delivery side drive it while the client side reads it, the bench
#   reads it by seat, and a stranger learns nothing?
#   And is the budget absent for a seat that has no money — in the API, not
#   only on the page?
#
# Usage:  bash tools/e2e-scheduler.sh [api-base] [tenant-admin-email] [password]
# Needs:  the API running (Development), pwsh (fixtures) and node (JSON). Run from the repo root.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

API="${1:-http://localhost:5140/api}"
ADMIN_EMAIL="${2:-userone@mowt.com}"
ADMIN_PASS="${3:-password}"

PASS=0; FAIL=0; SKIP=0
CURL=(curl -sk --max-time 120)
STAMP="$(date +%H%M%S)"
FIXTURES="tools/fixtures/scheduler"
DEMO=de300000-0000-4000-8000-000000000010
DEMO_PASS='Assetlen#2026'
L() { printf 'de300000-0000-4000-8000-0000000013%02d' "$1"; }   # a demo plan line

c_pass=$'\033[32m'; c_fail=$'\033[31m'; c_dim=$'\033[2m'; c_off=$'\033[0m'

ok()   { printf "  ${c_pass}PASS${c_off}  %-62s ${c_dim}%s${c_off}\n" "$1" "${2:-}"; PASS=$((PASS+1)); }
bad()  { printf "  ${c_fail}FAIL${c_off}  %-62s got %s, want %s\n" "$1" "$2" "$3"; FAIL=$((FAIL+1)); }
eq()   { if [ "$2" = "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "$2"; fi; }   # eq LABEL WANT GOT
ne()   { if [ "$2" != "$3" ]; then ok "$1" "$3"; else bad "$1" "$3" "not $2"; fi; }
yes()  { eq "$1" "true" "$2"; }
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

# jn 'JS returning a value' — r is the JSON on stdin; days stay the strings the wire carries.
jn() {
  node -e "let d='';process.stdin.on('data',c=>d+=c).on('end',()=>{let r;try{r=JSON.parse(d)}catch(e){console.log('unparsable');return}
    const v=(()=>{ $1 })();
    console.log(v===undefined||v===null?'null':typeof v==='object'?JSON.stringify(v):String(v));})"
}
# The activity with this id, as a|b|c of the fields named.
act() { jn "const a=(r.activities||[]).find(x=>x.id==='$1'); if(!a) return 'missing'; return [$2].join('|');"; }
addd() { node -e "const d=new Date('$1T00:00:00Z');d.setUTCDate(d.getUTCDate()+($2));console.log(d.toISOString().slice(0,10))"; }
sched()   { req GET "/WorkPlan/GetSchedule?projectId=$1${3:+&asAt=$3}" "$2"; }
save()    { req POST /WorkPlan/SaveSchedule "$1" "$2"; }
preview() { req POST /WorkPlan/PreviewSchedule "$1" "$2"; }
oid()  { printf '%s' "$1" | jn "return r.id"; }

echo "ASSETLEN scheduler — the plan computed, previewed and driven — $API"

# ── Fixture: one machine-drawn site photo ─────────────────────────────────────
rm -rf "$FIXTURES"; mkdir -p "$FIXTURES"
WIN_OUT=$(cygpath -w "$FIXTURES" 2>/dev/null || echo "$FIXTURES")
pwsh -NoProfile -Command "
  \$ErrorActionPreference = 'Stop'
  Add-Type -AssemblyName System.Drawing
  \$font = New-Object System.Drawing.Font('Arial', 26, [System.Drawing.FontStyle]::Bold)
  \$b = New-Object System.Drawing.Bitmap 480,320
  \$g = [System.Drawing.Graphics]::FromImage(\$b)
  \$g.Clear([System.Drawing.Color]::FromArgb(190, 175, 140))
  \$g.DrawString('Screed done $STAMP', \$font, [System.Drawing.Brushes]::Black, 20, 130)
  \$b.Save('$WIN_OUT\\done.jpg', [System.Drawing.Imaging.ImageFormat]::Jpeg)
  \$g.Dispose(); \$b.Dispose()
" >/dev/null || { echo "FATAL: could not draw the fixture. Is pwsh on PATH?"; exit 1; }
command -v node >/dev/null || { echo "FATAL: node is needed to read the JSON."; exit 1; }

ADMIN=$(tok "$ADMIN_EMAIL" "$ADMIN_PASS")
[ -z "$ADMIN" ] && { echo "FATAL: tenant admin login failed. Is the API up?"; exit 1; }

mkuser() {
  "${CURL[@]}" -o /dev/null -X POST "$API/Authorization/CreateUser" \
    -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
    -d "{\"Password\":\"password\",\"Email\":\"$1\",\"UserName\":\"${1%%@*}\",\"FirstName\":\"$3\",\"LastName\":\"$4\",\"UserRolesDto\":{\"Roles\":[\"$2\"]},\"defaultRole\":[\"$2\"]}"
}

BUYER="peter.sc$STAMP@assetlen.test"
mkuser "$BUYER"                   Contractor Peter  Developer
mkuser nalan.sc@assetlen.test     Manager    Nalan  Architect
mkuser kato.sc@assetlen.test      Crew       Kato   Foreman
mkuser obi.sc@assetlen.test       Crew       Obi    Observer
mkuser dinah.sc@assetlen.test     Client     Dinah  Principal
mkuser mara.sc@assetlen.test      Client     Mara   Stranger

PETER=$(tok "$BUYER" password)
NALAN=$(tok nalan.sc@assetlen.test password)
KATO=$(tok  kato.sc@assetlen.test password)
OBI=$(tok   obi.sc@assetlen.test password)
DINAH=$(tok dinah.sc@assetlen.test password)
MARA=$(tok  mara.sc@assetlen.test password)
for t in PETER NALAN KATO OBI DINAH MARA; do
  [ -z "${!t}" ] && { echo "FATAL: $t login failed."; exit 1; }
done

CREATED=$(req POST /ProjectsRS/CreateProject "$PETER" \
  "{\"ProjectName\":\"Scheduler House $STAMP\",\"Description\":\"Scheduler subject\",\"Location\":\"Test plot\",\"TotalBudget\":300000000,\"Currency\":\"UGX\",\"Stages\":[{\"StageName\":\"Finishes\",\"DisplayOrder\":1,\"BudgetAmount\":90000000}]}")
PID=$(oid "$CREATED")
[ -z "$PID" ] || [ "$PID" = "null" ] && { echo "FATAL: project create failed: $CREATED"; exit 1; }
S_FIN=$(req GET "/Stages/GetStagesByProjectId?projectId=$PID" "$PETER" | jn "return r[0].id")

req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"nalan.sc@assetlen.test\",\"Specialization\":3,\"Side\":1,\"Title\":\"Architect-contractor\"}" >/dev/null
req POST /ProjectMembers/AddMember "$PETER" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"dinah.sc@assetlen.test\",\"Specialization\":9,\"Side\":0,\"Title\":\"Representative\"}" >/dev/null
ROSTER=$(req GET "/ProjectMembers/GetMembersByProject?projectId=$PID" "$PETER")
member_id() { printf '%s' "$ROSTER" | jn "return (r.find(m=>m.userFullName==='$1')||{}).id"; }
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id 'Nalan Architect')\",\"IsMediator\":true}" >/dev/null
req PUT /ProjectMembers/UpdateMember "$PETER" "{\"MemberId\":\"$(member_id 'Peter Developer')\",\"IsMediator\":false}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"kato.sc@assetlen.test\",\"Specialization\":1,\"Side\":1,\"Title\":\"Site foreman\"}" >/dev/null
req POST /ProjectMembers/AddMember "$NALAN" "{\"ProjectId\":\"$PID\",\"UserEmail\":\"obi.sc@assetlen.test\",\"Specialization\":7,\"Side\":1,\"Title\":\"Observer\"}" >/dev/null

# ═════════════════════════════════════════════════════════════════════════════
head_ "The budget is money, and money is a seat: absent for the bench, in the API too"

eq "the developer reads the budget"                      "300000000" "$(req GET "/ProjectsRS/GetProjectById?projectId=$PID" "$PETER" | jn "return r.totalBudget")"
eq "…and so does the representative"                   "300000000" "$(req GET "/ProjectsRS/GetProjectById?projectId=$PID" "$DINAH" | jn "return r.totalBudget")"
eq "the foreman's seat has no money"                     "false" "$(req GET "/ProjectMembers/GetMyStanding?projectId=$PID" "$KATO" | jn "return r.canSeeMoney")"
KP=$(req GET "/ProjectsRS/GetProjectById?projectId=$PID" "$KATO")
eq "…and the project is sent to him with no budget"    "null" "$(printf '%s' "$KP" | jn "return r.totalBudget")"
eq "…nor a stage budget"                                "null" "$(printf '%s' "$KP" | jn "return r.stages[0].budgetAmount")"
eq "…nor what was released"                             "0|0" "$(printf '%s' "$KP" | jn "return r.totalFunded+'|'+r.totalRemaining")"
eq "…nor a figure on his dashboard card"                "0" "$(req GET /ProjectsRS/GetPortfolioDashboard "$KATO" | jn "return (r.projects.find(p=>p.id==='$PID')||{}).totalBudget")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Three lines, each with its days — computed before anyone has scheduled them"

A=$(oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Screed\",\"Trade\":\"Own crew\",\"Area\":\"Main house\",\"WorkDays\":3}")")
B=$(oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Epoxy floor\",\"Trade\":\"Epoxy team\",\"Area\":\"Main house\",\"WorkDays\":2}")")
C=$(oid "$(req POST /Commitments/AddDeliverable "$PETER" "{\"ProjectId\":\"$PID\",\"StageId\":\"$S_FIN\",\"Title\":\"Snagging\",\"Trade\":\"All trades\",\"Area\":\"Handover\",\"WorkDays\":1}")")
ne "three lines planned"                                 "null" "$C"
S0=$(sched "$PID" "$NALAN")
eq "the mediator reads a computed plan, not yet saved"  "false|3|true" "$(printf '%s' "$S0" | jn "return [r.isScheduled, r.activities.length, r.canEdit].join('|')")"
TODAY=$(printf '%s' "$S0" | jn "return r.today")
ne "…computed from today"                              "null" "$TODAY"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Who drives it: the delivery side edits, the client side reads, the bench by seat"

rd() { sched "$PID" "$1" | jn "return r.canEdit+'|'+r.canTick"; }
eq "the mediator plans"                                  "true|true"  "$(rd "$NALAN")"
eq "the developer reads"                                 "false|false" "$(rd "$PETER")"
eq "the representative reads"                            "false|false" "$(rd "$DINAH")"
eq "the foreman reads and ticks, and does not plan"      "false|true" "$(rd "$KATO")"
eq "a read-only bench seat has no plan"                  "404" "$(code GET "/WorkPlan/GetSchedule?projectId=$PID" "$OBI")"
eq "a stranger learns nothing"                           "404" "$(code GET "/WorkPlan/GetSchedule?projectId=$PID" "$MARA")"
NOOP="{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$A\",\"WorkDays\":4}]}"
eq "the developer cannot save a change"                  "403" "$(codeb POST /WorkPlan/SaveSchedule "$PETER" "$NOOP")"
eq "…nor the representative"                           "403" "$(codeb POST /WorkPlan/SaveSchedule "$DINAH" "$NOOP")"
eq "…nor the foreman"                                  "403" "$(codeb POST /WorkPlan/SaveSchedule "$KATO" "$NOOP")"
eq "…the read-only seat is told nothing"               "404" "$(codeb POST /WorkPlan/SaveSchedule "$OBI" "$NOOP")"
eq "…nor is a stranger"                                "404" "$(codeb POST /WorkPlan/SaveSchedule "$MARA" "$NOOP")"
eq "the client side cannot preview an edit"              "403" "$(codeb POST /WorkPlan/PreviewSchedule "$DINAH" "{\"ProjectId\":\"$PID\",\"Proposed\":$NOOP}")"
eq "…nor can the foreman"                              "403" "$(codeb POST /WorkPlan/PreviewSchedule "$KATO" "{\"ProjectId\":\"$PID\",\"Proposed\":$NOOP}")"
eq "…a stranger is told nothing"                       "404" "$(codeb POST /WorkPlan/PreviewSchedule "$MARA" "{\"ProjectId\":\"$PID\",\"Proposed\":$NOOP}")"
eq "nothing the refused changed"                         "3" "$(sched "$PID" "$NALAN" | act "$A" "a.workDays")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "Saving the order: waits on another line, drying, and a committed handover"

H1=$(addd "$TODAY" 60)
SV=$(save "$NALAN" "{\"ProjectId\":\"$PID\",\"Handover\":\"$H1\",\"Activities\":[
  {\"Id\":\"$B\",\"Waits\":[{\"Kind\":0,\"PredecessorId\":\"$A\"}]},
  {\"Id\":\"$C\",\"Waits\":[{\"Kind\":0,\"PredecessorId\":\"$B\"},{\"Kind\":2,\"Days\":3,\"Title\":\"Floor drying\"}]}]}")
eq "the plan is saved and is now scheduled"              "true" "$(printf '%s' "$SV" | jn "return r.isScheduled")"
AF=$(printf '%s' "$SV" | act "$A" "a.finish"); BS=$(printf '%s' "$SV" | act "$B" "a.start"); BF=$(printf '%s' "$SV" | act "$B" "a.finish"); CS=$(printf '%s' "$SV" | act "$C" "a.start")
yes "the epoxy starts after the screed finishes"         "$([[ "$BS" > "$AF" ]] && echo true || echo false)"
yes "snagging after the epoxy"                           "$([[ "$CS" > "$BF" ]] && echo true || echo false)"
eq "…and waits out the drying too"                     "Drying|3" "$(printf '%s' "$SV" | act "$C" "a.waits[1].kind, a.waits[1].days")"
eq "the handover is the one committed"                   "$H1|1" "$(printf '%s' "$SV" | jn "return r.handover+'|'+r.handoverHistory.length")"
WC1=$(printf '%s' "$SV" | jn "return r.worksComplete")
eq "works complete is the snagging's finish"             "$(printf '%s' "$SV" | act "$C" "a.finish")" "$WC1"
ne "…with a reserve counted in working days"           "null" "$(printf '%s' "$SV" | jn "return r.reserveDays")"
eq "snagging sets the date"                              "true|0" "$(printf '%s' "$SV" | act "$C" "a.critical, a.floatDays")"
PL=$(req GET "/WorkPlan/GetPlan?projectId=$PID" "$NALAN")
eq "the computed dates are the plan's stored dates"      "${BS}T00:00:00" "$(printf '%s' "$PL" | jn "return (r.items.find(i=>i.id==='$B')||{}).plannedStart")"
eq "a scheduled line's dates are not typed"              "400" "$(codeb PUT /Commitments/UpdateDeliverable "$PETER" "{\"Id\":\"$A\",\"PlannedStart\":\"2026-11-08T00:00:00Z\"}")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A preview says what a change does, compares two, and saves nothing"

PIN=$(addd "$AF" 10)
PV=$(preview "$NALAN" "{\"ProjectId\":\"$PID\",\"Proposed\":{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$A\",\"PinnedFinish\":\"$PIN\"}]},
  \"Alternative\":{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$A\",\"WorkDays\":1}]}}")
eq "the preview says works complete moves"               "true" "$(printf '%s' "$PV" | jn "return r.sentences[0].startsWith('Moves works complete from')")"
yes "…later"                                             "$(printf '%s' "$PV" | jn "return r.proposed.worksComplete > r.current.worksComplete")"
eq "…and what it moves"                                "true|true|true" "$(printf '%s' "$PV" | jn "return ['$A','$B','$C'].map(k=>r.moved.some(m=>m.id===k)).join('|')")"
eq "…with the proposed plan in full, for the lanes"    "$PIN" "$(printf '%s' "$PV" | jn "return r.plan.activities.find(a=>a.id==='$A').finish")"
yes "the alternative is set beside it: earlier"          "$(printf '%s' "$PV" | jn "return r.alternative.worksComplete < r.current.worksComplete")"
eq "…in the same words"                                "true" "$(printf '%s' "$PV" | jn "return r.alternativeSentences[0].startsWith('Moves works complete from')")"
AFTER=$(sched "$PID" "$NALAN")
eq "nothing was saved: no pin, same works complete"      "null|$WC1" "$(printf '%s' "$AFTER" | jn "return r.activities.find(a=>a.id==='$A').pinnedFinish+'|'+r.worksComplete")"
eq "…and the stored dates did not move"                "${BS}T00:00:00" "$(req GET "/WorkPlan/GetPlan?projectId=$PID" "$NALAN" | jn "return (r.items.find(i=>i.id==='$B')||{}).plannedStart")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A save persists and re-dates what follows"

SP=$(save "$NALAN" "{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$A\",\"PinnedFinish\":\"$PIN\"}]}")
eq "the screed now runs to its known finish"             "$PIN" "$(printf '%s' "$SP" | act "$A" "a.finish")"
BS2=$(printf '%s' "$SP" | act "$B" "a.start")
yes "…and the epoxy moves behind it"                   "$([[ "$BS2" > "$PIN" ]] && echo true || echo false)"
yes "works complete moved later"                         "$(printf '%s' "$SP" | jn "return r.worksComplete > '$WC1'")"
eq "the movement is on record, read first"               "$WC1" "$(printf '%s' "$SP" | jn "return r.previousWorksComplete")"
eq "…and in the stored plan"                           "${BS2}T00:00:00" "$(req GET "/WorkPlan/GetPlan?projectId=$PID" "$DINAH" | jn "return (r.items.find(i=>i.id==='$B')||{}).plannedStart")"
LOOP=$("${CURL[@]}" -w '\n%{http_code}' -X POST "$API/WorkPlan/SaveSchedule" -H "Authorization: Bearer $NALAN" -H "Content-Type: application/json" \
  -d "{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$A\",\"Waits\":[{\"Kind\":0,\"PredecessorId\":\"$C\"}]}]}")
eq "an order that loops back is refused"                 "400" "$(printf '%s' "$LOOP" | tail -1)"
eq "…by name"                                          "true" "$(printf '%s' "$LOOP" | sed '$d' | grep -q 'loops back' && echo true || echo false)"
eq "…and nothing of it was kept"                       "0" "$(sched "$PID" "$NALAN" | act "$A" "a.waits.length")"
eq "a rest day that is not a day of the week is refused" "400" "$(codeb POST /WorkPlan/SaveSchedule "$NALAN" "{\"ProjectId\":\"$PID\",\"RestDay\":9}")"
eq "…as is a wait of no known kind"                    "400" "$(codeb POST /WorkPlan/SaveSchedule "$NALAN" "{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$B\",\"Waits\":[{\"Kind\":7,\"Days\":3}]}]}")"
eq "…and nothing of either was kept"                   "Saturday|1" "$(sched "$PID" "$NALAN" | jn "return r.restDay+'|'+r.activities.find(a=>a.id==='$B').waits.length")"
HOL=$(save "$NALAN" "{\"ProjectId\":\"$PID\",\"Holidays\":[\"$BS2\"]}")
ne "a holiday on the epoxy's first day moves it"         "$BS2" "$(printf '%s' "$HOL" | act "$B" "a.start")"
eq "…and the calendar keeps it"                        "$BS2" "$(printf '%s' "$HOL" | jn "return r.holidays[0]")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "A tick pins the actual finish and moves the successors, with nobody editing"

BEFORE=$(sched "$PID" "$NALAN" | act "$B" "a.start")
TK=$("${CURL[@]}" -w '\n%{http_code}' -X POST "$API/WorkPlan/Tick" -H "Authorization: Bearer $KATO" -F "deliverableId=$A" -F "photo=@$FIXTURES/done.jpg;type=image/jpeg")
eq "the foreman ticks the screed on a photo"             "200" "$(printf '%s' "$TK" | tail -1)"
ST=$(sched "$PID" "$DINAH")
eq "the screed is done, finished today"                  "true|$TODAY" "$(printf '%s' "$ST" | act "$A" "a.done, a.finish")"
BT=$(printf '%s' "$ST" | act "$B" "a.start")
yes "the epoxy comes forward from the pinned finish"     "$([[ "$BT" < "$BEFORE" ]] && echo true || echo false)"
yes "…to after today"                                  "$([[ "$BT" > "$TODAY" ]] && echo true || echo false)"
eq "the tick re-dated the stored plan by itself"         "${BT}T00:00:00" "$(req GET "/WorkPlan/GetPlan?projectId=$PID" "$DINAH" | jn "return (r.items.find(i=>i.id==='$B')||{}).plannedStart")"
eq "…and a done line has nothing left to slip"         "null" "$(printf '%s' "$ST" | act "$A" "String(a.floatDays)")"

NM=$(save "$NALAN" "{\"ProjectId\":\"$PID\",\"Activities\":[{\"Id\":\"$B\",\"NeedsMoreDays\":2}]}")
eq "\"needs 2 more days\" pins the finish"               "true" "$(printf '%s' "$NM" | act "$B" "a.pinnedFinish !== null && a.pinnedFinish === a.finish")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The handover is a Date commitment: a new date restates it, the old one is kept"

H2=$(addd "$TODAY" 67)
HV=$(save "$NALAN" "{\"ProjectId\":\"$PID\",\"Handover\":\"$H2\"}")
eq "the handover moves to the new date"                  "$H2" "$(printf '%s' "$HV" | jn "return r.handover")"
eq "…both statements kept, oldest first"               "$H1,$H2|false,true" "$(printf '%s' "$HV" | jn "return r.handoverHistory.map(h=>h.date).join(',')+'|'+r.handoverHistory.map(h=>h.isCurrent).join(',')")"
REG=$(req GET "/Commitments/GetCommitments?projectId=$PID&includeSuperseded=true" "$PETER")
eq "the register holds both, the first restated"         "2|1" "$(printf '%s' "$REG" | jn "const d=r.filter(c=>c.kind==='Date'&&/^Handover/.test(c.title)); return d.length+'|'+d.filter(c=>c.supersededAt).length")"
eq "the client side reads the new handover first"        "$H2|false" "$(sched "$PID" "$PETER" | jn "return r.handover+'|'+r.canEdit")"

# ═════════════════════════════════════════════════════════════════════════════
head_ "The demo house computes the issued plan of 30 Sep"

"${CURL[@]}" -o /dev/null -X POST "$API/Dev/SeedDemo"
D_NALAN=$(tok nalan@assetlen.dev "$DEMO_PASS"); D_PETER=$(tok peter@assetlen.dev "$DEMO_PASS")
D_MUSA=$(tok musa@assetlen.dev "$DEMO_PASS")
if [ -n "$D_NALAN" ] && [ -n "$D_PETER" ] && [ -n "$D_MUSA" ]; then
  DS=$(sched "$DEMO" "$D_NALAN" 2026-09-30)
  eq "works complete Wed 2 Dec, handover Fri 4 Dec"      "2026-12-02|2026-12-04" "$(printf '%s' "$DS" | jn "return r.worksComplete+'|'+r.handover")"
  eq "one working day in reserve"                        "1|0" "$(printf '%s' "$DS" | jn "return r.reserveDays+'|'+r.daysOver")"
  eq "the aluminium sequence sets the date"              "$(L 31),$(L 16),$(L 13),$(L 14),$(L 15),$(L 30)" "$(printf '%s' "$DS" | jn "return r.criticalPath.join(',')")"
  eq "thirty-one activities; the wall's old checklist is not on it" "31|0" "$(printf '%s' "$DS" | jn "return r.activities.length+'|'+r.activities.filter(a=>a.id.startsWith('de300000-0000-4000-8000-0000000008')).length")"
  eq "main-house doors 29 Sep – 21 Oct"                   "2026-09-29|2026-10-21" "$(printf '%s' "$DS" | act "$(L 31)" "a.start, a.finish")"
  eq "epoxy after 20 days of drying, cured 7 Nov"        "2026-10-02|2026-10-21|2026-10-29|2026-11-04|2026-11-07" "$(printf '%s' "$DS" | act "$(L 6)" "a.waits.find(w=>w.kind==='Drying').start, a.waits.find(w=>w.kind==='Drying').end, a.start, a.finish, a.cureEnd")"
  eq "kitchen on the cured floor, 8 – 12 Nov"            "2026-11-08|2026-11-12" "$(printf '%s' "$DS" | act "$(L 11)" "a.start, a.finish")"
  eq "the guest-wing set made in the workshop"           "2026-10-26|2026-11-17" "$(printf '%s' "$DS" | act "$(L 13)" "a.makeStart, a.makeEnd")"
  eq "tank stands wait for the pump after making"        "2026-09-30|2026-10-06|2026-10-23|2026-10-25" "$(printf '%s' "$DS" | act "$(L 22)" "a.makeStart, a.makeEnd, a.waits[0].end, a.start")"
  eq "snagging 29 Nov – 2 Dec"                            "2026-11-29|2026-12-02" "$(printf '%s' "$DS" | act "$(L 30)" "a.start, a.finish")"
  eq "touch-up can slip 6 days before handover moves"   "5|6" "$(printf '%s' "$DS" | act "$(L 29)" "a.floatDays, a.slipDays")"
  eq "paving can slip 8"                                 "8" "$(printf '%s' "$DS" | act "$(L 28)" "a.slipDays")"
  eq "what must happen by when: seven, the first a booking" "7|2026-10-01|Booking" "$(printf '%s' "$DS" | jn "return r.actions.length+'|'+r.actions[0].by+'|'+r.actions[0].kind")"
  eq "the handover restates the 30 Sep completion"      "3|Handover Fri 4 Dec|2026-09-30" "$(printf '%s' "$DS" | jn "const h=r.handoverHistory; return h.length+'|'+h[2].title+'|'+h[0].date")"
  eq "the aluminium team is one queue of five"           "Aluminium team|5" "$(printf '%s' "$DS" | jn "return r.teams[0].teamKey+'|'+r.teams[0].orderedIds.length")"

  CS2=$(sched "$DEMO" "$D_PETER" 2026-09-30)
  eq "the developer reads the same plan, read-only"      "2026-12-02|1|false" "$(printf '%s' "$CS2" | jn "return r.worksComplete+'|'+r.reserveDays+'|'+r.canEdit")"
  eq "…naming trades, never people"                    "false" "$(printf '%s' "$CS2" | jn "return /Musa|Grace|Nalan|Kaggwa|Opio|Nabirye/.test(JSON.stringify(r.activities.map(a=>[a.title,a.trade,a.waits.map(w=>w.title)])))")"
  eq "the foreman reads it and does not plan"            "200|false" "$(code GET "/WorkPlan/GetSchedule?projectId=$DEMO" "$D_MUSA")|$(sched "$DEMO" "$D_MUSA" | jn "return r.canEdit")"
  eq "the foreman's project header carries no budget"     "null" "$(req GET "/ProjectsRS/GetProjectById?projectId=$DEMO" "$D_MUSA" | jn "return r.totalBudget")"

  DOORS="{\"ProjectId\":\"$DEMO\",\"Activities\":[{\"Id\":\"$(L 31)\",\"PinnedFinish\":\"2026-10-23\"}]}"
  S1="{\"ProjectId\":\"$DEMO\",\"Queues\":[{\"TeamKey\":\"Aluminium team\",\"OrderedIds\":[\"$(L 31)\",\"$(L 13)\",\"$(L 14)\",\"$(L 15)\",\"$(L 16)\"]}],
      \"Activities\":[{\"Id\":\"$(L 8)\",\"Waits\":[{\"Kind\":0,\"PredecessorId\":\"$(L 14)\",\"Link\":1,\"AfterMaking\":true}]},
                      {\"Id\":\"$(L 10)\",\"Waits\":[{\"Kind\":0,\"PredecessorId\":\"$(L 14)\",\"Link\":1,\"AfterMaking\":true}]}]}"
  DP=$(preview "$D_NALAN" "{\"ProjectId\":\"$DEMO\",\"AsAt\":\"2026-09-30\",\"Proposed\":$DOORS,\"Alternative\":$S1}")
  eq "doors to 23 Oct: the preview reads as it should"   "Moves works complete from Wed 2 Dec to Fri 4 Dec.|Reserve 1 → 0 days." "$(printf '%s' "$DP" | jn "return r.sentences.slice(0,2).join('|')")"
  eq "…the handover would move by a day"               "1|false" "$(printf '%s' "$DP" | jn "return r.proposed.daysOver+'|'+r.criticalPathChanged")"
  eq "S1 beside it: railing with the guest-wing doors"   "2026-12-06|Moves works complete from Wed 2 Dec to Sun 6 Dec." "$(printf '%s' "$DP" | jn "return r.alternative.worksComplete+'|'+r.alternativeSentences[0]")"
  eq "…two sets of doors, told apart by their area"    "Doors & windows installation · Main house|Doors & windows installation · Guest wing" "$(printf '%s' "$DP" | jn "const t=r.alternative.criticalTitles; return t[0]+'|'+t[2]")"
  eq "…main-house louvres last on its chain"           "$(L 31),$(L 13),$(L 14),$(L 15),$(L 16),$(L 29),$(L 30)" "$(printf '%s' "$DP" | jn "return r.alternative.criticalPath.join(',')")"
  SNAGW=$(printf '%s' "$DS" | jn "const a=r.activities.find(x=>x.id==='$(L 30)'); const w=a.waits.map(w=>({Id:w.id,Kind:0,PredecessorId:w.predecessorId})); w.push({Kind:1,Arrival:4,Until:'2026-12-01',Title:'Cleaning crew booked'}); return w")
  BK=$(preview "$D_NALAN" "{\"ProjectId\":\"$DEMO\",\"AsAt\":\"2026-09-30\",\"Proposed\":{\"ProjectId\":\"$DEMO\",\"Activities\":[{\"Id\":\"$(L 30)\",\"Waits\":$SNAGW}]}}")
  eq "a booking holding snagging sets the date alone"    "$(L 30)" "$(printf '%s' "$BK" | jn "return r.proposed.criticalPath.join(',')")"
  eq "…and nothing that can slip says it sets the date" "0" "$(printf '%s' "$BK" | jn "return r.plan.activities.filter(a=>a.critical&&a.floatDays!==0).length")"
  LS=$(sched "$DEMO" "$D_NALAN" 2026-10-03)
  eq "a late line is held at the next working day, not the rest day" "2026-10-04|2026-10-05" "$(printf '%s' "$LS" | act "$(L 1)" "a.finish")|$(printf '%s' "$LS" | act "$(L 25)" "a.start")"
  eq "the preview saved nothing on the demo"             "null|2026-12-02" "$(sched "$DEMO" "$D_NALAN" 2026-09-30 | jn "return r.activities.find(a=>a.id==='$(L 31)').pinnedFinish+'|'+r.worksComplete")"
  eq "the developer cannot preview the demo"             "403" "$(codeb POST /WorkPlan/PreviewSchedule "$D_PETER" "{\"ProjectId\":\"$DEMO\",\"Proposed\":$DOORS}")"
  eq "…nor save it"                                    "403" "$(codeb POST /WorkPlan/SaveSchedule "$D_PETER" "$DOORS")"
else
  bad "demo personas sign in" "no token" "a token"
fi

printf "\n  %d passed, %d failed, %d skipped\n\n" "$PASS" "$FAIL" "$SKIP"
[ "$FAIL" -eq 0 ]
