using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// #17/#20 tenant auto-provisioning: signup creates the tenant DB, runs every
/// migration, seeds the baseline (units, VAT, MAIN outlet, owner user) and flips
/// the tenant to Trialing — after which the owner can request a magic link.
/// Creates a REAL throwaway database on the test server and drops it.
/// </summary>
[Collection("pg")]
public class ProvisioningTests(PostgresFixture fx) : IAsyncLifetime
{
    private const string Slug = "provtest";
    private const string Db = "hms_tenant_provtest";

    public async Task InitializeAsync() { await fx.ResetAsync(); await DropDb(); }
    public async Task DisposeAsync() => await DropDb();

    private string AdminConn => fx.TenantTemplate.Replace("{tenant_db}", "postgres");

    private async Task DropDb()
    {
        await using var c = new NpgsqlConnection(AdminConn);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{Db}\" WITH (FORCE)", c);
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Signup_provisions_a_working_tenant()
    {
        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();

        // signup
        var create = await anon.PostAsJsonAsync("/api/v1/tenants", new
        {
            slug = Slug, displayName = "Prov Test Restaurant", ownerEmail = "owner@provtest.local",
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await create.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("provisioned").GetBoolean().Should().BeTrue();

        // tenant flipped to Trialing (status enum 2)
        var bySlug = await anon.GetFromJsonAsync<JsonElement>($"/api/v1/tenants/by-slug/{Slug}");
        bySlug.GetProperty("status").GetInt32().Should().Be(2);

        // the owner exists in the freshly provisioned DB → magic-link succeeds
        var ml = await anon.PostAsJsonAsync("/api/v1/auth/magic-link", new { tenantSlug = Slug, email = "owner@provtest.local" });
        ml.StatusCode.Should().Be(HttpStatusCode.OK);

        // baseline seeded: VAT tax charge + MAIN outlet present
        await using var conn = new NpgsqlConnection(fx.TenantTemplate.Replace("{tenant_db}", Db));
        await conn.OpenAsync();
        await using var check = new NpgsqlCommand(
            "SELECT (SELECT count(*) FROM tax_charges) + (SELECT count(*) FROM locations) + (SELECT count(*) FROM users)", conn);
        Convert.ToInt32(await check.ExecuteScalarAsync()).Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Duplicate_slug_is_rejected()
    {
        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();
        // "it" is the fixture's seeded tenant
        var dup = await anon.PostAsJsonAsync("/api/v1/tenants", new { slug = "it", displayName = "Dup" });
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Bad_slug_is_rejected()
    {
        var anon = new HmsAuthTestFactory(fx.ControlConn, fx.TenantTemplate).CreateClient();
        var bad = await anon.PostAsJsonAsync("/api/v1/tenants", new { slug = "Bad Slug!", displayName = "X" });
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
