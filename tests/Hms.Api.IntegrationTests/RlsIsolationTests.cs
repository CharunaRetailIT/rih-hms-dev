using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>
/// Proves Postgres Row-Level Security is a genuine isolation layer, independent of
/// the EF Core global query filter. We seed two tenants' rows into the SAME
/// physical DB as the superuser (which bypasses RLS), then read/write through a
/// context that connects as the non-superuser app role (which OWNS the tables and
/// is therefore subject to the FORCE'd RLS) with the EF filter explicitly disabled.
///
/// Each test guards BOTH halves of the fix at once:
///   • a missing GUC (interceptor) ⇒ the policy compares against NULL ⇒ zero rows
///     visible / writable ⇒ the assertions fail;
///   • a missing FORCE (migration 0051) ⇒ the owning role bypasses RLS ⇒ it sees
///     both tenants / the cross-tenant write succeeds ⇒ the assertions fail.
/// </summary>
[Collection("pg")]
public class RlsIsolationTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Cross_tenant_rows_are_invisible_even_with_query_filter_disabled()
    {
        var tenantA = fx.TenantId;
        var tenantB = Guid.NewGuid();
        var custA = Guid.NewGuid();
        var custB = Guid.NewGuid();

        // Plant both tenants' rows in the one test DB, as the superuser (RLS off).
        await fx.ExecTenantSqlAsAdminAsync(
            "INSERT INTO customers (id, tenant_id, code, name) VALUES " +
            $"('{custA}','{tenantA}','RLS-A','Tenant A'), " +
            $"('{custB}','{tenantB}','RLS-B','Tenant B')");

        // Control: the superuser sees both — they really do coexist physically, so
        // whatever hides B below is RLS, not DB-per-tenant separation.
        await using (var god = fx.NewTenantContext())
        {
            (await god.Customers.IgnoreQueryFilters()
                .CountAsync(c => c.Id == custA || c.Id == custB))
                .Should().Be(2);
        }

        // Tenant A, as the app role, EF filter OFF: must see only its own row.
        await using (var dbA = fx.RlsTenantContext(tenantA))
        {
            var rows = await dbA.Customers.IgnoreQueryFilters()
                .Where(c => c.Id == custA || c.Id == custB).ToListAsync();

            rows.Should().ContainSingle(
                "RLS must hide tenant B even when the EF global query filter is ignored")
                .Which.Id.Should().Be(custA);
            (await dbA.Customers.IgnoreQueryFilters().AnyAsync(c => c.Id == custB))
                .Should().BeFalse();
        }

        // Symmetric: a tenant-B context sees only B, never A.
        await using (var dbB = fx.RlsTenantContext(tenantB))
        {
            (await dbB.Customers.IgnoreQueryFilters().AnyAsync(c => c.Id == custA)).Should().BeFalse();
            (await dbB.Customers.IgnoreQueryFilters().AnyAsync(c => c.Id == custB)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Cannot_write_a_row_for_another_tenant()
    {
        var tenantB = Guid.NewGuid();
        await using var dbA = fx.RlsTenantContext(fx.TenantId);

        // A tenant-A connection trying to plant a tenant-B row: the policy's
        // WITH CHECK (defaulted from its USING clause) rejects it.
        Func<Task> act = () => dbA.Database.ExecuteSqlRawAsync(
            "INSERT INTO customers (id, tenant_id, code, name) VALUES " +
            $"('{Guid.NewGuid()}','{tenantB}','RLS-X','x')");

        (await act.Should().ThrowAsync<PostgresException>())
            .Which.SqlState.Should().Be(PostgresErrorCodes.InsufficientPrivilege); // 42501
    }
}
