# The Works Report — deliverable plan

**Written 2026-09-28**, the evening Peter asked Nalan:
*"Send me a full works report and let me know if you need more time and how long?"*
— two days before the end-of-September completion date he set on 13 Aug.

This file is the spec for that report as a product feature: **a visual, issued account of a
project at any moment — what is, and what is scheduled — assembled from the record, with
nobody having to write it.** It is built in slices across P4–P7; [plan.md](plan.md) carries
the track, this file carries the detail. It defers to [assetlen.md](assetlen.md).

---

## 1. Why this, and why now

The request is not for a photo album. Read against the thread, *"full works report … do you
need more time and how long"* is the §8 ship test asked in Peter's own words: **hold a past
commitment (done by 30 Sep) against present reality (doors started 28 Sep).**

What the 5 Aug – 28 Sep window shows the report must carry
([whatsapp-evidence.md](whatsapp-evidence.md), window to be appended):

| Observation | What the report needs |
|---|---|
| Deadline set 13 Aug, restated "on track" 22 Aug, queried 28 Sep | A **date commitment with its history**, today, and a forecast |
| Guest wing plaster: 80% (24 Aug) → 90% "complete this week" (2 Sep) → 90% (4 Sep) → "continues" to 22 Sep | **Progress readings over time**, and a stall that surfaces itself |
| "By Tuesday", "tomorrow", "this week" — several lapsed | **Short-horizon promises** kept and checked |
| Pantry → dining, terrazzo on terrace + laundry, epoxy grey, guest wing "as planned", parapet raised, first-floor doors cut | **Decisions and variations**, each with who, when, evidence — and cost/time impact, or a visible *"not costed"* |
| Window team unreachable, epoxy no-show, aluminium unfabricated, 4 openings without windows, machinery, power | **Blockers owned by named third parties**, with age |
| Nalan's ~20 bulleted *"Summary update"* posts | The richest structured input — an extraction target, never a dependency |
| Peter asked *"Any updates?"* at least seven times | The report must be **pull-free**: available at any moment, and issued on a cadence regardless |
| 91 files, 20 videos, a dozen misfiled by day, 3 duplicates, one hand-annotated line | **Footage re-joined to its moment** by filename timestamp; hash-deduplicated; annotations shown as layers |

### How it differs from the Client Brief

| | Client Brief (P7) | **Works Report** |
|---|---|---|
| Question | *What moved today?* | *Where does everything stand, and what is next?* |
| Span | One day | The whole project, at a point in time, with a "since last report" window |
| Shape | Grouped by deliverable, newest first | Stage board + schedule + decisions + money + footage |
| Life | Publishes at the cutoff, then scrolls away | **Issued and frozen** — a permanent, citable record, like a valuation report |

Both are assembled with no curator. The Brief is the daily trickle; the Report is the
document that gets forwarded, printed, and pulled in a dispute.

---

## 2. Non-negotiables

1. **Numbers, dates and states come from records, never from prose generation.** Every
   figure on the page is computed from a table. The drafting model writes short descriptions
   only, and cannot introduce a number that is not already a fact (§6).
2. **Law 0.** The report generates with the contractor silent — from Peter's own imported
   export (a client-side import is his: `tbl_IngestBatch.ImportedSide`), his funding
   declarations and extraction. Captures from Nalan's side enrich it; they are not required.
3. **Truth floor.** Money, dates, agreed specs, blockers and decisions owed appear whether or
   not anyone would like them to. A mediator may add a covering note; they may not remove a
   floor item. Gaps are shown as gaps: *"not costed"*, *"no approval on record"*,
   *"no reading since 4 Sep"*.
4. **Every claim is one tap from its evidence.** Each card carries source chips — message,
   photo, capture, release — that open the original.
5. **Issued means frozen.** An issued report is an immutable snapshot with a permanent
   address. The next report shows what changed since this one; it never rewrites it.
6. **Holds, not a network** (amended 2026-09-29 — see §4.4). Peter thinks in stages, so there
   is still no project-wide Gantt, no network of arrows and no computed critical path. What
   *is* drawn is the pattern the contractor reports from site: an activity **waits** while
   something is put in place, then **works** in a day or three. A hold is a wait placed in front
   of the activities a person chose; it pushes those, and only those. The one project deadline
   strip stays.
7. **About the work, not the people** (owner decision, 2026-09-29). The report names roles —
   *client*, *contractor*, *engineer*, *aluminium team* — never a person's name, in every
   section: decisions, blockers, quotes, footers and the drafting model's output. Accountability
   still lives in the app (the register keeps who agreed what); the report is the document that
   gets forwarded and printed, and it describes the work.
8. **Slides, not screens** (owner decision, 2026-09-29). The report reads as a deck: one claim
   per 16:9 sheet, a headline that states it, one piece of evidence under it — usually a site
   photo — and generous space. It prints one sheet per page to PDF and keeps a viewing link.
   Reference: the hand-built R0 deck of 29 Sep.
9. **Sides and seats.** The client-side report never contains Crew-channel material. Money
   sections render only for `CanSeeMoney`; a reader without it gets no money section at all —
   absent, not refused. Support seats do not receive the report.

---

## 3. The page — visual first

