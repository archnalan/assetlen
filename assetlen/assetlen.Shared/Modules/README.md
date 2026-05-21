# ASSETLEN Modules

Each module is a vertical slice — its own `Pages/`, `Components/`, and (later) `Services/` under `assetlen.Service`. Routes namespace under the module name (e.g. `/projects`, `/projects/{id}/journal`).

When a feature crosses two modules (e.g. a Stream attached to a Journal entry), the *owning* module hosts the component; the other one consumes it. Single source of truth. No duplication.

See [/CLAUDE.md](../../../../CLAUDE.md) for naming, aesthetic, and code conventions.

---

## Module map

| # | Module | Responsibility | Phase |
|---|---|---|---|
| 1 | **Identity** | Tenants (Contractor orgs), Users, Roles (`Contractor`, `Crew`, `Client`), Invitations, Auth. Multi-tenant RLS enforcement. | 0–1 |
| 2 | **Projects** | `Project`, `SubProject` (one-level nesting), members, project Dashboard, project detail shell, breadcrumbs, create-project flow. | 1 |
| 3 | **Journal** | Site Journal entries (point-and-shoot), Flags (issues with weekly nudges + archive), per-entry threads. **The collaboration core.** | 2 |
| 4 | **Streams** | Real-time chat (SignalR) attached to any item — Entry, image, doc, video, Project. Reactions, mentions, AI-image-modify. Dual channels: Client / Crew. | 2 |
| 5 | **Timeline** | Multi-layer expected-vs-actual graph. Hide/overlay layers. Stalled/resumed states. Exceedance warnings. Deadline countdowns. Uses `--al-blueprint`. | 3 |
| 6 | **Finance** | Receipt capture (point-and-shoot), budgets, projections, revisions. Tabular figures everywhere. | 3 |
| 7 | **Documents** | Drawings, strategy docs, version history with deprecation. Surfaces "current" doc, archives prior versions visibly. | 3 |
| 8 | **Lookbook** | Auto-generated portfolio. 3 templates contractor can fine-tune. Hide/show projects, edit captions, instant export. | 4 |
| 9 | **Search** | Global text search + visual search ("trench" filters media containing trenches). Same component, multiple entry points. | 4 |
| 10 | **Integrations** | WhatsApp bridge (import/export), Google Drive (per-contractor media store), 3D explorer embed, AWS later. | 4 |

---

## Cross-module shared primitives

Live in [assetlen.Shared/Components/](../Components/):

- `Navigation/` — Breadcrumbs (name-based), Header, SideNav, NavMenuSearch.
- `Media/` — MediaCarousel (auto-slide thumbnails), GhostFrame, EntryThumb, ImageReactionBar.
- `UI/` — Card, Badge, Button overrides if FluentUI primitives don't fit.

Promote here only when a primitive is used by 2+ modules. Otherwise it lives inside the module that owns it.

---

## Multi-tenancy contract

- Every persisted entity carries `TenantId` (the Contractor org).
- Server-side filters enforce tenant scope on **every** query — no client-side trust.
- Cross-tenant access is impossible except for system admin ops (not user-facing).
- A user belongs to exactly one Tenant.
- A Client user is a special role: their account belongs to the Contractor's Tenant, but their visibility is restricted to projects they're explicitly invited to + the `ClientVisible` flag on Entries/media.
