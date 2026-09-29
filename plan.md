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
| **P4** | Commitments, deliverables, ledger, variations | **Built** — 128/128 | Exit criterion is a human test; see P4 below |
| **P5** | Extraction, OCR, progress readings, media re-join | **Built** — 78/78 | Measured on the real export; see P5 below |
| **P6** | Retrieval — one search, grouped, with provenance | **Built** — 65/65 | Exit met on a synthetic export; see P6 below |
| **P7** | Peter's home and the daily brief | **Built** — 92/92 | Exit is §11 test 3, a human test; see P7 below |
| **R1–R5** | Works report — as built, schedule, agreed & stuck, drafted, on a cadence | **Built** — 117/117 | R0 is Peter reading it; see *Works Report* below |
| **P8** | Markup layers, query on cleared items, parked ideas + decide-by | **Built** — 104/104 | Exit met on a synthetic receipt; see P8 below |
| **P9** | Three-tap capture, offline queue, curation by exception, claim evidence, push, voice notes | **Built** — 90/90 | Exit is §11 test 2, a human test; see P9 below |

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
| P4 | Commitment model + the money ledger | 1 | **Built** — exit awaits Peter (§11 test 1) |
| P5 | Extraction — pile into register | 1 | **Built** — measured on the real export; real accept rate awaits Peter |
| P6 | Retrieval — Peter's four searches | 1 | **Built** — exit met on a synthetic export; real-export run awaits Peter |
| P7 | Peter's surfaces — home and the daily brief | 1 | **Built** — silent-contractor artefact ready; the three weeks with Peter await him (§11 test 3) |
| P8 | Markup, query state, parked ideas | 1–2 | **Built** — exit met end to end (API + headless browser) on a synthetic receipt; see P8 below |
| P9 | The contractor tier | 3 | **Built** — 90/90, and the P7 and report suites re-run with the contractor never signing in; §11 test 2 awaits the contractor and Peter |
| R | **Works Report** — track across P3–P7, slices R0–R5 ([works-report.md](works-report.md)) | 1 | **R1–R5 built** — 117/117; R0 (Peter reads it) and live drafting await a human and a key; see *Works Report* below |
| — | **Full test pass** after P4–P9 and the report (2026-09-29) | — | **Done** — chain green at 900/900 with a new Law 0 suite; six UI/seat faults found in the browser and fixed; see *Full test pass* below |
| — | **PostgreSQL** — the switch from SQL Server (2026-09-29) | — | **Done** — one baseline migration, Hangfire on Postgres, full-text + pg_trgm search; chain green at 906/906 from an empty database, twice, the second time with the SQL Server code removed; browser walk clean; an adversarial review fixed three Postgres-only defects (user search, inbound-address case, NUL characters) and an older inverted keyword filter, added `e2e-postgres.sh` and rewrote `remote-db-setup.md` for Postgres — **919/919** from an empty database; see *Postgres* below |

> **Added 2026-09-28.** Peter asked for *"a full works report … do you need more time and
> how long?"* two days before his 30 Sep completion date. The report is the §8 ship test in
> his own words, so it runs as a track through the phases rather than waiting for P7:
> **R0** hand-build the 28 Sep report before any schema (doubles as the P4 hand test) ·
> **R1** as-built report from existing data, media re-join, progress readings, issued
> snapshots · **R2** schedule — baselines, date commitments, pace forecast (needs P4) ·
> **R3** decisions, variations, blockers by owner (P4) · **R4** drafted descriptions with
> cited sources + extraction feeds (P5) · **R5** weekly and milestone issuing.
> Numbers never come from the drafting model; the report must issue with the contractor
> silent and with the model switched off.

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
Site Diary exists; the tier never rises without a person accepting it.

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

**Who may read an import.** Everything ingested is Site Diary material (assetlen.md §5), so
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

> **Landed ahead of the rest of P4 on 2026-08-18** — the seat model, the funding
> back-and-forth and the staging spine. All three are covered by
> `tools/e2e-p5-money-and-staging.sh`, and the whole chain runs at **186
> assertions, 0 failures**.
>
> **Seats.** `ProjectSeat { Principal, Support }`, derived from the existing
> `Specialization` column rather than stored, so an existing roster classifies
> itself. It is the second half of assetlen.md §10.1: the developer names the two
> principals and the contractor staffs their own bench underneath. The bench —
> fabricator, photographer, foreman — reports on its own work and sees no money,
> no register, no raw thread. Tabs and the ⋮ menu are now driven off standing on
> *this project*, never a tenant-level role, and a tab a seat has no business in
> is **absent, not present-and-refused**. `ProjectCardDto.Standing` carries the
> same answer to the dashboard, resolved in one membership query by
> `IProjectAccessService.ResolveManyAsync` rather than a query per tile.
>
> **Money is assignable per project.** `tbl_ProjectMember.HandlesMoney` (nullable
> — null follows the seat) puts a consulting engineer on the releases for the one
> job where they are chasing accountability, without promoting them everywhere
> else. Reading the ledger and moving it are separate rights: `CanConfirm` /
> `CanSettle` are stamped per release by the server, so the UI never offers a
> button that would 403.
>
> **The funding back-and-forth.** `FundingStatus` gains `AmountQueried` and
> `Settled`. The funder declares what they sent — in UGX or USD with a rate, both
> figures kept — with optional transfer evidence stored as an artifact. The
> delivery side answers in one tap ("yes, all of it") or names what actually
> landed, which opens the gap for the funder to accept or take up. Totals count
> what arrived, not what was sent.
>
> **Staging runs across the app.** `tbl_Stage` gains `ParentStageId` (one level,
> as with sub-projects), `CatalogueKey` and `Phase`. `StageCatalogue` holds 40-odd
> known stages of construction with their detail and aliases, grouped into nine
> phases; the panel is reachable from the foot of every stage dropdown for the
> life of the project, greys out what this project already uses, and accepts a
> custom stage filed under a major one. `IActiveStageService` fills in the active
> stage on anything created without one — captures, questions, releases — so
> nothing floats without taxing the clerk. Each phase carries a muted accent
> (`--al-stage-0…9`), used as a hairline or a dot in the picker, the catalogue and
> the search results, which is what tells plinth walling from parapet walling.
>
> ~~Still open in P4: the Commitment and Deliverable tables themselves, the
> variation register, and verbal decisions.~~ Landed 2026-09-28 — see below.

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

**Landed (2026-09-28).** Migration `20260928193443_P4_CommitmentsAndLedger`, **applied to the
dev database**, purely additive. `CommitmentDAL` / `LedgerDAL`, `CommitmentsController` /
`LedgerController`, the Refit clients `ICommitmentsApi` / `ILedgerApi`, and a new
`Modules/Commitments/` (`CommitmentCard`, `CommitmentForm`, `CommitmentRegister`,
`AccountabilityPanel`, `DeliverableChecklist`) plus `StageLedger`, `ClaimsPanel`,
`VariationRegister` in `Modules/Finance/`.