Route: `/project/{ProjectId}/report` (live, "as at now") and
`/project/{ProjectId}/report/{ReportId}` (an issued snapshot). One component tree renders
both; the only difference is where the data comes from. Mobile first at 360 px, print to A4.

Order is the order Peter asks his questions in: *am I on time → where is each piece → what
did we agree → what is stuck → what's next → where is my money → show me.*

```
┌──────────────────────────────────────────────────────────────────┐
│ ①  COVER            hero photo 21/9, project name (display serif) │
│                     "Works report · as at 28 Sep 2026"            │
│                     window: since last report 14 Sep · mediator   │
├──────────────────────────────────────────────────────────────────┤
│ ②  HEADLINE CARDS   ┌────────┐┌────────┐┌────────┐┌────────┐      │
│                     │ nn%    ││ 30 Sep ││ UGX    ││ 0 owed │      │
│                     │ built  ││ at risk││ 100M in││ 5 stuck│      │
│                     └────────┘└────────┘└────────┘└────────┘      │
├──────────────────────────────────────────────────────────────────┤
│ ③  DEADLINE STRIP   Aug ───●────────◆────────────▲──■── Oct       │
│    (blueprint)          set 13 Aug  "on track"  today promised   │
│                          ×  ×  ×  lapsed short promises           │
│                          ○ contractor's date   ◌ pace forecast    │
├──────────────────────────────────────────────────────────────────┤
│ ④  STAGE BOARD      grouped by phase (stage accent hairline)      │
│    ┌─────────────────────┐ ┌─────────────────────┐                │
│    │▌Guest wing plaster  │ │▌Windows — main house│                │
│    │ ◔ 90%  STALLED 26d  │ │ ◕ 4 openings left   │                │
│    │ ╱╲__________ spark   │ │ planned — · fcst —  │                │
│    │ [before] [after]    │ │ [photo]  [photo]    │                │
│    │ Two lines of prose. │ │ Two lines of prose. │                │
│    │ ⓜ 2 Sep  ▣ 4 photos │ │ ⓜ 23 Sep ▣ 6 photos │                │
│    └─────────────────────┘ └─────────────────────┘                │
├──────────────────────────────────────────────────────────────────┤
│ ⑤  CHANGED SINCE LAST REPORT   same-vantage before/after pairs    │
├──────────────────────────────────────────────────────────────────┤
│ ⑥  DECISIONS & AGREEMENTS  cards: what · who agreed · when · proof│
│ ⑦  VARIATIONS              cards: scope · cost Δ · time Δ · status│
├──────────────────────────────────────────────────────────────────┤
│ ⑧  BLOCKERS          lanes by owner: windows team │ aluminium │ … │
│                      each: what · since · days open · last chase  │
├──────────────────────────────────────────────────────────────────┤
│ ⑨  AHEAD             next 14/28 days: due deliverables, date      │
│                      commitments, decisions Peter owes (by when,  │
│                      consequence), next phase (tiling, doors…)    │
├──────────────────────────────────────────────────────────────────┤
│ ⑩  MONEY             per stage: funded → received → released →   │
│                      carried forward (stacked hairline bars)      │
├──────────────────────────────────────────────────────────────────┤
│ ⑪  FOOTAGE           gallery by stage; video posters with length; │
│                      annotations drawn as layers                  │
│ ⑫  SOURCES           every chip resolves here, printable          │
└──────────────────────────────────────────────────────────────────┘
```

### 3.1 The visual vocabulary

Every diagram is **inline SVG driven by tokens** — no chart library. It prints, it works in
the MAUI BlazorWebView offline, and it inherits dark mode. Follow the dataviz skill when
building them.

| Element | Encodes | Rules |
|---|---|---|
| **Headline card** | One number + one word of state + one line | Tabular numerals. State word coloured by `--al-success / warning / danger`; the number stays `--al-text`. |
| **Deadline strip** | The project completion commitment and its history | The only use of `--al-blueprint`. Markers: ● set, ◆ restated, ▲ today, ■ promised, × lapsed short-horizon promise, ○ contractor's revised date, ◌ pace forecast. One axis, no rows. |
| **Progress ring** | Stage completion % | Stroke in `--al-stage-accent` at the stage's phase; track in `--al-border`. |
| **Sparkline** | Progress readings over time | 64×16, no axes. A flat tail longer than the stall threshold gets a `--al-warning` segment and the word **Stalled**. |
| **Plan/forecast tick** | Planned end vs forecast end on a stage card | Two dots on a hairline; distance in days as text. |
| **Hold lanes** | Wait vs work, and what a hold pushes | See §4.4. Hatched `--al-warning` wait segment, solid `--al-stage-accent` work segment, dashed ghost at the original position, a `+n d` push chip. Rows grouped under their stage — never one project-wide network. |
| **Money bar** | funded → received → released → carried | Stacked hairline segments using existing `.al-meter__seg--*`. |
| **Before/after pair** | Change at one vantage point | Two 3/2 frames side by side with dates; the one visual the 17-photo dump proved is missing. |
| **Source chip** | Provenance | Icon by kind (message, photo, capture, release, document) + date. Tap opens the original. |
| **Gap marker** | Missing truth | Dashed border, muted text: *"not costed"*, *"no approval on record"*. A gap is a finding, not an empty state. |

