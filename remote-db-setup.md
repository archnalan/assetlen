# ASSETLEN — remote PostgreSQL + API setup (agent prompt)

**What this file is.** A single self-contained prompt to hand to an agent with SSH access to
the VPS. It ends with the ASSETLEN APK talking to a real PostgreSQL database over the internet,
with a nightly backup that has been restored at least once. Everything the agent needs is
below — repo facts, port map, file contents, gotchas, and the verification that proves it worked.

**Read this before handing it over.** §0 is context for *you*, not the agent. The prompt
itself starts at "PROMPT BEGINS".

*Rewritten for PostgreSQL on 2026-09-29. The SQL Server version of this prompt is in git
history; nothing in it applies any more (CLAUDE.md §5.1.1).*

---

## 0. Why this is not just "a database"

The APK cannot talk to PostgreSQL. It talks to `assetlen.API`, which talks to PostgreSQL.
Publishing 5432 to the internet and putting a database password inside an APK would hand every
installer of that APK full write access to every tenant's record — and the record is the
product (assetlen.md Law 2). So the deliverable is a **two-tier stack**: Postgres reachable
only on ASSETLEN's own Docker network, and the API in front of it behind TLS.

```
Android APK  ──HTTPS/443──▶  Caddy  (api.assetlen.com, auto Let's Encrypt)
                                │
                                ▼
                       assetlen-api  :8080          ← container port
                                │                     host: 127.0.0.1:8091
                                ▼   (assetlen-net)
                    assetlen-postgres :5432         ← never published publicly
                                                      host: 127.0.0.1:5433 (admin only, via SSH tunnel)
```

Four things about the current repo make this more than a copy of FRELODY's setup:

1. **There is no Dockerfile in this repo.** FRELODY has four. The agent writes ASSETLEN's.
2. **The APK's API address is hard-coded to the Android emulator's loopback**
   (`https://10.0.2.2:7264` in `assetlen.Maui/MauiProgram.cs`). On a real phone that resolves
   to nothing. Part of this job is making it configurable at build time.
3. **The VPS already runs FRELODY**, which owns ports 80, 1433, 8080, 5080, 5000, 1025, 4317,
   4318, 16686 and 127.0.0.1:5001–5002. ASSETLEN must not collide with any of them. FRELODY
   runs SQL Server, not Postgres, so there is nothing to share: ASSETLEN gets its own
   `postgres:17` container. It is small (≈100–200 MB resident at this size), so there is no
   RAM decision to make as there was with SQL Server.
4. **The schema builds itself.** The API applies its EF migrations (`assetlen.Postgres/`, one
   baseline) and creates the `pg_trgm` extension and Hangfire's `hangfire` schema on first
   boot. The only thing the database needs beforehand is an empty `assetlen` database **owned
   by the app's login**. No superuser step at runtime.

---

## PROMPT BEGINS — copy everything below this line

---

You are setting up remote infrastructure for **ASSETLEN**, a .NET 10 construction-commitments
app on **PostgreSQL 17**. The goal: an Android APK installed on a real phone signs in and works
against a database on my VPS, and that database is backed up every night. Right now the APK
points at an emulator loopback address and there is no server-side deployment at all.

The VPS already runs a second project, **FRELODY**, in Docker. You must not break it.

### Your working context

- **Repo (local dev machine):** `D:\PROGRAMMER\.NET\Projects 26\assetlen`
- **Reference deployment on the same VPS:** `D:\PROGRAMMER\.NET\Projects 26\FRELODY` — read
  its `docker-compose.yml`, `docker-compose.prod.yml`, `nginx.conf` and
  `.github/workflows/deploy.yml`. Copy its *patterns*; do not copy its ports.
- **VPS access:** the same host/user/key as FRELODY's GitHub Actions secrets (`SERVER_HOST`,
  `SERVER_USER`, `SERVER_SSH_KEY`, `PROJECT_DIR`). Ask me for them.
- Read `CLAUDE.md` in the assetlen repo before writing code — §5.1.1 is the database section.
  §0 forbids you from running `git commit` or `git push` — leave the tree dirty and let me
  inspect the diff.

### Step 0 — Reconnaissance, before you write anything

Run these on the VPS and report the output back to me before proceeding:

