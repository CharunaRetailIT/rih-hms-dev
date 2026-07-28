using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Sets the <c>app.tenant_id</c> Postgres session GUC on every tenant-DB
/// connection as it opens, so the Row-Level Security policies
/// (<c>tenant_id::text = current_setting('app.tenant_id', true)</c>) resolve to
/// this context's tenant.
///
/// <para>This is one half of making RLS a real defence-in-depth layer; the other
/// half is <c>FORCE ROW LEVEL SECURITY</c> (migration 0051), which stops the
/// owning app role from bypassing RLS. Without the GUC, every policy compares
/// against <c>NULL</c> and matches nothing, so once RLS is enforced a connection
/// that forgot to set it would simply see (and be able to write) zero rows.</para>
///
/// <para>One interceptor instance is created per <see cref="TenantDbContext"/> and
/// captures that context's tenant id, so the value written is unambiguous no
/// matter which physical pooled connection EF hands us.</para>
///
/// <para><b>Pooling safety.</b> The GUC is (re)written on <i>every</i> connection
/// open — not just the first — before any query runs on the connection. So a
/// stale tenant id left on a pooled connection by an earlier request is always
/// overwritten with the correct one here; there is no window in which a query
/// could observe the previous borrower's value. That is why we don't need
/// <c>SET LOCAL</c>/an explicit transaction or a reset-on-return hook. We use
/// session scope (<c>set_config(..., is_local =&gt; false)</c>) deliberately: EF
/// often executes in autocommit, where <c>SET LOCAL</c> would survive only for
/// the single implicit statement.</para>
/// </summary>
public sealed class TenantGucConnectionInterceptor(Guid tenantId) : DbConnectionInterceptor
{
    // tenantId is a Guid, so its text form is always [0-9a-f-] — safe to inline,
    // and parameters aren't available this early in the connection lifecycle.
    private string SetGucSql => $"SELECT set_config('app.tenant_id', '{tenantId}', false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = SetGucSql;
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = SetGucSql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
