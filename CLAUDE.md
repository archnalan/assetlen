# ASSETLEN — Design & Engineering Charter

**Status (2026-08-12):** Phase P0 — unblocking the three-user MVP. See [plan.md](plan.md).

> **Precedence.** [assetlen.md](assetlen.md) is the product truth, with [Peter.md](Peter.md) and [David.md](David.md) as the user truth. **This file is the engineering charter** — aesthetic, CSS, folder layout, multi-tenancy — and **defers to assetlen.md wherever they disagree.** Read assetlen.md first, then this file, then plan.md.
>
> Every feature must answer assetlen.md §7: *"Does it help someone hold a past commitment against present reality?"* If not, it does not ship. The §7 "Do not build" table is binding and is mirrored in plan.md.

---

## 0. Working agreement

- **Never run `git commit` or `git push`.** The user commits manually. You may stage with `git add` only when the user explicitly asks; otherwise leave the working tree dirty after a change and let the user inspect the diff. This applies even when "the phase is done" or "the build is green" — the call to commit is the user's.
- Update [plan.md](plan.md) when a phase completes, but **leave it unstaged** along with everything else. The user will batch all changes into a single commit on their side.
- Verify CSS reachability in the browser DevTools after touching anything under `wwwroot/` or any `.razor.css` (see §4.1). A green `dotnet build` does not prove the styles loaded.

---

## 1. Product naming (canonical)

Use these terms everywhere — UI copy, file names, type names, route segments.

| Concept | Canonical term | Rationale |
|---|---|---|
| The app | **ASSETLEN** | Hero name. Display in display-serif. |
| **The one object** | **Commitment** | A spec, price, date, material or choice, carrying who agreed, when, and the evidence. Everything else hangs off it. |
| Commitment maturity | **Idea → In discussion → Agreed → Delivered → Verified** | assetlen.md §3. |
| Commitment query state | **Cleared → Query raised → Resolved** | Cleared is not closed. Resolution updates *the item*, not a message. |
| Top-level container | **Project** | A residential / commercial development. |
| Nested container | **Sub-project** | One level of nesting only (e.g. a guest wing). |
| Funded unit of work | **Stage** | Peter funds a stage up front. Holds 5–8 **Deliverables**. |
| Stage checklist item | **Deliverable** | The thing capture is aimed at. Nothing floats. |
| A stored file | **Artifact** | Uploaded once, permanent address, hash-matched. Later mentions are pointers, never copies. |
| Markup on an artifact | **Annotation** | A versioned, attributed *layer* on the original. Never a new image. |
| David's full record | **Site Log** | Complete, unsanitised, internal. The clerk posts here without judgement. |
| Peter's daily page | **Client Brief** | One page per day, **grouped by deliverable, not by time**. Auto-drafted, publishes at the cutoff regardless. |
| Non-curatable facts | **Truth floor** | Money, dates, agreed specs, blockers and decisions Peter owes reach Peter regardless of curation. |
| The two channels | **Client channel** / **Crew channel** | David controls emphasis, not truth. |

**Retired terms** — do not reintroduce: *Site Journal* and *Entry* (→ Site Log / capture against a Deliverable), *Flag* (→ a Commitment in `QueryRaised`, or a blocker), *Lookbook*, *Client view* (→ Client Brief).

If you propose to rename one of these, update this table and assetlen.md in the same change.

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