```bash
free -h                                   # RAM headroom
df -h /                                   # disk headroom — backups live here too
docker ps --format 'table {{.Names}}\t{{.Ports}}'
sudo ss -tlnp | sort -k4                  # what actually owns :80, :443, :5432, :5433, :8091
ls -la $PROJECT_DIR                       # FRELODY's checkout
docker network ls
docker volume ls
dig +short api.assetlen.com               # does the DNS record exist yet?
timedatectl                               # host time zone, for the backup cron
```

One decision comes out of this and **you must not guess it**:

**Who terminates TLS?**
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

Also confirm from the `ss` output that nothing on the host already listens on **5432 or
5433**. If something does (a host-installed Postgres), stop and tell me — do not pick another
port on your own.

### Step 1 — Port and name allocation

FRELODY owns: `80, 1433, 8080, 5080, 5000, 1025, 4317, 4318, 16686`, plus
`127.0.0.1:5001-5002`. Use these for ASSETLEN and nothing else:

| Service | Container port | Host binding | Public? |
|---|---|---|---|
| `assetlen-postgres` | 5432 | `127.0.0.1:5433` | **No** — loopback only; admin via SSH tunnel |
| `assetlen-api` | 8080 | `127.0.0.1:8091` | **No** — only the TLS terminator reaches it |
| `assetlen-caddy` | 443 | `0.0.0.0:443` | Yes — the only public surface |

**Why 5433 and not 5432 on the host:** 5432 is where a host-level Postgres, or a later
project's, will want to sit; keeping ASSETLEN's admin port off it means neither has to move.
The container itself still listens on 5432 inside the network — the API connects to
`assetlen-postgres:5432`, never to the host port.

**The host binding must be `127.0.0.1:5433:5432`, never `5433:5432`.** Docker writes its own
iptables rules for published ports and **bypasses `ufw`**: a bare `5433:5432` is reachable from
the internet no matter what the firewall says. Loopback-only is the whole protection.

Docker network: `assetlen-net` (bridge, separate from `frelody-net`). Only `assetlen-postgres`,
`assetlen-api` and `assetlen-caddy` join it. FRELODY's containers must not be attached to it,
and `assetlen-postgres` must not be attached to anything else.

Volumes: `assetlen_pgdata`, `assetlen_artifacts`, `assetlen_caddy_data`. Prefixed so a stray
`docker volume prune` in the FRELODY directory cannot reach them.

Host directories: `/var/backups/assetlen/` (dumps and the artifact mirror), `/opt/assetlen/`
(the backup script). Owned by root, mode `0700` — a dump contains every tenant's record.

### Step 2 — Files to create in the assetlen repo

Create all of these. Full contents are yours to write, but the specifics below are
load-bearing — they come from reading the actual code, and every one of them is a failure I
have already traced or can see coming.

#### `Dockerfile` (repo root)

Multi-stage, .NET 10 SDK → `mcr.microsoft.com/dotnet/aspnet:10.0`. Builds `assetlen.API`.

- The API's csproj targets `net10.0`. The solution also contains `assetlen.Maui`, which
  targets `net10.0-android` and **will not restore inside a Linux SDK image**. Copy in only
  the projects the API needs — `assetlen.API`, `assetlen.Service`, `assetlen.Shared.Models`
  and **`assetlen.Postgres`** (the migrations assembly: the API references it, and
  `Program.cs` names it in `MigrationsAssembly("assetlen.Postgres")` — leave it out and the
  API boots against an empty database and creates nothing) — or build the `.csproj`, never
  the `.sln`. Verify with `dotnet list assetlen.API/assetlen.API.csproj reference`.
