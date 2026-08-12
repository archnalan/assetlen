# ASSETLEN — Implementation Plan

**Living document.** Rewritten **2026-08-12 (third revision)** against
[assetlen.md](assetlen.md) §0, after deciding that **Peter buys**.

---

## How to use this file (read first, every session)

1. **Read [assetlen.md](assetlen.md) first.** It is the product truth. [CLAUDE.md](CLAUDE.md) is the *engineering* charter — aesthetic, CSS, folder layout — and defers to it.
2. **Read this file end-to-end** before writing code.
3. **Two ship tests, both binding.** assetlen.md §8: *"Does it help someone hold a past commitment against present reality?"* And Law 0: *"Does it still work when the contractor is silent?"* A feature failing the second is **tier 3** and cannot be counted toward launch.
4. **Challenge the plan.** If a phase no longer fits, say so before implementing and propose the revision in the same turn.
5. **Never commit.** Per CLAUDE.md §0 the user commits manually. Leave the tree dirty.

---

## The reframe that reorders everything

The previous two versions of this plan were written for Nalan. They assumed the contractor
captures, the system drafts, the contractor curates, and Peter reads. Every step depended
on the person with the least incentive to pay, and Peter — the buyer — was last in the
chain.

The correction is not just "build for Peter." It is this:

> **Peter is not missing updates. He receives 1,055 of them. He cannot read them.**

The corpus is unambiguous: 1,529 messages, 723 of them media, arriving in chronological
batches of thirteen to eighteen. Peter chased seventeen times *on days when photos had
already been posted* ([evidence](whatsapp-evidence.md) F1, F2). On 25 February he received
seventeen photos and replied *"Nothing much changed."*

**The content already exists and already reaches him.** Assetlen's tier-1 job is not to
generate it. It is to **restructure what already arrives** into something readable,
searchable and reconcilable — with no contractor involvement whatsoever.

That single change reorders the phases. Ingest becomes the front door. Extraction moves
from P8 to the middle of the plan because it is now the only path from a forwarded pile to
a register. Capture and curation — the old P4 and P5 — drop to the end, where the
contractor tier belongs.

### Peter's standing, restated

- **He owns the account.** Projects belong to it (assetlen.md D1).
- **He hires and fires contractors at will.** A contractor is a participant in a project,
  never its owner. Removing one loses no commitment, no artifact and no history.
- **Accountability is per contractor, per project.** Every commitment carries the
  accountable mediator's name (assetlen.md §10.1), so *"what did this contractor commit to,
  and did they deliver it"* falls out of the commitment model rather than needing a feature.

---

## Audit — 2026-08-12 (historical)

Fifteen scenarios run against a live API with three real users. Findings A1 (project
membership granted nothing) and A2 (`TrimStart(char[])` corrupted every uploaded JPEG) were
the blockers; both were fixed in P0 and the audit script is
[tools/e2e-access-audit.sh](tools/e2e-access-audit.sh), 18/18 green. A7 (POS scaffold debt)
was cleared in P1.

**A5 and A6 stand, and the buyer decision makes them worse:**

- **A5 — Peter's product still does not exist.** No commitment, no register, no money
  ledger, no search, no ingest. Everything built to date serves the contractor's workflow.
- **A6 — features the vision cuts.** `TimelineChart` is a Gantt chart and is cut.
  `/portfolio` was demoted as a landing page — **that demotion is now reversed**, see below.

**A8 — new, and the largest.** *There is no way to get a year of existing project history
into Assetlen.* Peter's entire record lives in a WhatsApp thread and an email account. Under
Law 0 this is the front door, and no phase of the previous plan contained it.
**Closed in P3** — the mechanism exists and is tested. What is still unrun is the mechanism
against the *real* export, which is a validation task, not a build one.

---

## Status of prior phases

