# RIT HMS — Deployment Runbook

RIT HMS (v2) is **ASP.NET Core 8 (API) + Next.js 15 (web) + PostgreSQL**, multi-tenant
(a control DB + one DB per tenant, created at signup).

Two deployment shapes are documented:

- **§A — Incubator (bare-metal, LIVE).** The current test/staging deployment on the
  shared RIT incubator VM, alongside Retail Pulse. System Postgres + nginx + systemd +
  certbot. **This is what's running today.**
- **§B — Portable / own-server (Docker Compose).** A self-contained stack (API + web +
  Postgres + Caddy) for a dedicated box. Use this to graduate HMS to its own server.

> ⚠️ This has **nothing to do** with the legacy RIT servers `MF-SW-LP24` / `161.97.172.35`.
> Never deploy to or touch those.

---

## A) Incubator deployment (live)

**URL:** https://hms.retailit.lk  ·  **Box:** `azureuser@20.212.21.252` (Ubuntu 24.04, shared with Retail Pulse)
**SSH:** `ssh -i ~/.ssh/retailit_incubator azureuser@20.212.21.252`

```
                              ┌──────────── incubator VM (shared with Pulse) ───────────┐
 browser ──HTTPS──► nginx ──/api/* , /health──► hms-api  (.NET 8, 127.0.0.1:8002) ──┐    │
 (Let's Encrypt)     │     ──everything else───► hms-web  (Next.js,  127.0.0.1:8003) │    │
                     │                                                    Postgres 16 ◄┘   │
                     └──pulse.retailit.lk────► pulse (FastAPI, :8001)   (127.0.0.1:5432)   │
                              └──────────────────────────────────────────────────────────┘
```

### Conventions (coexist cleanly with Pulse — never share names)

| Layer | HMS value |
|---|---|
| Postgres role / control DB | `hms_app` (LOGIN, CREATEDB) / `hms_main` |
| Per-tenant DBs | `hms_tenant_<slug>` (owned by `hms_app`; invisible to `pulse_app`) |
| App dir / logs | `/opt/hms/{api,web,db}` · `/var/log/hms/{api,web}.{log,err}` |
| Ports (localhost only) | API `:8002`, web `:8003` (HMS uses both; next product → `:8004`) |
| systemd units | `hms-api.service`, `hms-web.service` |
| nginx vhost / TLS | `hms.retailit.lk` · Let's Encrypt (certbot, auto-renew) |
| Secrets | `/opt/hms/hms.env` (mode 600) + appended to `~/Downloads/rit-incubator-creds.txt` |

**Stack note:** the original handover templates the Pulse stack (Python/FastAPI + Angular).
HMS is .NET + Next.js, so this deploy adapts it: a **self-contained .NET binary** (no .NET
runtime installed on the box), the **Next.js standalone Node server** (Node 20 installed),
and **two** units/ports instead of one.

### First-time provisioning (already done — recorded for rebuilds)

Versioned copies of the units + vhost live in [`infra/incubator/`](../infra/incubator/).

```bash
KEY=~/.ssh/retailit_incubator; HOST=azureuser@20.212.21.252

# 1) DB role + control DB (idempotent). Password is generated locally and saved to
#    ~/Downloads/rit-incubator-creds.txt as HMS_DB_PASSWORD (also HMS_JWT_SECRET / HMS_MASTER_KEY).
PW=$(grep '^HMS_DB_PASSWORD=' ~/Downloads/rit-incubator-creds.txt | cut -d= -f2-)
ssh -i $KEY $HOST "HMS_PWD='$PW' bash -s" <<'R'
  sudo -u postgres psql -c "CREATE ROLE hms_app WITH LOGIN CREATEDB PASSWORD '$HMS_PWD'" 2>/dev/null \
    || sudo -u postgres psql -c "ALTER ROLE hms_app WITH LOGIN CREATEDB PASSWORD '$HMS_PWD'"
  sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='hms_main'" | grep -q 1 \
    || sudo -u postgres psql -c "CREATE DATABASE hms_main OWNER hms_app"
R

# 2) Install Node 20 (for the Next.js server). .NET is NOT installed — the API ships self-contained.
ssh -i $KEY $HOST 'command -v node || (curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash - && sudo apt-get install -y nodejs)'

# 3) Build + ship the code, and (re)start services:
./infra/incubator/redeploy.sh        # publishes API, builds web, rsyncs, restarts

# 4) Write /opt/hms/hms.env (mode 600). See "Secrets & config" below for the keys.

# 5) Apply control-plane migrations to hms_main (as hms_app):
ssh -i $KEY $HOST "PGPASSWORD='$PW' bash -s" <<'R'
  for f in /opt/hms/db/postgres/migrations/0001_*.sql /opt/hms/db/postgres/migrations/0026_*.sql; do
    psql -h 127.0.0.1 -U hms_app -d hms_main -v ON_ERROR_STOP=1 -f "$f"
  done
R

# 6) Install the systemd units + nginx vhost (templates in infra/incubator/), then:
ssh -i $KEY $HOST 'sudo systemctl daemon-reload && sudo systemctl enable --now hms-api hms-web'
ssh -i $KEY $HOST 'sudo cp <vhost> /etc/nginx/sites-available/hms.retailit.lk && sudo ln -sf ... && sudo nginx -t && sudo systemctl reload nginx'

# 7) TLS:
ssh -i $KEY $HOST 'sudo certbot --nginx -d hms.retailit.lk --non-interactive --agree-tos -m mubeen@aurumhealthtech.com --redirect'
```