- `ENV ASPNETCORE_HTTP_PORTS=8080`, `EXPOSE 8080`.
- **OCR needs `tesseract-ocr` in the runtime image** (`apt-get install -y --no-install-recommends
  tesseract-ocr`). Every uploaded file is OCR'd on a Hangfire queue, and on Linux the only
  engine is `tesseract` on the `PATH` (the other one is Windows' built-in OCR). Without it,
  receipts photographed on site are never searchable and `e2e-p6-search.sh` fails. `ffmpeg`
  is optional (voice-note transcription and video posters degrade without it).
- `tzdata` must be present so `TZ` is honoured — see gotcha 5. Check with
  `docker run --rm -e TZ=Africa/Nairobi <image> date`.
- ImageSharp (thumbnails) is pure managed; `aspnet:10.0` needs nothing extra for it.
- Add a `.dockerignore` (copy FRELODY's and add `**/bin`, `**/obj`, `assetlen.Maui/`,
  `tools/fixtures/`). `tools/fixtures/` is gitignored because it holds a real WhatsApp export
  with real names, banks and account numbers — it must never reach a build context or an image
  layer.

#### `deploy/postgres/init/01-assetlen.sh` (repo, committed — contains no secret)

The official image runs scripts in `/docker-entrypoint-initdb.d/` **once, on an empty data
directory**. Use one to create the app login and its database. The app login is **not** a
superuser; it owns the `assetlen` database, and that ownership is exactly what lets it
`CREATE EXTENSION pg_trgm` (a trusted extension) and create the `hangfire` schema on first boot.

```bash
#!/bin/bash
set -euo pipefail
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
     --set=app_password="$ASSETLEN_DB_PASSWORD" <<'SQL'
CREATE ROLE assetlen_app LOGIN PASSWORD :'app_password';
CREATE DATABASE assetlen OWNER assetlen_app ENCODING 'UTF8' TEMPLATE template0;
SQL
```

Mount it read-only: `./deploy/postgres/init:/docker-entrypoint-initdb.d:ro`. It will **not**
re-run against an existing `assetlen_pgdata` volume — changing the password later is an
`ALTER ROLE` by hand, not an edit to this file.

#### `docker-compose.yml` + `docker-compose.prod.yml` (repo root)

Mirror FRELODY's split: base file for local, prod file for the VPS overrides.

`assetlen-postgres`:

```yaml
image: postgres:17                      # record the digest you pulled in remote-deploy.md
container_name: assetlen-postgres
restart: unless-stopped
environment:
  POSTGRES_PASSWORD: "${PG_SUPERUSER_PASSWORD}"   # the 'postgres' superuser; admin only
  ASSETLEN_DB_PASSWORD: "${DB_PASSWORD}"           # read by 01-assetlen.sh on first init
volumes:
  - assetlen_pgdata:/var/lib/postgresql/data
  - ./deploy/postgres/init:/docker-entrypoint-initdb.d:ro
ports:
  - "127.0.0.1:5433:5432"               # loopback only — see Step 1
networks: [assetlen-net]
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U assetlen_app -d assetlen"]
  interval: 10s
  timeout: 5s
  retries: 10
  start_period: 30s
shm_size: 256mb
```

Leave the container's time zone at its default (UTC). Every column is `timestamptz` and the
app never asks the database for "today", so the database's zone decides nothing (CLAUDE.md
§5.1.1).

`assetlen-api` prod environment:

```yaml
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_HTTPS_PORT: ""          # see gotcha 3
AppMode: "2"                       # see gotcha 1
TZ: "Africa/Nairobi"               # see gotcha 5
ConnectionStrings__Postgres: >-
  Host=assetlen-postgres;Port=5432;Database=assetlen;Username=assetlen_app;
  Password=${DB_PASSWORD};Maximum Pool Size=40;Timeout=15;Command Timeout=60
Jwt__Key: "${JWT_KEY}"
Artifacts__StorageRoot: "/app/artifacts"
Ingest__InboundSecret: "${INGEST_SECRET}"
```

- **One connection string.** Hangfire uses `ConnectionStrings:PostgresHangfire` if set and
  falls back to `ConnectionStrings:Postgres`; it keeps its tables in its own `hangfire`
  schema of the same database. Do not set a second one unless you have a reason.
- **Do not add `Include Error Detail=true` in production.** The dev machine uses it; it puts
  parameter values — people's text, amounts — into exception messages and from there into logs.
- No `SSL Mode` is needed: the traffic never leaves `assetlen-net`.

`assetlen-api` also gets `depends_on: {assetlen-postgres: {condition: service_healthy}}`,
mounts `assetlen_artifacts:/app/artifacts`, joins `assetlen-net`, publishes
`127.0.0.1:8091:8080`, and has a healthcheck on `/health` with `start_period: 90s`.

#### `.env.example` (repo root, committed) and `.env` (VPS only, gitignored)

