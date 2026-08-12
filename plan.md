# ASSETLEN — Implementation Plan

**Living document.** Rewritten 2026-08-12 against [assetlen.md](assetlen.md), [Peter.md](Peter.md) and [David.md](David.md).

---

## How to use this file (read first, every session)

1. **Read [assetlen.md](assetlen.md) first.** It is the product truth. [CLAUDE.md](CLAUDE.md) is the *engineering* charter — aesthetic, CSS, folder layout — and defers to assetlen.md wherever they disagree.
2. **Read this file end-to-end** before writing code.
3. **Apply the ship test to everything.** assetlen.md §7: *"Does it help someone hold a past commitment against present reality?"* If not, it does not ship. The §7 "Do not build" table is binding.
4. **Challenge the plan.** If a phase no longer fits, say so before implementing and propose the revision in the same turn.
5. **Never commit.** Per CLAUDE.md §0 the user commits manually. Leave the tree dirty.

---

## The one-line reframe

> **WhatsApp stores messages. Construction runs on commitments.**

What is built today is a competent generic construction-PM app: Project → Stage → ProgressUpdate → Flag → Budget. What Peter and David need is a **commitments register**. These are not the same product, and the gap is the plan.

---

## Audit — 2026-08-12

Run against a live API (`https://localhost:7264`) with three real users: David (Contractor, project owner), Peter (Client, active project member), Colin (Crew, active project member). Every row below is an observed HTTP response, not a code reading.

| # | Scenario | Vision objective | Result |
|---|---|---|---|
| 1 | David logs in, lists projects | baseline | **200** ✅ |
| 2 | David adds Peter + clerk as members | 3-user MVP | **200**, rows created ✅ |
| 3 | Clerk opens the project | clerk can work | **403 Access denied** ❌ |
| 4 | Clerk captures a photo | 3-tap capture | **403 Access denied** ❌ |
| 5 | Peter opens the project | Peter's daily loop | **403 Access denied** ❌ |
| 6 | Peter reads the Site Journal | evidence he can trust | **403 Access denied** ❌ |
| 7 | Peter reads the budget | money against progress | **403 Access denied** ❌ |
| 8 | Peter raises a flag | asking a question at all | **403 Access denied** ❌ |
| 9 | David publishes an entry to Client, Peter reads it | curated Client view | **403 Access denied** ❌ |
| 10 | Peter + clerk load the portfolio | — | **200, 1 card each** ⚠️ inconsistent with 3–9 |
| 11 | Same photo posted twice | Law 2, hash dedup | 2 entries, 2 copies ❌ |
| 12 | Photo bytes round-trip | media is the product | **corrupted on write** ❌ |
| 13 | `GET /Brief/Today` | Peter's one page | **404 — not built** |
| 14 | `GET /Search` | Peter's 4 retrievals | **404 — not built** |
| 15 | `GET /Commitments` | the one object | **404 — not built** |

### A1 — Project membership is decorative *(blocks everything)*

`ProjectDAL.GetPortfolioDashboard` ([ProjectDAL.cs:52](assetlen.Service/DbServices/ProjectDAL.cs)) includes `tbl_ProjectMembers` in its visibility query. Every other authorization path — `ProjectDAL.GetProjectById:275`, and the private `IsProjectStakeholder` duplicated in [ProgressDAL.cs:400](assetlen.Service/DbServices/ProgressDAL.cs), [FlagDAL.cs:264](assetlen.Service/DbServices/FlagDAL.cs), [BudgetDAL.cs:280](assetlen.Service/DbServices/BudgetDAL.cs) — checks **only** `InvestorId` / `ProjectManagerId`.

Consequence: a member sees the project card, then gets 403 on every route behind it. Only two user IDs on earth can use a project. **Peter and the clerk cannot exist.** Phase 1.5 shipped a members UI with no effect on access; Phase 2.4's "curated Client view" is unreachable because no client can read anything.

### A2 — Every uploaded photo is corrupted on write *(highest severity)*

[ProgressDAL.cs:92](assetlen.Service/DbServices/ProgressDAL.cs):

```csharp
img.Base64Image.TrimStart("data:image/jpeg;base64,".ToCharArray())
```

`TrimStart(char[])` strips *any* leading character in that set, and `/` is in it. JPEG base64 always begins `/9j/`. Verified against the DB: sent `/9j/4AAQSkZJ`, stored `9j/4AAQSkZJR`, and `Convert.FromBase64String` throws on the stored value. **No Site Journal photo has ever decoded.** Images are also stored as data-URIs inline in `tbl_ProgressImage.ImageUrl`, with the same string reused as the thumbnail.

