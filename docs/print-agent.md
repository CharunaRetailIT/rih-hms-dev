# Print Agent (#16) — contract

The **Print Agent** is a thin process that runs at the venue (one per outlet, e.g.
a small .NET/Node service or a Raspberry Pi) and prints KOT chits + bills to
thermal hardware. The server side (this repo) is a durable **print-job queue**;
the agent binary is a separate deliverable that drives the contract below.

## Why a queue (not direct printing)
The cloud API can't reach a printer behind the venue's LAN/NAT. So the API
**queues** jobs and the agent **pulls** them — the same pull/mirror model used for
PickMe. Jobs survive restarts and are retried until acked.

## Endpoints (tenant JWT today; a per-agent token is a follow-on)
- `POST /api/v1/print/jobs` — enqueue `{ locationId, kind: "kot"|"bill", orderId?, payload, printerName? }`.
  `payload` is the receipt/ticket markup (the POS already renders 80mm HTML; an
  agent may convert to ESC/POS).
- `GET  /api/v1/print/jobs?locationId=&status=queued` — the agent's work list (oldest first).
- `POST /api/v1/print/jobs/{id}/ack` — `{ status: "printed"|"failed", error? }`. `printed` removes it from the queue.

## Agent loop (reference)
1. Poll `GET /print/jobs?locationId=<this outlet>` every ~2–5s.
2. For each job: render `payload` → send to the mapped printer (`printerName`).
3. `POST /jobs/{id}/ack { status }`. On failure, ack `failed` with the error; an
   operator can requeue.

## Wiring (follow-on)
- POS "Send to KOT" auto-print and "Print bill" can **enqueue** instead of using the
  browser dialog when an agent is configured for the outlet (a per-outlet toggle).
- Dedicated per-agent auth token (like the PickMe per-outlet key) instead of a
  tenant JWT.
- Multi-printer routing by station (`printerName` already carried on the job).