`SERVER_HOST`, `PG_SUPERUSER_PASSWORD`, `DB_PASSWORD`, `JWT_KEY`, `INGEST_SECRET`, `SMTP_*`,
`APP_VERSION`. Generate the real secrets on the box. **Use `openssl rand -hex 32` for the two
database passwords** — hex has no `;`, `=`, `+` or `/`, so the password can never split the
connection string or need quoting in a `pg_dump` command line. `openssl rand -base64 36` is
fine for `JWT_KEY` and `INGEST_SECRET`. `.env` is already covered by `.gitignore`; check it
with `git check-ignore -v .env` before you write it.

#### `Caddyfile` (repo root, if you took the Caddy path)

```
api.assetlen.com {
    reverse_proxy assetlen-api:8080
    encode gzip
}
```

Caddy handles ACME, renewal and HTTP/2 with no further configuration. Persist its `/data`
volume (`assetlen_caddy_data`) or it re-issues certificates on every restart and hits Let's
Encrypt rate limits.

#### `deploy/backup/assetlen-backup.sh` (repo, committed — contains no secret)

Installed on the VPS as `/opt/assetlen/assetlen-backup.sh`, mode `0700`, run by root's cron.
**The record is the database *and* the artifact files** — a dump without the files restores a
register that points at nothing, so the script takes both.

```bash
#!/usr/bin/env bash
# Nightly ASSETLEN backup: a pg_dump of the record, and a mirror of the artifact store.
set -euo pipefail
DEST=/var/backups/assetlen
KEEP_DAYS=14
STAMP=$(date -u +%Y%m%dT%H%M%SZ)
umask 077
mkdir -p "$DEST/db" "$DEST/artifacts"

# 1. The database. Custom format (-Fc): compressed, and pg_restore can pick from it.
#    The hangfire schema is left out: it is a job queue the API rebuilds on boot, and
#    its job arguments hold request headers and bodies that do not belong in a backup.
docker exec assetlen-postgres \
  pg_dump -U assetlen_app -d assetlen -Fc --exclude-schema=hangfire \
  > "$DEST/db/assetlen-$STAMP.dump.partial"
mv "$DEST/db/assetlen-$STAMP.dump.partial" "$DEST/db/assetlen-$STAMP.dump"
find "$DEST/db" -name 'assetlen-*.dump' -mtime +"$KEEP_DAYS" -delete
find "$DEST/db" -name '*.partial' -mtime +1 -delete

# 2. The files. Content-addressed and never rewritten, so a mirror is incremental by
#    nature. No --delete: a file that vanished from the volume is exactly what a backup
#    is for.
SRC=$(docker volume inspect -f '{{ .Mountpoint }}' assetlen_artifacts)
rsync -a "$SRC"/ "$DEST/artifacts/"

echo "$(date -u +%FT%TZ) ok $STAMP" >> "$DEST/backup.log"
```

- `docker exec … pg_dump -U assetlen_app` needs no password: the official image trusts
  connections over the container's own Unix socket. If you tighten `pg_hba.conf`, pass
  `-e PGPASSWORD` to `docker exec` instead of writing the password into the script.
- **Schedule** — root's crontab, once a night, outside the Sunday-evening report run:
  `30 2 * * * /opt/assetlen/assetlen-backup.sh >> /var/log/assetlen-backup.log 2>&1`.
  Check `timedatectl` from Step 0 so you know which 02:30 that is.
- **A backup on the same disk as the database is not a backup.** Copy `/var/backups/assetlen/`
  off the box at least weekly (`rclone` to object storage, or `rsync` to another machine —
  ask me which). Say in your report which one you set up, or that you did not.
- The app's own backup / restore endpoints (`ConfigController`, legacy desktop mode) answer
  **501** on Postgres. They are not the backup; this script is.

#### `remote-deploy.md` (repo root)

Write this last, from what you actually did — not from this prompt. It must contain: the exact
`docker compose` command to redeploy, where `.env` lives, the image digest you pulled for
`postgres:17`, how to reach Postgres from a laptop, where backups land and how to restore one
(below), and when the last restore drill was done.

- **From a laptop:** `ssh -N -L 5433:127.0.0.1:5433 $SERVER_USER@$SERVER_HOST`, then
  `psql "host=127.0.0.1 port=5433 dbname=assetlen user=assetlen_app"`.