| Phase | Title | Build | Verdict under the buyer decision |
|---|---|---|---|
| 0 – 0.6 | Strip POS, rename, .NET 10 | Done | Fine — neutral |
| 1.1 – 1.2 | Domain, roles, `tbl_ProjectMember` | Done | Fine; roles collapse in P2 |
| 1.3 – 1.4 | Dashboard, ProjectCard, Breadcrumbs | Done | **Promoted** — this becomes Peter's home |
| 1.5 – 1.5.1 | Member-add flow, ProjectCreate | Done | Reworked in P2 for sides + mediator |
| 2.1 – 2.4 | Site Journal, channels, entry detail, curated view | Done | **Tier 3.** Correct, but not launch-critical |
| 3.1 | Timeline chart | Done | **Cut.** Retire `TimelineChart` |
| 3.2 | Finance | Done | Becomes the stage money ledger in P3 |
| 3.3 | Accessibility / nav | Done | Fine |
| **P0** | Unblock membership access | **Done** — 18/18 | Still correct and still necessary |
| **P1** | Scaffold strip + migration baseline | **Done** — 18/18 from empty | Fine |
| **P2** | Ownership, sides, artifact store | **Done** — 65/65 | — |
| **P3** | Ingest — the front door | **Done** — 51/51 | — |

P0 and P1 were both right and both invisible. The detail of what landed in each is
preserved in git history and in the audit script; it is not repeated here because it no
longer informs a decision.

---

## Phase plan

Ordered by **when Peter would pay**, not by dependency convenience. Every phase up to P8
must work with the contractor silent.

| Phase | Title | Tier | Status |
|---|---|---|---|
| P2 | Ownership, sides, and the artifact store | 1 | **Done** |
| P3 | Ingest — the front door | 1 | **Done** |
| P4 | Commitment model + the money ledger | 1 | Planned |
| P5 | Extraction — pile into register | 1 | Planned |
| P6 | Retrieval — Peter's four searches | 1 | Planned |
| P7 | Peter's surfaces — home and the daily brief | 1 | Planned |
| P8 | Markup, query state, parked ideas | 1–2 | Planned |
| P9 | The contractor tier | 3 | Planned |

---

### P2 — Ownership, sides, and the artifact store *(done)*

Establishes who owns a project, who is on which side of it, and where files live. Every
later phase writes through this.

**Ownership (assetlen.md D1, §10.2).**
- `tbl_Project.OwnerTenantId` — the developer's account owns the project.
- `tbl_TenantMembership { TenantId, UserId, Roles, IsDefault }` — one human, one login,
  many accounts. Nalan is a guest in several developers' accounts; `AppUser.TenantId`
  demotes to a *default*, not the truth.
- `TenantId` on project-scoped rows is **derived from the project**, not from the writer's
  org. One change in `UpdateTimestamps` ([AssetlenDbContext.cs:472](assetlen.Service/DataAccess/AssetlenDbContext.cs)) —
  the single place TenantId is stamped. Without this, a guest writing into the owner's
  project stamps their own tenant and the row vanishes behind the query filter.
- **P2.5 — done.** `GetMyAccounts` / `SwitchTenant` re-issue the token against another of
  the caller's accounts, membership verified server-side. Roles now come from
  `tbl_TenantMembership.Roles` when set and fall back to the global roles, so a person can
  be a developer in their own account and delivery-side in another. The claims DTO carries
  the **active** account, not `AppUser.TenantId`, which is only where they land at sign-in.
  `TenantSwitcher` sits in the Projects top bar and only renders past one account.

**Sides and the accountable face (assetlen.md §10.1).**
- `ProjectSide { Client, Contractor }` and `tbl_ProjectMember.{ Side, IsMediator, PartyName }`.
- `ProjectAccess { Level, Side, IsMediator }` — resolved once by `IProjectAccessService`.
  `CanSeeSiteLog = Side == Contractor || IsMediator`. `CanExposeToClient = IsMediator || Manage`.
- **Per-project, never tenant-global.** The old `_tenant.IsExternal()` read a JWT role
  claim, so one person had one standing everywhere. Replaced throughout `ProgressDAL` and
  `FlagDAL`; `AssetlenHub` still carries the old check and must follow.
- Mediator cap of two, enforced; the last one cannot be stood down.
- The mediator may add and remove **delivery-side** members only. The client side is
  Peter's alone.
- **Three lists, never merged:** the accountable face (one name, on everything Peter sees),
  true authorship (delivery side only), and the access roster (Peter, always — who holds a
  key, nothing more).
