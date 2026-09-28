# ASSETLEN — remote SQL Server + API setup (agent prompt)

**What this file is.** A single self-contained prompt to hand to an agent with SSH access to
the VPS. It ends with the ASSETLEN APK talking to a real database over the internet.
Everything the agent needs is below — repo facts, port map, file contents, gotchas, and the
verification that proves it worked.

**Read this before handing it over.** §0 is context for *you*, not the agent. The prompt
itself starts at "PROMPT BEGINS".

---

## 0. Why this is not just "a database"

The APK cannot talk to SQL Server. It talks to `assetlen.API`, which talks to SQL Server.
Publishing port 1433 to the internet and putting `sa` credentials inside an APK would hand
every installer of that APK full write access to every tenant's record — and the record is
the product (assetlen.md Law 2). So the deliverable is a **two-tier stack**: SQL Server
reachable only inside the Docker network, and the API in front of it behind TLS.

```
Android APK  ──HTTPS/443──▶  Caddy  (api.assetlen.com, auto Let's Encrypt)
                                │
                                ▼
                       assetlen-api  :8080          ← container port
                                │                     host: 127.0.0.1:8091
                                ▼
                    assetlen-sqlserver :1433        ← never published publicly
                                                      host: 127.0.0.1:1434 (admin only)
```

Three things about the current repo make this more than a copy of FRELODY's setup:

1. **There is no Dockerfile in this repo.** FRELODY has four. The agent writes ASSETLEN's.
2. **The APK's API address is hard-coded to the Android emulator's loopback**
   (`https://10.0.2.2:7264` in `assetlen.Maui/MauiProgram.cs`). On a real phone that resolves
   to nothing. Part of this job is making it configurable at build time.
3. **The VPS already runs FRELODY**, which owns ports 80, 1433, 8080, 5080, 5000, 1025, 4317,
   4318, 16686 and 127.0.0.1:5001–5002. ASSETLEN must not collide with any of them.

---

## PROMPT BEGINS — copy everything below this line

---

You are setting up remote infrastructure for **ASSETLEN**, a .NET 10 construction-commitments
app. The goal: an Android APK installed on a real phone signs in and works against a database
on my VPS. Right now the APK points at an emulator loopback address and there is no
server-side deployment at all.

The VPS already runs a second project, **FRELODY**, in Docker. You must not break it.

### Your working context

- **Repo (local dev machine):** `D:\PROGRAMMER\.NET\Projects 26\assetlen`
- **Reference deployment on the same VPS:** `D:\PROGRAMMER\.NET\Projects 26\FRELODY` — read
  its `docker-compose.yml`, `docker-compose.prod.yml`, `nginx.conf` and
  `.github/workflows/deploy.yml`. Copy its *patterns*; do not copy its ports.
- **VPS access:** the same host/user/key as FRELODY's GitHub Actions secrets (`SERVER_HOST`,
  `SERVER_USER`, `SERVER_SSH_KEY`, `PROJECT_DIR`). Ask me for them.
- Read `CLAUDE.md` in the assetlen repo before writing code. §0 forbids you from running
  `git commit` or `git push` — leave the tree dirty and let me inspect the diff.

### Step 0 — Reconnaissance, before you write anything

Run these on the VPS and report the output back to me before proceeding:

```bash
free -h                                   # RAM headroom
df -h /                                   # disk headroom
docker ps --format 'table {{.Names}}\t{{.Ports}}'
sudo ss -tlnp | sort -k4                  # what actually owns :80 and :443
ls -la $PROJECT_DIR                       # FRELODY's checkout
docker network ls
dig +short api.assetlen.com               # does the DNS record exist yet?
```

Two decisions come out of this and **you must not guess them**:

**(a) Dedicated SQL Server instance, or share FRELODY's?**
`mcr.microsoft.com/mssql/server:2022-latest` wants ~2 GB RAM to itself and FRELODY already
runs one. Decision rule:

- **≥ 4 GB free** → dedicated instance (`assetlen-sqlserver`). Preferred: clean blast radius,
  independent backup and restore, and a `docker compose down -v` on one project cannot
  destroy the other's data.