Palette discipline per [CLAUDE.md](CLAUDE.md) §2: paper stays the hero; stage accents are
hairlines and dots, never fills; `.al-shimmer` while media loads; thumbnails 3/2, cover 21/9,
no squares.

### 3.2 Small descriptions — the only prose on the page

- **Stage card:** at most two sentences, about 35 words. What changed in the window, what is
  outstanding.
- **Decision / variation / blocker card:** one sentence.
- **Cover:** a three-sentence status paragraph, followed by the direct answer to the question
  Peter actually asks — *on time or not, and by how much*.
- **Footage:** a caption per frame when none exists (optional, §6.3).

Everything else is labels, numbers, dates and pictures.

### 3.3 Print and share

Peter reads abroad, on a phone, and the conversation stays in WhatsApp (D3). So:

- **v1 — print stylesheet**: A4 portrait, one section per page break, footage at
  artifact-original resolution, source chips rendered as short references with a sources
  appendix. Browser "Save as PDF" produces the file Nalan or Peter forwards into the group.
- **v2 — server PDF**: headless Chromium (the same Playwright build already used for
  verification) renders the issued snapshot to a PDF artifact, stored once and addressed
  permanently. Only if v1 proves too fiddly on phones.
- **Share link** to the issued snapshot for anyone on the project with the right standing.

---

## 4. Data — what exists, what is added

### 4.1 Already in the app (usable today)

| Source | Feeds |
|---|---|
| `tbl_Project_RS.ExpectedCompletionDate`, `RevisedCompletionDate` | Deadline strip (promised, revised) until date commitments land |
| `tbl_Stage` — `StartDate`, `ExpectedEndDate`, `ActualEndDate`, `CompletionPercentage`, `Status`, `Phase`, `ParentStageId` | Stage board, plan ticks, phase grouping and accents |
| `tbl_ProgressUpdate` + `tbl_ProgressImage` (`Channel`, `ExposedAt`) | Captured footage and readings, side-filtered |
| `tbl_FundingEntry` — declared, received, settled, evidence | Money section, headline money card |
| `tbl_Flag` — severity, due date, assignee, resolved | Blockers until P4 folds flags into commitments |
| `tbl_Artifact` + `tbl_ArtifactRef` — hash, `CapturedAt`, thumbnails | Footage, dedupe, source chips |
| `tbl_IngestedMessage` — raw, immutable | Extraction input; source chips for messages |
| `IProjectAccessService.ResolveAsync` → `ProjectAccess` | Who gets which sections |

### 4.2 Added by the report track

| Table / change | Purpose | Lands with |
|---|---|---|
| `tbl_WorksReport { ProjectId, AsAt, WindowFrom, Audience (ProjectSide), IssuedById?, IssuedAt?, IssueKind (Manual\|Scheduled), SnapshotJson, NarrativeJson, ContentSha256, PreviousReportId? }` | The frozen, citable report | R1 |
| `tbl_ProgressReading { StageId, Percent, ObservedAt, SourceKind (Stage\|Capture\|Ingested\|Manual), SourceId }` | Progress **over time** — sparklines, stalls, pace. Written whenever a stage %, a capture % or an extracted *"about 80%"* is recorded. Append-only. | R1 (stage/capture), R4 (ingested) |
| `tbl_Stage.BaselineStartDate`, `BaselineEndDate` | The plan as first agreed, so re-planning is visible rather than silent. Set once, when a stage is first given dates. | R2 |
| `tbl_Commitment` with `Kind = Date`, `SupersedesId` | The completion date and every short-horizon promise, with its restatement chain — what the strip draws | R2 (P4) |
| `tbl_Deliverable { StageId, Title, Status, DueDate }` | What is scheduled inside a stage | R2 (P4) |
| `tbl_Variation` | Scope, cost Δ, time Δ, approval | R3 (P4) |
| Blocker party — `AgreedWithPartyName` / `OwnerPartyName` on commitment or flag | Blockers grouped by who owns them | R3 (P4) |
| `tbl_ArtifactPoster { ArtifactId, PosterArtifactId, DurationSeconds }` | Video posters for the gallery and print | R1 |

### 4.3 The derived numbers — computed, explained on the page

| Number | Rule | Shown as |
|---|---|---|
| **Overall progress** | Budget-weighted mean of leaf-stage completion; stage count weighting when budgets are missing — and the page says which. | "nn% built · weighted by budget" |
| **Stall** | Stage `InProgress`, latest reading unchanged for ≥ 10 days (configurable) while the thread still says it continues | "Stalled 26 days at 90%" |
| **Pace forecast** | Superseded by the wait + work forecast in §4.4 wherever an activity has a work duration. Pace (readings over the last 21 days) remains only as the fallback for long, steady trades with no stated duration — plaster, blockwork — and is labelled as such. | ◌ on the strip, with *"from waits and work"* or *"from pace since 7 Sep"* |
| **Contractor's date** | The latest `Date` commitment from the delivery side | ○ on the strip, beside ◌ — two answers to "how long", side by side |
| **Lapsed promise** | A `Date` commitment whose due date passed with its deliverable not done | × on the strip; counted on the headline card |
| **Days open** | Today − blocker raised | Blocker lanes |

A forecast is never presented as a promise. When pace and the contractor's date disagree,
both are shown and neither wins — the gap between them is the finding.