- **Roles collapse 6 → 4**: developer, representative, mediator, delivery. Do not add a fifth.

**Artifact store (Law 2).**
- `tbl_Artifact { ProjectId, Sha256, ByteSize, MimeType, StoragePath, ThumbnailPath, OriginalFileName, UploadedById, CapturedAt, Width, Height }`, unique on `(ProjectId, Sha256)`.
- `tbl_ArtifactRef { ArtifactId, ProjectId, TargetType, TargetId, Channel, Caption, DisplayOrder, ExposedById, ExposedAt }` — the pointer, and **the unit of exposure**.
- Content-addressed storage, sharded two hex deep, behind `IArtifactStorage`. Thumbnails
  behind `IThumbnailGenerator` (ImageSharp).
- `tbl_ProgressImage` becomes a pointer; its `Channel` is **enforced** — it existed before
  P2 and no query read it, so promoting an entry pushed all eighteen frames across.
- `tbl_Document` + `tbl_ArtifactRevision` — current revision pinned, superseded archived
  never deleted ([evidence](whatsapp-evidence.md) F4).

**Billing — per project, by size (assetlen.md §10.3).**
- `ProjectSizeTier { Small, Medium, Large }` and `ProjectSizingPolicy` — thresholds in one
  place, because changing a boundary changes what every project is billed.
- `tbl_Project.{ FloorAreaSqm, SizeTier, SizeSource, SizeTierConfirmedById/At }`.
- `IProjectSizingService` rolls sub-project areas into the billable parent. Upgrades are
  **proposed** (`PendingTier`) and require confirmation; downgrades apply at once.
- Area-from-drawings is deferred, but `ProjectSizeSource` already distinguishes a declared
  figure from a derived one so automation can never silently overwrite a person.

**Landed:** all of the above — schema, services, `ArtifactsController` (multipart upload +
streaming), the sizing endpoints, `SetImageChannel`, DI, the Refit clients, and migration
`20260812130526_P2_OwnershipSidesArtifacts`, **applied to the dev database**. The migration
backfills `OwnerTenantId` from each project's existing tenant, derives every member's
`Side` and `IsMediator` from their specialization, and seeds `tbl_TenantMemberships` from
`AppUser.TenantId` — without those, existing rows come up on the wrong side of the channel
boundary. Solution builds with 0 errors.

**UI — the two-sided model made visible.**
- `ProjectRoster.razor` replaces the Team tab. Two columns, the mediator seat above them,
  off-platform parties as first-class rows, side reassignment and appointment inline. The
  old surface was an email box and a raw enum `<select>`, which could not express a side at
  all — the most important fact about a person would have stayed invisible.
- `ProjectSizingPanel.razor` on Overview: area, band, roll-up breakdown, and a pending
  upgrade that states what it will cost before it is accepted.
- `EntryPhotoPanel` gains curation — select frames, expose or withdraw, with
  *"3 of 18 shown to the client"* always on screen for the mediator and the true
  denominator always on screen for the client.
- `GetMyStanding` returns the caller's per-project `ProjectAccess` so a page renders the
  right surface without re-deriving the rules. It is a mirror of the server's decision,
  never a substitute for it.

**Also closed:** the project creator is now seated as mediator #1 at create (a project
without one leaves the client side permanently dark); `OwnerTenantId` is written on create
and inherited by sub-projects, without which every child row fell back to the writer's
tenant and the P2 ownership model did nothing; `AssetlenHub` resolves the side per project
instead of from the tenant-global role claim, and now also gates `JoinProject`; and a
**mediator may staff their own side** — required by D5 and previously impossible, which
would have left Peter hiring the subcontractors himself.

**Found by the suite, and fixed:** `ProjectSizingService.GetAsync` never resolved to the
billable parent, so reading a guest wing reported the wing billing as its own project; and
`OwnAreaSqm` returned the parent's area when viewing a sub-project, which would have
invited an editor to overwrite the house's figure with the wing's.

**Exit — met.** `tools/e2e-p2-peter.sh` drives the live API as Peter, Nalan, a foreman and
an unrelated principal: **48 assertions, 0 failures.** The same file uploaded twice yields
one artifact; a client-side reader receives only exposed frames while the entry reports its
true total; a crew entry answers 404 rather than 403, since a refusal would confirm the
Site Log exists; the tier never rises without a person accepting it.