- **< 4 GB free** → reuse FRELODY's `frelody-sqlserver` container, creating a dedicated
  database and a dedicated SQL login for ASSETLEN (**never** reuse `sa`). Say so explicitly in
  your report, because it couples the two projects' uptime and you must record that coupling
  in the deploy doc you write.

**(b) Who terminates TLS?**
FRELODY's gateway container binds host `:80` and its `nginx.conf` has no `443` block, so TLS
is terminated somewhere you have not seen yet — a host-level nginx, or Cloudflare in front.
Find out. Then:

- **If `:443` is free** → run Caddy for ASSETLEN and let it solve ACME **TLS-ALPN-01**, which
  needs only 443. This is the recommended path: it does not touch FRELODY's `:80` binding at
  all, and certificate renewal is automatic and unattended.
- **If `:443` is taken** → add an `api.assetlen.com` `server` block to whatever owns it, and
  proxy to `127.0.0.1:8091`. If that is FRELODY's containerised nginx, follow the dynamic
  upstream pattern its `nginx.conf` documents (`resolver 127.0.0.11 valid=30s;` +
  `set $var http://...;` + `proxy_pass $var;`) — FRELODY's `CLAUDE.md` explains why a pinned
  IP 502s after a redeploy.

**TLS is not optional.** Android blocks cleartext HTTP by default for `targetSdk >= 28`. A
plain-`http://` base URL means every request fails with `CLEARTEXT communication not
permitted`, and the failure surfaces in the app as a generic network error that will cost you
an afternoon. Do not "temporarily" work around it with a `networkSecurityConfig` exemption.

### Step 1 — Port and name allocation

FRELODY owns: `80, 1433, 8080, 5080, 5000, 1025, 4317, 4318, 16686`, plus
`127.0.0.1:5001-5002`. Use these for ASSETLEN and nothing else:

| Service | Container port | Host binding | Public? |
|---|---|---|---|
| `assetlen-sqlserver` | 1433 | `127.0.0.1:1434` | **No** — loopback only, admin via SSH tunnel |
| `assetlen-api` | 8080 | `127.0.0.1:8091` | **No** — only the TLS terminator reaches it |
| `assetlen-caddy` | 443 | `0.0.0.0:443` | Yes — the only public surface |

Docker network: `assetlen-net` (bridge, separate from `frelody-net`).
Volumes: `assetlen_sqldata`, `assetlen_artifacts`. Prefixed so a stray `docker volume prune`
in the FRELODY directory cannot reach them.

### Step 2 — Files to create in the assetlen repo

Create all of these. Full contents are yours to write, but the specifics below are
load-bearing — they come from reading the actual code, and every one of them is a failure I
have already traced or can see coming.

#### `Dockerfile` (repo root)

Multi-stage, .NET 10 SDK → `mcr.microsoft.com/dotnet/aspnet:10.0`. Builds `assetlen.API`.

- The API's csproj targets `net10.0`. The solution also contains `assetlen.Maui`, which
  targets `net10.0-android` and **will not restore inside a Linux SDK image**. Copy in only
  the projects the API needs (`assetlen.API`, `assetlen.Service`, `assetlen.Shared.Models`,
  `assetlen.SqlServer`, and `assetlen/assetlen.Shared` if the API references it) — or build
  the `.csproj`, never the `.sln`. Verify with
  `dotnet list assetlen.API/assetlen.API.csproj reference`.
- `ENV ASPNETCORE_HTTP_PORTS=8080`, `EXPOSE 8080`.
- The API calls `ImageSharpThumbnailGenerator` for artifact thumbnails. `aspnet:10.0` is fine
  for ImageSharp (pure managed, no libgdiplus needed), but confirm — if thumbnail generation
  throws at runtime, that is where to look.
- Add a `.dockerignore` (copy FRELODY's and add `**/bin`, `**/obj`, `assetlen.Maui/`,
  `tools/fixtures/`). `tools/fixtures/` is gitignored because it holds a real WhatsApp export
  with real names, banks and account numbers — it must never reach a build context or an image
  layer.

#### `docker-compose.yml` + `docker-compose.prod.yml` (repo root)

Mirror FRELODY's split: base file for local, prod file for the VPS overrides. Prod environment
for `assetlen-api`:

```yaml
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_HTTPS_PORT: ""          # see gotcha 3
AppMode: "2"                       # see gotcha 1
ConnectionStrings__DefaultConnection: >-
  Server=assetlen-sqlserver,1433;Database=assetlen;User Id=assetlen_app;
  Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=True;
  MultipleActiveResultSets=True;Connect Timeout=30
ConnectionStrings__DefaultConnectionHangfire: >-
  Server=assetlen-sqlserver,1433;Database=assetlen;User Id=assetlen_app;
  Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=True
Jwt__Key: "${JWT_KEY}"
Artifacts__StorageRoot: "/app/artifacts"
Ingest__InboundSecret: "${INGEST_SECRET}"
```

`assetlen-sqlserver` gets `ACCEPT_EULA=Y`, `MSSQL_SA_PASSWORD=${SA_PASSWORD}`,
`MSSQL_PID=Developer`, the `assetlen_sqldata` volume, a `sqlcmd` healthcheck (copy FRELODY's,
note the `-No -C` flags), and `ports: ["127.0.0.1:1434:1433"]`. `assetlen-api` gets
`depends_on: {assetlen-sqlserver: {condition: service_healthy}}` and mounts
`assetlen_artifacts:/app/artifacts`.

#### `.env.example` (repo root, committed) and `.env` (VPS only, gitignored)

`SERVER_HOST`, `SA_PASSWORD`, `DB_PASSWORD`, `JWT_KEY`, `INGEST_SECRET`, `SMTP_*`,
`APP_VERSION`. Generate the real secrets on the box with `openssl rand -base64 36`. Add `.env`
to `.gitignore` alongside the existing signing-material block.

#### `Caddyfile` (repo root, if you took the Caddy path)

```
api.assetlen.com {
    reverse_proxy assetlen-api:8080
    encode gzip
}
```

Caddy handles ACME, renewal and HTTP/2 with no further configuration. Persist its `/data`
volume or it re-issues certificates on every restart and hits Let's Encrypt rate limits.

#### `remote-deploy.md` (repo root)

Write this last, from what you actually did — not from this prompt. It must contain: the exact
`docker compose` command to redeploy, where `.env` lives, how to take and restore a database
backup, how to reach SQL Server from a laptop via SSH tunnel, and — if you shared FRELODY's
instance — a plain statement that the two projects now share a database server.

### Step 3 — Code changes in the repo

Two, both small, both required.

**(a) Make the MAUI API address a build-time property.** Right now:

```csharp
// assetlen.Maui/MauiProgram.cs
private static Uri BaseAddressApi =>
    DeviceInfo.Current.Platform == DevicePlatform.Android
        ? new Uri("https://10.0.2.2:7264")
        : new Uri("https://localhost:7264");
```

`tools/release-apk.sh` already accepts an API base URL as its first argument and passes
`-p:AssetlenApiBaseUrl=` to MSBuild, but nothing consumes it — finish that wiring. Surface the
property to C# as a constant (an MSBuild-generated `AssemblyMetadata` attribute, or a
`<DefineConstants>`-driven partial — your call, but keep it simple and greppable) and have
`BaseAddressApi` prefer it, falling back to today's per-platform loopback when it is absent so
emulator debugging keeps working untouched. Then a release build is:

```bash
bash tools/release-apk.sh https://api.assetlen.com
```

**(b) Add a health endpoint.** The API has none — `grep -rn "MapHealthChecks" assetlen.API/`
returns nothing, and Swagger only mounts in Development, so there is no anonymous URL to
probe. Add `app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();`
and use it for the container healthcheck and for Caddy. Without it you cannot tell "API down"
from "API up, database unreachable" during a deploy.

### Step 4 — Gotchas, all of them specific to this codebase

These are read out of the source, not guessed. Each one costs an hour if you meet it cold.

1. **`AppMode` is the EF provider switch, and it throws if unset.** `assetlen.API/Program.cs`
   reads `builder.Configuration["AppMode"]` and does
   `_ => throw new Exception($"Unsupported provider: {provider}. use 1 or 2")`. Set
   `AppMode: "2"` — it is the SQL Server branch that also sets
   `MigrationsHistoryTable("__EFMigrationsHistory", "dbo")`. `AppMode: "1"` and `"3"`
   additionally call `builder.WebHost.UseUrls(...)` / `ListenAnyIP(0)` and will bind a
   **random port** or loopback-only, which in a container means the API starts, logs nothing
   wrong, and is unreachable forever.