### Redeploy (code updates)

```bash
./infra/incubator/redeploy.sh    # build locally → rsync → restart → health-check
```

For **config/secret** changes, edit `/opt/hms/hms.env` on the box, then
`ssh … 'sudo systemctl restart hms-api'`.

### Continuous deployment (GitHub Actions)

`.github/workflows/deploy-incubator.yml` runs the same `redeploy.sh` from CI
**on demand** (*Run workflow* / `gh workflow run deploy-incubator.yml`). It is
deliberately **manual-only** so merging to `main` has no runtime side effect; add
a `push: { branches: [main] }` trigger to auto-deploy on merge once you trust it.
One-time activation:

```bash
# Store the incubator SSH key as a repo secret (grants SSH + sudo to the shared box):
gh secret set INCUBATOR_SSH_KEY < ~/.ssh/retailit_incubator
```

- **Security:** that secret is full SSH + passwordless `sudo` to the box that also
  runs Pulse. Limit who can push to `main`, and configure an **`incubator`
  Environment** with *required reviewers* so each deploy needs human approval.
  Hardening later: a dedicated deploy user without broad sudo.
- Not gated on the CI workflow yet (e2e has pre-existing failures). Add a
  `workflow_run`/`needs` gate once those are triaged so only green builds deploy.
- Manual run: `gh workflow run deploy-incubator.yml` (or the Actions tab).

### Logging in

There are two sign-in methods. **There is no email sender wired** (it's the Sprint-2
"wire real email delivery" TODO), so the magic-link *email* is never delivered —
`/api/v1/auth/magic-link` only **logs** the link.

**Recommended for testers — PIN login** (the "Staff PIN" tab; no email needed). The `demo`
tenant has PIN users seeded via `POST /api/v1/users` (Owner-only). To add/rotate:
```bash
# As an owner JWT:  PUT /api/v1/users/{id}/pin {"pin":"1234"}   (4–8 digits; rate-limited, 15-min lockout)
# or create one:    POST /api/v1/users {"displayName":"…","role":2,"username":"cashier","pin":"…"}
```
Sign in with: **Workspace** `demo` · **Username** + **PIN** (creds shared with the team, not in git).
A PIN user with no email is **not** a platform admin (the `Platform:AdminEmails` allowlist keys on email).