**Documents (F4).** `DocumentRegister` is the drawing register: reissue supersedes rather
than replaces, the superseded issue stays downloadable, and `SetDocumentChannel` lets the
mediator issue a drawing to the client or withdraw it. Downloads go through
`IArtifactDownloadService` — artifact bytes sit behind an authenticated endpoint, so an
`href` or `src` fetches them without a bearer token and against the wrong origin.

**Found while building the register:** `IsVisibleAsync` consulted only `tbl_ArtifactRefs`,
so a document released to the client was *listed* and its bytes still 404'd. Visibility now
follows the document's own channel.

**CSS debt cleared.** All 16 `.razor` files have siblings, and the four inline `<style>`
blocks are gone. Three auth forms (forgot-password, phone-reset, reset-password) had been
rendering **unstyled** since the login refactor: their class vocabulary lived in
`LoginComponent.razor.css`, which CSS isolation never applied to a child component, and the
rewrite took the orphaned rules with it.

**Exit — met.** `tools/e2e-p2-peter.sh`: **65 assertions, 0 failures.**

**Outstanding:** `tools/e2e-access-audit.sh` still casts the contractor as project owner and
is superseded by the suite above; artifact *images* still render through
`ProgressImageDto.ImageUrl`, which has the same authenticated-endpoint problem the download
service solves for files.

---

### P3 — Ingest: the front door *(done — A8, assetlen.md D3)*

**Nothing else matters if Peter cannot get his year of history in.** This phase is the
whole tier-1 thesis and it did not exist in the previous plan.

- **WhatsApp export import.** Accepts the `.txt` transcript or the `.zip` with its media,
  detected by content rather than extension. Every media file becomes an artifact,
  hash-deduplicated — the same receipt sent five times collapses to one with five refs,
  which is Law 2 proving itself on real data before a single new photo is taken.
- **Share-sheet target** and **email-in** per project, for the ongoing trickle.
- `tbl_IngestedMessage` — raw and immutable. Extraction reads it; nothing else writes it.
- **Author mapping.** An export names people who may have no login. Each participant maps
  to a `tbl_ProjectMember`, including off-platform ones created via `PartyName`, so
  attribution survives import.
- Re-importing an overlapping export must not duplicate.

**Landed:** `tbl_IngestBatch` + `tbl_IngestedMessage`, `WhatsAppExportParser`,
`IngestArchive`, `IngestDAL`, `IngestController`, the Refit client, DI, the
`ProjectImportPanel` / `IngestedThread` UI on a new **History** tab, and migration
`20260812210209_P3_Ingest`, **applied to the dev database**. Purely additive — nothing
before P3 wrote ingested material, so unlike P2 there was nothing to backfill.

**Import is two calls, not one.** `UploadArchive` stores and reports; `CommitImport`
writes. The step between exists because attribution is the only part of an import that is
expensive to undo — filing 1,055 messages against the wrong person is worse than not
importing them, so who each name belongs to stays a decision somebody makes.

**Four things that would each have silently corrupted a year of history:**

- **The dedupe key needs an occurrence ordinal.** Android stamps to the minute and the
  corpus's normal pattern is thirteen to eighteen photos inside one — same author, same
  timestamp, all bodied `<Media omitted>`. The key named in the old plan text,
  `(ProjectId, SentAt, ExternalAuthor, hash(Body))`, hashes those eighteen identically and
  keeps **one**. The loss is invisible: the import reports success. The key is now
  `(SentAt, Author, Body, MediaFileName, occurrence)`, and a re-import still adds nothing
  because the same transcript always yields the same ordinals.
- **Day/month order must be proven, not assumed.** `03/12/2025` is 3 December or 12 March
  depending on the phone, and WhatsApp records no locale. Resolved across the whole file
  from the first date whose component exceeds 12, and **reported** — a wrong guess moves
  most of a year by months with no error anywhere.