2. **Hangfire needs its own connection string, and the committed one uses Integrated
   Security.** `ConnectionStrings:DefaultConnectionHangfire` in `appsettings.json` is
   `Integrated Security=True` — Windows auth, which does not exist on Linux. If you set
   `DefaultConnection` and forget this one, the API boots, then every endpoint that enqueues a
   job returns 500 with `Invalid object name 'HangFire.Job'`. Set both. They can point at the
   same database; Hangfire builds its own `HangFire.*` schema there.

3. **`app.UseHttpsRedirection()` is unconditional, and it will break you. Reproduced, not
   theorised.** Behind a TLS-terminating proxy the container speaks plain HTTP. Driving the
   API on its all-interfaces binding under the stock `https` launch profile:

   ```
   $ curl -D - -o /dev/null -X POST http://192.168.1.4:49337/api/Authorization/Login ...
   HTTP/1.1 307 Temporary Redirect
   Location: https://192.168.1.4:7264/api/Authorization/Login
   ```

   The middleware infers the HTTPS port from the local HTTPS endpoint and rewrites the caller
   to a port that does not exist on the server. Every request dies. FRELODY solves it with
   `ASPNETCORE_HTTPS_PORT: ""` — with no HTTPS port configured the middleware logs a warning
   and no-ops. Same request with that set and only an HTTP endpoint bound returns **200**.
   Do the same. **Do not delete the line from `Program.cs`.**

4. **The database creates and migrates itself on boot.** `DatabaseSeeder.InitializeDb` calls
   `database.Migrate()` and rethrows on failure, and `SeedRolesAndAdminAsync` runs straight
   after. There are 6 migrations in `assetlen.SqlServer/Migrations/`, the newest being
   `20260818060449_P5_StageCatalogueAndNesting`. So: do **not** hand-run
   `dotnet ef database update` against the VPS. Point the connection string at the container
   and start the API — the first boot builds the schema and seeds roles plus the admin account
   from `UserSettings` in `appsettings.json`. It is slow; give the healthcheck a
   `start_period` of at least 90s or the orchestrator will kill it mid-migration and leave a
   half-built schema.

5. **Environment variables win, and `__` is the nesting separator.** `builder.Configuration`
   ends with `.AddEnvironmentVariables()`, so `ConnectionStrings__DefaultConnection` overrides
   `appsettings.json` cleanly. You do not need to rebuild the image to change configuration.

6. **`appsettings.json` currently contains live third-party secrets** — a Brevo SMTP password,
   a Google OAuth client secret, and an SMS gateway password, all committed to git. Exposing
   this API to the internet exposes them. **Rotate all three before the box takes public
   traffic**, move them to `.env`, and reference them as env vars. Flag this in your report as
   a finding even if I do not act on it immediately; do not treat it as done because you moved
   the config.

7. **The default database name has a dot in it.** `appsettings.json` says
   `Initial Catalog=assetlen-db1.0`. Legal, but every `sqlcmd`/`USE` statement then needs
   `[assetlen-db1.0]` brackets. Use plain `assetlen` for the container deployment via the
   env-var override and sidestep it.

8. **SignalR.** `Program.cs` maps a hub at `/hubs/assetlen`. Whatever terminates TLS must
   proxy WebSocket upgrades (`Upgrade`/`Connection` headers, HTTP/1.1) or real-time silently
   degrades to long-polling. Caddy does this by default; nginx needs it spelled out — copy the
   `proxy_set_header Upgrade $http_upgrade;` block from FRELODY's `nginx.conf`.

9. **Artifacts are files on disk, not blobs in the database.** `IArtifactStorage` resolves
   `Artifacts:StorageRoot`, defaulting to `App_Data` under the content root — which is *inside
   the container* and dies with it. Mount `assetlen_artifacts:/app/artifacts` and set
   `Artifacts__StorageRoot=/app/artifacts`, or every uploaded photo vanishes on the next
   deploy while its `tbl_Artifact` row survives, and the register points at nothing.

10. **CORS is already wide open** (`AllowAnyOrigin/Method/Header` under `AllowAllOrigins`).
    Nothing to configure for the APK — native HTTP clients do not enforce CORS anyway. Do not
    "fix" this as part of this task; note it and move on.