### 4.4 Holds — the wait before the work (added 2026-09-29)

**The pattern.** Nalan, from site: an activity can sit for weeks while things get in place —
material, a third-party team, money, a decision, the machine, the weather, the screed drying —
and once the thing holding it is dealt with, it completes in a day or two. Epoxy waits 20-odd
days for the screed and then goes down in three. A pace forecast reads that as "zero progress,
no forecast" and then, after one busy day, as "done tomorrow". Both are wrong. The honest model
has two parts, and the report draws both.

**The objects.**

| Object | Holds | Notes |
|---|---|---|
| **Activity** — a deliverable with `WorkDays` (the burst) and an optional `EarliestStart` | How long the doing takes once it starts | A stated duration ("2 days to level") is work, not wait. |
| **Hold** — `tbl_Hold { ProjectId, StageId?, Title, Cause, OwnerMemberId?, OwnerPartyName?, ExpectedDays, StartsAfter (date or another hold/activity), Status (Open\|Cleared), ClearedAt?, Source }` | The wait: its cause, who owns it, how long it is expected to last | `Cause` ∈ Curing / Material / ThirdPartyTeam / Money / ClientDecision / Machinery / Weather / Power / Design / Other. A P4 blocker flag can be promoted to a hold. |
| **Push** — `tbl_HoldPush { HoldId, TargetType (Deliverable\|Hold), TargetId }` | Which activities this hold pushes | **Chosen by a person.** Nothing is inferred and nothing propagates beyond what was chosen — one level, like sub-projects. A hold may push another hold (screed drying → epoxy team booking), which is as deep as a chain goes. |
| **Project calendar** — rest days (the Sabbath), public holidays, a rainy-season flag per month | Which days count | Curing and drying run on calendar days; work runs on working days. |

**The forecast.** For each activity: *start = the later of its earliest start and the clear date
of every hold that pushes it; finish = start + WorkDays working days.* An open hold's clear date
is `today + max(0, ExpectedDays − days already held)`, and once a hold outlives its expected
days it is extended by the cause's measured overrun rather than frozen at today — a hold that is
already late is the likeliest to stay late. Each cause (and each owner party) carries a
**measured lag** from this project's own history: expected vs actual days on cleared holds, and
promised vs delivered dates extracted by P5. That gives two dates per activity and for the
project: **P50** (as likely early as late) and **P80** (four times in five). Weather-exposed
activities in a rainy month take the rainy-month factor. The contractor's own date sits beside
both, as before.

**The picture — the part that must be beautiful.** One component, `HoldLanes.razor`, used on the
stage page, on the works report (§3 ④ and ⑨) and on the projection view:

```
 Epoxy · main house              today▼                         promised■
 ┊ screed drying (curing, 20 d) ░░░░░░░░░░░░░░░░░░░░┐
 ┊                                                  └▶███ apply 3 d          +20 d
 Boundary wall
 ┊ septic slab 3 d ███─┐
 ┊ levelling 2 d       └▶██─┐
 ┊ wall 7 d                 └▶███████                                         +5 d
 ┊ ╌╌╌╌╌╌╌ (ghost: where the wall sat before the holds) ╌╌╌╌╌╌╌
```

- **Wait** is a hatched `--al-warning` segment labelled with cause and owner
  (*"Aluminium team · fabrication"*). **Work** is a solid segment in the stage accent, labelled
  with its days. The eye should read at once that two days of work are being pushed by three
  days of preparation.
- A **ghost** outline stays where the activity sat before the hold; a `+n d` chip states the push.
- Adding, extending or clearing a hold **animates the push**: the pushed segments slide
  (`--al-transition-slow`, `--al-ease`) and the ghost fades in behind them. Clearing a hold
  snaps the work segment left to today.
- The P50 / P80 range is a soft band behind the finish, not a second bar.
- **Wait vs work** headline: a single stacked hairline per stage and for the project — *"Held
  41 days · worked 12"* — the number that explains a late project without blaming anyone.
- Rows are grouped under their stage, with connectors only between a hold and what it pushes.
  There is no project-wide canvas of arrows; Peter still reads stage by stage.
- At 360 px each activity is a card: the wait/work strip across its width, dates beneath, the
  hold's cause and owner as a chip. Inline SVG, tokens only, prints to A4.