- **Invisible characters.** WhatsApp wraps stamps in direction marks (U+200E/200F) and
  newer iOS separates the time from AM/PM with a narrow no-break space (U+202F). None
  render; all defeat `\s` or a leading `^\[`. The symptom is a file that parses to zero
  messages while looking perfect in an editor. The iOS fixture carries both deliberately.
- **`UpdateTimestamps` resolves each new row's owning tenant** and falls back to a database
  query per row when the project is not in the change tracker — 1,529 extra round trips.
  The project is loaded tracked and `TenantId` is stamped explicitly.

**Who may read an import.** Everything ingested is Site Log material (assetlen.md §5), so
the contractor side and mediators read it. Beyond that **the importing side owns it**:
`tbl_IngestBatch.ImportedSide` is captured at import time and stored, not re-derived, so
material does not become readable — or stop being readable — because of a later roster
edit. `CanManage` alone grants nothing; ownership answers *who holds a key*, not *who did
what* (§10.1), and Peter has no business reading the crew's operational chatter (D5).

**Exit — met.** `tools/e2e-p3-ingest.sh`: **51 assertions, 0 failures**, and
`tools/e2e-all.sh` runs the whole chain at **116 assertions, 0 failures**. A 1,529-message
export imports whole; re-uploading it reports 1,529 already present and adds none; the
eighteen-photo minute survives as eighteen; an iOS archive yields 13 artifacts from 17
attachments with the receipt sent five times stored once; a delivery-side import answers
404 to the client side rather than 403.

**The corpus is not in this repository and must not be added** — `whatsapp-evidence.md`
forbids it, and the export carries real names, banks, account numbers and a location. The
exit criterion therefore runs against `tools/make-ingest-fixtures.sh`, which synthesises an
export with the same *shape* as the documented profile (§2: 1,529 messages, 47% media,
three participants, an 18-photo minute). The generator counts what it writes and the parser
reads it back independently, so agreement is evidence rather than a shared assumption.
`tools/fixtures/` is gitignored, primarily as a guard for the day the real export is
dropped there. **Running it against the genuine export remains outstanding** and is the one
part of this exit criterion that a fixture cannot stand in for.

**Outstanding:** media is stored one file at a time through `ArtifactDAL`, which re-checks
access per call — correct, and the right choke point for Law 2, but a 723-file import is
slow enough to want the Hangfire queue P5 already introduces.

**`appsettings.json` is gitignored**, so the `Ingest` block added for this phase does not
survive a clone. Unset, `InboundEmail` answers 503 rather than standing open, and the suite
detects that and **skips** the four inbound-mail assertions rather than reporting a failure
the reader would learn to ignore. On a new machine, add:

```jsonc
"Ingest": { "InboundDomain": "in.assetlen.app", "InboundSecret": "<per-environment>" }
```

or run with `INGEST_SECRET=… bash tools/e2e-p3-ingest.sh`. It must be a real secret before
any deployment that can receive mail — anyone holding it can post into any project whose
address they know.

---

### P4 — Commitment model + the money ledger *(assetlen.md §3, §6)*

Peter's second-worst pain and the one that cost him a 9 AM meeting
([evidence](whatsapp-evidence.md) F3).

- `tbl_Deliverable { StageId, Title, DisplayOrder, Status }` — 5–8 per funded stage.
- `tbl_Commitment { ProjectId, DeliverableId?, Kind (Spec|Price|Date|Material|Choice), Title, Body, Maturity, QueryState, SourceChannel (App|Ingested|Verbal|Meeting), AccountableMemberId, AgreedById, AgreedWithPartyName?, AgreedAt, Amount?, Currency?, DueDate?, LeadTimeDays?, SupersedesId?, CounterpartyConfirmedAt? }`.
- `tbl_CommitmentLink { CommitmentId, TargetType, TargetId, Relation }` — backlinks both ways.
- **The money ledger.** `tbl_Stage` gains `FundedAmount` / `FundedAt`; a per-stage rollup of
  **funded → claimed → cleared → carried forward**. This is the single screen that would
  have prevented *"Too many stages combined. I want to know if they were cleared or not."*
- **Variation register.** `tbl_Variation { CommitmentId, Reason, CostDelta, Currency, RaisedById, ApprovedById?, ApprovedAt?, Status }`. Eight costed variations in the corpus,
  including an entire added floor, none of them recorded. F3 is *caused* by F4.
