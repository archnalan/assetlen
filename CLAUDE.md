# ASSETLEN — Design & Engineering Charter

**Status (2026-05-21):** Phase 0 — Foundation. The old `mowt` POS scaffold is being stripped; the project-management vision drives every UI/architecture decision from here forward.

This file is the single source of truth for product naming, aesthetic, multi-tenancy contract, and module map. Future-Claude reads it first.

---

## 1. Product naming (canonical)

Use these terms everywhere — UI copy, file names, type names, route segments.

| Concept | Canonical term | Rationale |
|---|---|---|
| The app | **ASSETLEN** | Hero name. Display in display-serif. |
| Top-level container | **Project** | A residential / commercial development. |
| Nested container | **Sub-project** | One level of nesting only (e.g. a guest wing). |
| Daily site captures | **Site Journal** | Replaces "daily logs". The journal is *the* collaboration surface. |
| Single journal capture | **Entry** | Photo + caption + thread. |
| Chat tied to a media item | **Stream** | A live conversation rooted at an Entry, image, doc, or video. |
| Issue tracker item | **Flag** | A site issue raised on an Entry until resolved. |
| Timeline visualization | **Timeline** | Multi-layer expected-vs-actual graph. |
| Portfolio generator | **Lookbook** | Auto-curated showcase, 3 templates. |
| The two chat channels | **Client channel** / **Crew channel** | "Crew" feels right for the internal contractor team. |
| Client-only filtered view | **Client view** | A curated slice the contractor publishes. |

If you propose to rename one of these, update this table in the same PR.

---

## 2. Aesthetic — Architectural Calm

ASSETLEN looks like a well-printed architectural folio: warm paper, hairline rules, generous whitespace, confident type. **Not** a SaaS dashboard, **not** a construction-yellow site app. Media is the hero — UI chrome recedes around it.

### 2.1 Palette

Warm-neutral graphite + a single bold terracotta accent + a structural blueprint blue reserved for the Timeline. Dark mode is first-class (architects review at night).

| Token | Light | Dark | Use |
|---|---|---|---|
| `--al-bg` | `#fafaf7` | `#15171a` | Page background. |
| `--al-bg-elevated` | `#ffffff` | `#1c1f23` | Cards, modals. |
| `--al-surface` | `#ffffff` | `#1f2227` | Inputs, raised surfaces. |
| `--al-surface-2` | `#f4f3ee` | `#272a30` | Subtle fills, hover. |
| `--al-text` | `#1a1d21` | `#f0eee8` | Body text. |
| `--al-text-muted` | `#6b6f76` | `#a5a8af` | Secondary text. |
| `--al-text-subtle` | `#9ca0a7` | `#6e7178` | Captions, metadata. |
| `--al-border` | `rgba(26,29,33,0.08)` | `rgba(255,255,255,0.08)` | Hairline dividers. |
| `--al-border-strong` | `rgba(26,29,33,0.16)` | `rgba(255,255,255,0.14)` | Focus, emphasis. |
| `--al-accent` | `#c2542a` | `#d66948` | Primary action. Use sparingly. |
| `--al-accent-soft` | `#f3dcd0` | `#3a2620` | Accent-tinted backgrounds. |
| `--al-blueprint` | `#2f5d8c` | `#5288c0` | **Timeline only.** |
| `--al-success` | `#4a7c3a` | `#73a85e` | Resolved, on-track. |
| `--al-warning` | `#c69430` | `#e0b34a` | Stalled, approaching deadline. |
| `--al-danger` | `#b54545` | `#d46060` | Overdue, destructive. |

**Rule:** Never hard-code a color in a `.razor.css` file. Always go through a token.

### 2.2 Typography

| Token | Stack | Use |
|---|---|---|
| `--al-font-display` | `"Fraunces", "Inter Tight", "Segoe UI Variable", serif` | Project names, dashboard headings, Lookbook covers. |
| `--al-font-body` | `"Inter", "Segoe UI Variable", "Segoe UI", sans-serif` | Everything else. |
| `--al-font-mono` | `ui-monospace, "JetBrains Mono", Menlo, monospace` | Numerals in financial tables (tabular figures). |

Display sizes climb on a 1.25 ratio: `xs 12 / sm 14 / base 16 / md 18 / lg 22 / xl 28 / 2xl 36`.

