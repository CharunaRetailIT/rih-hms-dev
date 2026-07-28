using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Hms.Api.Features.Kitchen;

/// <summary>
/// In-memory pub/sub that pushes "the kitchen board changed" signals to any
/// connected SSE clients for a tenant, so the KDS updates in real time instead
/// of waiting for its safety poll. A new ticket (order sent), a bump
/// (preparing/ready/served), a settle (PAID badge) or a void all publish here.
///
/// Single-process only: subscribers live in this instance's memory. For a
/// multi-instance deployment this would sit behind Postgres LISTEN/NOTIFY or a
/// Redis backplane so a publish on one node reaches SSE clients on another.
/// </summary>
public sealed class KitchenBroadcaster
{
    // tenant -> subscriberId -> channel. Bounded + drop-oldest: a slow/stuck
    // client can never back-pressure publishers or leak unbounded memory.
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<string>>> _tenants = new();

    /// <summary>Subscribe a client; dispose the handle to unsubscribe.</summary>
    public (IDisposable handle, ChannelReader<string> reader) Subscribe(Guid tenantId)
    {
        var subId = Guid.NewGuid();
        var ch = Channel.CreateBounded<string>(new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
        var subs = _tenants.GetOrAdd(tenantId, _ => new());
        subs[subId] = ch;
        return (new Unsubscriber(() =>
        {
            if (_tenants.TryGetValue(tenantId, out var s)) s.TryRemove(subId, out _);
        }), ch.Reader);
    }

    /// <summary>Notify every connected client for a tenant that the board changed.</summary>
    public void Publish(Guid tenantId, string evt = "changed")
    {
        if (_tenants.TryGetValue(tenantId, out var subs))
            foreach (var ch in subs.Values)
                ch.Writer.TryWrite(evt);   // drop-oldest channel never blocks
    }

    private sealed class Unsubscriber(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;
        public void Dispose() { _onDispose?.Invoke(); _onDispose = null; }
    }
}