- **`tbl_Deliverable`**, **`tbl_Commitment`** (all plan fields, plus `RecordedBySide` so the
  counterparty is computable, `OwedBySide` for decisions owed, dispute / resolution / cleared /
  delivered / verified stamps), **`tbl_CommitmentLink`** (one row, both directions — `GetLinks`
  returns outgoing and citing commitments, `GetBacklinks` answers "what is this photo
  evidence for"; a link never reaches across the channel boundary or off the project),
  **`tbl_Variation`**, and **`tbl_StageClaim`**.
- **Restated, never overwritten.** A new date, figure or wording is a new row that
  `SupersedesId` the old one; the register shows heads, `GetChain` shows every statement,
  `RestatementCount` is on the card. A commitment never moves backwards in maturity.
- **Verbal decisions.** `LogDecision` lands at Agreed, `Verbal` or `Meeting`, attributed to
  both parties (defaulting the counterparty to the other side's principal). Only a principal
  on the *other* side may **Confirm** or say **That's not what we said** — the server stamps
  `CanConfirm` / `CanDispute`, never the client. A dispute is a query flag assigned back to
  whoever wrote it down; **resolving writes into the item** — a changed figure becomes a
  successor statement, so the questioned figure stays on record beside the agreed one.
- **Flags folded in, not replaced.** `tbl_Flag` gains `CommitmentId`, `OwnerMemberId`,
  `OwnerPartyName`. A flag with a commitment *is* that commitment in QueryRaised — raising
  and resolving keep the two in step, including `ResolveProjectFlags`. A flag without one is
  a **blocker**, grouped by whoever has to move. Every earlier flag assertion is unchanged.
- **Accountability is a query.** `AccountableMemberId` is stamped with the mediator (the
  parent's, for a sub-project); `GetAccountability` is a group-by by maturity, open queries,
  unconfirmed and overdue, with blockers by owner beside it.
- **The ledger.** Per stage, computed from rows, never stored: funded (acknowledged releases
  at what landed) → claimed → cleared → carried forward. Only a **closed** stage carries its
  balance to the next ("issue a receipt and carry the balance towards the next stage"); an
  open one holds it in hand. Cleared beyond funding shows as negative, not hidden. The plan's
  `tbl_Stage.FundedAmount / FundedAt` columns were **not** added: funding already lives in
  `tbl_FundingEntry` with its back-and-forth, and a second stored total would drift from it.
  Claims and extras are decided by the **funder** (client side, money seat) — the side
  claiming or proposing never approves its own. Every variation is also a commitment; an
  uncosted one is a visible gap, never a zero.
- **Works-report fields (works-report.md §4.2):** `tbl_Stage.BaselineStartDate/EndDate`, set
  once when a stage first gets dates (create, project-create, or first update) and never
  moved; the migration backfills existing stages from their current dates — honest best
  available, re-planning before today is invisible. The stage page shows *First planned* and
  the slip once it differs. Blocker owner = `OwnerMemberId` / `OwnerPartyName` on the flag.
- **Seats.** The register, accountability and commitment writes are principal-only
  (`CanSeeRegister`); the bench reads the deliverable checklist it works to and nothing
  else, and gets 404, not 403. Amounts on commitments are masked for a principal off the money
  (`HandlesMoney = false`) with `AmountHidden` saying one exists.
- **Also fixed on the way:** `StageDAL.Create/UpdateStage` did its own owner check inline
  (plan finding A1's pattern) — now `IProjectAccessService.CanManageAsync`; `StageDetail`
  gated money on the tenant-level `CanSeeFinancials` — now per-project `CanSeeMoney`;
  project purge now soft-deletes the P4 tables with the rest of the tree; and
  `SyncDAL.UpdateConfigSettings` is serialised — Hangfire draining a backlog of sync jobs
  after a restart corrupted `IConfiguration`'s dictionary and every request, sign-in
  included, answered 500.

**The §11 test-1 page exists and is seeded.** `DevSeedService` now writes the retaining wall
as Peter would see it — seven deliverables, the fifteen commitments and one blocker of
[whatsapp-evidence.md](whatsapp-evidence.md) §7, the stone-pitching statement #1 superseded,
#7's "too high" dispute open as a query, #14 as a decision Peter owes, the delivery items
linked to what they deliver — all filed by Peter from his own import (Law 0), Nalan the
accountable face on every one. Plus eight claims and five variations from F4 across the
other stages, so the ledger carries a balance forward. Pseudonymised like the rest of the
seed: brands, quantities and dates kept; the cement's place-name brand, the sand pit, people
and accounts are not. Open **`/project/de300000-0000-4000-8000-000000000010/register`** as
Peter.

**Exit — not met, and cannot be by a build.** The page is ready to put in front of Peter;
the question *"would this have saved the scrolling?"* has not been asked. The plan's "run
the hand version before writing the schema" was not honoured either — the schema followed
the plan's field list directly, so if Peter's answer reshapes the object, the migration is
additive and cheap to follow. `tools/e2e-p4-commitments.sh`: **128 assertions, 0 failures**;
`tools/e2e-all.sh` runs the whole chain at **314, 0 failures**.

**Outstanding.**
- A second project is not on the free tier, so `AddFundingEntry` refuses it; the suite
  therefore proves the funded column on the seeded project and runs its own ledger with
  claims only. Funding confirmation is still `ProjectManagerId`-only, not per-project standing.
- `GetFundingByProject` / `GetFundingByStage` still gate on `CanRead`, not `CanSeeMoney` —
  the money tab is absent for the bench but the endpoint answers. Pre-existing.
- Pending confirmations and owed decisions show on the register's *Needs you* filter but
  do not yet feed `AttentionState` (the rail count); disputes do, because they are flags.
- Links are created by API only — there is no "link evidence" picker in the UI yet (P6/P8
  surface it with the provenance strip). Raising a query on a *cleared* item works; the
  markup-driven version is P8.
- The accountability table scrolls sideways at 360 px rather than reflowing.
- The dev seed sweeps the money suite's "Chain test" / "Conversion test" releases on each
  call; it does not reset demo state a person has clicked through (the suite reads the
  retaining wall's two release columns together for that reason).

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

**Landed (2026-09-29).** Migration `20260928204700_P5_Extraction`, **applied to the dev
database**, additive: `tbl_ArtifactText`, `tbl_ExtractionRun`, `tbl_ExtractionProposal`,
`tbl_ProgressReading`, `tbl_MediaBinding`, and `tbl_IngestBatch.BoundMediaCount`. The
migration gives every existing stage one reading at its current percentage — the honest
best available, as P4 did for baselines. `ExtractionDAL` / `ExtractionController` /
`IExtractionApi`; `RejoinMedia` on `IngestController`; a new `Modules/Extraction/`
(`ProposalQueue`) shown as **From the thread** on the register, and `MediaRejoinPanel` on
History.

- **Extraction is deterministic by default.** `RuleMessageExtractor` (pure, in
  `FileProcessingServices/Extraction/`) reads money (`MoneyReader`: "UGX 12M", "12M",
  "4,578,100/="), materials (schedule lines, quantities, rebar, deliveries), dates
  (`DatePhraseReader`: "tomorrow", "2morrow", "by Tuesday", "this week", "end of September",
  pinned to the day the message was **sent**, never import day), decisions (instructions,
  "chosen / settled on / agreed", changes of spec, decisions one side is waiting on, a "yes"
  to a proposal) and blockers with a named owner. Acknowledgements yield nothing. Every
  proposal names the rule that fired it.
- **Replies are read in context.** "Tomorrow" answering "When do you intend to cast?" is a
  dated promise; "Yes, all the space up there" answering "We recommend terrazzo in the
  laundry too" is one agreed decision, and the bare recommendation is dropped; a decision
  owed that the other side makes within two days is not queued. "UGX 12M is too high for
  labour" does not add a proposal — it marks the 12M one **contested**, which lands it In
  discussion.
- **Summary updates write progress readings.** Percentages in the contractor's bulleted
  posts ("Guest wing plastering is currently at 90%") go straight to `tbl_ProgressReading`
  — an observation with a source, not a commitment — with a subject borrowed from the
  previous sentence when the bullet has none ("90% is already done"). Stage and capture
  percentage changes now append readings too (works-report §4.2, the R1 half).
- **The review queue** runs on import, unasked (Law 0), and is idempotent: a fingerprint
  per message, rule and wording means a re-run never re-proposes anything — including what
  was rejected. Peter clears it in bulk; accepting goes through `CommitmentDAL.AddCommitment`
  (source `Ingested`, a Source link to the message, the message quoted in the body) or, for
  a blocker, `FlagDAL.AddFlag` with the owner. **Accept rate is instrumented** overall and
  per rule; under two-thirds with at least ten decided, the queue says so and lists the
  weakest trigger first.
- **Privacy follows the source.** A proposal or reading is exactly as readable as its
  message (`ImportedSide` rule from P3, stored on the row); the bench gets 404; amounts are
  masked for a principal off the money.
- **OCR** runs on its own Hangfire queue (`ocr`, two workers) for every new artifact:
  text files are read as-is, images go to Tesseract when configured or on the PATH, else to
  Windows' built-in OCR through PowerShell. With no engine the row says
  `EngineUnavailable`, never "no text"; `QueueOcr` backfills once one exists.
- **Claude, behind the same interface, off by default.** `ClaudeMessageExtractor`
  (`claude-sonnet-5-5`, structured output) runs only when an API key **and**
  `Extraction:UseClaude=true` are set — sending a client's thread out is the owner's call.
  It sends sides, not names; every item must quote its message verbatim, a figure must be
  one `MoneyReader` finds there, a date one `DatePhraseReader` resolves there; a failed or
  refused window falls back to the rules. **Untested live — no key on this machine.**
- **Media re-join (works-report §5).** `WhatsAppMediaName` reads `WhatsApp Image
  2026-09-28 at 7.58.25 PM.jpeg` and its copies (`Image2`, `WhatsApp2`, `(1)`);
  `MediaRejoinPlanner` binds by minute, seconds order against transcript order, then the
  neighbouring minute; folder names are ignored. Human names become captions; byte-identical
  copies collapse (Law 2); unbound files are kept with a pointer and reported. Bindings are
  their own rows — the raw record is never edited. Because the stored body has the marker
  removed, the media lines are re-derived by re-parsing each export's own stored archive.

**Exit — the measurement as written is done; judged against the real export.** Run offline
over the private export (outside the repo) with the same `RuleMessageExtractor`; no real
name, place or account entered any repo file.

| Window | Messages | Proposals | Recall | Precision | Notes |
|---|---|---|---|---|---|
| 11 Jun – 5 Jul, vs the sixteen of [evidence](whatsapp-evidence.md) §7 | 165 | 24 | **15 / 16** | **19 / 24 strict** | Missed #16, "first half cast" — delivery of *work* is none of the four kinds. The 5 outside the list are genuine commitments from the two interleaved workstreams (a steel-team no-show and its next-morning date, two "casting tomorrow" promises, a burglar-proof design owed). 4 proposals restate an item already proposed. |
| 5 Aug – 28 Sep, vs a 32-item reference | 778 | 33 | **30 / 32** | **33 / 33** | Reference: works-report §11's decisions, variations and blockers plus the dated promises in the thread. Missed a restated "setting up tomorrow" and "wall tiles secured". 3 restate. All four readings of §11 (80 %, 70 %, 90 %, 90 %) recorded. |
| 6 Jul – 4 Aug, held out, **before** tuning | 238 | 7 | ≈ 3 / 12 | 4 / 7 | Figures with no currency, "Ush", a "yes" paired with the wrong question, an added floor, and **a real wrong figure**: "UGX 4,578,100?" read as 4,578 by a clause split. All fixed; the window is now in-sample. |
| 1 Apr – 10 Jun, held out, **after** tuning | 274 | 6 | ≈ 5 / 8 | 5 / 6 | Missed "300m" in lowercase, "saved approximately 15M", "let me remove them", a "90% completed" with no subject. Left untuned on purpose. |
| Whole thread | 2,227 | 109 | — | — | 0 proposals from acknowledgement-only messages. |

**Read these honestly.** The first two rows are in-sample: the rules were written against
those windows, and the second reference was compiled by the same agent after seeing the
output (from works-report §11, which predates it). The held-out rows are the real estimate:
**precision around 60–80 %, recall around 25–60 %** on unseen weeks. That is inside the
plan's tripwire only if Peter accepts at least two in three, and the per-rule instrumentation
exists precisely so that is measured rather than assumed.

**Media re-join on the real footage** (91 files, 15–28 Sep): **84 bound, 3 byte-duplicates
collapsed, 4 human-named files kept as captions**; of 11 files sitting in another day's
folder, 10 bound to their stamp's day and 1 was a duplicate. Accuracy is to the minute —
which frame belongs to which of eighteen lines inside one minute is ordered by seconds and
cannot be checked without the sender's phone.

`tools/e2e-p5-extraction.sh`: **78 assertions, 0 failures**, on a synthetic thread and
footage from `tools/make-extraction-fixtures.sh` (pseudonyms only, the real shape); the OCR
assertion reads a generated receipt with the engine this host has and **skips**, rather than
fails, where there is none. `tools/e2e-all.sh`: **392, 0 failures**.

**Configuration** (`appsettings.json` is gitignored): `Ocr:Engine` = `auto` (default) |
`tesseract` | `windows` | `none`; `Ocr:TesseractPath`; `Extraction:Engine` = `auto` |
`rules`; `Extraction:UseClaude` (default false); `Extraction:Model` (default
`claude-sonnet-5-5`); `Anthropic:ApiKey` or `ANTHROPIC_API_KEY`.

**Outstanding.**
- **Peter has not cleared a real queue.** The accept rate that decides whether the trigger
  is narrow enough exists only on synthetic data. Import the real export into his account and
  let him decide it — that is the P5 number that matters.
- Recall on unseen weeks is the weak side. Worth trying the Claude extractor under the
  validator once a key and consent exist; it has not run.
- Extraction runs inside the import request. Fine for the rules (milliseconds); with Claude
  enabled it belongs on the Hangfire queue.
- OCR text is stored, not yet searchable (P6). *Searchable since P6.* OCR was validated on a generated receipt,
  not on a real one — the real footage is site photos. No video posters yet (works-report
  §5.3, R1).
- Re-join holds each upload in memory and inherits the 40 MB artifact ceiling, so a long
  video is reported unbound rather than stored.
- Pending proposals do not feed the rail's *Needs you* count. At 360 px the register's view
  switcher scrolls sideways and the new tab starts off-screen.
- The dev database now holds a synthetic demo project, *Guest wing thread review*, under
  the Peter persona.

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

**Landed (2026-09-29).** No migration — retrieval reads what P2–P5 already store. `SearchDAL` /
`SearchController` (`GET /api/Search/Query?q=&projectId=&take=`) / `ISearchApi`; a new
`Modules/Search/` holding the page (moved from `Pages/Search.razor`, which fanned out one
request per project per source from the browser) and `SearchResult`, `ProvenanceStrip`,
`SearchThumb`. `/search?q=` keeps the question in the address; `/project/{id}/history?day=`
opens the thread on the day a result was said; `/project/{id}/register?commitment=` opens the
register scrolled to, and ringed around, the commitment.

- **Sources.** Commitments (current statements only, title / body / counterparty), the text
  OCR read out of every artifact (`tbl_ArtifactText`, `Done` rows), file names and captions,
  the imported thread, the Site Diary, projects and stages. The uploaded transcript itself
  is excluded — its text is already searchable message by message.
- **Grouped by object, commitments first.** A message that is where a commitment was agreed
  (`IngestedMessageId`, or a `Source` link) is folded into that commitment rather than
  listed twice; a restated commitment answers with its current statement. Other messages stay
  under *From the thread*. Question words and the verbs of agreeing ("what did I **approve**
  on the") are set aside, shown, and bias the ranking toward commitments.
- **Provenance strip on every result.** A source chip (thread / shared / email / capture /
  drawing register / register / call / meeting, who, when — the accountable face for the
  client side) and, for commitments, *agreed → evidence → invoiced → cleared → queried →
  resolved* from its own dates and the links the reader may open. A photo or diary entry
  borrows the strip of the commitment it proves, bills or settles, with its own step marked
  (the receipt *is* the tiles' "invoiced"); a file tied to nothing says **"Not tied to any
  commitment yet"**.
- **Strictly side- and seat-filtered, one rule per source, via `IProjectAccessService`.**
  Register → `CanSeeRegister`; money → `CanSeeMoney` (a principal off the money gets
  `amountHidden`, no figure); the thread → principal seat plus the importing side's claim
  (IngestDAL's rule, narrowed by `CanSeeHistory`); files → at least one way they reached the
  project that the reader may see (their message, an exposed capture, a client-channel
  drawing); Diary → `CanSeeSiteLog`, or exposed and `CanSeeBrief`. A link to a crew-only
  photo does not light "evidence" for the client side. A stranger searches no project and
  naming one returns 404.
- **Says where it could not look**: files still waiting for OCR are counted, and a server
  with no OCR engine says so on the page.
- **Matching — the documented fallback.** SQL Server Full-Text Search is **not installed**
  on the dev instance (`SERVERPROPERTY('IsFullTextInstalled') = 0`). Matching is a
  case-insensitive substring per term (the collation is CI), unioned per term, scored in
  memory (coverage, then a word in the title, then recency); three or more words need all but
  one. The answer reports `backend` and `fullTextInstalled`. `SearchDAL.CandidatesAsync` is
  the one seam where a `CONTAINS` query replaces the `LIKE` once full-text is installed —
  worth it only when a tenant's corpus reaches hundreds of thousands of rows.

**Exit — met, on a synthetic export.** `tools/make-search-fixtures.sh` draws **ZENTARA TILES**
into the pixels of `IMG-20260805-WA0012.jpg` inside a WhatsApp export zip; the name is in no
message and no file name. After Peter imports it, the OCR queue reads it (Windows OCR on this
host) and a search for *zentara*, *zentara tiles* or *where is the receipt from Zentara?*
returns exactly that photo, matched in "the text read from the photo", with its chip *From
the thread · Nalan · 5 Aug 2026, 16:14* and a link to that day of the thread — while the
thread itself has no message containing the word. Verified in the browser at 360 / 768 /
1280, light and dark: no console errors, every CSS bundle 200, no horizontal overflow,
thumbnails through the authenticated pipeline, both deep links land.

`tools/e2e-p6-search.sh`: **65 assertions, 0 failures** — the exit, the balustrade question,
provenance (invoiced / cleared / queried / resolved, the gap), seats (money, the bench, a
stranger), sides (a crew export Peter cannot find once he stands down as mediator), and the
Diary. It skips rather than fails where the host has no OCR engine. `tools/e2e-all.sh`:
**457, 0 failures**.

**Outstanding.**
- **Not run against the real export.** The exit was proven on a clean, synthesised receipt;
  a real photographed receipt (angle, glare, thermal print) is harder for OCR, and the real
  footage is site photos. Import Peter's export and search for a vendor he remembers — that
  is the P6 number that matters.
- Substring matching finds "cement" in "cementitious" and misses OCR misreads ("ZENTARA" read
  as "ZENTAHA"); there is no fuzzy match. Numbers must be typed as written ("4,578,100", not
  "4578100").
- Variations, releases and claims are not searched yet (their notes); nor are flags.
- The provenance strip lives only on search results. The register's `CommitmentCard` still
  shows its maturity strip; putting `ProvenanceStrip` there means promoting it to
  `Components/` (two modules) — and the evidence picker is still P8.
- The dev database now holds a synthetic P6 project per suite run under the
  `peter.buyer@assetlen.test` pseudonym, as the earlier suites do.

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

**Landed (2026-09-29).** No migration: both surfaces read what P2–P6 store. `BriefDAL` /
`BriefController` (`GET /api/Brief/Home?days=`, `GET /api/Brief/Owed`,
`GET /api/Brief/Day?projectId=&day=&days=`) / `IBriefApi`; pure helpers `BriefFiler` and
`VantageIndex` in `FileProcessingServices/Brief/`; a new `Modules/Brief/` holding the page
(moved from `Projects/Pages/ProjectBrief.razor`, same route, now `?day=`-addressable) and
`TruthFloor`, `BriefBlockView`, `VantagePair`, `ProjectStanding`. `SearchThumb` was promoted
to `Components/Media/ArtifactThumb` (two modules now use it).

- **`/` is the multi-project home.** A new *Where each project stands* section, one row per
  project the reader stands on: money position (funded → claimed → cleared → in hand, plus
  claimed-not-cleared and sent-not-acknowledged — the stage ledger's rules on the same rows,
  so the two figures cannot disagree; asserted), what he owes (the first item and a count)
  with open blockers, and what moved in the last seven days (register, thread counts,
  captures, readings, releases and claims, blockers). One call, one membership query
  (`ResolveManyAsync`). Money is per project and per seat; a row with no money seat says so.
  The per-project browser loop over progress updates is gone.
- **Decisions owed, across projects, server-side.** Choices his side owes (`OwedBySide`),
  spoken agreements waiting on his Confirm (`CommitmentDAL.CanAnswerSpoken`, now shared),
  questions and blockers assigned to him on either channel, claims and extras for the funder,
  stalled releases (the `GetFundingNeedingMe` rule), and pending proposals from the thread —
  each with by-when and a consequence in words ("Holds up Terrace floor tiles"). `AttentionState`
  now reads this one list, so **the rail, Home and Needs-you count spoken decisions, owed
  choices and pending proposals** (carried over from P4/P5), in one request instead of one per
  project.
- **The daily brief assembles itself.** Server-side, from the thread the reader may read
  (SearchDAL's rule), captures and exposed frames (the Diary's rule), readings (ExtractionDAL's
  rule) and the register. **Grouped by deliverable**: a line is filed to a deliverable when two
  words of its title appear (or a whole one-word title), else to a project stage
  (`StageMatcher`), else — for a project with no stages yet — to the **stage catalogue's** name
  for the work, labelled *not a stage on this project yet*; else *not tied to a deliverable*,
  counted. A human's filing (a commitment's deliverable) beats the guess; a reply with no
  subject inherits what it answers (20 min); a photo with no words joins its sender's nearest
  line (45 min) or its human file name. Acknowledgements are read, counted and given no space.
  A reading that names a deliverable is that deliverable's; one naming a stage speaks for the
  stage's other blocks.
- **Same-vantage pairing.** A 64-bit difference hash of each thumbnail (`VantageIndex`, cached
  by content hash — no column, so a better fingerprint is a code change, not a migration); each
  frame in the window pairs with the closest earlier frame of the same deliverable (else stage)
  under 0.25, at most two pairs per block, the rest shown capped with a count.
- **The truth floor is injected, every section for every reader, empty or not.** Money
  (register figures and restatements, releases, claims, extras), dates (agreed, moved from → to,
  stage end dates off their baseline), specs agreed / changed / queried, **every open blocker
  whichever channel raised it** (crossing to the client side in the mediator's name without the
  crew's wording — Peter's flag list still does not carry it; asserted), and what he owes.
  Figures and dates read from the thread but not confirmed are on it too, dashed and marked
  *not yet in your register* — the silent contractor's facts. The rule is stated to both sides.
- **Emphasis per reader orders, never drops.** Funder: money, dates, blockers, owed, specs;
  blocks where progress or money moved first. Representative (`ClientRepresentative`, or a
  client principal off the money): owed, specs, blockers, dates, money; blocks with a choice she
  owes, a spec change or finishes first. The suite asserts both floors hold exactly the same
  keys in different orders; off the money, figures are masked and ledger rows absent. The bench
  and strangers get 404.
- **Also fixed on the way:** `RejoinMedia` failed with *"the inner stream position has changed"*
  whenever more than one loose file was sent in a request; each form file is now copied out
  before reading.

**Exit — not met, and cannot be by a build.** §11 test 3 is three weeks of Peter's own use.
The artefact is ready: `tools/silent-contractor-trial.sh` signs in as Peter, creates a project
only he is on, imports his export with no author mapped, re-joins the footage, and writes a
per-day account of the brief for 21 days to a report **outside the repository**. Run on the
real export (no stages set up, nobody else on the project): 84 of 91 files bound, blocks on 16
of 21 days, acknowledgements set aside, unconfirmed dates and specs on the floor most days, and
**zero same-view pairs** — on the real footage the closest earlier frame is under 0.25 for only
4 of 64 photos even before filing, so the fingerprint that pairs the synthetic before/after
cleanly does not find the real ones. That is this build's headline weakness.

`tools/e2e-p7-brief.sh`: **92 assertions, 0 failures** — Law 0 (Peter alone, authors unmapped),
grouping, the pair (and the photo from elsewhere left unpaired), readings, the floor with the
contractor silent, owed with by-when and consequence, the home's money against the stage
ledger, the crew-channel blocker crossing, emphasis without loss, masking, seats, curation as an
upgrade, and a project with no stages. Verified in the browser at 360 / 768 / 1280, light and
dark: no console errors, every CSS bundle 200, no horizontal overflow, pair thumbnails through
the authenticated pipeline. `tools/e2e-all.sh`: **549, 0 failures**.

**Outstanding.**
- **Pairing on real footage is 0.** Handheld photos of one view differ more than the
  difference hash tolerates; raising the threshold without a labelled set would pair unrelated
  frames. Needs a small hand-labelled set of real same-view pairs, then a better fingerprint
  (or a model) behind `IVantageIndex`.
- Filing is words only. A real thread names the work loosely ("the wing", "upstairs"); with no
  stages set up most lines land on catalogue names or unfiled. Peter adding his stages and
  deliverables changes this more than any rule will.
- The brief is computed on read; "publishes at the cutoff" is a label (`Brief:CutoffHour`,
  default 20:00), not an issued snapshot. Issued snapshots are works-report R1.
- Consequence text is derived (deliverable, stage start, lead time); no field lets a person
  write what waiting costs. Backwards-computed decide-by dates are P8.
- Carried over, unchanged: second-project funding; `GetFundingBy*` gating on read rather than
  `CanSeeMoney`; the 360 px accountability table and register switcher; the real place name in
  the dev seed's location and the dev personas' shared surname — both worth checking against
  the anonymity rule.
- The dev database holds a synthetic P7 project per suite run under the
  `peter.buyer@assetlen.test` pseudonym, and **one project built from the real export**
  (*Silent-contractor trial*, same account) left for review; an earlier failed run of it is
  archived. Delete both if the real content should not sit in the dev database.

---

### Works Report — R1 to R5 *(works-report.md; the §8 ship test in Peter's own words)*

**Landed (2026-09-29).** Migration `20260928232921_R_WorksReport`, **applied to the dev
database**, additive: `tbl_WorksReport` (frozen snapshot + narrative JSON, SHA-256, previous
report, issue kind, milestone trigger key, delivery note), `tbl_ArtifactPoster`,
`tbl_Flag.RaisedAt` (a blocker's age runs from when it was reported, not typed — extraction
now stamps it from the message), `tbl_Project.ReportDraftingEnabled`. `WorksReportDAL` /
`WorksReportController` / `IWorksReportApi`; `FileProcessingServices/Report/` holds
`StageLedgerMath` (the ledger's arithmetic, now shared by `LedgerDAL` and the report so they
cannot disagree), `ReportNarration` (`IReportNarrator`, `TemplateReportNarrator`,
`NarrativeValidator`), `ClaudeReportNarrator`, `VideoPosterJob`, `ScheduledReportJob`. A new
`Modules/Report/` — `Pages/WorksReport.razor` at `/project/{id}/report` (live) and
`/project/{id}/report/{reportId}` (issued), and twenty components, each with its `.razor.css`.
`ProjectAccess` gains `CanSeeReport` / `CanIssueReport` (mirrored on the DTO); a **Report** tab,
an Overview jump and a link on each home row are the three doors.

- **R1 — as built.** Cover (hero 21/9, the answer first), headline cards, stage board grouped by
  phase with progress rings, 64×16 sparklines whose flat tail turns warning and says
  *Stalled n days*, plan/pace ticks, at most two frames per card, before/after same-view pairs,
  footage by stage, money, blockers, sources appendix. **Assembled as at any moment**: every
  row is filtered by its own date (readings, commitments, variations, blockers, releases and
  claims *as they stood*), so "issue as at 14 Sep" is the record at the end of 14 Sep. Issued
  = frozen: the snapshot and narrative are stored as JSON, hashed, and read back byte for byte;
  the next report links it, takes its window from it and says what moved (stage %, forecast,
  lapsed count, new decisions/variations, blockers opened and cleared, funded Δ).
  **Print stylesheet** (A4, one section per break, chrome dropped, chips print as `S12`
  references resolved in the appendix) — "Save as PDF" from the browser is v1.
  **Video posters**: new videos queue a Hangfire job; ffmpeg (config `Media:FfmpegPath` or the
  PATH) extracts a frame and ffprobe the length; the frame becomes the video's thumbnail, so
  every existing surface shows it through its existing visibility check. With no ffmpeg the
  row says `EngineUnavailable` and the tile says *length unknown*.
- **R2 — scheduled.** The completion date is the chain of `Date` commitments worded as
  completion and naming no stage: ● set, ◆ restated with the same date, ○ the latest different
  date, ■ promised; × every short promise whose day passed undelivered and unrestated. Pace per
  open stage from the last 21 days of readings; zero pace gives no forecast, and the strip names
  the stages it cannot speak for rather than guessing. Baselines (P4) show as *First planned …
  moved +n d*. ⑨ Ahead: deliverables and promised dates in 28 days, decisions owed with by-when
  and consequence, the next stages — and a revised completion date listed as owed **to** Peter
  when the date is at risk and none is on record. Deadline strip in `--al-blueprint`, one axis,
  no rows, with the same marks as a table beneath it.
- **R3 — agreed & stuck.** Decisions with who/when/proof (a spoken one unconfirmed, a queried
  one, one with no evidence are gaps); variations with cost Δ, time Δ and approval or
  *not costed* / *not stated* / *no approval on record*; a spec restated in the window with no
  variation behind it is its own gap. Blockers in lanes by owner, oldest first, days open from
  `RaisedAt`; a crew-channel blocker crosses without the crew's wording.
- **R4 — drafted.** `NarrativeRequest` per target (cover, answer, stage, decision, variation,
  blocker) carries the final facts, the allowed source ids, the source words and a templated
  fallback. `NarrativeValidator` refuses a sentence that cites nothing or a source it was not
  given, carries a figure or date word the facts do not, forecasts/sells/blames beyond the
  record, or runs over budget; a refused target falls back to its template. Claude drafts only
  with a key **and** `Report:UseClaude` **and** the owner's per-project consent (the setting
  states what is sent); cards on `claude-sonnet-5-5`, the cover on `claude-opus-5-5`, structured
  output; drafting runs on issue only — the live page is always templated, so looking never sends
  anything. **Also fixed:** the reading extractor gave a one-word subject ("Terrazzo is at 45%",
  "Screeding is at 60%") to the previous sentence — "Doors started" was recorded at 60%. Only a
  pronoun or filler borrows now.
- **R5 — on a cadence.** Hangfire recurring jobs: weekly (Sunday 18:00 local, `Report:WeeklyCron`)
  and hourly milestones (a stage completing, a completion date passing unmet), each at most once
  (a week window; a trigger key). Issued with no user, recorded as *Issued on schedule*, and
  delivered in the app to the audience's principals (the delivery note names them). History lists
  every report the reader's side may read.
- **Also fixed on the way:** project create dropped the stages' `Phase` and `CatalogueKey`, so
  every stage made with its project read as *Custom* — no accent, no phase grouping.

**Exit — R1–R5 met on a synthetic Aug–Sep window; R0 not met, and cannot be by a build.**
`tools/make-report-fixtures.sh` synthesises the window's shape (pseudonyms only): an Android
export *without media* plus loose files in day folders (one misfiled, one stray, a 3-second
video), the 13 Aug date restated 22 Aug, plaster 80 → 90 → 90 while the thread says it
continues, "complete this week" lapsed, readings that give a pace, six variations (five
uncosted), five third-party blockers, one UGX 100M release never acknowledged, and a separate
crew thread. `tools/e2e-report.sh`: **117 assertions, 0 failures** — every check of
works-report.md §10 plus each slice's exit: the report issues with the contractor not on the
roster; the 21 Sep re-issue shows tiling 20 → 50 and terrazzo — → 45; the strip reads set 13 Aug
→ restated 22 Aug → two lapsed → promised 30 Sep, pace 10 Oct, and once a new date is recorded,
○ 20 Oct beside ◌ 10 Oct; every scope change is a variation or a gap; every sentence cites a
source in the snapshot; the validator refuses a stray figure, a missing or foreign source,
selling and promising; with drafting on and no model it still issues, templated; the weekly job
issues with nobody logged in, once, and the milestone job on a stage completing, once; money
reconciles to the stage ledger and stage figures to the readings table; a client report carries
no crew photo, words or blocker wording; the bench and a stranger get 404; a reader off the money
gets no `money` key at all. Verified in the browser at 360 / 768 / 1280, light and dark: no
console errors, every CSS bundle 200, no horizontal overflow; print media renders without the app
chrome.

**Outstanding.**
- **R0 — Peter reads the report and answers *does this answer "more time and how long"?*** The
  artefact: issue one from his real export in his own account (import → re-join footage → enter
  the completion date he set → Issue as at 28 Sep) and print it. Not done here: the real export
  stays outside the repository.
- **Drafting has never run live** — no API key on this machine. `ClaudeReportNarrator` compiles
  against the same SDK surface as the P5 extractor; it uses the plain Messages API with the
  refusal falling back to templates rather than the server-side `fallbacks` beta.
- The completion date is found by wording ("complete / handover / finish by", no stage named).
  A completion commitment titled otherwise is read as a short promise.
- Captures still store photos inline (data URIs, pre-P2 path in `ProgressDAL.AddProgressUpdate`),
  so a captured frame has no artifact and does not reach the report's footage; thread photos and
  re-joined files do. Folding captures into the artifact store is P9's.
- Same-view pairing inherits P7's fingerprint — 0 pairs on the real footage.
- Delivery is in-app only: no email or push. Server-side PDF (v2) not built; v1 is the print
  stylesheet. Optional photo captions (§6.3) not built.
- The live report reads the client audience; the delivery side's own view exists only as an
  issued report with audience *Contractor*.
- Each suite run adds a project and a buyer (`peter.report…@assetlen.test`) to the dev database.

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

**Landed (2026-09-29).** Migration `20260929003310_P8_MarkupAndParkedIdeas`, **applied to the dev
database**, additive: `tbl_Annotation` (plan fields plus `LayerId`, `AuthorSide`, `Channel`, `Note`,
`CommitmentId`, `SupersededAt`), `tbl_CommitmentEstimate`, `tbl_Commitment.DependsOnStageId`.
`AnnotationDAL` / `AnnotationsController` / `IAnnotationsApi`; `DecideByRule` (Shared.Models, pure,
used by the register and by `BriefDAL`); a new `Modules/Markup/` — `ArtifactMarkup` at
`/project/{id}/artifact/{artifactId}` (`?commitment=` preselects), `MarkupCanvas`, `MarkupPreview` —
and `MarkupOverlay` in `Components/Media/` (used by Markup and Commitments). `ArtifactThumb` gains
`Full` / `Natural` / `Overlay` so a layer lines up with the picture's own box.

- **Markup is a layer, never a new image.** Marks are stored as fractions of the image (ellipse,
  box, arrow, freehand), validated server-side (inside the file, at most 40 marks). A change is the
  layer's next version; the previous one is kept and listed as history. Only the author adds a
  version. The original's bytes and its reference count are asserted unchanged.
- **Sides and seats.** The file's own visibility decides first (`IArtifactDAL.GetAsync`), then the
  layer's channel. A client-side layer is `Client`; a delivery-side layer lands `Crew` (fail-closed)
  until the mediator exposes it, and then reads in the **accountable face's name**, never the
  bench's. A layer that asked a question is visible only to seats that read the register. A stranger
  gets 404.
- **Circle the thing, ask why.** `Annotations/Ask` is a circle plus a question on a commitment: it
  goes through `CommitmentDAL.RaiseQuery` (same flag, owed to the same person, 409 if already open),
  works on a **cleared** item, and links the layer (`CommitmentLinkTarget.Annotation`) and the file
  back to the item. The register card shows the circled file beside the item (`MarkupPreview`),
  carried through restatements. The links drawer opens files in the markup page ("Mark up & ask"),
  as does a picture in search results.
- **Resolution writes into the item** (P4's `ResolveQuery`, unchanged): the answer is a successor
  statement; the card now reads *"Revised from UGX 4,320,000 (+UGX 480,000); the earlier figure was
  cleared on …"* (`PreviousAmount` / `PreviousDueDate` / `PreviousClearedAt`, masked off the money).
  The new figure can be cleared in its turn.
- **Parked ideas.** An `Idea` against a future stage gathers references (links) and estimates
  (`AddEstimate`, figure only on the money seat) **silently**. `Park` re-files an undecided item:
  its stage, the stage that must not start first (`DependsOnStageId`), lead time.
- **Decide-by (Law 4).** `DecideByRule`: the earliest of *stage start − lead time* and *dependency
  start − lead time*; no start date, no deadline. It **surfaces** only within 14 days of that date,
  or when the idea's own stage is within 7 days of starting (the stage-kickoff hand-back). Surfaced
  ideas enter `Brief/Owed` as `OwedKind.ParkedIdea` for the side that owes them (the client side
  unless someone said otherwise), so the rail, Home and the truth floor carry them; a moved stage
  date moves or silences them. **Changed behaviour:** an unsurfaced Idea-maturity choice no longer
  appears in the owed list or the register's *Needs you* (nothing in the seed or the earlier suites
  relied on it). An open choice with no date of its own takes its decide-by as its by-when. The
  register gains a *Parked ideas* filter.

**Exit — met, on a synthetic receipt, end to end.** `tools/make-markup-fixtures.sh` draws an invented
receipt (40 bags at UGX 4,800,000) against a register line of 36 bags at 4,320,000 that Peter has
cleared. `tools/e2e-p8-markup.sh`: **104 assertions, 0 failures** — the ask on a cleared item, the
layer and its versions, the untouched original, sides / seats / exposure in the accountable face's
name, Nalan's resolution writing 4,800,000 onto the item beside the kept 4,320,000, masking for
Dinah, and parked ideas that stay silent, then surface by dependency and by kickoff, then go quiet
when the driveway slips. **In the headless browser** (Peter, 1280): register → the card's link →
*Mark up & ask* → drag an ellipse over the cement line → the item is preselected → *Ask on the item*;
then Nalan resolves on the card with the figure; Peter's card shows the revised figure, the delta,
the earlier clearing and the circled receipt. Checked at 360 / 768 / 1280, light and dark: no console
errors, every CSS bundle 200, no horizontal overflow (a long layer badge overflowed at 360 and was
fixed). `tools/e2e-all.sh`: **770, 0 failures**.

**Outstanding.**
- Not run on a real photographed receipt; the circle is not read against OCR (the layer does not
  know which printed line it covers).
- The resolve pane changes the figure or the date, not the wording, so a resolved *"36 bags"* keeps
  its title and the resolution note carries *"revised to 40"*.
- Estimates cannot yet be pointed at a message from the UI (the API takes `IngestedMessageId` /
  `ArtifactId`); ideas are not yet proposed by extraction.
- The consequence of waiting is still derived text; there is no field for a person to write it.
- The dev database holds a project *Terrace finishes (P8 walk)* under Peter, left from the browser
  walk (answered query, two parked ideas) — delete it if not wanted. Each suite run adds and bins a
  throwaway project.
- Carried over, unchanged: second-project funding; `GetFundingBy*` gating on read rather than
  `CanSeeMoney`; the 360 px accountability table and register switcher; the real place name and
  shared surname in the dev seed.

---

### P9 — The contractor tier *(tier 3 — everything above still works without it)*

Only now, and only because nothing above depends on it.

- Three-tap capture against today's deliverables; **bulk camera-roll import** as the primary
  path — real capture is thirteen to eighteen frames at 22:00, not one in the moment.
- Offline queue with background sync.
- **Site Diary** — the complete unsanitised record, delivery side only.
- **Curation by exception**: the mediator drops, promotes and exposes **individual frames**;
  the brief publishes at the cutoff whether or not he touches it.
- Mediator staffs the delivery side; Peter keeps the access roster.
- Web push at WhatsApp-comparable speed; voice notes with transcription.
- Claims carry their own evidence so the contractor gets paid without a phone call.

**Exit:** assetlen.md §11 test 2 — hand-build one real site day; the contractor says
*"I'd have dropped two of those"*, Peter says *"this is what I wanted."*

**Landed (2026-09-29).** Migration `20260929013551_P9_ContractorTier`, **applied to the dev database**,
additive: `tbl_ProgressUpdate.{DeliverableId, ClientCaptureId, CapturedAt, VoiceArtifactId}` (unique on
project + client capture id), `tbl_ProgressImage.{Curation, CuratedById, CuratedAt}`, `tbl_BriefPublication`,
`tbl_ClaimEvidence`, `tbl_PushSubscription`, `tbl_PushDelivery`. New: `IFrameExposure` (the one place a
frame crosses — frame and artifact pointer move together, for the mediator, the cutoff and a claim alike),
`CurationDAL` / `CurationController` / `CutoffPublishJob`, `PushDAL` / `PushController`, `Notifier` +
`PushDispatcher` + `WebPushCrypto` (`FileProcessingServices/Push/`), `WindowsSpeechTranscriber`
(`Ocr/Transcription.cs`), `IProjectAccessService.ResolveUnscopedAsync` (the same rules for jobs that run
with nobody signed in). Client: the capture page rewritten, `CaptureOutbox` + `outbox.js`, `OutboxStatus`
(Components/UI — Capture and the Diary), `VoiceRecorder`, `BriefCuration` at `/project/{id}/log/curate`,
`ClaimEvidenceStrip` and an evidence picker in `ClaimsPanel`, `PushOptIn` on Account, `push-sw.js`.

- **Three taps.** `Progress/GetCaptureToday` lists today's deliverables (stages under way first); the page
  is *the work → the camera roll → post*, with note, voice note, reading, issue flag and (mediator only)
  channel behind one disclosure, none required. `Progress/Capture` is multipart, up to 24 frames (the old
  JSON path capped at 5 and the page at 12); frames are resized to 1600 px on the phone. **Captures now go
  into the artifact store** (the works-report outstanding item): hash-deduplicated, thumbnailed, OCR'd,
  pointed at by a `ProgressUpdate` ref — so captured frames reach the brief, the report's footage,
  search and claims. The JSON path was folded into the same code. Posting needs `CanCapture`; a reading
  is recorded only when one is given (it used to null the stage's percentage). A capture aimed at a
  deliverable moves it from not started to under way, and the brief files it there (a person's filing
  beats the word guess).
- **Offline queue.** Every post is written to IndexedDB first and sent by `CaptureOutbox` — at once, on
  the `online` event, on returning to the tab and every 30 s while the app is open; the strip says what is
  waiting and why the server refused one. The server keeps one entry per `ClientCaptureId`, so a retry after
  a lost reply never posts twice, and files a capture on the day it was **shot** (`CapturedAt`, clamped to
  the last fortnight), not the day the signal came back. Sync runs while the app is open; there is no
  service-worker Background Sync.
- **Site Diary.** Unchanged in principle and now asserted from P9's side: complete, true authorship for
  the delivery side, 404 to the client side and to strangers (a stranger on an entry got 403 before; now
  404). On the client side an entry's author reads as the accountable face. Photo-only and voice-only
  entries read sensibly in the list; a voice note stays delivery-side.
- **Curation by exception.** Per frame: *keep*, *drop*, or leave it to the rule — at most three per piece
  of work (first of the day, latest, one between), kept frames counted first, anything already shown
  counted too. At the cutoff (`Brief:CutoffHour`, default 20:00; Hangfire every 15 minutes, dev endpoint
  `Curation/RunCutoff`) the selection crosses **in the mediator's name** whether or not he opened the page,
  once per day, and not while a batch is still arriving (last capture under ten minutes old). No mediator
  appointed → nothing crosses (fail-closed). Dropping a frame already shown withdraws it. The brief itself is
  untouched: it still assembles from the thread with the contractor silent, and the truth floor is not his to
  curate.
- **Mediator staffs the delivery side; Peter keeps the roster** — P2's rules, re-asserted: the mediator adds
  his bench (200) and never Peter's side (403); Peter's roster carries names and sides, no traffic; Peter can
  remove the contractor and his bench and **loses nothing** — the Diary, the claim and its evidence stay
  (D1).
- **Claims carry their own evidence.** `Ledger/GetClaimEvidenceOptions` offers the frames captured on the
  stage since its last claim (newest of each capture pre-selected), its deliverables (done ones pre-ticked)
  and its latest reading; `AddClaim` records them as `tbl_ClaimEvidence`. Attaching a crew frame **exposes it
  in the accountable face's name** and is the mediator's call (403 otherwise). The funder's claim row shows
  the photos, "*n* of *m* deliverables done" and the reading; the client side reads the claimant as the
  accountable face.
- **Web push.** RFC 8291 `aes128gcm` + RFC 8292 VAPID on the framework's own P-256/HKDF/AES-GCM (no new
  package). The VAPID key is config or a key file under the storage root, generated once. Deliveries go out
  from an in-memory channel as they are queued (not a polling job) and are logged with status and latency;
  404/410 unsubscribes. Who is woken is decided per event by standing: a capture wakes the mediator (never
  the client side — the bench's traffic is the delivery side's), a claim wakes the funder, the day's crossing
  wakes the client side. Opt-in on Account. The dev push sink (`Dev/PushSinkSubscribe`) holds the browser's
  half of the keys, decrypts, and checks the VAPID signature.
- **Voice notes.** Recorded in the page (MediaRecorder), stored as an artifact, transcribed on the OCR
  queue into `tbl_ArtifactText` — so search finds a voice note by its words, delivery side only. The engine
  is Windows' dictation recogniser through Windows PowerShell (WAV, or anything ffmpeg can convert); with no
  engine the row says `EngineUnavailable` and the recording is kept. `Transcription:Engine = none` turns it off.

**Law 0 re-proven.** `tools/e2e-p7-brief.sh` and `tools/e2e-report.sh` take `SILENT_CONTRACTOR=1`: no
delivery-side account signs in, and every assertion that needs one to act is **skipped, not faked** (P7:
38 pass, 1 section skipped; report: 104 pass, 5 skipped). The P9 suite runs both that way and checks the
server's own login record (`Dev/LoginStats`) for the contractor and his foreman before and after — unchanged.

**Exit — not met, and cannot be by a build.** §11 test 2 needs the contractor and Peter. The artefact:
`tools/two-surface-day.ps1` imports the real export into a project only Peter is on, re-joins the footage and
writes one page for one day — the Site Diary on the left (every message and photo, true authorship), the brief
on the right (the three leading blocks, every frame with a *drop* box, the truth floor), and two blank boxes
for their answers — **to a path outside the repository** (it refuses one inside). Built for 22 Sep from the
real export: 18 diary items, 12 frames embedded; the brief for that day has one block and two floor items,
because Peter's project has no stages — the curation model will be judged on a thin right-hand side unless
he sets up his stages first.

`tools/e2e-p9-contractor.sh`: **90 assertions, 0 failures** (fixtures from `tools/make-p9-fixtures.sh`:
25 drawn frames and a machine-spoken voice note, nothing real). `tools/e2e-all.sh`: **860, 0 failures**, every
earlier suite's count unchanged. **Changed for P9 in earlier suites:** P2's capture frames now use their own
picture, because captures are stored as artifacts and the upload test after it must start from unseen bytes.
In the headless browser (Musa, Nalan, Peter; 360 / 768 / 1280, light and dark): three taps post five frames;
offline, the post is kept on the phone and the strip says so; back online it drains; Nalan drops a frame the
rule chose and another takes its place; the claim form pre-selects frames; Peter's claim row shows its proof.
No console errors, every stylesheet 200, no horizontal overflow (the native file input showing through CSS
isolation and a post button overflowing at 360 were found and fixed).

**Outstanding.**
- **§11 test 2** — show the page to the contractor and to Peter.
- **Push has never reached a real phone.** Headless Chromium has no push service and reports notification
  permission as denied, so the opt-in was seen only in its blocked state and delivery only through the dev
  sink. Try it on Android Chrome over HTTPS on a real host; iOS needs the app added to the home screen.
- Offline sync needs the app open; no service-worker Background Sync, and the outbox is per browser.
- Transcription is Windows' dictation engine: rough (it heard "steel team" as "deal team"), en-GB only
  (`Transcription:Culture`), Windows only. A better engine goes behind `IAudioTranscriber`.
- The curation rule is positional (first / latest / between). It does not yet use same-view pairing, which
  finds nothing on real footage anyway (P7).
- The cutoff publishes at `Brief:CutoffHour` server-local time, one hour for every project.
- Spoken decisions and queries do not push yet — only captures, claims and the day's crossing.
- Voice notes are delivery-side only by rule; there is no way to expose one.
- Each suite run adds a buyer (`peter.p9…@assetlen.test`) and a project to the dev database. The dev
  database also now holds *Two-surface test (real day)* under `peter.buyer@assetlen.test`, built from the real
  export — delete it if real content should not sit there. The demo *Kira Residence* holds captures and a claim
  from the browser walk.
- Carried over, unchanged: second-project funding; `GetFundingBy*` gating on read rather than `CanSeeMoney`;
  the 360 px accountability table and register switcher; the real place name in the dev seed's location and
  the personas' shared surname (the walk's screenshots show *Kira, Wakiso* — worth fixing in the seed).

---

### Full test pass after P4–P9 and the Works Report *(2026-09-29)*

A pass over everything built since P3, to lay the ground for the next plan. No new phase work.

**Build.** A clean `--no-incremental` rebuild of the whole solution, `assetlen.Maui` included (android and
windows heads; the workloads are installed): 0 errors.

**The chain was red on arrival — 2 of 860.** The P4 suite read 5M too much claimed on the demo's retaining
wall: two claims filed by hand during an earlier browser walk. Not a code fault in the ledger — the demo seed
only ever *added* its claims, so anything clicked through stayed forever. `DevSeedService.EnsureLedgerAsync`
now restores the demo ledger: claims it did not write are archived, its own are put back to their seeded
state. The same sweep now removes the capture and the question `e2e-p5-money-and-staging.sh` files on the
demo every run to prove nothing floats — 32 runs of them had left the demo's works report saying
*"46 blockers are open"*.

**Found in the browser walk** (Peter, Dinah, Nalan, Musa; 360 / 768 / 1280; light and dark; home, overview,
brief, register, money, search, history, live and issued report, markup on a client and a crew-only file,
diary, capture, drawings — 336 page loads):

- **Peter had no History tab — his front door.** `CanSeeHistory` required the Site Diary, so a client-side
  principal could not reach the import that P3 and Law 0 are built on, though the server was already letting
  him import and read his own side's thread. The rule is now *principals on either side*; the server's
  per-side filter (`IngestDAL.CanReadSide`) is unchanged, and the bench still does not get the tab.
- **A typed URL to a section the seat was not invited into rendered the section anyway** — Peter on
  `/capture` got the capture form with "Today's work didn't load"; the foreman on `/brief` or `/report` got
  an error state. Brief, report, drawings, history and capture had no page-level guard. `ProjectPage` now
  refuses any section whose tab the seat does not have, from the same tab table, with a page that names
  nothing behind it (no eyebrow, no subtitle). The Site Diary, tonight's curation and markup keep their own
  answers (`GateBySection="false"`).
- **Printing the report from a dark-mode device put near-white ink on white paper.** The print palette was
  set on `body`, but the dark palette lives on `:root`, and `index.html`'s splash rule (`html, body { color:
  var(--sp-text) }`) comes after `app.css` and wins. The print block now sets the light palette on `:root`,
  including `--al-text-strong` and the splash variables.
- **Search told the client side the Site Diary exists** ("…the Site Diary and stages were all searched").
  `SearchResultDto.SearchedSiteDiary` now says whether it was a source, and the footnote names it only then.
- **An uninvited reader's 404 logged as a console error.** 404 is the server's deliberate answer to a seat
  that was not invited; `ApiResponseHandler` now logs 403/404 as warnings.
- **An empty register sat above twenty-one waiting proposals.** After an import the register said *"Nothing
  agreed on record yet"* while the *From the thread* view — off-screen at 360 px — held everything extracted.
  The commitments view now says how many items wait and has a *Review* button into them.

**Fixed from the carried-over list:** `GetFundingByProject` / `GetFundingByStage` now gate on `CanSeeMoney`
(404, like the missing tab), not on project read. The foreman on the demo was already refused by his Crew
role; the hole was a support seat whose tenant role reads finance, now covered by a test.

**Held to account and clean:** every stylesheet 200 in all 24 browser contexts; no horizontal overflow at any
width; no console errors after the fixes; tabs exactly as the seats intend (client principals: no Capture, no
Site Diary; the foreman: no Brief, Report, Money, Register or History); a crew-only file opens as *File not
found* on the client side. Server-side, every crew-only artifact and diary id on the demo (85) was grepped
out of 13 responses each for Peter and Dinah — home, owed, brief, live and issued report, three searches,
register, thread, refs, project, ledger: **zero**. From Nalan's chair the same grep finds 35 in his brief,
so a zero means something. Print preview checked light and dark, live and issued.

**Law 0, in one sitting.** New suite `tools/e2e-law0.sh`, wired into `e2e-all.sh`: Peter opens a project
nobody else is on, imports the synthetic thread with the contractor mapped as a name only, clears the 21
proposals, and gets his home, a 19-item register, a brief marked *assembled without a curator* with the truth
floor on it, and a works report he issues himself, hash verified — with the server's login record showing the
delivery-side accounts unused. It also pins the seat rules above. Repeated in the browser: import through the
History tab, the brief fills, the register prompt leads to a bulk accept.

`tools/e2e-all.sh`: **900 assertions, 0 failures** across twelve suites; no earlier assertion changed.

**Outstanding.**
- **§11 tests 1–3 and R0 are still Peter's** (and test 2 the contractor's). Nothing here stands in for them.
- **The foreman can read his side's raw thread through the API** (`Ingest/GetBatch`, `GetMessages`), although
  the seat model says the raw thread is not part of the bench's job and search and the History tab already
  keep it from him. `e2e-p3-ingest.sh` asserts *"…and so can the foreman on that side"* — written before
  seats. Left as is rather than change an assertion without a decision: **decide whether the bench reads the
  raw thread**, then align `IngestDAL.CanReadSide` and that line.
- A refused section's breadcrumb still carries the section's name (it comes from the route map, not the page).
- The demo persona Peter mediates every project he created himself, so his search names the Site Diary; only a
  project with a named mediator shows the pure client view (Dinah's chair).
- The anonymity question on the seed: *Ssembatya*, *Kaggwa*, *Opio*, *Nabirye*, *Kira* and *Wakiso* were
  checked against the real export and **none appears in it** — they are invented, not leaked. *Kira, Wakiso*
  is still a real place; whether to keep a real town in the seed is the user's call.
- The pass left, in the dev database: a *Law 0 walk* project under the demo Peter (driven through the UI),
  a *Law0 probe* project and one project per `e2e-law0.sh` run under `peter.buyer@assetlen.test`, a
  `site.eng@assetlen.test` user, and an issued report on *Kira Residence*.
- Carried over, unchanged: second-project funding; the 360 px accountability table; the register switcher
  still scrolls at 360 px (the new prompt makes the waiting items reachable without it).

---

### Adversarial review of the uncommitted P4–P9 diff *(2026-09-29)*

A hunt through the whole working-tree diff for tenancy holes, side and seat leaks, inline access checks,
model-introduced numbers, mutation of the raw thread or of issued reports, migrations, real names and charter
breaches. Each candidate was tried against the running API before it was kept. The chain was green on arrival
(900/0).

**Confirmed and fixed.**
- **The bench's remarks crossed with the frames.** When a frame crossed — at the cutoff with nobody watching,
  or as claim evidence — its whole entry flipped to the client channel, and `ProgressDAL.MapUpdateToDto` handed
  the client side *every* comment on it, Crew-channel ones included, under the commenter's true name.
  Reproduced: the foreman's *"do not tell the client"* on a Diary entry reached Peter at 20:00. Now a
  client-side reader gets only client-channel comments and the client side's own, and a delivery-side author
  reads as the accountable face. A comment is stamped with its entry's channel when written, and a reader
  who cannot see a Diary entry gets 404 for commenting on it (he got 200 before, and the remark was
  broadcast to the crew).
- **The narrative validator let spelled-out figures through.** It checked digits, so *"Open three weeks"*
  passed against facts that said 12 days. Number words (*two … ninety, hundred, million, dozen, half, twice,
  fortnight, percent …*) now have to appear in the facts, as digits and month names already did.
- `tools/start-dev.ps1` could not start anything from this repo's path: `Start-Process` joins an argument
  array without quoting, and the path has spaces.

Five assertions added (P9: the remark stays in the Diary, the mediator still reads it, the blind comment
is refused; works report: the spelled-out figure is refused). All five failed on the old build.
`tools/e2e-all.sh`: **905 assertions, 0 failures**.

**Checked and clean.** Every new table has `TenantId` and a `TenantScoped` filter, and every row written with
no signed-in user either gets `TenantId` set explicitly or has a `ProjectId` for the owner stamp. Access goes
through `IProjectAccessService` (`ResolveUnscopedAsync` for jobs). `tbl_IngestedMessage` is written only by the
importer. There is no update or delete path on `tbl_WorksReport`. The drafting model sees facts only after
side filtering, and numbers come from the assembler. The three new migrations apply in order to an empty
database and the snapshot has no pending changes. No surname, bank or place from the real export appears
in the diff. Every new `.razor` has its `.razor.css`, and there are no literal colours outside `app.css`
tokens. `PlanTick` and `DeadlineStrip` are dots on a hairline, not bars. Dev endpoints return 404 outside
Development.

**Left for a decision.**
- **A crossed entry carries its note.** The capture's note (`Description`) crosses with its frames and reads
  in the brief and the report as the mediator's words, cutoff included. The curation page shows the note, so
  a mediator who opens it sees what will go. One who does not open it sends the bench's note without having
  read it. Decide whether an unattended cutoff should hold notes back.
- The bench can read its side's raw thread through the API. This is carried over from the full test pass.

### Postgres — the switch from SQL Server *(2026-09-29)*

The database is PostgreSQL 17 (`assetlen_dev` locally). The rules are in CLAUDE.md §5.1.1. No data was
carried over: the demo seed rebuilds the demo. The SQL Server database was left as it was, for the user to retire.

**What moved.**
- **`assetlen.Postgres/`** holds the migrations, with **one baseline** (`Pg_Baseline`) generated from the final
  model. The SQL Server history was not ported. Npgsql EF Core 10.0.0. Postgres is the only provider (see
  *Proven, and SQL Server removed* below).
- **Hangfire** moved 1.7.33 → 1.8.25 for **Hangfire.PostgreSql 1.20.13** (the 1.20 line needs Core 1.8). Its
  tables are in a `hangfire` schema, created by `EnsureHangfireSchema` after EF migrates. The `pull-changes`
  recurring job used to register before the schema existed, which on a fresh database spun on a missing
  `hangfire.lock` for about 17 s. It now registers after the schema, with the other recurring jobs.
- **Raw SQL removed.** The dev seed backdates with `ExecuteUpdateAsync`, `LogsDAL.SearchLogs` is LINQ, the
  `SERVERPROPERTY` full-text probe is gone, and `PushDAL`'s `DateDiffMillisecond` is computed in memory.
  `NEWID()` → `gen_random_uuid()::text`, and the `[ClientCaptureId]` index filter is written in Postgres quoting.
- **`ConfigDAL` backup / restore / schema script answer 501.** They are SQL Server's
  (`BACKUP DATABASE`, `SINGLE_USER`, T-SQL scripts). They belong to the legacy desktop mode, and `pg_dump` from
  inside the API process would need the client binaries on the server's path. **Back up with `pg_dump` outside
  the app.** A scheduled `pg_dump` is still to do for any real deployment.
- **Dates.** All columns are `timestamptz`. A model-wide converter labels values UTC on write without shifting
  them and returns them unlabelled, so the API's JSON is unchanged. The PowerShell `[datetime]` checks in the
  suites would have moved by three hours on a `Z` suffix. Wall-clock values (message sent time, shot time,
  report as-at, the brief's day) are carried unshifted. One column type keeps SQL comparisons independent of
  the session TimeZone. The one persisted `DateTime.Now` instant was `tbl_WorksReport.DeliveredAt`, now `UtcNow`.
  The deliberate wall-clock `DateTime.Now` uses (the brief's day, the cutoff, the report's as-at) are unchanged.
- **Case.** Everything is folded with `ToLower()` on both sides: login by email or username, register and
  username checks, invites by email, inbound-mail sender, the admin and user keyword filters, and the thread's
  `Search`. There is no citext and no nondeterministic collation, because the latter breaks `LIKE` before PG 18.
- **Search** runs `ILIKE`, then a `'simple'` `tsvector @@ tsquery` prefix match, then `pg_trgm`
  `word_similarity ≥ 0.6` for terms of six or more letters. Five trigram GIN indexes back it: message body,
  OCR text, commitment title and body, diary entry. `SearchTerms` scores with the same fuzzy rule. The server
  reports `backend: "full-text"`. `e2e-p6-search.sh` gained the assertion **ZENTAHA finds the ZENTARA receipt**.
- **Anonymised seed.** The demo project is now *Riverstone Residence*, *Riverstone Heights*, replacing the real
  town and district. `e2e-ux-personas.sh` follows it.

**The chain on Postgres, from an empty database: 906 passed, 0 failed, 0 skipped** (the 905 held on SQL
Server, plus the ZENTAHA assertion). The first run was 877/0 across
eleven suites. `e2e-p5-money-and-staging.sh` stopped with FATAL because it assumed an earlier run had seeded
the demo. It now calls the idempotent `Dev/SeedDemo` itself, like the P4 and P8 suites.

**Proven, and SQL Server removed** *(same day, second pass)*.
- **A clean machine works.** `assetlen_dev` was dropped and recreated empty, owned by the app login, with no
  superuser step. On boot the API applied `Pg_Baseline`, created `pg_trgm` itself (a trusted extension),
  built the `hangfire` schema, and the suites seeded the rest. The chain: **906 passed, 0 failed, 0 skipped**.
  No suite skipped anything, so no skip was introduced that SQL Server did not have.
- **Browser walk** (playwright-core, headless Chromium). Peter, Dinah, Nalan and Musa at 360 / 768 / 1280, light
  and dark, on 14 pages: home, overview, brief, register, money, search, history, the live report, an issued
  report, markup on a client file and on a crew file, the Site Diary, capture and drawings. That is 336 page
  loads. There were **no console errors**, no horizontal overflow and no error alerts, and every stylesheet
  (`app.css`, `assetlen.Client.styles.css`, the scoped bundle) came back 200. Each persona's tabs match the SQL
  Server walk exactly; only the renamed project title changed.
- **The actions, done through the pages.** Musa captured a frame from the capture page (`Progress/Capture` 200).
  Peter issued a report from the live report. The live and issued reports printed to PDF with no navigation
  showing and a white page in both colour schemes. Peter circled the vendor on a receipt and saved a layer
  (`Annotations/Save` 200), and Nalan saw it attributed to Peter. **Searching "zentaha" found the ZENTARA
  receipt** for the demo Peter, on a photo he forwarded into the demo project, and for the P6 suite's buyer,
  at 360 and 1280 in both colour schemes.
- **Removed:** the `assetlen.SqlServer` project and folder, and the empty `assetlen.Sqlite` folder (bin/obj only).
  Also gone: `Microsoft.EntityFrameworkCore.SqlServer` and `.Sqlite` from Service and API,
  `Hangfire.SqlServer`, and `System.Data.SqlClient` from Shared. `EntityFramework.DynamicFilters` went too: an
  EF6 package nothing used, which pulled in EF6 and `System.Data.SqlClient` 4.7.0, a version with a
  high-severity advisory. On the code side, the provider switch and the SQL Server branches are gone from
  `Program.cs`, the model's `IsSqlServer` / `IsSqlite` branches from `AssetlenDbContext`, the T-SQL backup /
  restore and `ExecuteSqlRaw` script path from `ConfigDAL` (it now answers 501 outright), and a stray
  `using Azure.Core` that had only compiled through the SQL client. `dotnet ef migrations
  has-pending-model-changes` reports no change, so the baseline still describes the model exactly.
- **Clean rebuild** of the whole solution with `--no-incremental`, after wiping bin/obj: API, client, and the
  MAUI Android (APK) and Windows heads. **0 errors.** The API output no longer carries any SQL Server or SQLite
  assembly. The chain was then **re-run from an empty database: 906 passed, 0 failed, 0 skipped**. A browser
  check as Peter and Musa afterwards was also clean.
- **Startup log** no longer prints the database password. The connection string is logged with
  `Password` stripped. The gitignored `appsettings.json` keeps only `ConnectionStrings:Postgres`.
- **Left as found:** on an empty database EF logs one `ERR Failed executing DbCommand` while it looks for
  `__EFMigrationsHistory` before creating it. It is harmless and happens once.

**Adversarial review of the move** *(same day, third pass)*. The whole diff was read against the running API,
and every GET in the Swagger document (98) was called as Peter and as the tenant admin. Three defects that only
Postgres has, and one older one beside them, all confirmed live before they were fixed:
- **User search 500'd.** `SearchUsersForComboBoxes` and `SearchForEmployees` filtered on
  `FirstName.ToString() == keywords`. SQL Server translated that; Npgsql cannot, so both lists threw. They now
  fold first names like every other field. The same pass found `SearchUserByKeywords` testing
  `IsNullOrEmpty` the wrong way round — a search for anyone returned everyone — and fixed it.
- **A capitalised inbound address bounced.** `in+<key>@…` resolved only in the exact case it was issued in;
  the old CI collation had hidden that. The key is lower-cased on the way in.
- **One NUL character failed a whole save.** Postgres `text` cannot hold U+0000 (SQL Server could), and it comes
  in with pasted PDF text, OCR output and forwarded mail. `SaveChanges` now strips it from every string.
- **Hangfire's jobs** were checked in `hangfire.job` rather than assumed: the cutoff, milestone, OCR and poster
  jobs all succeed on Postgres. The job logs carry no session-TimeZone-dependent SQL (`now()`, `date_trunc`,
  `::timestamp`) anywhere in a full chain.
- **`tools/e2e-postgres.sh`** (13 assertions, last in `e2e-all.sh`) pins each of these, plus mixed-case sign-in,
  the thread filter's case, a row written now reading back as now, and the cutoff and milestone runs called
  exactly as their jobs call them.
- **Correction to the second pass:** the `SyncDisabledException` failures are not `pull-changes` (it succeeds).
  They are `SyncDAL.ProcessSyncJobAsync`, which the legacy sync middleware enqueues for **every write request** —
  about 380 per chain — and whose stored arguments carry the request's headers (bearer tokens) and body (a
  registration's password included). It behaved the same on SQL Server. Not fixed here: turning the middleware
  off changes the desktop sync mode; it is flagged for a decision, and the backup excludes the `hangfire` schema.
- **`remote-db-setup.md` rewritten for Postgres:** a `postgres:17` container on `assetlen-net` only, admin port
  `127.0.0.1:5433` (clear of every FRELODY port, loopback-bound because Docker bypasses `ufw`), an init script
  that makes the app login the database owner (so it can create `pg_trgm` itself), one connection string,
  `TZ` on the API container, a nightly `pg_dump -Fc` plus an artifact mirror with 14-day retention, a restore
  procedure, and a restore drill in the verification steps.
- Still open: the in-app backup endpoints answer 501 by design; a real deployment's off-box backup target is the
  user's choice.

**The chain after the review, from an empty database: 919 passed, 0 failed, 0 skipped** (906 + the 13 new).

---

## Explicitly not building

| Cut | Source |
|---|---|
| Holding or moving money / escrow | §8 — funds route through three agents, two banks and a third party's account |
| **Any in-app informal channel** | §8 — cut harder under D3. WhatsApp keeps the conversation; we ingest it |
| **Voice notes as a launch item** | §8 — parity aimed at a contractor who may never log in. Tier 3 |
| Project-wide Gantt canvas, dependency networks, computed critical path (**retire `TimelineChart`**) | §8 — *"Peter thinks in stages, not networks"*. **Narrowed 2026-09-29:** holds are in — see below |
| Bills of quantities | §8 — his own BoQ was cut down twice for being too heavy |
| Accounting integrations | §8 — not the bottleneck |
| Roles beyond developer / representative / mediator / delivery | §8 — permissions complexity, no user value |
| Anything that adds a tap to capture | §8 — directly causes churn back to WhatsApp |
| Lookbook, 3D explorer, visual search | Absent from the vision |

### Reversed on 2026-08-12

| Previously cut | Now | Why |
|---|---|---|
| Multi-project portfolio dashboard | **Peter's home screen (P7)** | He pays, and he runs four workstreams. One project must work first; it must not be the only thing that ever works. |

### Narrowed on 2026-09-29

| Previously cut | Now | Why |
|---|---|---|
| Gantt charts and critical path | **Holds** ([works-report.md](works-report.md) §4.4): a wait placed in front of the activities a person chose, pushing only those, drawn as wait/work lanes grouped by stage, with a P50/P80 projection. Still cut: a project-wide network, inferred dependencies, a computed critical path. | The contractor reports the site's real rhythm as *wait for weeks, then finish in a day or two*. A pace forecast cannot represent it, and 30 Sep was missed in exactly that way. Peter's question — *"do you need more time and how long?"* — needs the waits made visible. |

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
2. Run **`bash tools/e2e-all.sh https://localhost:7264/api`** — every suite in one command, currently **860 assertions**.
   No row regresses. (`tools/e2e-access-audit.sh` is superseded: it still casts the
   contractor as project owner, which the buyer decision reverses.)
3. Update the phase table above; add rows for sub-phases.
4. Leave everything unstaged — the user commits.