Use tabular numerals (`font-variant-numeric: tabular-nums`) for all money, dates, and timeline values.

### 2.3 Rhythm, radii, elevation

- **4px grid.** Spacing tokens `--al-space-1` (4) through `--al-space-8` (64).
- **Radii:** `--al-radius-xs` 4 / `-sm` 6 / `--al-radius` 10 / `-lg` 16 / `-pill` 999.
- **Elevation:** Almost flat. Prefer 1px borders over shadows. Reserve `--al-shadow` for modals, popovers, drag previews.

### 2.4 Motion

One curve, three durations.

- Easing: `--al-ease: cubic-bezier(0.2, 0, 0, 1)` — "architectural pull" (heavy ease-out).
- `--al-transition-fast: 120ms` — hover, focus, dot indicators.
- `--al-transition: 180ms` — most state changes.
- `--al-transition-slow: 320ms` — modal/drawer enter, accordion expand.

Carousel auto-slide: **5000ms** dwell, **400ms** cross-fade. Pause on hover and on visibility loss.

### 2.5 Imagery & media

- Cards are mostly image with text overlay; chrome is minimal.
- Aspect ratios: `--al-aspect-card 16/10` (dashboard project cards), `--al-aspect-thumb 3/2` (entry thumbnails), `--al-aspect-hero 21/9` (Lookbook headers). **No squares** — architectural work isn't square.
- **Always** render a ghost-shimmer placeholder while remote media loads. Use the `.al-shimmer` utility (defined in `app.css`).
- Lazy-load below the fold; eagerly load only the first dashboard card.

---

## 3. Layout & navigation

- **Mobile first.** Every component must work at 360px. Test at 360, 768, 1280.
- **Breadcrumbs use names, never IDs.** Pattern: `Riverstone Heights / Guest Wing / Site Journal / 2026-05-21`. Each segment is a clickable link. Component lives at `Components/Navigation/Breadcrumbs.razor`.
- **Global search** is a single input in the header. Visual search ("trench") is a tab/toggle inside the search results.
- **Same feature, multiple entry points.** Each feature is a single Razor component reused wherever it's needed — never duplicated.

---

## 4. Code conventions

### 4.1 CSS

- Component styles live in a co-located `Foo.razor.css`. No global selectors there.
- Shared tokens, resets, utilities, and keyframes live in [assetlen/assetlen.Shared/Styles/app.css](assetlen/assetlen.Shared/Styles/app.css). When you find duplication across two `.razor.css` files, promote it to `app.css`.
- Class naming: kebab-case, component-prefixed. `al-card`, `al-card__title`, `al-card--featured`. The `al-` prefix is mandatory for shared utilities; per-component classes use the component name (e.g. `project-card`, `project-card__cover`).
- Never write a literal color, font-family, radius, or duration. Use a token.

### 4.2 Razor

- One feature = one Razor component. Don't duplicate logic across pages — extract into `Components/`.
- Loading, empty, and error states are first-class — every component that fetches data must render all three. Use `.al-shimmer` for loading, an icon + caption for empty, an inline alert for error.
- Parameters are typed; avoid `object` and `dynamic`.
- Use `@inject` for services; never call APIs directly from markup.

### 4.3 Folder structure — module-based vertical slices

```
assetlen/assetlen.Shared/
  Modules/                          ← see Modules/README.md for the module map
    Identity/                       tenants, users, roles, invites, auth
    Projects/                       Project, SubProject, dashboard, detail shell
    Journal/                        Site Journal entries, Flags
    Streams/                        real-time chat tied to media, dual channels
    Timeline/                       multi-layer expected-vs-actual graph
    Finance/                        receipts, budgets, projections
    Documents/                      drawings, strategy, version history
    Lookbook/                       portfolio templates, export
    Search/                         text + visual
    Integrations/                   WhatsApp, Drive, 3D explorer
  Components/                       cross-module shared primitives ONLY
    Navigation/                     Breadcrumbs (name-based), Header, SideNav
    Media/                          MediaCarousel, GhostFrame, EntryThumb
    UI/                             Card, Badge, Button overrides
  Styles/
    app.css                         tokens, resets, utilities, keyframes — ONLY
  Layout/                           MainLayout, header/footer shell
  Pages/                            top-level routes only (e.g. /, /login). Module routes live inside the module.
```