- **Restore** (stop the API first, so nothing writes mid-restore):

  ```bash
  docker compose -f docker-compose.yml -f docker-compose.prod.yml stop assetlen-api
  docker exec assetlen-postgres psql -U postgres -v ON_ERROR_STOP=1 \
    -c "DROP DATABASE IF EXISTS assetlen WITH (FORCE)" \
    -c "CREATE DATABASE assetlen OWNER assetlen_app ENCODING 'UTF8' TEMPLATE template0"
  docker exec -i assetlen-postgres pg_restore -U postgres -d assetlen \
    --no-owner --role=assetlen_app --exit-on-error < /var/backups/assetlen/db/assetlen-<STAMP>.dump
  rsync -a /var/backups/assetlen/artifacts/ "$(docker volume inspect -f '{{ .Mountpoint }}' assetlen_artifacts)"/
  docker compose -f docker-compose.yml -f docker-compose.prod.yml start assetlen-api
  ```

  The API recreates the `hangfire` schema on boot and re-registers its recurring jobs.

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

**(b) Add a health endpoint that touches the database.** The API has none —
`grep -rn "MapHealthChecks\|/health" assetlen.API/` returns nothing. Add
`builder.Services.AddHealthChecks().AddDbContextCheck<AssetlenDbContext>()` (package
`Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`, same 10.0 line as the
rest) and `app.MapHealthChecks("/health").AllowAnonymous();`. Use it for the container
healthcheck and for Caddy. A `/health` that only returns 200 cannot tell "API up, database
unreachable" from "all well", which is the one thing you need to know during a deploy.

### Step 4 — Gotchas, all of them specific to this codebase

These are read out of the source, not guessed. Each one costs an hour if you meet it cold.

1. **`AppMode` must be `"2"`, and the API throws if it is unset.** `Program.cs` refuses to
   start with `Unsupported AppMode`. `"2"` is the plain hosted mode (`app.Run()`). `"1"` binds
   `http://127.0.0.1:0` and `"3"` does `ListenAnyIP(0)` — a **random port** — for the old
   desktop builds; in a container either one means the API starts, logs nothing wrong, and is
   unreachable forever. (`AppMode` no longer chooses a database — Postgres is the only one.)

2. **The database must exist and be owned by `assetlen_app` before the API's first boot**
   — that is what the init script is for. The API then does the rest, in this order:
   `Database.Migrate()` (applies `Pg_Baseline` from `assetlen.Postgres/Migrations/`, which
   runs `CREATE EXTENSION IF NOT EXISTS pg_trgm`), seeds roles and the admin account from
   `UserSettings`, and only then builds the `hangfire` schema and registers the recurring
   jobs. If the login is not the owner, the first boot dies on `permission denied to create
   extension "pg_trgm"`; do **not** fix that by making the app a superuser — fix the
   ownership. Do not hand-run `dotnet ef database update` against the VPS. Give the
   healthcheck a `start_period` of at least 90s.

   On a brand-new database the log shows **one** `ERR Failed executing DbCommand` while EF
   looks for `__EFMigrationsHistory` before creating it. It is harmless and happens once. Any
   second one is real.

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
   and no-ops. Do the same. **Do not delete the line from `Program.cs`.**

4. **Environment variables win, and `__` is the nesting separator.** `builder.Configuration`
   ends with `.AddEnvironmentVariables()`, so `ConnectionStrings__Postgres` overrides
   `appsettings.json` cleanly. You do not need to rebuild the image to change configuration.
   The committed repo has **no** database connection string at all — `appsettings.json` is
   gitignored — so an image built from a clean clone has nothing to fall back on: an unset
   `ConnectionStrings__Postgres` fails at the first query, not at build time.

5. **The API's clock must be the site's clock: set `TZ` on `assetlen-api`.** The daily
   brief's "today" and its evening cutoff, the works report's as-at day, and the Hangfire
   schedules (`works-report-weekly` on Sunday 18:00, `brief-cutoff` every quarter hour,
   `works-report-milestones` hourly) all run on the server's local time
   (`DateTime.Now`, `TimeZoneInfo.Local`). A container defaults to UTC, which moves Peter's
   day by three hours. The dev machine runs East Africa Time, so `TZ=Africa/Nairobi`; confirm
   with `docker exec assetlen-api date`. The **database** stays in UTC and it does not matter
   (see the compose notes).