- **Verbal decisions.** One tap creates a Commitment at `Agreed` with
  `SourceChannel = Verbal`, attributed to both parties. The counterparty gets
  **Confirm** / **That's not what we said**; a dispute flips it to `QueryRaised`.
- **Accountability is a query, not a feature.** `AccountableMemberId` is always the
  mediator. *"What did this contractor commit to on this project, and what state is each in"*
  is a group-by.
- Fold `tbl_Flag` in: a Flag is a Commitment in `QueryRaised`, or a blocker.

**Exit:** assetlen.md §11 test 1 — hand-enter the sixteen retaining-wall commitments already
drafted in [whatsapp-evidence.md](whatsapp-evidence.md) §7, show Peter the page, and ask
whether it saves the scrolling. **Run the hand version before writing the schema.**

---

### P5 — Extraction: pile into register *(Law 3 — promoted from P8)*

Under the old thesis the contractor posted structure and extraction tidied it. Under Law 0
**extraction is the only path from forwarded material to a register**, so it moves from
second-to-last to the middle of the plan.

- OCR every artifact on ingest via a Hangfire queue → `tbl_ArtifactText`.
- Read `tbl_IngestedMessage` and propose commitments. **Only money, materials, dates and
  decisions.** The corpus is hundreds of *"Okay"*, *"Noted"*, *"Good progress"* — those must
  yield nothing.
- Proposals land in a review queue Peter clears in bulk, not one nag at a time. Instrument
  the accept rate; if it drops below roughly two-thirds, narrow the trigger rather than
  shipping a confidently wrong register.
- A single real message carries five material commitments and another carries four
  quantities, in plain text with no OCR needed — the yield is high where the trigger is narrow.

**Exit:** run extraction over the raw 11 Jun – 5 Jul window and diff against the sixteen
hand-extracted commitments. Report precision and recall honestly. **This is the riskiest
phase in the plan** — tier 1's entire value rests on it, and the old thesis had the
contractor's structured posting as a safety net that no longer exists.

---

### P6 — Retrieval *(Peter's four searches)*

- Unified search over commitments, OCR text, ingested messages and artifacts, on SQL Server
  full-text.
- `/search` shaped as *"what did I approve on the balustrade?"* — grouped by object, not a
  message list.
- Every result carries its provenance strip: *agreed → evidence → invoiced → cleared →
  queried → resolved*.

**Exit:** a receipt that only ever existed as a photo inside a WhatsApp export is findable
by its vendor name.

---

### P7 — Peter's surfaces *(the demotion reversed)*

- **`/` is Peter's multi-project home.** The old plan cut this citing *"one project must
  work first"* — a sequencing note that hardened into a ban. Peter runs a main house, a
  guest wing, external works and a second site simultaneously. If he pays, the first thing
  he opens is all of them, each with: money position, decisions he owes, what moved.
- **The daily brief, assembled with no curator.** One page per project per day, **grouped by
  deliverable, not by time**, from ingested and captured material. Same-vantage-point
  pairing inside each block — seventeen chronological frames provably read as *"nothing much
  changed"*; one before/after pair does not.
- **Decisions Peter owes**, with by-when and consequence, across all projects.
- **The truth floor** — money, dates, agreed specs, blockers — is injected regardless of
  curation and cannot be dropped. State the rule to both parties once, plainly.
- Emphasis weighting per reader: the funder gets progress, money and dates; the
  representative gets specs, finishes and choices owed ([Dinah.md](Dinah.md)).

**Exit:** assetlen.md §11 test 3, the **silent-contractor test** — three weeks of tier 1
built from a real export with zero contractor involvement. Would Peter pay for this alone?

---

### P8 — Markup, query state, parked ideas

- `tbl_Annotation { ArtifactId, Version, AuthorId, ShapesJson, CreatedAt }` — a versioned,
  attributed layer over the original, never a new image. This is Peter's fourth search:
  circle the thing, ask why.
- Raise a query on a cleared commitment; resolving it **writes back into the commitment**,
  not into a message.
