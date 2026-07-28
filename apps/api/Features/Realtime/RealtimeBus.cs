using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Hms.Api.Features.Realtime;

/// <summary>
/// General-purpose in-memory pub/sub that pushes lightweight "topic changed"
/// signals to any connected SSE client for a tenant — the app-wide sibling of
/// the KDS-specific <c>KitchenBroadcaster</c>. The notification bell, dashboard
/// and delivery board subscribe to one stream and react to the topics they care
/// about (e.g. "notifications", "orders") instead of polling.
///
/// The published value IS the topic string; the SSE endpoint writes it on the
/// data line so the browser can route it. Bounded + drop-oldest per subscriber,
/// so a slow client can never back-pressure publishers or leak memory.
///
/// Single-process only (subscribers live in this instance's memory). For a
/// multi-instance deployment this would sit behind Postgres LISTEN/NOTIFY or a
/// Redis backplane so a publish on one node reaches SSE clients on another.
/// </summary>
public sealed class RealtimeBus
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Subscriber>> _tenants = new();
    private sealed record Subscriber(Channel<string> Channel, Guid? UserId);

    /// <summary>
    /// Subscribe a client; dispose the handle to unsubscribe. The reader yields topic strings.
    /// <paramref name="userId"/> (the connecting staff member) is tracked so a floor-scoped
    /// publish (<see cref="PublishToUsers"/>) can target just them — unscoped <see cref="Publish"/>
    /// still reaches everyone regardless of userId, unchanged.
    /// </summary>
    public (IDisposable handle, ChannelReader<string> reader) Subscribe(Guid tenantId, Guid? userId = null)
    {
        var subId = Guid.NewGuid();
        var ch = Channel.CreateBounded<string>(new BoundedChannelOptions(32)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
        var subs = _tenants.GetOrAdd(tenantId, _ => new());
        subs[subId] = new Subscriber(ch, userId);
        return (new Unsubscriber(() =>
        {
            if (_tenants.TryGetValue(tenantId, out var s)) s.TryRemove(subId, out _);
        }), ch.Reader);
    }

    /// <summary>Notify every connected client for a tenant that <paramref name="topic"/> changed.</summary>
    public void Publish(Guid tenantId, string topic)
    {
        if (_tenants.TryGetValue(tenantId, out var subs))
            foreach (var s in subs.Values)
                s.Channel.Writer.TryWrite(topic);   // drop-oldest channel never blocks
    }

    /// <summary>
    /// Notify only the given users (e.g. the stewards covering the floor a guest order
    /// landed on) that <paramref name="topic"/> changed — everyone else's connection is
    /// untouched. Callers should fall back to unscoped <see cref="Publish"/> when
    /// <paramref name="userIds"/> is empty (e.g. no stewards configured yet) so nothing
    /// silently goes unnoticed.
    /// </summary>
    public void PublishToUsers(Guid tenantId, string topic, IReadOnlyCollection<Guid> userIds)
    {
        if (userIds.Count == 0 || !_tenants.TryGetValue(tenantId, out var subs)) return;
        foreach (var s in subs.Values)
            if (s.UserId is Guid uid && userIds.Contains(uid))
                s.Channel.Writer.TryWrite(topic);
    }

    private sealed class Unsubscriber(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;
        public void Dispose() { _onDispose?.Invoke(); _onDispose = null; }
    }
}
