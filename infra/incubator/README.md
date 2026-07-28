# Incubator deploy (bare-metal, live)

This is the **live test/staging deployment** of RIT HMS, running on the shared RIT
incubator VM (`hms.retailit.lk` → `20.212.21.252`) **alongside Retail Pulse** — not
in Docker. The box uses system Postgres, system nginx, systemd, and certbot, per the
incubator conventions. Full runbook: [`docs/deploy.md`](../../docs/deploy.md).

| File | What |
|------|------|
| `hms-api.service` | systemd unit — .NET API (self-contained) on `127.0.0.1:8002`. |
| `hms-web.service` | systemd unit — Next.js standalone server on `127.0.0.1:8003`. |
| `hms.retailit.lk.nginx` | nginx vhost (HTTP-only template; certbot adds TLS). |
| `redeploy.sh` | Build locally → rsync → restart. Use for code updates. |
| `smoke-test.sh` | End-to-end signup → login → POS order → settle against the live URL. |

**Why bare-metal and not the repo's `docker-compose.yml`?** The incubator already runs
nginx (owns :80/:443) and a shared Postgres (:5432); a Docker stack with its own Caddy +
Postgres would collide. The Compose stack (repo root) is the **portable / own-server**
option — see `docs/deploy.md` §B and the handover's "graduate to its own server".

**Stack note:** the handover templates a Python/FastAPI + Angular deploy (Retail Pulse).
HMS is **.NET 8 + Next.js**, so this deploy adapts those conventions: a self-contained
.NET binary (no runtime installed on the box), the Next.js standalone Node server (Node 20
installed), and **two** systemd units / ports (`:8002` API + `:8003` web; next product → `:8004`).
