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
| 1.4 | Project detail Breadcrumbs + sub-project create (one-level limit) | Done | _pending_ |
| 1.5 | ProjectCreate aesthetic refresh + Member-add flow (`tbl_ProjectMember` UI) | Planned | — |
| 2.1 | Site Journal — Entry capture, list, detail | Planned | — |
| 2.2 | Flags — issue lifecycle + weekly nudge | Planned | — |
| 2.3 | Streams — SignalR chat tied to media, dual channels | Planned | — |
| 2.4 | Curated Client view — `ClientVisible` gating end-to-end | Planned | — |
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

## Phase 1.5 — Member + Create flow refresh (planned)

- `ProjectCreate.razor` aesthetic refresh (currently functional but pre-token).
- Cover upload pipeline (re-use ProgressImage upload path).
- Member-add flow on `ProjectDetail`: pick a User, set `ProjectMemberSpecialization`, write `tbl_ProjectMember`.

## Phase 2 — Site Journal + Streams (planned)

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