### Step 5 — Verification, in this order

Do not report success until all five pass. Paste real output, not a summary.

**1. Database reachable from inside the network, not from outside.**

```bash
docker exec assetlen-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT name FROM sys.databases" -No -C
# From your laptop — this MUST fail or time out:
nc -vz <SERVER_HOST> 1433
```

**2. Schema actually built.** Expect 6 rows matching the migration files in the repo:

```bash
docker exec assetlen-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -d assetlen \
  -Q "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId" -No -C
```

**3. API answers over TLS from off the box**, with a valid chain:

```bash
curl -sS https://api.assetlen.com/health
curl -sSI https://api.assetlen.com/health | head -1
echo | openssl s_client -connect api.assetlen.com:443 -servername api.assetlen.com 2>/dev/null \
  | openssl x509 -noout -subject -issuer -dates
```

**4. The real end-to-end suite, run from my dev machine against the VPS.** This is the strong
test and it already exists — `tools/e2e-all.sh` takes an API base as its first argument,
forwards it to all four sub-suites (`bash "$2" "$API" ...`), and issues every request with
`curl -sk`, so a self-signed or staging certificate will not stop it:

```bash
bash tools/e2e-all.sh https://api.assetlen.com/api
```

**This is already proven to work over a network address, not just loopback.** Before you
started, I ran the API in a container-equivalent configuration and drove the whole suite at it
across the LAN:

```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="http://0.0.0.0:5199" \
ASPNETCORE_HTTPS_PORT="" \
dotnet run --project assetlen.API --no-launch-profile

curl -X POST http://192.168.1.4:5199/api/Dev/SeedDemo          # 200
bash tools/e2e-all.sh "http://192.168.1.4:5199/api"
#   186 passed, 0 failed, 0 skipped — the chain is green
```

So the suite is not localhost-bound, the API is happy on `0.0.0.0`, and 186/186 is the number
you must match against the VPS. Anything less is a deployment defect, not a flaky test — bisect
it, do not accept it.

(Incidental, worth knowing: the stock `https` launch profile already binds
`http://0.0.0.0:0` — an all-interfaces listener on a *random* port. Useful for a quick
off-box probe during development, useless for anything you want to point a phone at.)

Two caveats, both real:

- `e2e-p5-money-and-staging.sh` and `e2e-ux-personas.sh` call `POST /api/Dev/SeedDemo`, and
  `DevController` refuses to run outside `Development`. So: bring the stack up **once** with
  `ASPNETCORE_ENVIRONMENT=Development` on a throwaway database, run the full 186, confirm
  green, then tear that database down, switch to `Production`, and re-run only the suites that
  do not touch `/api/Dev`. Report both numbers separately. Never leave `Development` set on
  the box — it also enables Swagger at the root and the demo seeder.
- The suite needs the tenant admin credentials from `UserSettings` in `appsettings.json`; pass
  them as arguments 2 and 3 if you changed them.

**5. The APK, on a real phone.** Build with the release host baked in, install, sign in:

```bash
bash tools/release-apk.sh https://api.assetlen.com
adb install -r assetlen.Maui/bin/Release/net10.0-android/publish/com.assetlen.app-Signed.apk
adb logcat -s DOTNET:* chromium:* | grep -i "http\|exception"
```

Watch for `CLEARTEXT communication not permitted` (you used `http://`) and for TLS handshake
failures (incomplete certificate chain — Android is stricter than curl about intermediates;
`openssl s_client -showcerts` on the box is the check).

### Step 6 — Report back

Give me, in this order:

1. The two Step-0 decisions and the numbers you based them on.
2. Every file created or changed, with paths.
3. The five verification outputs, verbatim.
4. Both e2e assertion counts (Development run and Production run).
5. **Anything you could not do, stated plainly** — a missing DNS record, a port you could not
   free, a secret you could not rotate. Do not paper over it; an unmentioned gap here is a
   production outage later.
6. The secrets you generated, once, so I can store them. Then confirm they are only in `.env`
   on the box and nowhere in git.

**Do not run `git commit` or `git push`** — repo rule, `CLAUDE.md` §0. Leave the working tree
dirty and tell me what to review.