### A3 — The clerk has no route to capture, by construction

`/project/{id}/progress/add` is linked from exactly one place: a quick action on `/pm` ([PMDashboard.razor:309](assetlen/assetlen.Shared/Modules/Projects/Pages/PMDashboard.razor)), which is `[Authorize(Roles = Manager,Contractor)]`. Crew cannot load the only page that links to capture. The mobile FAB points at `/project/create` — the rarest action in the product.

### A4 — Capture is a form, and the vision forbids forms

[ProgressUpload.razor](assetlen/assetlen.Shared/Modules/Projects/Pages/ProgressUpload.razor) requires stage select, completion-% slider, a mandatory description, a visibility choice and an issue checkbox before the photo. Roughly 8–9 interactions plus typing, against WhatsApp's 3. David.md failure condition: *"Posting a photo takes longer than it does in WhatsApp."*

### A5 — Peter's product does not exist yet

No Commitment, no deliverable checklist, no daily brief, no OCR, no search route, no markup layer, no parked ideas, no extraction, no provenance. Peter's four retrievals have nowhere to happen. `SearchProjects` matches project names only.

### A6 — Built features that the vision cuts

- `TimelineChart` (Phase 3.1) is a Gantt chart. assetlen.md §7: *"Gantt charts and critical path — Peter thinks in stages, not networks."*
- `/portfolio` is the landing page. §7: *"Multi-project portfolio dashboards — One project must work first."*
- Planned Phase 4 (Lookbook, 3D explorer, visual search) appears nowhere in the vision.

### A7 — Scaffold debt

36 POS `DbSet`s, ~24 POS controllers, ~150 POS DTOs, 37 unregistered-or-unused Refit clients, POS tables inside `InitialAssetlen`. `assetlen.Sqlite` has zero migrations. Orphaned UI: `AssetlenHeadedLayout` (Uganda MoWT), `SplashScreen`/`Register` still branded Billtrick, `MainLayout` footer links to billtrick.com and a dead `admin/dashboard`. Build is green (0 errors, 682 warnings); `RefundsController`/`CustomerDepositController` are `#if false` and harmless.

---

## Status of prior phases, re-marked against the vision

Phases 0–3 were graded against their own acceptance criteria and passed. Graded against Peter and David, most are partial.

| Phase | Title | Build status | Vision status |
|---|---|---|---|
| 0 – 0.6 | Strip POS, rename, .NET 10 | Done | Incomplete — see A7 |
| 1.1 – 1.2 | Domain, roles, `tbl_ProjectMember` | Done | Table exists, grants nothing — A1 |
| 1.3 – 1.4 | Dashboard, ProjectCard, Breadcrumbs | Done | Serves David; §7 cuts it as a landing page |
| 1.5 | Member-add flow | Done | **Cosmetic** — A1 |
| 1.5.1 | ProjectCreate refresh | Done | Fine |
| 2.1a–c | Site Journal, channels, entry detail | Done | **Photos corrupt** — A2, A3, A4 |
| 2.2 | Flags | Done | Peter cannot raise one — A1 |
| 2.3 | Streams (SignalR) | Done | Works; entry-rooted only |
| 2.4 | Curated Client view | Done | **Unreachable** — A1 |
| 3.1 | Timeline chart | Done | §7 cuts Gantt — A6 |
| 3.2 | Finance | Done | Client gets 403 despite the matrix — A1 |
| 3.3 | Accessibility / nav | Done | Fine |

---

## Phase plan

Ordered so that each phase is testable by a real person. Nothing after P0 can be evaluated until P0 lands.

| Phase | Title | Status |
|---|---|---|
| P0 | Unblock the three-user MVP | **Done** — 18/18 green |
| P1 | Scaffold strip + migration baseline | **Done** — 18/18 green from an empty server |
| P2 | Artifact store — one canonical file, hash-addressed | Planned |
| P3 | Commitment model + funded stages with deliverables | Planned |
| P4 | Dumb capture | Planned |
| P5 | Two surfaces — Site Log and Client Brief | Planned |
| P6 | Retrieval — OCR and unified search | Planned |
| P7 | Markup layers + query state | Planned |
| P8 | Extraction + parked ideas | Planned |
| P9 | Parity — notifications, voice, share sheet | Planned |

### P0 — Unblock the three-user MVP *(done)*

Nothing in the vision was testable until Peter and the clerk could use the app.