**Promotion rule:** A component lives in its owning module until **two** modules need it. Then it moves up to `Components/`. Don't pre-promote.

(Solution renamed `mowt.*` → `assetlen.*` in Phase 0.5; the WASM client is now `assetlen.Client`. DbContext renamed `mowtDbContext` → `AssetlenDbContext` in Phase 0.6.)

---

## 5. Architecture decisions

### 5.1 Multi-tenancy — SaaS, row-level isolation

- **Tenant = Contractor organization.** Many contractors share one deployment.
- Every persisted entity carries `TenantId`. **No exceptions.**
- All queries scope by `TenantId` server-side via a global EF query filter. Never trust the client.
- A User belongs to **exactly one Tenant**. Cross-tenant access doesn't exist (except system-admin ops, which are not user-facing).
- **Clients are a role inside the Contractor's Tenant** — not their own tenant. They see only projects they're explicitly invited to, filtered further by the `ClientVisible` flag.
- Domain models in Phase 1: `Tenant`, `User`, `TenantMembership { TenantId, UserId, Role: Contractor|Crew|Client }`, `Project { TenantId, ... }`, `ProjectMember { ProjectId, UserId, ClientVisible: bool }`.

### 5.2 Media — hybrid storage

- Thumbnails + current-thread media: local/cached on the server, served fast.
- Bulk: remote (Google Drive per-contractor in Phase 4, AWS later).
- Videos: always remote (YouTube embed Phase 4, AWS later).
- Render `.al-shimmer` ghost placeholders while remote media loads.

### 5.3 Real-time — SignalR

- One hub per Tenant. Groups: per-Project (presence), per-Stream (chat thread).
- Dual channels: every Stream message carries `Channel { Client, Crew }`. Server-side filter enforces; the client never sees Crew messages.
- Reconnect with last-seen cursor; back-fill missed messages on reconnect.

### 5.4 Curated client view

- Contractor explicitly marks Entries / media / timeline layers as `ClientVisible`.
- **Default is Crew-only** (fail-closed). A new upload is private until the contractor opts in.
- The client app uses the same routes and components as the contractor app; visibility is filtered server-side. No separate client codebase.

### 5.5 Auth — roles × module-permission matrix

Authorization runs on two layers:

**Layer 1 — ASP.NET Core `[Authorize(Roles = ...)]`** for coarse gates (routes, controllers). Roles live in [statics.cs](assetlen.Shared.Models/statics/statics.cs) → `UserRoles`.

**Layer 2 — Module access matrix** for fine-grained section gating (UI + service-layer checks). Source of truth: [RolePermissions.cs](assetlen.Shared.Models/Models/Authorization/RolePermissions.cs). Call:
```csharp
if (!RolePermissions.HasAccess(userRoles, AppModule.Finance, ModuleAccess.Read))
    return Forbid();
```

#### Roles

| Role | Scope | Purpose |
|---|---|---|
| `AssetlenSuperAdmin` | Platform | Cross-tenant operator. |
| `ViewSystemlog` | Platform | Audit read access. |
| `Contractor` | Tenant | Tenant owner. Full read/write across the org. |
| `ProjectLead` | Project | Runs specific projects; sees their financials. |
| `Foreman` | Project | Site supervisor; ops only, no financials. |
| `Inspector` | Project | Quality/safety; raises Flags. |
| `Cameraman` | Project | Media uploader only. No finance, no flags. |
| `Crew` | Tenant | Generic internal team member. |
| `Subcontractor` | Project | External trade worker, scoped to assigned work. |
| `Client` | Project | Principal client; sees curated view + financials. |
| `ClientObserver` | Project | Read-only stakeholder; no financials. |

Project-level scoping (e.g. ProjectLead seeing only their projects) is enforced at the service layer via `tbl_ProjectMember` (Phase 1.2).

#### Module access matrix

`A` = Admin, `W` = Write, `R` = Read, blank = None.