- Component styles live in a co-located `Foo.razor.css`. No global selectors there. **Every `.razor` file must have its sibling `.razor.css`** — even if the file just declares the component-scoped class names. A missing sibling is a bug.
- Shared tokens, resets, utilities, and keyframes live in [assetlen/assetlen.Shared/wwwroot/app.css](assetlen/assetlen.Shared/wwwroot/app.css). **It MUST live under `wwwroot/`** — Razor Class Libraries only serve static assets from `wwwroot/`, never from arbitrary folders like `Styles/`. Files outside `wwwroot/` will silently 404 in the browser and every `var(--al-*)` token will resolve to nothing. The bundle is referenced as `<link href="_content/assetlen.Shared/app.css" />` from `assetlen.Client/wwwroot/index.html`.
- Component-scoped `.razor.css` files are bundled separately by Blazor CSS isolation and reach the browser via `assetlen.Client.styles.css` (which auto-imports the RCL's `*.bundle.scp.css`). You do **not** link them by hand — but they only work once `app.css` (the token source) is loading, since scoped rules reference `--al-*`.
- Class naming: kebab-case, component-prefixed. `al-card`, `al-card__title`, `al-card--featured`. The `al-` prefix is mandatory for shared utilities; per-component classes use the component name (e.g. `project-card`, `project-card__cover`).
- Never write a literal color, font-family, radius, or duration. Use a token.
- After adding or moving any global CSS, manually verify it's reachable in DevTools (Network tab → filter `.css` → confirm 200, not 404).

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
  wwwroot/
    app.css                         tokens, resets, utilities, keyframes — ONLY
                                    (MUST live here so the RCL serves it
                                    as /_content/assetlen.Shared/app.css)
  Layout/                           MainLayout, header/footer shell
  Pages/                            top-level routes only (e.g. /, /login). Module routes live inside the module.
```

**Promotion rule:** A component lives in its owning module until **two** modules need it. Then it moves up to `Components/`. Don't pre-promote.

**Module map vs. the vision.** The tree above predates the assetlen.md rewrite. `Lookbook/` is retired — do not build into it. `Journal/` becomes the **Site Log**, and gains siblings `Commitments/` (the one object, P3), `Artifacts/` (canonical files, P2) and `Brief/` (the Client Brief, P5). Rename as each phase lands rather than in one sweep.

(Solution renamed `mowt.*` → `assetlen.*` in Phase 0.5; the WASM client is now `assetlen.Client`. DbContext renamed `mowtDbContext` → `AssetlenDbContext` in Phase 0.6.)

---

## 5. Architecture decisions

### 5.1 Multi-tenancy — SaaS, row-level isolation

- **Tenant = Contractor organization.** Many contractors share one deployment.
- Every persisted entity carries `TenantId`. **No exceptions.**
- All queries scope by `TenantId` server-side via a global EF query filter. Never trust the client.
- A User belongs to **exactly one Tenant**. Cross-tenant access doesn't exist (except system-admin ops, which are not user-facing).
- **Clients are a role inside the Contractor's Tenant** — not their own tenant. They see only projects they're explicitly invited to, filtered further by the `ClientVisible` flag.
- Domain: `Tenant`, `User`, `Project { TenantId, ParentProjectId?, ... }` (one-level sub-project nesting via self-ref), `tbl_ProjectMember { ProjectId, UserId, Role }` (per-project specialization — Phase 1.2). Tenant-level roles in `UserRoles` enum (6 roles — see §5.5).

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

Two layers:

**Layer 1 — ASP.NET Core `[Authorize(Roles = ...)]`** for coarse route/controller gates. Roles in [statics.cs](assetlen.Shared.Models/statics/statics.cs) → `UserRoles`. Six generic roles only; specific job titles (foreman, photographer, subcontractor, inspector, etc.) are **per-project specializations** stored on `tbl_ProjectMember.Role` (Phase 1.2).

**Layer 2 — Module access matrix** for fine-grained UI + service-layer checks. Source of truth: [RolePermissions.cs](assetlen.Shared.Models/Models/Authorization/RolePermissions.cs).

```csharp
if (!RolePermissions.HasAccess(userRoles, AppModule.Finance, ModuleAccess.Read))
    return Forbid();
```

#### Roles (6)

| Role | Scope | Purpose |
|---|---|---|
| `SystemAdmin` | Platform | Cross-tenant operator. |
| `Contractor` | Tenant | Tenant owner. Full read/write across the org including finance. |
| `Manager` | Project | Runs projects; sees finance + timelines of their projects; publishes Client content. |
| `Crew` | Project | Internal operator (authors Journal entries, raises Flags, comments). **No financial visibility.** |
| `Client` | Project | External principal; sees curated Client-channel content + their project's finance. |
| `Guest` | Project | Read-only stakeholder; no finance. |

`UserRolesDto` exposes convenience accessors: `IsTenantAdmin` (Contractor || SystemAdmin), `CanSeeFinancials` (Contractor || Manager || Client || SystemAdmin), `IsInternal` (Contractor || Manager || Crew || SystemAdmin), `IsExternal` (Client || Guest).

#### Project-level access — `tbl_ProjectMember` is authoritative

**Roles say what a user may do. `tbl_ProjectMember` says which projects they may do it in.** Both must pass.

A user has access to a project if they are its owner, its manager, or an **active `tbl_ProjectMember`** row — resolved parent-aware so sub-projects inherit. This lives in **one** service (`IProjectAccessService`); every DAL calls it.

> **Never re-implement this check inline.** Until P0, four DALs each carried a private `IsProjectStakeholder` that tested only `InvestorId`/`ProjectManagerId`, so members were visible on the dashboard and got 403 everywhere else — the client and the clerk of works could not use the product at all. See plan.md finding A1. A private stakeholder helper in a DAL is a bug.

Against the vision, six roles is more than assetlen.md §7 asks for (*"no roles beyond developer / contractor / clerk"*). We keep the six deliberately — the cost is already paid and collapsing them touches every controller — but **do not add a seventh**, and do not let role checks substitute for the membership check.

#### Module access matrix

`A` = Admin, `W` = Write, `R` = Read, blank = None.

| Role / Module | Identity | Projects | Journal | Streams | Timeline | Finance | Documents | Lookbook | Search | Integrations |
|---|---|---|---|---|---|---|---|---|---|---|
| **SystemAdmin** | A | A | A | A | A | A | A | A | A | A |
| **Contractor** | A | A | A | A | A | A | A | A | A | A |
| **Manager** | R | W | A | A | W | R | W | W | R | R |
| **Crew** | R | R | W | W | R |   | R |   | R |   |
| **Client** | R | R | R | W | R | R | R | R |   |   |
| **Guest** | R | R | R | R | R |   | R | R |   |   |

The matrix is the **only** place these decisions live. Don't hard-code role checks scattered across controllers — call `HasAccess()`.

---

## 6. Phase plan

**The phase plan lives in [plan.md](plan.md).** It was rewritten on 2026-08-12 against assetlen.md after a live end-to-end audit; the old Phase 0–4 sequence (Lookbook, 3D explorer, visual search) is retired because those features fail the §7 ship test.

Current shape: **P0** unblock the three-user MVP → **P1** scaffold strip → **P2** Artifact store → **P3** Commitment model → **P4** dumb capture → **P5** Site Log / Client Brief → **P6** OCR + search → **P7** markup + query state → **P8** extraction + parked ideas → **P9** parity.

A phase ends when it is usable on mobile and desktop, has loading / empty / error states, and passes its exit criterion in plan.md. Phases P3, P5 and P8 are additionally gated by the three validation tests in assetlen.md §9.

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