- Ideas parked against a future stage accumulate references and estimates silently.
- Lead times compute a "decide by" date backwards from stage start, and surface **only** when
  waiting costs something (Law 4). Six weeks of finishing blocked on a shipping container is
  the case this exists for.

**Exit:** Peter circles a line on a receipt, asks, the answer changes the commitment value.

---

### P9 — The contractor tier *(tier 3 — everything above still works without it)*

Only now, and only because nothing above depends on it.

- Three-tap capture against today's deliverables; **bulk camera-roll import** as the primary
  path — real capture is thirteen to eighteen frames at 22:00, not one in the moment.
- Offline queue with background sync.
- **Site Log** — the complete unsanitised record, delivery side only.
- **Curation by exception**: the mediator drops, promotes and exposes **individual frames**;
  the brief publishes at the cutoff whether or not he touches it.
- Mediator staffs the delivery side; Peter keeps the access roster.
- Web push at WhatsApp-comparable speed; voice notes with transcription.
- Claims carry their own evidence so the contractor gets paid without a phone call.

**Exit:** assetlen.md §11 test 2 — hand-build one real site day; the contractor says
*"I'd have dropped two of those"*, Peter says *"this is what I wanted."*

---

## Explicitly not building

| Cut | Source |
|---|---|
| Holding or moving money / escrow | §8 — funds route through three agents, two banks and a third party's account |
| **Any in-app informal channel** | §8 — cut harder under D3. WhatsApp keeps the conversation; we ingest it |
| **Voice notes as a launch item** | §8 — parity aimed at a contractor who may never log in. Tier 3 |
| Gantt charts and critical path (**retire `TimelineChart`**) | §8 — *"Peter thinks in stages, not networks"* |
| Bills of quantities | §8 — his own BoQ was cut down twice for being too heavy |
| Accounting integrations | §8 — not the bottleneck |
| Roles beyond developer / representative / mediator / delivery | §8 — permissions complexity, no user value |
| Anything that adds a tap to capture | §8 — directly causes churn back to WhatsApp |
| Lookbook, 3D explorer, visual search | Absent from the vision |

### Reversed on 2026-08-12

| Previously cut | Now | Why |
|---|---|---|
| Multi-project portfolio dashboard | **Peter's home screen (P7)** | He pays, and he runs four workstreams. One project must work first; it must not be the only thing that ever works. |

---

## Open risks, stated not solved

1. **Extraction quality is the whole bet.** If P5 produces a half-wrong register, Peter
   trusts none of it and tier 1 has no value. Validate by hand before building.
2. ~~**Seat economics.**~~ **Resolved 2026-08-12** — billing is per project by floor area,
   three tiers, delivery-side seats free and uncapped (assetlen.md §10.3, shipped in P2).
   Automatic area-from-drawings is deferred; the source is recorded so it can never
   silently overwrite a declared figure.
3. **A silent contractor still means a thin day.** Restructuring what arrives is worth
   paying for; it cannot manufacture a site photo nobody took. Tier 1 sells retrieval and
   reconciliation, not omniscience — say so in the marketing rather than discovering it in
   churn.
4. **Nothing has been validated with a real person yet.** All three tests in assetlen.md §11
   remain unrun. P3 removes the excuse rather than the risk: the import path exists and is
   green against a synthetic corpus, so the remaining cost of running test 1 for real is an
   afternoon and one file that is deliberately not in this repository.

5. **The parser is tested against fixtures this repository generates.** Two dialects, both
   invisible-character classes and the ambiguous-date case are covered, and the fixtures
   were built from the documented profile rather than from the parser's behaviour. But a
   real export can still carry a shape nobody anticipated — a localised media marker, a
   fourth timestamp format — and the failure mode is quiet: fewer messages than expected,
   not an error. The preview step is the mitigation, and it only works if somebody reads
   the count before pressing the button.

---

## Update protocol

1. Build green, no new errors.
2. Run **`bash tools/e2e-all.sh`** — P0+P1+P2+P3 in one command, currently 116 assertions.
   No row regresses. (`tools/e2e-access-audit.sh` is superseded: it still casts the
   contractor as project owner, which the buyer decision reverses.)
3. Update the phase table above; add rows for sub-phases.
4. Leave everything unstaged — the user commits.