**Adding a hold** is one sheet from the stage page, the register's blocker list, or a gap on the
lanes themselves: cause, owner, expected days (pre-filled from the cause's measured lag), and
tick-boxes for the activities it pushes. Extraction (P5) proposes holds from the thread —
*"we are waiting for…"*, *"once the … arrive"*, *"the team didn't show"* — into the same review
queue; accepted, they appear on the lanes.

**Calibration — measured on the real thread (2026-09-29).** 44 dated promise → delivery pairs and
14 closed open-ended waits, 18 Sep 2025 – 28 Sep 2026 (analysis kept outside the repo). The
pattern is real but **only for gated work**; own-crew wet trades behave differently. The
forecaster's defaults, until the project's own cleared holds replace them:

| Rule | Default |
|---|---|
| **Forecast the gate, not the announced date.** Start = later of predecessor finish and every hold clearing. | Hold lag by cause/state: third party **not yet confirmed** ~3 wk (tail 7–16 wk) · third party **confirmed "tomorrow"** 0–2 d · client decision, open-ended ~4–6 wk · funding 2–5 d (tail ~2 wk), work starts ~3–4 d after money lands · machinery ~1 d · authority ~4 wk · curing/drying = stated calendar days, +30 % chance of 3–7 d in humid months |
| **Trust depends on the horizon.** | A "today/tomorrow" from the delivery side: P50 +0, P80 +2 d (+1–2 d if it lands on the rest day). A date a week or more out, or project-level: little signal — forecast from the holds, never from the date. An *"on track"* reassurance is **zero evidence**. |
| **After start, the work model depends on the trade.** | Third-party installs, casts, fix-ups: stated duration ×1.0–1.5. Own-crew wet trades (formwork, plaster, screed, paint, stonework, fabrication): stated remaining ×2 at P50, ×3 at P80. *"90 % done"* = 1–2 more weeks. |
| **Stalls re-draw; a team is a queue.** | A hold with no activity for > 7 d re-draws its lag from the tail rather than counting down; *"following up"* is not progress. Items owned by the **same third party** (e.g. one aluminium team's guest-wing windows, missing units and doors) run in series, not in parallel. |
| **Calendar and absences.** | 6-day week with the rest day, public holidays, a holiday break; rain loss on outdoor work from climate data (the record is mostly dry-season); 1–2 wk gaps for contractor-absence events. |

The lanes must make the two families visibly different: a **gated** activity is a long hatched
wait then a short solid burst; an **own-crew trade** is a short or no wait then a long solid run
with its ×2 overrun drawn as a lighter extension. Sample-size caveats (few long-horizon pairs,
seven duration overruns) are stated on the page wherever a default, not the project's own
history, sets a date.

### 4.5 The work plan and knocking items off (owner request, 2026-09-29)

The 29 Sep hand-built plan (26 activities across main house, guest wing and external works,
handover Fri 4 Dec) is the reference for what the app must produce by itself. It reads as a
**work plan in the project's own voice** — "completion was set for 30 September", never
"the client asked / the contractor said" — giving the client clarity and the site a list to
work down.

- **One project-wide activity list.** Each `tbl_Deliverable` gains `PlannedStart`,
  `PlannedEnd`, `WorkDays`, `Trade` (a role, never a person), an optional off-site `MakeDays`,
  and predecessors (one level, per §4.4). Dates are **computed**, not typed: duration + holds +
  predecessors on the project calendar. The critical path falls out of the computation and is
  marked, not asserted.
- **Knock-off.** Ticking a deliverable Done takes **one photo** (an artifact ref) and stamps the
  date; no photo, no tick. The existing `DeliverableChecklist` gains the tick-with-photo flow,
  and a project-wide checklist view groups by area with trade, waiting-on, start, finish and
  status.
- **Friday re-issue.** The works report re-dates the plan from what was ticked and shows any
  movement of the handover date first. The reserve (working days between works complete and
  handover) is its own number, with the risks that could use it and how much each can absorb.
- **Actions that hold the date** are the holds' clear-by dates, labelled by kind (sign-off,
  order, booking, delivery, funding), not by party.

### 4.6 The scheduler, and how the contractor drives it (built 2026-09-30 — see plan.md *Scheduler*)

Tick-off (§4.5) gives each activity typed dates. The scheduler makes them **computed**, the
way the 29–30 Sep plan was built by hand: every date follows from durations, waits and order,
and every change shows its effect on handover before it is saved.

**Inputs, per activity** — working days on site; optional off-site making days; what it
**waits on**, one of: another activity finishing (finish → start), **something arriving by a
date** (order, delivery, sign-off, purchase, booking, funding — these become the "what must
happen, and by when" list), or **drying / curing for N calendar days**; an optional **team**
(activities sharing a team run one after another, in an order the contractor sets); and an
optional **pin** — a known finish ("doors run to 21 Oct") or an actual start/finish from a
tick. **Per project** — the site calendar (rest day, public holidays; drying counts every
day) and the committed handover date (a `Date` commitment, so its restatements are kept).

**Outputs** — start and finish for every activity; the **critical path** ("sets the date");
works-complete date; **reserve** in working days against the committed handover; for every
activity **how many days it can slip** before handover moves, in plain words. A pinned date or
a tick re-dates everything downstream; a wait that outlives its date extends by the measured lag
for its cause (§4.4 calibration), never silently frozen.

**The acceptance test** — fed the 30 Sep inputs, the engine reproduces the issued plan exactly:
works complete Wed 2 Dec, one working day of reserve, the aluminium sequence as the critical
path. The hand-built scheduler (`plan3.mjs`, outside the repo) is the reference implementation.

**How it reads to the contractor** (mobile first; this is the Plan tab's edit mode):

- **Each activity is one sentence**, edited as chips: *Gypsum ceiling · Ceiling crew · 14 days
  · after boards on site 7 Oct*. Tap a chip to change it; no forms, no Gantt editing.
- **"Waits on" is a picker of plain choices**: *another activity*, *something arriving by a
  date*, *drying / curing*. A team chip queues activities automatically; drag to reorder the
  team's queue.
- **Every edit previews its consequence before saving**: *"Moves works complete from Wed 2 Dec
  to Fri 4 Dec. Reserve 1 → 0 days."* Save or discard. The same preview compares two sequences
  side by side — the S1/S2 table of 30 Sep, on screen (*"railing after the main-house doors:
  complete 2 Dec · with the guest-wing doors: complete 6 Dec"*).
- **"This week"** — starts, finishes and anything past its planned finish, each with *Tick
  (photo)* or *Needs N more days*. Answering re-dates the plan; ignoring it leaves the item
  flagged late, never quietly on time.
- **Critical items say so** ("sets the date"), everything else says what it can absorb
  ("can slip 6 days"). The reserve is one number at the top, next to the handover date.
- Desktop adds the timeline lanes (hold / making / on site, critical outlined, reserve band,
  handover line) beside the list — the slide-5 view of the issued plan, live.

**The client side** sees the same plan read-only: handover, reserve, what sets the date, what
must happen by when. Handover movement is shown first (truth floor). **Law 0**: with nobody
editing, the plan still re-dates from ticks and elapsed time, and waits are proposed from the
thread by extraction; the Friday Works Report issues regardless.

**Law 0.** Holds and their pushes can be entered by either side and are proposed from the
thread, so the projection still moves when the contractor is silent. Clearing a hold is
attributable and dated, and a cleared hold is never deleted — the waits are the record of why
the project took as long as it did.

---

## 5. Where the footage comes from

The report is only as visual as the record, so three intake fixes come first.

1. **Re-join loose media to the transcript (P3 follow-up).** Android exports usually strip
   media (`<Media omitted>`), and the photos arrive separately — as they did this week —
   named `WhatsApp Image 2026-09-28 at 7.58.25 PM.jpeg`. That stamp matches the message
   line to the minute. Import accepts a folder or zip of loose media alongside an already
   imported transcript and binds each file to the `<Media omitted>` line from the same
   author-minute, ordered by seconds against the existing occurrence ordinal. Ignore the
   human's folder names — a dozen of this week's files sat in the wrong day's folder, and
   the stamp is right where the folder is wrong. Files that bind to nothing are kept as
   unattached artifacts, reported, never dropped.
2. **Human filenames are captions.** `intended stoppage line.jpeg`, `Dining alterations.jpeg`,
   `view of the guest wing overhang.jpeg` — a filename that is not a WhatsApp stamp becomes
   the artifact's caption.
3. **Video posters.** A Hangfire job extracts a poster frame and duration (ffmpeg) into
   `tbl_ArtifactPoster`. Without it, 20 of 91 files render as blank tiles.

**Choosing frames for a stage card.** v1: the newest client-visible frame plus the earliest
frame in the window — honest, crude. v2: a *vantage* tag (a one-tap "same spot as…" on
capture, or set by the mediator) so the pair is a true before/after. v3: image-similarity
pairing. Never more than two frames on a card; the rest live in ⑪.

---

## 6. Drafting — how the report writes its own descriptions

The drafting ability used to write this week's analysis becomes a service, with guard rails
that make it safe to put in front of the person paying.

### 6.1 The pipeline

```
records ──► ReportAssembler ──► WorksReportFacts (deterministic, every figure final)
                                       │
                                       ├──► IReportNarrator (Claude) ──► NarrativeDraft
                                       │         prompt = facts + source snippets
                                       │
                                       └──► NarrativeValidator ──► accepted sentences
                                                 │                  (or templated fallback)
                                                 ▼
                                        WorksReportDto ──► page / snapshot / PDF
```

- **`IWorksReportService.AssembleAsync(projectId, asAt, windowFrom, reader)`** — pure data.
  Access resolved once through `IProjectAccessService`; side and seat filtering happen here,
  before anything is sent to a model.
- **`IReportNarrator`** — calls the Claude API (`claude-sonnet-5-5` for card descriptions;
  `claude-opus-5-5` for the cover paragraph). Input is the facts plus the source snippets
  each fact came from: message bodies, captions, capture descriptions. Output is structured
  JSON: `{ targetId, sentences: [{ text, sourceIds[] }] }`.
- **`NarrativeValidator`** rejects any sentence that
  - cites no source, or cites a source outside the facts it was given;
  - contains a number, date or percentage not present in the facts;
  - forecasts, promises or assigns responsibility the facts do not record;
  - exceeds the word budget.
  Rejected sentences fall back to a **templated description** built from the facts
  (*"90% since 2 Sep. Last reported 22 Sep: work continuing on the front stonework."*).
  **The report always generates, with or without the model** — a report that fails because an
  API is down breaks Law 0.

### 6.2 Voice

Plain, specific, no reassurance. *"Terrazzo strips and screed are down on the terrace;
grinding has started at the water tank."* — not *"Great progress on the terrace this week!"*
The model describes; it does not sell. This is written into the system prompt and checked
by the validator's word list.

### 6.3 Optional: captions from the photos

Where a frame has no caption, the narrator may be given the image to describe what is
visible (*"sliding aluminium door fixed, protective film on, screed at threshold"*). Off by
default; enabled per project, because it sends site imagery to an external service. The
setting states that plainly.

### 6.4 Consent and data

Nothing is sent to a model unless the tenant has enabled drafting. Facts are filtered to the
reader's side **before** the call, so crew-channel material never leaves the deployment in a
client-side report. Snippets are sent, not the whole thread. Responses are stored in
`NarrativeJson` so an issued report never changes when a model does.

---

## 7. Issuing

| Trigger | Who | Notes |
|---|---|---|
| **Live view** | Anyone with `CanSeeBrief` | Always "as at now", never stored |
| **Issue now** | Mediator, or a client principal | Freezes a snapshot; one tap; optional covering note |
| **Scheduled** | System | Weekly (default Sunday evening, project's timezone). Issues whether or not anyone logs in — Law 0 |
| **Milestone** | System | When a stage completes or a completion date commitment lapses |

On issue: snapshot written, content hashed, previous report linked, and a notification
carries the link (and, when v2 lands, the PDF) to the audience. The **since last report**
window defaults to the previous issued report's `AsAt`.

---

## 8. Components

Module `Modules/Report/` (new — the name follows CLAUDE.md §1). Every `.razor` gets its
sibling `.razor.css`. Promote to `Components/` only when a second module needs it — the
source chip and the before/after pair are likely candidates (Brief, Search).

| Component | Section |
|---|---|
| `Pages/WorksReport.razor` | Shell; live vs issued; print CSS; loading / empty / error states via `StateBlock` |
| `ReportCover.razor` | ① |
| `HeadlineCards.razor` | ② |
| `DeadlineStrip.razor` | ③ SVG |
| `StageBoard.razor`, `StageReportCard.razor`, `ProgressRing.razor`, `ProgressSparkline.razor`, `PlanTick.razor` | ④ |
| `BeforeAfterPair.razor` | ⑤ and inside stage cards |
| `DecisionCard.razor`, `VariationCard.razor` | ⑥ ⑦ |
| `BlockerLanes.razor` | ⑧ |
| `AheadList.razor` | ⑨ |
| `MoneyLadder.razor` | ⑩ — reuses the funding ledger's rollup |
| `FootageGallery.razor` | ⑪ — reuses `PhotoLightbox` |
| `SourceChip.razor`, `SourceAppendix.razor` | Everywhere / ⑫ |
| `GapMarker.razor` | Everywhere |
| `IssueReportDialog.razor`, `ReportHistory.razor` | Issuing, past reports |

Server: `WorksReportController`, `IWorksReportService` / `WorksReportDAL`,
`IReportNarrator` / `ClaudeReportNarrator` / `TemplateReportNarrator`,
`NarrativeValidator`, `ProgressReadingWriter`, `MediaRejoinService`, `VideoPosterJob`
(Hangfire), `ScheduledReportJob` (Hangfire). Refit client `IWorksReportApi`.

Entry points (one component, several doors — CLAUDE.md §3): a **Report** tab on the project,
a card on Peter's multi-project home (P7), and the notification link.

---

## 9. Delivery — slices, each usable on its own

| Slice | Delivers | Depends on | Exit |
|---|---|---|---|
| **R0 — Hand build** | The 28 Sep report built by hand from the 5 Aug – 28 Sep window and this week's footage, as a static page in this layout, anonymised. | Nothing | Peter reads it and answers: *does this answer "more time and how long"?* Doubles as the P4 hand test (assetlen.md §11 test 1). **Build before any schema.** |
| **R1 — As built** | Live + issued report from data that exists: cover, headline cards, stage board with rings and sparklines, footage, money, flags as blockers, deadline strip from project dates. Media re-join, filename captions, video posters, `tbl_ProgressReading`, `tbl_WorksReport`, print CSS. Templated descriptions only. | P3 | A report issues from a synthetic Aug–Sep fixture with the contractor account never logging in, and a re-issue a week later shows the deltas. |
| **R2 — Scheduled** | Baselines, date commitments and their restatement chain, deliverables with due dates, pace forecast, lapsed promises, ⑨ Ahead. | P4 Commitment + Deliverable | The strip shows set → restated → lapsed → contractor's date vs pace forecast for the 30 Sep deadline. |
| **R3 — Agreed & stuck** | Decisions, variations (with *not costed* gaps), blockers by owner. | P4 Variation, party fields | Every scope change in the window appears as a variation or as a gap. |
| **R4 — Drafted** | Claude narrator + validator; extraction writes progress readings, date promises and blockers from ingested summaries. Optional photo captions. | P5 | Zero validator escapes on the fixture; with the API disabled the report still issues, templated. |
| **R5 — On a cadence** | Scheduled and milestone issuing, notifications, report history, server PDF if v1 print falls short. | R1 | A week passes with nobody logging in and a report was issued and delivered. |

R1 is shippable before P4 and is already more than Peter has today.

### Slice status (2026-09-29)

| Slice | Status | Notes |
|---|---|---|
| **R0** | **Not done — needs Peter** | The page exists; issue it from his real export as at 28 Sep, print it, and ask the question. The real export stays outside the repository. |
| **R1** | **Built** | Live + issued (frozen, hashed, byte-stable), as-at any past day, deltas since the last report, print stylesheet, video posters (ffmpeg via Hangfire; *length unknown* without it). Exit met on the synthetic fixture. Captured photos are still inline data URIs and do not reach the footage section (P9). |
| **R2** | **Built** | Strip from date commitments (set, restated, lapsed, contractor's date, pace forecast, the stages it cannot speak for), baselines, ⑨ Ahead. Exit met on the fixture. The completion date is recognised by wording. |
| **R3** | **Built** | Decisions, variations with *not costed* / *no approval on record* gaps, unvaried spec changes as gaps, blockers by owner aged from `tbl_Flag.RaisedAt`. Exit met on the fixture. |
| **R4** | **Built, model untested** | Validator, templated fallback, per-project consent, Claude narrator (Sonnet cards, Opus cover). Issues fully templated with the model off (asserted); no key on the build machine, so no live draft has been validated. Extraction's reading rule fixed for one-word subjects. |
| **R5** | **Built** | Weekly and milestone issuing on Hangfire, once each, delivered in the app, history. Exit met by a dev-only trigger standing in for the week; email/push delivery and the server PDF are not built. |

Full detail and outstanding items: [plan.md](plan.md), *Works Report — R1 to R5*.

---

## 10. Verification

`tools/e2e-report.sh`, added to `tools/e2e-all.sh`:

- An issued snapshot is byte-stable: re-reading returns the same content hash.
- A client-side report contains no Crew-channel item; a support seat gets 404.
- A reader without `CanSeeMoney` receives no money section in the payload, not an empty one.
- Every figure in the payload reconciles to its source table (funding totals, stage %).
- Every narrative sentence carries ≥ 1 source id that exists in the snapshot.
- With drafting disabled the report issues, fully templated.
- With only a client-side import and funding declarations (contractor silent), a report issues.
- Media re-join binds the fixture's loose files to `<Media omitted>` lines; misfiled folders
  do not change the binding; unbound files are reported.

**Fixture.** `tools/make-report-fixtures.sh` synthesises the Aug–Sep window's *shape* —
stages (guest wing plaster, main-house undercoat, windows, doors, screeding, terrazzo,
epoxy, bathroom tiling, septic tank, stonework, external works), the stalled 90%, the lapsed
"this week", six variations, five third-party blockers, one UGX 100M release — with
pseudonyms only. **The real export and footage never enter the repository**
([whatsapp-evidence.md](whatsapp-evidence.md) *Anonymity*).

Visual checks per CLAUDE.md §0: 360 / 768 / 1280, light and dark, print preview, and DevTools
confirming the scoped CSS bundle loads.

---

## 11. The worked example — what the 28 Sep report says

Drawn from the thread; this is R0's content and R1–R4's acceptance target.

**Cover answer.** *Not on course for 30 Sep. Main-house windows are in bar four openings,
screeding is done in the rooms, and terrazzo is under way on the terrace; doors started
today. Guest-wing plaster has read 90% since 2 Sep, guest-wing aluminium has not been
fabricated, and the septic tank is not yet closed. The contractor has not yet given a
revised date.*

| Stage | State | Evidence in window |
|---|---|---|
| Guest wing plaster | 90% · **stalled** since 2 Sep · "complete this week" lapsed | 24 Aug, 2 Sep, 4 Sep, 22 Sep |
| Main house undercoat | 70% on 24 Aug · no later reading (gap) | 21–24 Aug |
| Windows — main house | Fixed 22 Sep except 4 openings; 2 terrace units not in the shipment | 22–23 Sep |
| Windows — guest wing | Aluminium not fabricated · blocker, aluminium team | 23 Sep |
| Doors | Started 25/28 Sep; first-floor height cut, engineer cleared 25 Sep | 25, 28 Sep |
| Screeding | Main-house rooms done, lobbies left; guest wing started | 24 Sep |
| Terrazzo — terrace, laundry | In progress since 15 Sep | 15–23 Sep |
| Epoxy — main house | Sample approved, grey; floor prep under way | 8–12 Sep |
| Bathroom wall tiling | In progress since 14 Sep | 14–24 Sep |
| Septic tank | Waterproofed, plastered, floor screeded; top not cast | 26 Aug – 25 Sep |
| Stonework, external works | Continuing; site levelling started 7 Sep | 4–22 Sep |

**Decisions on record:** window design *"as in that design, no changes"* (3 Aug); mosquito
net sliding upward (5 Sep); epoxy grey (8 Sep) and settled (12 Sep); terrazzo on the terrace
(15 Sep), one colour, no black (16 Sep), laundry and all of that level (17 Sep); guest wing
maintained as planned (20 Sep); burglar bars inside, doors outside (25 Sep).

**Variations — all uncosted:** pantry → dining (no approval on record); terrazzo scope;
epoxy scope; parapet raised a course plus coping (queried by Peter 17 Sep, answered with a
marked-up photo, not closed); first-floor door openings cut; bathroom tiles chosen to match
epoxy.

**Blockers by owner:** window team — slow to respond (8 Sep); epoxy team — no-show (7 Sep);
aluminium team — guest-wing windows and 4 missing units (23 Sep); machinery — cast postponed
(17 Aug); power cuts — stonework (4 Sep).

**Money:** UGX 100M announced 13 Aug through a payment agent; receipt not confirmed in the
record (gap).

**Ahead:** Phase 4 as stated 18 Aug — tiling, door and window fitting, railing, the
perforated wall panel behind the master, then external works. **Decisions Peter owes:** none
recorded; the revised completion date is owed *to* him.
