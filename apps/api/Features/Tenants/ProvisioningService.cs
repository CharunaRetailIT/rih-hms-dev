using System.Text.RegularExpressions;
using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hms.Api.Features.Tenants;

/// <summary>
/// Tenant auto-provisioning: CREATE DATABASE → run every tenant migration in
/// order → seed a minimal baseline (units, VAT, a default outlet, the owner) →
/// flip the tenant to Trialing. Runs inline on signup (no background-job infra
/// yet — a queue can be swapped in here without changing callers). Idempotent:
/// re-running on an already-provisioned tenant is a no-op.
/// </summary>
public sealed class ProvisioningService(ControlDbContext control, IConfiguration config, ILogger<ProvisioningService> logger)
{
    public async Task<bool> ProvisionAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await control.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} not found");

        if (tenant.Status is TenantStatus.Trialing or TenantStatus.Active)
            return true; // already provisioned

        if (!Regex.IsMatch(tenant.DatabaseName, "^[a-z0-9_]+$"))
            throw new InvalidOperationException($"Unsafe database name '{tenant.DatabaseName}'");

        var template = config.GetConnectionString("TenantTemplateDb")
            ?? throw new InvalidOperationException("TenantTemplateDb connection string missing");

        tenant.Status = TenantStatus.Provisioning;
        await control.SaveChangesAsync(ct);

        try
        {
            await CreateDatabaseIfMissing(template, tenant.DatabaseName, ct);

            var tenantConn = template.Replace("{tenant_db}", tenant.DatabaseName);
            var migrationsDir = FindMigrationsDir();

            foreach (var file in TenantMigrationFiles(migrationsDir))
                await RunSql(tenantConn, await File.ReadAllTextAsync(file, ct), ct);

            await SeedBaseline(tenantConn, migrationsDir, tenant, ct);

            tenant.Status = TenantStatus.Trialing;
            tenant.TrialEndsAt ??= DateTime.UtcNow.AddDays(14);
            await control.SaveChangesAsync(ct);
            logger.LogInformation("Provisioned tenant {Slug} ({Db})", tenant.Slug, tenant.DatabaseName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Provisioning failed for tenant {Slug}", tenant.Slug);
            tenant.Status = TenantStatus.Pending;   // allow a retry
            await control.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task CreateDatabaseIfMissing(string template, string dbName, CancellationToken ct)
    {
        // Connect to the maintenance DB to issue CREATE DATABASE (can't run in a tx).
        var adminConn = template.Replace("{tenant_db}", "postgres");
        await using var conn = new NpgsqlConnection(adminConn);
        await conn.OpenAsync(ct);
        await using (var check = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @n", conn))
        {
            check.Parameters.AddWithValue("n", dbName);
            if (await check.ExecuteScalarAsync(ct) is not null) return; // already exists
        }
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn);
        await create.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Ordered tenant migrations — every NNNN_*.sql except the control-plane ones.</summary>
    private static IEnumerable<string> TenantMigrationFiles(string dir) =>
        Directory.EnumerateFiles(dir, "*.sql")
            .Where(f => Regex.IsMatch(Path.GetFileName(f), @"^\d{4}_") && !Path.GetFileName(f).Contains("control"))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal);

    private async Task SeedBaseline(string tenantConn, string migrationsDir, Tenant tenant, CancellationToken ct)
    {
        // provision_baseline.sql lives one level up from migrations/ (db/postgres/).
        var seedPath = Path.Combine(Directory.GetParent(migrationsDir)!.FullName, "provision_baseline.sql");
        var sql = (await File.ReadAllTextAsync(seedPath, ct))
            .Replace("{{TENANT_ID}}", tenant.Id.ToString())
            .Replace("{{OWNER_EMAIL}}", Esc(tenant.OwnerEmail ?? $"owner@{tenant.Slug}.local"))
            .Replace("{{OWNER_NAME}}", Esc($"{tenant.DisplayName} Owner"))
            .Replace("{{BASE_CURRENCY}}", Esc((tenant.DefaultCurrency ?? "LKR").Trim().ToUpperInvariant()))
            .Replace("{{COUNTRY_CODE}}", Esc((tenant.CountryCode ?? "LK").Trim().ToUpperInvariant()))
            .Replace("{{TAX_LABEL}}", Esc(TaxLabelFor(tenant.CountryCode)));
        await RunSql(tenantConn, sql, ct);
    }

    // The consumption tax goes by different names per jurisdiction; default everywhere is VAT.
    private static string TaxLabelFor(string? country) => (country ?? "LK").Trim().ToUpperInvariant() switch
    {
        "IN" or "AU" or "NZ" or "SG" or "MY" or "CA" => "GST",
        "US" => "Sales Tax",
        _ => "VAT",
    };

    private static string Esc(string s) => s.Replace("'", "''");

    private static async Task RunSql(string connStr, string sql, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Locate db/postgres/migrations from config or by walking up from the content root.</summary>
    private string FindMigrationsDir()
    {
        if (config["Provisioning:MigrationsPath"] is { Length: > 0 } configured && Directory.Exists(configured))
            return configured;
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 12 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir, "db", "postgres", "migrations");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException("Could not locate db/postgres/migrations — set Provisioning:MigrationsPath.");
    }
}
