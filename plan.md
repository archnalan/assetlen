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
| 1.3 | Dashboard polish: ProjectCard carousel, shimmer, Breadcrumbs, sub-project rendering | In progress | — |
| 1.4 | Project detail shell + create-project flow refresh | Planned | — |
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

## Phase 1.3 — Dashboard polish (active)

**Goal:** Make the Projects dashboard feel like ASSETLEN — architectural calm, media-first, name-based navigation.

**Scope:**
- `ProjectCard` component (in `Modules/Projects/Components/`) with auto-sliding cover carousel: 5000ms dwell, 400ms cross-fade, pause on hover + visibility loss.
- Ghost-shimmer placeholder utility wired into the card while covers load. Promote `.al-shimmer` keyframe to `app.css` if not already there.
- `Breadcrumbs.razor` in `Components/Navigation/` — name-based, no IDs in URL labels. Pattern: `Riverstone Heights / Guest Wing / Site Journal / 2026-05-21`.
- Render sub-projects on parent cards (visible one-level nesting). Sub-project count badge + "open" affordance.
- Verify aspect ratios use `--al-aspect-card` (16/10).

**Exit criteria:**
- Dashboard renders cleanly at 360 / 768 / 1280.
- Loading, empty, error states present on every fetch.
- No hard-coded colors / radii / durations in any new `.razor.css`.
- Build green, single commit, this file updated.

---

## Phase 1.4 — Project detail shell + create-project (planned)

- Reskin `ProjectDetail.razor` against the token system; remove POS-era table chrome.
- Sub-project create flow (one-level limit enforced at service).
- `ProjectCreate.razor` validation + cover upload pipeline.
- Member-add flow using `tbl_ProjectMember` + `ProjectMemberSpecialization`.

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