**Magic-link** (the link must be pulled from the log, since it isn't emailed):
```bash
ssh -i ~/.ssh/retailit_incubator azureuser@20.212.21.252 \
  "grep 'MAGIC LINK' /var/log/hms/api.log | tail -1"
#  …MAGIC LINK for owner@…: https://hms.retailit.lk/auth/callback?token=…
```
Open that URL within 15 minutes to sign in.

The seeded **demo** tenant (`slug=demo`, owner `mubeen@aurumhealthtech.com`, also the
platform admin) is ready to use. Create more tenants via `POST /api/v1/tenants`.

### Smoke test

```bash
./infra/incubator/smoke-test.sh   # signup → login → product → shift → POS order → settle
```

### Day-2 ops

```bash
ssh -i ~/.ssh/retailit_incubator azureuser@20.212.21.252
sudo journalctl -u hms-api -f                 # or: tail -f /var/log/hms/api.log
systemctl is-active postgresql nginx pulse hms-api hms-web   # all should be 'active'
sudo systemctl restart hms-api hms-web        # restart HMS only — Pulse untouched

# Backup (control + every tenant DB owned by hms_app):
sudo -u postgres pg_dump hms_main | gzip > ~/hms_main-$(date +%F).sql.gz
for db in $(sudo -u postgres psql -tAc "SELECT datname FROM pg_database WHERE datname LIKE 'hms_tenant_%'"); do
  sudo -u postgres pg_dump "$db" | gzip > ~/$db-$(date +%F).sql.gz; done

# Rollback: redeploy a previous git checkout with ./infra/incubator/redeploy.sh
#           (data in Postgres is unaffected; a schema migration is not auto-reverted — restore from a dump).
```

### TLS renewal
Automatic via certbot's systemd timer (set up during the Pulse deploy). Verify:
`sudo certbot renew --dry-run`.

---

## B) Portable / own-server (Docker Compose)

A self-contained stack for a **dedicated** box (e.g. when HMS graduates off the incubator).
Files: `docker-compose.yml`, `apps/api/Dockerfile`, `apps/web/Dockerfile`, `infra/Caddyfile`,
`infra/bootstrap.sh`, `infra/gen-secrets.sh`, `infra/.env.example`.

```
caddy ──/api/*──► api (:5000) ──► postgres        bootstrap = one-shot: create + migrate hms_control
  └──else──► web (:3000)
```

```bash
# On a box with Docker Engine + compose, ports 80/443 free:
git clone https://github.com/mubs62/rit-hms.git && cd rit-hms
./infra/gen-secrets.sh          # writes ./.env with fresh JWT key + AES master key + DB pw
nano .env                       # set PUBLIC_BASE_URL, SITE_ADDRESS (http:// or a domain), PLATFORM_ADMIN_EMAIL
docker compose up -d --build    # postgres → bootstrap → api → web → caddy
./infra/smoke-test.sh           # end-to-end check (reads the magic link from `docker compose logs api`)
```

`SITE_ADDRESS=http://` serves plain HTTP on an IP; set it to a domain for automatic HTTPS.
Do **not** run this stack on the incubator — its Caddy/Postgres would collide with the
shared nginx/Postgres there.

---

## Secrets & config

| Key (env / `hms.env`) | What | Notes |
|---|---|---|
| `ConnectionStrings__ControlDb` | control DB conn string | incubator: `…Database=hms_main;Username=hms_app;…` |
| `ConnectionStrings__TenantTemplateDb` | per-tenant template (`{tenant_db}` placeholder) | factory substitutes the tenant DB name |
| `Jwt__SigningKey` | HMAC key signing access JWTs | **fresh**, ≥32 chars. *Must not* equal the dev key. |
| `Secrets__MasterKey` | AES-256-GCM key for aggregator creds | **fresh**, base64 of 32 bytes. *Must not* equal the dev key. |
| `Platform__AdminEmails__0` | platform-admin allowlist (config, no DB row) | set to a tenant owner's email so they can also log in |
| `App__WebBaseUrl` | public URL used to build the magic-link | incubator: `https://hms.retailit.lk` |
| `Aggregators__PickMe__PollingEnabled` | PickMe poller | `false` (no live key yet) |
| `Provisioning__MigrationsPath` | where the API reads tenant SQL at signup | incubator: `/opt/hms/db/postgres/migrations` |

The API **refuses to boot in Production** if `Jwt__SigningKey` or `Secrets__MasterKey`
still equal the repo dev defaults (`SecurityStartup.AssertProductionSecrets`). Secrets live
in `/opt/hms/hms.env` (incubator) or `./.env` (Docker) — both gitignored, **never committed**.
In real production, source them from a secret store.

---

## Known limitations & notes (this iteration)

- **Single API instance only.** Scaling >1 node later needs an SSE backplane for the KDS
  (currently in-memory) and PickMe-poller leader election.
- **RLS is dormant.** Tenant isolation is DB-per-tenant + EF Core global query filters; the
  API connects as a role that owns the tenant DBs, so any RLS policies are bypassed —
  acceptable for a test box, same as dev/CI.
- **`UseHttpsRedirection` warning** on boot is expected (the proxy terminates TLS and talks
  HTTP to the API, which has no HTTPS port). No-op, not an error.
- **Magic links are in-memory** (15-min, lost on API restart) with **no email sender** —
  retrieve them from the API log.
- **Tenant DB naming** is `hms_tenant_<slug>` (HMS-native), not the handover's `hms_<slug>`;
  still `hms_`-namespaced and owned by `hms_app`, so coexistence with Pulse holds.