**What landed:**
- `ProjectAccessLevel` enum (`None < Read < Write < Manage`) in `RemoteSiteEnums.cs`.
- `IProjectAccessService` + `ProjectAccessService` — the single parent-aware resolver. Ownership → `Manage`; an active `tbl_ProjectMember` → `Write`, or `Read` when the specialization is `Observer`. Sub-projects inherit the parent's ownership *and* membership.
- The four private `IsProjectStakeholder` copies deleted. `ProjectDAL`, `ProgressDAL`, `FlagDAL`, `BudgetDAL`, `StageDAL` and `FundingDAL` now all route through the service; registered scoped in `Program.cs`.
- `BudgetDAL` read and manage finally diverge — `CanManageBudget` was aliased to the stakeholder check, so any stakeholder could edit the budget.
- `ProgressDAL.AddProgressUpdate` accepts any member with `Write`. The clerk of works can capture.
- Comments require `Write`, not ownership — this is how Peter asks a question at all.
- Funding reads, project analytics and `SearchProjects` opened to members. Peter's "money against progress" needs the funding endpoints, and search was returning less than the dashboard already showed.
- **A2 fixed.** `BuildImageUrl` strips a data-URI prefix by `IndexOf(',')` and preserves the declared content type. Verified: a `/9j/…` JPEG now round-trips byte-identical and decodes with the right magic bytes. The corrupt rows were purged — unrecoverable.
- Capture is reachable: the mobile bottom-nav centre button is context-aware (capture inside a project, create outside), and the Site Journal tab gained a desktop "Capture entry" action, role-gated to Contractor/Manager/Crew.

**Exit criterion met:** [tools/e2e-access-audit.sh](tools/e2e-access-audit.sh) — 18 assertions across the three personas, **18 passed**. Run it against a live API on the https profile after any auth change.

**Carried forward:**
- `StageDAL` create/update/delete and `FundingDAL` add/confirm still use inline ownership tests. Correct behaviour (owner/PM only) but not parent-aware; fold into `CanManageAsync` when P3 touches stages.
- The membership lookup is one extra query per authorization call. Fine at this scale; cache per-request if it shows up.

### P1 — Scaffold strip + migration baseline *(done)*

Done before Commitment work so every later phase moves through a small surface.

**What landed:**

- **Entities.** 36 POS entity classes deleted. 21 DbSets remain: 10 platform
  (tenant, config, log, sync log, role values, refresh tokens, verification
  codes, subscription request/seat, employee approval) and 11 ASSETLEN.
  `tbl_EmployeeApproval` was briefly deleted and restored — it backs the
  two-admin promotion flow in `UsersDAL`, which is Identity, not POS.
- **Query filters.** The 52 hand-copied `HasQueryFilter` lambdas collapsed to a
  single generic `TenantScoped<T>()` helper called once per entity. Adding a
  DbSet without its filter used to leak rows across tenants silently; the rule
  now lives in one place. `AssetlenDbContext` went 1201 → 415 lines.
- **Service + API.** 92 files deleted (30 DALs, 31 interfaces, 31 controllers)
  plus the Excel-import / file-upload / slip-printing stack. DI registrations in
  `assetlen.API/Program.cs` went from 44 to 12.
- **Seeding.** `InitialSeedDataDto` no longer carries POS reference data
  (categories, segments, suppliers, taxes, payment modes, cash denominations,
  order statuses) or ~40 till-behaviour settings. A tenant now starts empty
  except for 13 platform settings. `SeedSegmentsSupplierCategoriesTaxAsync` →
  `SeedTenantSettingsAsync`.
- **Removed two dangerous endpoints.** `ConfigDAL.DeleteAllFromSpecifiedTable`
  interpolated a caller-supplied table name straight into `DELETE FROM {table}`,
  and `ResetDataBaseTransactions` wiped a hard-coded list of POS tables. Neither
  had a caller.
- **Client.** 31 Refit interfaces deleted; the remaining 11 register through one
  `AddApi<T>()` helper (`assetlen.Client/Program.cs` 258 → 92 lines).
  `_Imports.razor` had been globally injecting every POS API into every
  component — with the endpoints gone that would have failed DI on every page.
- **UI.** Deleted `AssetlenHeadedLayout`, the Billtrick-branded `SplashScreen`
  and `Register`, three unreferenced product-search widgets, and the `/setup`
  desktop install wizard (SQL Express installer, LAN server discovery, kills a
  `Billtrickv2.API` process — none of it applies to a hosted SaaS). Stripped the
  dead 134-line Billtrick block from `EmptyLayout`, the POS splash from
  `MainLayout`, and 5 orphaned brand images. Fixed the dead `admin/dashboard`
  link and gave `Routes.razor` a styled 404 + not-authorized state with a
  sibling `.razor.css`.