6. **`appsettings.Development.json` is tracked and carries an SMS gateway password**, and the
   untracked `appsettings.json` on the dev machine carries a Brevo SMTP password and a Google
   OAuth client secret. Neither file belongs in the image — pass configuration as env vars
   from `.env`. Exposing this API to the internet with those values in use exposes them:
   **rotate all three before the box takes public traffic.** Flag this in your report as a
   finding even if I do not act on it immediately; do not treat it as done because you moved
   the config.

7. **Hangfire job rows hold request bodies.** The legacy sync middleware enqueues a job for
   every write request, and the job's arguments carry the request headers (bearer tokens) and
   body (a registration's password included). With sync off, each such job fails with
   `SyncDisabledException` — noise in the Hangfire tables, not a deployment fault. This is why
   the backup script excludes the `hangfire` schema. Report it; do not fix it as part of this
   task.

8. **SignalR.** `Program.cs` maps a hub at `/hubs/assetlen`. Whatever terminates TLS must
   proxy WebSocket upgrades (`Upgrade`/`Connection` headers, HTTP/1.1) or real-time silently
   degrades to long-polling. Caddy does this by default; nginx needs it spelled out — copy the
   `proxy_set_header Upgrade $http_upgrade;` block from FRELODY's `nginx.conf`.

9. **Artifacts are files on disk, not blobs in the database.** `IArtifactStorage` resolves
   `Artifacts:StorageRoot`, defaulting to `App_Data` under the content root — which is *inside
   the container* and dies with it. Mount `assetlen_artifacts:/app/artifacts` and set
   `Artifacts__StorageRoot=/app/artifacts`, or every uploaded photo vanishes on the next
   deploy while its `tbl_Artifact` row survives, and the register points at nothing. The web
   push keys (`push/vapid.json`) live under the same root, so losing the volume also
   unsubscribes every phone.

10. **CORS is already wide open** (`AllowAnyOrigin/Method/Header` under `AllowAllOrigins`).
    Nothing to configure for the APK — native HTTP clients do not enforce CORS anyway. Do not
    "fix" this as part of this task; note it and move on.

### Step 5 — Verification, in this order

Do not report success until all seven pass. Paste real output, not a summary.

**1. Postgres is reachable on the box, and not from outside it.**

```bash
docker exec assetlen-postgres psql -U assetlen_app -d assetlen -c "select version();"
docker port assetlen-postgres            # must print exactly: 5432/tcp -> 127.0.0.1:5433
sudo ss -tlnp | grep -E ':5432|:5433'    # 127.0.0.1:5433 only; nothing on 0.0.0.0 or [::]
# From your laptop, WITHOUT the tunnel — both MUST fail or time out:
nc -vz -w 5 <SERVER_HOST> 5432
nc -vz -w 5 <SERVER_HOST> 5433
# From your laptop, WITH the tunnel — this must answer:
ssh -N -L 5433:127.0.0.1:5433 $SERVER_USER@$SERVER_HOST &
psql "host=127.0.0.1 port=5433 dbname=assetlen user=assetlen_app" -c "select 1"
```

**2. The schema built itself, as the app login, with no superuser.**

```bash
docker exec assetlen-postgres psql -U assetlen_app -d assetlen -At \
  -c 'select "MigrationId" from "__EFMigrationsHistory" order by 1' \
  -c "select extname from pg_extension where extname = 'pg_trgm'" \
  -c "select nspname from pg_namespace where nspname = 'hangfire'" \
  -c "select rolsuper from pg_roles where rolname = 'assetlen_app'" \
  -c "select distinct key from hangfire.hash where key like 'recurring-job:%' order by 1"
```

Expect: one migration row per migration in `assetlen.Postgres/Migrations/` (today exactly
one, `20260929060334_Pg_Baseline`); `pg_trgm`; `hangfire`; `f` — the app is **not** a
superuser; and the recurring jobs `brief-cutoff`, `works-report-milestones`,
`works-report-weekly` (plus `pull-changes`).

**3. The clock.** `docker exec assetlen-api date` shows East Africa Time;
`docker exec assetlen-postgres date` shows UTC. Both are correct.

**4. API answers over TLS from off the box**, with a valid chain, and its health reaches the
database:

```bash
curl -sS https://api.assetlen.com/health            # Healthy
curl -sSI https://api.assetlen.com/health | head -1  # HTTP/2 200
echo | openssl s_client -connect api.assetlen.com:443 -servername api.assetlen.com 2>/dev/null \
  | openssl x509 -noout -subject -issuer -dates
docker stop assetlen-postgres; curl -sS https://api.assetlen.com/health; docker start assetlen-postgres
# the middle curl must say Unhealthy (503) — that is what the endpoint is for
```

**5. A backup exists, and it restores.** Run the script by hand once, then restore the dump
into a scratch database and compare it with the live one:

```bash
sudo /opt/assetlen/assetlen-backup.sh && ls -la /var/backups/assetlen/db/ && tail -1 /var/backups/assetlen/backup.log
LATEST=$(ls -t /var/backups/assetlen/db/*.dump | head -1)
docker exec assetlen-postgres psql -U postgres -c "create database assetlen_restorecheck owner assetlen_app template template0"
docker exec -i assetlen-postgres pg_restore -U postgres -d assetlen_restorecheck --no-owner --role=assetlen_app --exit-on-error < "$LATEST"
for db in assetlen assetlen_restorecheck; do
  docker exec assetlen-postgres psql -U postgres -d $db -At -c \
    'select (select count(*) from "AspNetUsers"), (select count(*) from "tbl_Projects_RS"), (select count(*) from "tbl_Commitments"), (select count(*) from "tbl_IngestedMessages")'
done                                     # the two lines must match
docker exec assetlen-postgres psql -U postgres -c "drop database assetlen_restorecheck"
sudo crontab -l | grep assetlen-backup   # the schedule is installed
```

A backup that has never been restored is a hope, not a backup. Record the date of this drill
in `remote-deploy.md`.

**6. The real end-to-end suite, run from my dev machine against the VPS.** This is the strong
test and it already exists — `tools/e2e-all.sh` takes an API base as its first argument,
forwards it to every sub-suite, and issues every request with `curl -sk`, so a self-signed or
staging certificate will not stop it:

```bash
bash tools/e2e-all.sh https://api.assetlen.com/api
```

The number to match is the one on the status line of `CLAUDE.md` at the time you run it —
**919 passed, 0 failed, 0 skipped** when this prompt was written, on PostgreSQL, from an empty
database. Anything less is a deployment defect, not a flaky test — bisect it, do not accept it.
The suite needs the tenant admin credentials from `UserSettings`; pass them as arguments 2
and 3 if you changed them.

Two caveats, both real:

- Several suites (`e2e-p4-commitments`, `e2e-p5-money-and-staging`, `e2e-report`,
  `e2e-p8-markup`, `e2e-p9-contractor`, `e2e-law0`, `e2e-postgres`) call Development-only
  endpoints (`/api/Dev/SeedDemo`, `RunCutoff`, `RunSchedule`), which answer 404 outside
  `Development`. So: bring the stack up **once** with `ASPNETCORE_ENVIRONMENT=Development` on a
  throwaway database (`docker compose down`, remove `assetlen_pgdata`, up again), run the whole
  chain, confirm green, then tear that database down, switch to `Production`, and re-run only
  `e2e-p2-peter`, `e2e-p3-ingest`, `e2e-p4-arrange`, `e2e-p5-extraction`, `e2e-p6-search` and
  `e2e-p7-brief`. Report both numbers separately. Never leave `Development` set on the box —
  it also enables the demo seeder and the Swagger UI.
- The Development run writes demo data. Removing `assetlen_pgdata` afterwards is the point;
  do not "clean it up" row by row.

**7. The APK, on a real phone.** Build with the release host baked in, install, sign in:

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

1. The TLS decision from Step 0 and what you found on 443, 5432 and 5433.
2. Every file created or changed, with paths.
3. The seven verification outputs, verbatim.
4. Both e2e assertion counts (Development run and Production run).
5. Where backups land, the cron line, whether they are copied off the box, and the date of the
   restore drill.
6. **Anything you could not do, stated plainly** — a missing DNS record, a port you could not
   free, a secret you could not rotate, an off-box backup target you did not have. Do not paper
   over it; an unmentioned gap here is a production outage later.
7. The secrets you generated, once, so I can store them. Then confirm they are only in `.env`
   on the box and nowhere in git.

**Do not run `git commit` or `git push`** — repo rule, `CLAUDE.md` §0. Leave the working tree
dirty and tell me what to review.
