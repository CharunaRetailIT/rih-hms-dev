using System.Net;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Hms.Api.Features.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// App-wide SSE stream (`/api/v1/events/stream`). Verifies a connected client
/// gets the right content-type + a "connected" hello, and that a topic
/// published on the in-process <see cref="RealtimeBus"/> reaches that client —
/// the mechanism that pushes the notification bell + delivery board live.
/// </summary>
[Collection("pg")]
public class RealtimeTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static string Token(PostgresFixture fx)
    {
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars")), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: "https://localhost:5001", audience: "rit-hms-api",
            claims: new[] { new Claim("tenant_id", fx.TenantId.ToString()), new Claim("role", "Owner") },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    [Fact]
    public async Task A_published_topic_reaches_a_connected_client()
    {
        var host = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate);
        var client = host.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/events/stream");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(fx));
        var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/event-stream");

        await using var stream = await res.Content.ReadAsStreamAsync(cts.Token);
        using var rd = new StreamReader(stream);
        var bus = host.Services.GetRequiredService<RealtimeBus>();

        bool sawConnected = false, sawTopic = false;
        while (!cts.IsCancellationRequested)
        {
            var line = await rd.ReadLineAsync(cts.Token);
            if (line is null) break;
            if (line.Contains("connected"))
            {
                sawConnected = true;
                bus.Publish(fx.TenantId, "notifications");   // publish only once a subscriber is live
            }
            else if (sawConnected && line.Contains("notifications"))
            {
                sawTopic = true;
                break;
            }
        }

        sawConnected.Should().BeTrue("the stream sends a 'connected' hello on open");
        sawTopic.Should().BeTrue("a published topic is pushed to the connected client");
    }
}