- **`assetlen.Sqlite` removed** — zero migrations, zero code, and its only
  wiring was a commented-out `UseSqlite`. The `Database.IsSqlite()` branch stays
  in the DbContext so it can come back cheaply.
- **Migration baseline.** Dev DB dropped, 3 migrations replaced by one
  `20260812040220_InitialAssetlen`. 28 tables: 8 Identity, 20 ASSETLEN +
  platform.

**Two bugs this surfaced (both only visible on a genuinely empty server):**

- **Hangfire lost a startup race.** It builds its schema when the storage is
  constructed during service registration — before EF creates the database. The
  `HangFire.*` tables were never created, so capture and flag-raising returned
  500 (`Invalid object name 'HangFire.Job'`). Now `PrepareSchemaIfNecessary =
  false`, with an explicit `EnsureHangfireSchema()` after the migration. It
  clears the connection pool first: SqlClient caches the pre-database failure
  for several seconds and fails fast without contacting the server.
- **The seeded admin was unguessable.** `DatabaseSeeder` prefixed a random
  4-character GUID to the configured username *unconditionally*, so the
  credentials in `appsettings.json` never worked and you had to read the real
  email out of the database to log in. Replaced by `UserNameAllocator`, which
  uses the desired name verbatim and suffixes only on an actual collision.
  Applied to the Google signup path too.

**Exit criterion met.** Build green; 21 DbSets; `tbl_Product` and 15 other POS
tables absent from the schema; the P0 suite passes 18/18 against a database
created from nothing (empty SQL Server → migration → seed → run). Captured
images still round-trip byte-identical and decode with valid `FFD8` magic.

**Carried forward:**
- 16 `.razor` files still have no sibling `.razor.css`, which CLAUDE.md §4.1
  calls a bug. All pre-date P1.
- `tools/e2e-access-audit.sh` now seeds its own subject project, so it runs from
  zero. Its isolation fixture had a non-ASCII em dash in a JSON payload that
  made the API reject it with a 400 — invisible until the DB was empty.
- The `Configurations` enum in `statics.cs` still lists ~40 POS keys. Harmless
  (no schema), and some surviving UI reads them. Prune when P5 revisits settings.

### P2 — Artifact store *(assetlen.md Law 2)*

The foundation for everything Peter does. One upload, one permanent address, every later mention a pointer.

- `tbl_Artifact { Id, TenantId, ProjectId, Sha256, ByteSize, MimeType, StoragePath, OriginalFileName, UploadedById, CapturedAt, Width, Height }`, unique index on `(TenantId, Sha256)`.
- Multipart upload endpoint (not base64). Hash on arrival; an existing hash returns the existing artifact — *"this is already Receipt R-014."*
- Content-addressed storage under `wwwroot/artifacts/{sha[0..2]}/{sha}`, served statically. Blob/Drive swap later behind the same interface.
- Server-side thumbnail generation.
- `tbl_ProgressImage` becomes a pointer (`ArtifactId`) rather than an owner of bytes.

**Exit:** the same file uploaded twice yields one artifact and two references; audit row 11 goes green.

### P3 — Commitment model *(assetlen.md §3, the one object)*

- `tbl_Deliverable { StageId, Title, DisplayOrder, Status }` — 5–8 checklist items per funded stage.
- `tbl_Commitment { ProjectId, DeliverableId?, Kind (Spec|Price|Date|Material|Choice), Title, Body, Maturity (Idea|InDiscussion|Agreed|Delivered|Verified), QueryState (Cleared|QueryRaised|Resolved), AgreedById, AgreedAt, Amount?, Currency?, DueDate?, LeadTimeDays?, SupersedesId? }`.
- `tbl_CommitmentLink { CommitmentId, TargetType, TargetId, Relation }` — backlinks in both directions to artifacts, entries, comments, receipts.
- `tbl_Stage` gains `FundedAmount` / `FundedAt`.
- Provenance strip derived from links: *agreed → evidence → invoiced → cleared → queried → resolved*.
- Fold `tbl_Flag` into the model: a Flag becomes a Commitment in `QueryRaised`, or a blocker, rather than a parallel concept.

**Exit:** assetlen.md §9 test 1 — hand-enter one finished stage's ~20 commitments from a real WhatsApp thread and show Peter the page.

