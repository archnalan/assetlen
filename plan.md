# ASSETLEN — Implementation Plan

**Living document.** Every phase commit updates this file. Status as of 2026-05-22.

---

## How to use this file (read first, every session)

You are likely a fresh Claude agent picking up where the last one left off. Before writing code:

1. **Re-read [CLAUDE.md](CLAUDE.md)** — the design charter is the source of truth for naming, aesthetic, multi-tenancy contract, role model, and module map.
2. **Re-read this file end-to-end.** Note what's done, what's in progress, and the upcoming phase boundary.
3. **Challenge the plan.** If the next phase no longer fits the vision (because the codebase has moved, requirements have shifted, or a better approach has emerged), say so before implementing. Propose a revision to this file in the same turn.
4. **Commit per phase.** Each phase ends with a green build, a single conventional-commit, and an update to the "Status" column below.
5. **Keep it terse.** This is a plan, not a journal. One line per decision. Move detail into CLAUDE.md or code comments only when it has long-term value.

---

## Phase status

| Phase | Title | Status | Commit |
|---|---|---|---|
| 0 | Strip POS scaffold, install ASSETLEN charter | Done | `efdfd66` |
| 0.5 | Rename `mowt.*` → `assetlen.*`, drop Web hosts | Done | `0be02be` |
| 0.6 | Rename DbContext, kill mowt refs, upgrade to .NET 10 | Done | `665e816` |
| 1.1 | Domain alignment: sub-projects, Flags, Channel, role × module matrix | Done | `182d5af` |
| 1.1.1 | Harmonize roles — 6 generic roles, list-based `UserRolesDto` | Done | `b77f547` |
| 1.2 | `tbl_ProjectMember`, module relocation, CSS retokening | Done | `2be565a` |
| 1.3 | Dashboard polish: ProjectCard carousel, shimmer, Breadcrumbs, sub-project rendering | Done | `8201805` |
| 1.4 | Project detail Breadcrumbs + sub-project create (one-level limit) | Done | `4238f75` |
| 1.5 | Member-add flow (`tbl_ProjectMember` UI: list/add/deactivate) | Done | `11e167d` |
| 1.5.1 | ProjectCreate aesthetic refresh + cover upload | Planned | — |
| 2.1a | Site Journal — Channel toggle on capture + entry feed pill | Done | `1f23372` |
| 2.1b | Site Journal — feed-side cards polish + photo lightbox | Done | `fcec0b8` |
| 2.1c | Site Journal — dedicated `/entry/{id}` detail route | Done | `3dbee35` |
| 2.2 | Flags — issue lifecycle + weekly nudge | Done | `284ad49` |
| 2.3 | Streams — SignalR chat tied to media, dual channels | Planned | — |
| 2.4 | Curated Client view — `ClientVisible` gating end-to-end | Done | `f0af240` |
| 3.1 | Timeline graph (expected vs actual layers) | Planned | — |
| 3.2 | Finance: receipts, budgets, projections, versioning | Planned | — |
| 4.1 | WhatsApp bridge | Planned | — |
| 4.2 | Visual search ("trench") | Planned | — |
| 4.3 | Hybrid media: Google Drive per contractor | Planned | — |
| 4.4 | 3D explorer | Planned | — |
| 4.5 | Lookbook templates + export | Planned | — |

---

## Phase 1.3 — Dashboard polish (done)

**What landed:**
- `Modules/Projects/Components/ProjectCard.razor` (+ co-located CSS) with 5s/400ms auto-sliding cover carousel, dot indicators, pause-on-hover, nest badge, and inline sub-project list.
- `Components/Navigation/Breadcrumbs.razor` (+ CSS) — name-based, `BreadcrumbItem(Name, Href?)` record.
- `ProjectCardDto` extended with `RecentImageUrls`, `ParentProjectId`, `SubProjectCount`, `SubProjects`.
- `ProjectDAL.GetPortfolioDashboard` now pulls the 3 most-recent progress updates + their top 3 images, prepends the project's own `CoverImageUrl`, dedupes, caps at 5. Top-level projects nest their sub-projects.
- Dashboard refactored: shimmer skeletons use `.al-shimmer` + `.al-ghost-card`; card rendering delegated to `<ProjectCard>`.
- `_Imports.razor`: added `Modules.Projects.Components` + `Components.Navigation` usings.

**Known follow-ups (Phase 1.4 candidates):**
- Breadcrumbs not yet wired into `ProjectsLayout` / `ProjectDetail`; do this once routing IDs are mappable to names.
- `MediaCarousel` may need promotion to `Components/Media/` when Journal also needs it.
- Sub-project creation flow still pending (Phase 1.4).

---