| Role / Module | Identity | Projects | Journal | Streams | Timeline | Finance | Documents | Lookbook | Search | Integrations |
|---|---|---|---|---|---|---|---|---|---|---|
| **AssetlenSuperAdmin** | A | A | A | A | A | A | A | A | A | A |
| **ViewSystemlog** | R |   |   |   |   |   |   |   |   |   |
| **Contractor** | A | A | A | A | A | A | A | A | A | A |
| **ProjectLead** | R | W | A | A | W | R | W | W | R | R |
| **Foreman** | R | R | W | W | R |   | R |   | R |   |
| **Inspector** | R | R | W | R | R |   | R |   | R |   |
| **Cameraman** | R | R | W | R |   |   |   |   |   |   |
| **Crew** | R | R | W | W | R |   | R |   | R |   |
| **Subcontractor** | R | R | W | R |   |   | R |   |   |   |
| **Client** | R | R | R | W | R | R | R | R |   |   |
| **ClientObserver** | R | R | R | R | R |   | R | R |   |   |

The matrix is the **only** place these decisions live. Don't hard-code role checks scattered across controllers — call `HasAccess()`.

---

## 6. Phase plan

- **Phase 0 (current):** Strip POS, write this doc, lay the token layer. Rename solution after the strip is done.
- **Phase 1:** Domain models (Project / SubProject / Member), Dashboard with ProjectCard + MediaCarousel, Project detail shell with Breadcrumbs, create-project flow.
- **Phase 2:** Site Journal, Flags (issues), Streams (chat), dual channels, curated Client view. *This is the collaboration core.*
- **Phase 3:** Timeline graph, Financial module (receipts, budgets, versioning).
- **Phase 4:** WhatsApp bridge, visual search, hybrid media storage with Google Drive, 3D explorer, Lookbook templates.

Each phase ends when the feature is usable on mobile + desktop, has loading/empty/error states, and is wired into Search and Breadcrumbs.

---

## 7. Phase 0 cleanup — DONE (2026-05-21)

The following POS subtrees were removed (147 .razor files). Don't re-introduce them, don't grep for them as references.

- `assetlen/assetlen.Shared/Pages/BillingModule/` ✓
- `assetlen/assetlen.Shared/Pages/OrdersModule/` ✓
- `assetlen/assetlen.Shared/Pages/DiscoveryModule/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/Products/` (entire subtree) ✓
- `assetlen/assetlen.Shared/Pages/Admin/Customers/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/Suppliers/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/ReceiveProducts(GRN)/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/CustomerBasedPricing/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/Transactions/` ✓
- `assetlen/assetlen.Shared/Pages/Admin/Settings/` (entire subtree — POS-only) ✓
- `assetlen/assetlen.Shared/Components/Feedback/` (POS product feedback) ✓
- Loose POS/boilerplate: `Chat.razor`, `Notification.razor`, `Counter.razor`, `Weather.razor`, `Button.razor` ✓
- `assetlen.Client/Pages/Counter.razor`, `Weather.razor` ✓

**Still to triage (don't delete yet — may reference deleted types):**

- `assetlen.Shared.Models/` — DTOs may include POS types (`ProductDto`, `CustomerDto`, `SaleDto`, etc.). Audit + delete in Phase 1 when domain models for ASSETLEN are introduced.
- `assetlen.Service/` — services for the POS domain. Same.
- `assetlen.API/Controllers/` — POS endpoints. Same.
- `assetlen.Sqlite/` + `assetlen.SqlServer/` — EF DbContext + migrations likely have POS entities. Will need fresh migration set in Phase 1.
- `assetlen/assetlen.Shared/Pages/Components/` — has some keepers (`RouteTracker`, `NavMenuSearch`, `SearchInput`, `SearchListMini`, `SetupWizard/`, `Startup/`). Will refactor into `Components/` + `Modules/Identity/` in Phase 1, not delete.

**Preserved:** project structure, FluentUI dependency, `CustomAuthStateProvider`, `RouteTracker`, `NavMenuSearch`, `Layout/MainLayout.razor` (will be re-skinned in Phase 1), Sqlite/SqlServer EF providers, `assetlen.Shared/Pages/Error.razor`.

**Module scaffolding created** under `assetlen/assetlen.Shared/Modules/` — see [assetlen/assetlen.Shared/Modules/README.md](assetlen/assetlen.Shared/Modules/README.md) for the full module map.
