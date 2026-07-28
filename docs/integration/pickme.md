# PickMe Food — Integration Spec

> Target: Sprint 4 (week 7).
> API: PickMe Food Partner API.

## Why PickMe matters

PickMe is the dominant ride-hail in Sri Lanka with a growing food-delivery
arm. For Sri Lankan outlets, PickMe Food often eclipses Uber Eats on order
volume. The integration shape is similar to Uber Eats (OAuth2 + webhooks +
status callbacks), but the payload shape and reconciliation cadence differ.

## Where this spec is currently thin

We won't fill this in fully until **Sprint 4**, because:

1. PickMe Partner API documentation is provided after sandbox approval, not
   public. The spec below is best-guess based on conversations with other
   integrators and may shift.
2. Building Uber Eats first (Sprint 3) lets us extract the right
   `IDeliveryAggregator` abstraction. PickMe will then be the second
   implementation of that interface — much faster to build than the first.

This file is a placeholder. Updated in Sprint 4 with the real spec.

## What we know today

| Capability | Likely endpoint | Direction |
|---|---|---|
| OAuth2 client credentials | `/oauth/token` | HMS → PickMe |
| Publish menu | `PUT /partner/menu` | HMS → PickMe |
| Update store status | `PATCH /partner/store/status` | HMS → PickMe |
| Receive new orders | `order.created` webhook | PickMe → HMS |
| Accept / reject | `PATCH /partner/orders/{id}` with `action: accept|reject` | HMS → PickMe |
| Status updates | `PATCH /partner/orders/{id}/status` | HMS → PickMe |
| Receive cancellations | `order.cancelled` webhook | PickMe → HMS |
| Settlement reports | `GET /partner/reports/...` | HMS pulls weekly |

## Shared abstraction (defined Sprint 3, used here)

```csharp
public interface IDeliveryAggregator {
    string Name { get; }
    Task PublishMenu(int locationId, CancellationToken ct);
    Task SetStoreOpen(int locationId, bool open, CancellationToken ct);
    Task AcceptOrder(string externalOrderId, CancellationToken ct);
    Task DenyOrder(string externalOrderId, string reason, CancellationToken ct);
    Task UpdateStatus(string externalOrderId, OrderStatus status, CancellationToken ct);
    Task<bool> VerifyWebhookSignature(HttpRequestMessage req);
    AggregatorOrder ParseWebhook(string body);
}
```

`UberEatsAggregator` and `PickMeAggregator` will be two implementations.
DI registration by name; controller picks the right one from the route
parameter `{name}`.

## Schema reuse

No additional schema changes. `OrderSource = 'PICKME'`, `ExternalOrderId`
carries the PickMe order ID. The `AggregatorOutbox` table accommodates any
aggregator.

## Open questions to resolve at Sprint 4 kickoff

1. Does PickMe support real-time stock sync, or only menu-level
   availability (item on/off)?
2. What's their commission structure — flat % or tiered? (Affects
   reconciliation logic.)
3. Are settlement reports CSV pull or webhook push?
4. Do they support partial refunds?