## Phase 1.4 — Breadcrumbs + sub-project create (done)

**What landed:**
- `ProjectDto` + `ProjectCreateDto` carry `ParentProjectId` (Create) and `ParentProjectName` + `SubProjects` (Detail).
- `ProjectDAL.CreateProject` enforces the **one-level nesting limit** — rejects a sub-project whose parent is itself a sub-project. Sub-projects share their parent's subscription (no new free-quota slot).
- `ProjectDAL.GetProjectById` now `Include`s `ParentProject` + `SubProjects`, and authorization extends to the parent's owner/PM so sub-projects inherit access.
- `ProjectDetail.razor`: Breadcrumbs (`Portfolio / <Parent>? / <Project>`) at the top, replacing the legacy back link. Sub-projects panel in Overview tab with inline "Add Sub-project" form (only on top-level projects). UI label "Project Manager" → "Manager".
- New co-located `ProjectDetail.razor.css` for sub-projects panel + form input styles, using only `--al-*` tokens. Sets the per-page CSS-isolation precedent.
- No schema change — `ParentProjectId` column already shipped in `InitialAssetlen`.

## Phase 1.5 — Member-add flow (done)

**What landed:**
- `ProjectMemberDto` + `ProjectMemberCreateDto` (email-or-userId, specialization, optional title).
- `IProjectMemberDAL` + `ProjectMemberDAL` with `AddMember` / `GetMembersByProject` / `DeactivateMember`. Auth scoped to project owner or PM (parent's if sub-project). Duplicate-membership re-activates instead of erroring.
- `ProjectMembersController` — POST/GET/DELETE, Contractor+Manager required for mutations.
- `IProjectMembersApi` Refit interface + WASM DI registration.
- `_Imports.razor` injects `_projectMembersApi`.
- `ProjectDetail.razor`: new **Team** tab; "+ Add Member" form (email + specialization dropdown + optional title); active/inactive member rows with remove action; first-letter fallback avatars.
- Scoped CSS additions in `ProjectDetail.razor.css` — `.project-team__*` using `--al-*` tokens; mobile reflow at 540px.
- No schema change — `tbl_ProjectMember` shipped in Phase 1.2's `AddProjectMembers` migration.

**Carried forward:**
- Live user-search autocomplete (currently uses raw email entry).
- Per-row Specialization edit without re-add.
- Showing member list inline on Overview tab (currently only in Team tab).

## Phase 1.5.1 — ProjectCreate refresh + cover upload (planned)

- `ProjectCreate.razor` aesthetic refresh.
- Cover upload pipeline (re-use ProgressImage path).

## Phase 2.1a — Channel-aware capture (done)

**What landed:**
- `ProgressUpdateDto` + `ProgressUpdateCreateDto` carry `Channel` (default Crew). `MapUpdateToDto` and `AddProgressUpdate` thread it through.
- `ProgressUpload.razor`: two-card segmented "Visibility" picker — Crew (default, fail-closed) vs Client (curated). Co-located `ProgressUpload.razor.css` for the toggle, using `--al-*` tokens.
- `ProjectDetail.razor` Site Journal feed: each entry now shows a Channel pill (lowercase color-coded). Issue badge stays alongside in a small meta stack.

**Carried forward (2.1c):**
- Approval flow that lets Manager promote a Crew-channel entry to Client (currently Channel is locked at create-time).
- Dedicated `/entry/{id}` route.

## Phase 2.1b — Journal cards polish + photo lightbox (done)

**What landed:**
- `Components/Media/PhotoLightbox.razor` (+ CSS) — cross-module primitive. Fullscreen overlay, backdrop blur, Esc/arrow-key navigation via `window.assetlen.lightbox` JS interop, body-scroll lock while open, caption + counter, mobile-safe layout.
- `Modules/Projects/Components/EntryPhotoPanel.razor` (+ CSS) — hero photo + horizontal thumbnail strip. Click hero opens lightbox at the active index; click a thumb swaps the hero. Hero uses `--al-aspect-card`; active thumb gets an accent ring.
- `ProjectDetail.razor` Site Journal feed now renders `<EntryPhotoPanel>` instead of the flat grid. `OpenImageModal` TODO removed.
- Subtle entry hover affordance in `projects.css` (`.project-progress-entry:hover` → `--al-border-strong`).
- Dead-strip: legacy duplicate `.project-progress-images` + `.project-progress-entry-author/avatar/meta/body/desc` CSS blocks removed (~75 lines).
- JS helper added in `assetlen.Client/wwwroot/scripts.js` (the loaded file) + mirrored in shared `scripts.js`.
- `_Imports.razor`: added `assetlen.Shared.Components.Media` usings.

**Carried forward:**
- Touch swipe gestures inside the lightbox (currently keyboard + click-only).
- Promote `EntryPhotoPanel` to `Components/Media/` once a second module (Streams? Lookbook?) needs the hero+strip pattern.

## Phase 2.1c — /entry/{id} detail route (done)

**What landed:**
- `IProgressDAL.GetProgressUpdate` + `SetChannel` (parent-aware stakeholder check; SetChannel locked to owner/PM).
- `ProgressController`: outer auth widened to all read-eligible roles; per-action gating; new `GET GetProgressUpdate` + `PUT SetChannel` endpoints.
- `IProgressApi`: `GetProgressUpdate` + `SetChannel`.
- `Modules/Journal/Pages/EntryDetail.razor` (+ CSS) at `/project/{projectId}/entry/{entryId}`. Breadcrumbs (Portfolio / Project / Site Journal / date), author meta, Channel pill + ApprovalStatus chip, `<EntryPhotoPanel>`, flat comment thread + compose box.
- `<AuthorizeView Roles="Contractor,Manager">` exposes "Publish to Client" / "Revert to Crew" toggle. Server enforces the role.
- `ProjectDetail` Site Journal feed entries get an "Open entry →" footer link.

## Phase 2.2 — Flag lifecycle (done)

**What landed:**
- `FlagDto / FlagCreateDto / FlagUpdateDto` added; `IFlagDAL + FlagDAL` with Add / Get / GetByProject (status filter) / GetByEntry / Update / Resolve / Nudge. Parent-aware stakeholder checks; resolve sets ResolvedBy + ResolvedDate, re-open clears them.
- `FlagsController` — POST/GET/PUT. AddFlag open to all stakeholder roles; mutations gated to Contractor/Manager (Crew may also Resolve).
- `IFlagsApi` (Refit) + WASM DI + `_Imports` injection.
- `Modules/Journal/Components/FlagRaiseForm.razor` (+ CSS) — reusable inline form. Used on the project Issues tab **and** on `EntryDetail` (auto-anchors to the entry).
- `Modules/Journal/Components/FlagCard.razor` (+ CSS) — severity-coded left border, status pill, days-open counter, last-nudged indicator, role-gated action row (Nudge / Mark in progress / Resolve).
- `ProjectDetail` "Issues" tab with status filter chips (Open / In progress / Resolved / All), raise form, FlagCard list. Optimistically inserts new flags and live-updates on action.
- `EntryDetail` surfaces flags anchored to that entry; raising defaults the anchor.

**Carried forward:**
- Background scheduler that actually fires the weekly nudge (Phase 4 hosted-service work). Currently the UI surfaces "Last nudged X ago" + a manual Nudge button.
- Assignee picker (only via API; UI defaults assignee to null).

## Phase 2.4 — Curated Client view (done)

**What landed:**
- `ITenantProvider.IsExternal()` — true when the caller's only roles are Client/Guest. Internal roles (Contractor/Manager/Crew/SystemAdmin) always win.
- `ProgressDAL` + `FlagDAL` now take `ITenantProvider`. Read paths (`GetProgressUpdate(s)`, `GetFlag(s)ByProject/ByEntry`) apply `Channel == Client` when caller is external. Single-entry fetch returns 404 (not 403) so existence isn't leaked.
- `FlagDAL.AddFlag` forces `Channel = Client` when the raiser is external — they cannot create Crew-only flags.

Defense-in-depth on top of the controller `[Authorize(Roles=…)]` gates and the existing 2.1c Manager "Publish to Client" toggle.

**Carried forward:**
- Stage-level Channel filter (no `Channel` field on `tbl_Stage` yet — out of scope until Timeline work in Phase 3.1).
- Per-image Channel override (images currently inherit parent entry's Channel).

## Phase 2 — Site Journal + Streams (in progress)

The collaboration core. Site Journal Entries with photos/captions, Flags raised on entries, Streams (SignalR) tied to media, dual Channel enforcement (Crew default, Client opt-in).

## Phase 3 — Timeline + Finance (planned)

Multi-layer timeline (expected baseline, revised baseline, actual). Finance with receipts, line-itemed budgets, projections vs actuals.

## Phase 4 — Integrations & Lookbook (planned)

WhatsApp bridge for entry capture, visual search, hybrid media (Google Drive per contractor), 3D explorer, Lookbook auto-curation with three templates.

---

## Update protocol

When you finish a phase:

1. Build green, no new errors.
2. Single commit, conventional message: `feat(phase X.Y): <one-line summary>`.
3. Update the row in the table above: status → Done, commit hash filled.
4. If you added or split a sub-phase, add a row.
5. If the next phase needs re-scoping based on what you learned, edit the phase description here in the same commit.