### P4 — Dumb capture *(David's adoption condition)*

- `/capture` reachable in one tap from anywhere. The screen is today's active deliverables as large tap targets. Tap one, shoot, done.
- No caption, no percentage, no channel, no form. Concurrent crews = three destinations, not three threads.
- Offline queue (IndexedDB) with background sync; posting must survive a bad site connection.
- Everything lands in the Site Log, never sanitised.

**Exit:** measured three interactions from cold app to posted photo, and a post that completes with the API stopped.

### P5 — Two surfaces *(assetlen.md §5)*

- **Site Log** — David's complete operational record. Internal only, grouped by deliverable.
- **Client Brief** — `tbl_Brief` + `tbl_BriefBlock` (one block per deliverable), auto-drafted from the day's captures with the best two or three frames and a progress line.
- David edits by exception: swipe to drop, tap to promote, one voice note per block becomes the narrative.
- **Publishes at the cutoff whether or not he touches it.** Hangfire is already registered ([Program.cs:123](assetlen.API/Program.cs)) — use a recurring job.
- **Truth floor:** commitments that move money, move a date, change an agreed spec, or are blockers / decisions Peter owes are injected into the brief regardless of curation and cannot be dropped. State the rule to both parties once, in plain language.
- `/today` becomes Peter's landing page. `/portfolio` is demoted to David's surface.

**Exit:** assetlen.md §9 test 2 — hand-build one real site day; David says *"I'd have dropped two of those"*, Peter says *"this is what I wanted."*

### P6 — Retrieval *(Peter's four searches)*

- OCR every artifact on ingest via a Hangfire queue → `tbl_ArtifactText`.
- Unified search over commitments, artifact text, entries and comments, using SQL Server full-text.
- `/search` with results grouped by object type, and a "what did I approve on X?" shape rather than a message list.

**Exit:** a receipt that only ever existed as a photo is findable by its vendor name.

### P7 — Markup layers + query state

- `tbl_Annotation { ArtifactId, Version, AuthorId, ShapesJson, CreatedAt }` — a versioned, attributed layer over the original. Never a new image.
- Raise a query on a cleared commitment; resolving it **writes back into the commitment** (*"revised to 4 bags, +£X, agreed 6 Aug"*), not into a message.

**Exit:** Peter circles a line on a receipt, asks, David answers, and the commitment value changes.

### P8 — Extraction + parked ideas *(Laws 3 and 4)*

- Propose structure only on money, materials, dates and decisions. One tap to confirm. Instrument the confirmation count — if David is confirming more than a handful a day, narrow the trigger.
- Ideas parked against a future stage accumulate references and estimates silently.
- Lead times compute a "decide by" date backwards from stage start; surface **only** when waiting costs something. Everything else stays silent.
- Stage kickoff brief hands parked items back.

**Exit:** assetlen.md §9 test 3 — the bad-week test. One live project, three weeks, David survives a bad week without drifting back to WhatsApp.

### P9 — Parity *(assetlen.md §8, price of entry)*

Web push at WhatsApp-comparable speed; voice notes with transcription; share-sheet target so a photo can be sent from the camera roll or from WhatsApp itself; a deliberately second-class informal channel with **"file this to an item"** on every message.

---

## Explicitly not building

Each line cites the clause that cuts it. Do not re-propose without amending assetlen.md.

| Cut | Source |
|---|---|
| Holding or moving money / escrow | §7 — *"will consume the entire runway"* |
| Gantt charts and critical path (**retire `TimelineChart`**) | §7 — *"Peter thinks in stages, not networks"* |
| Bills of quantities | §7 — *"reintroduces the machinery he is avoiding"* |
| Accounting integrations | §7 — *"not the bottleneck"* |
| Multi-project dashboard as the primary surface | §7 — *"one project must work first"* |
| Threaded general chat as a primary surface | §7 — *"rebuilds the problem"* |
| Anything that adds a tap to capture | §7 — *"directly causes churn back to WhatsApp"* |
| Lookbook, 3D explorer, visual search | Absent from the vision; fails the §7 ship test |

**Open tension, flagged not resolved:** Peter.md says Peter runs "two or three projects" at once, while §7 cuts multi-project dashboards. P5 demotes `/portfolio` rather than deleting it. Revisit after the bad-week test.

---

## Update protocol

1. Build green, no new errors.
2. Re-run the P0 audit script; no row regresses.
3. Update the phase table above; add rows for sub-phases.
4. Leave everything unstaged — the user commits.
