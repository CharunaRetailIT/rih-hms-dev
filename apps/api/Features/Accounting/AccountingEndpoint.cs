using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Accounting;

/// <summary>
/// GL / accounting (#73). Chart of accounts, double-entry journals (manual +
/// auto-posting from sales & purchases), expenses, supplier (AP) payments,
/// trial balance, AP aging and a CSV export. Back-office only (Accountant role
/// maps to the BackOffice policy).
/// </summary>
public static class AccountingEndpoint
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/accounting").WithTags("Accounting").RequireAuthorization("BackOffice")
            .AddEndpointFilter(new Hms.Api.Infrastructure.RequireFeatureFilter("accounting"));

        // ── Chart of accounts ──
        g.MapGet("/accounts", async (AccountingService svc, CancellationToken ct, bool? includeInactive) =>
            Results.Ok((await svc.ListAccountsAsync(includeInactive == true, ct))
                .Select(a => new { a.Id, a.Code, a.Name, a.AccountType, a.IsSystem, a.IsActive })))
            .WithName("Accounting.Accounts").WithSummary("Chart of accounts (seeds the default chart on first read).");

        g.MapPost("/accounts", async (AccountInput body, AccountingService svc, CancellationToken ct) =>
        {
            try
            {
                var a = await svc.UpsertAccountAsync(body.Id, body.Code ?? "", body.Name ?? "", body.AccountType ?? "expense", body.IsActive ?? true, ct);
                return Results.Ok(new { a.Id, a.Code, a.Name, a.AccountType, a.IsSystem, a.IsActive });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.UpsertAccount");

        g.MapDelete("/accounts/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var a = await db.GlAccounts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (a is null) return Results.NotFound();
            if (a.IsSystem) return Results.BadRequest(new { error = "Can't delete a system account" });
            a.IsDeleted = true; a.IsActive = false; await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Accounting.DeleteAccount");

        // ── Journals ──
        g.MapGet("/journals", async (AccountingService svc, CancellationToken ct, DateTime? from, DateTime? to, string? source) =>
            Results.Ok((await svc.ListJournalsAsync(from, to, source, ct)).Select(ToJournalDto)))
            .WithName("Accounting.Journals").WithSummary("List journal entries with their debit/credit lines.");

        g.MapPost("/journals", async (ManualJournalInput body, AccountingService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToJournalDto(await svc.PostManualAsync(body, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.PostJournal").WithSummary("Post a manual balanced journal entry.");

        // ── Auto-posting from operational data ──
        g.MapPost("/post/sales", async (AccountingService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId) =>
        {
            try
            {
                var (f, t) = Range(from, to);
                var n = await svc.GenerateSalesJournalsAsync(f, t, locationId, ct);
                await audit.LogAsync("gl.post.sales", "accounting", null, $"Posted {n} sales journals {f:yyyy-MM-dd}…{t:yyyy-MM-dd}", ct);
                return Results.Ok(new { posted = n });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.PostSales").WithSummary("Generate sales journals for settled orders in the period (idempotent).");

        g.MapPost("/post/purchases", async (AccountingService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct, DateTime? from, DateTime? to) =>
        {
            try
            {
                var (f, t) = Range(from, to);
                var n = await svc.GeneratePurchaseJournalsAsync(f, t, ct);
                await audit.LogAsync("gl.post.purchases", "accounting", null, $"Posted {n} purchase journals {f:yyyy-MM-dd}…{t:yyyy-MM-dd}", ct);
                return Results.Ok(new { posted = n });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.PostPurchases").WithSummary("Generate purchase journals for posted GRNs in the period (idempotent).");

        // ── Expenses ──
        g.MapGet("/expenses", async (ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.GlExpenses.AsNoTracking().AsQueryable();
            if (from is { } x) { var d = DateOnly.FromDateTime(OrderService.AsUtc(x)); q = q.Where(e => e.ExpenseDate >= d); }
            if (to is { } y) { var d = DateOnly.FromDateTime(OrderService.AsUtc(y)); q = q.Where(e => e.ExpenseDate <= d); }
            return Results.Ok(await q.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.ExpenseNo)
                .Select(e => new { e.Id, e.ExpenseNo, e.ExpenseDate, e.AccountId, e.Amount, e.Payee, e.PaymentMethod, e.Memo })
                .Take(500).ToListAsync(ct));
        }).WithName("Accounting.Expenses");

        g.MapPost("/expenses", async (ExpenseInput body, AccountingService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            try
            {
                var e = await svc.CreateExpenseAsync(body, ct);
                await audit.LogAsync("gl.expense", "expense", e.Id, $"Expense {e.ExpenseNo} · LKR {e.Amount:F2}", ct);
                return Results.Ok(new { e.Id, e.ExpenseNo, e.ExpenseDate, e.Amount, e.JournalEntryId });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.CreateExpense").WithSummary("Record an expense (posts Dr expense / Cr cash).");

        // ── AP payments ──
        g.MapGet("/ap-payments", async (ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            return Results.Ok(await db.ApPayments.AsNoTracking().OrderByDescending(p => p.PaymentDate)
                .Select(p => new { p.Id, p.PaymentNo, p.PaymentDate, p.SupplierId, p.Amount, p.Reference, p.Memo })
                .Take(500).ToListAsync(ct));
        }).WithName("Accounting.ApPayments");

        g.MapPost("/ap-payments", async (ApPaymentInput body, AccountingService svc, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            try
            {
                var p = await svc.CreateApPaymentAsync(body, ct);
                await audit.LogAsync("gl.ap_payment", "ap_payment", p.Id, $"Supplier payment {p.PaymentNo} · LKR {p.Amount:F2}", ct);
                return Results.Ok(new { p.Id, p.PaymentNo, p.PaymentDate, p.Amount, p.JournalEntryId });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Accounting.CreateApPayment").WithSummary("Record a supplier payment (posts Dr AP / Cr cash).");

        // ── Reports + export ──
        g.MapGet("/trial-balance", async (AccountingService svc, CancellationToken ct, DateTime? from, DateTime? to) =>
            Results.Ok(await svc.TrialBalanceAsync(from, to, ct)))
            .WithName("Accounting.TrialBalance").WithSummary("Per-account debit/credit totals + balance (posted entries).");

        g.MapGet("/ap-aging", async (AccountingService svc, CancellationToken ct) =>
            Results.Ok(await svc.ApAgingAsync(ct)))
            .WithName("Accounting.ApAging").WithSummary("Per-supplier outstanding payable (posted GRNs − AP payments).");

        g.MapGet("/export", async (AccountingService svc, CancellationToken ct, DateTime? from, DateTime? to) =>
        {
            var csv = await svc.ExportCsvAsync(from, to, ct);
            return Results.Text(csv, "text/csv");
        }).WithName("Accounting.Export").WithSummary("CSV export of posted journal lines for external accounting.");

        return app;
    }

    private static (DateTime from, DateTime to) Range(DateTime? from, DateTime? to)
    {
        var f = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var t = to ?? f.AddMonths(1);
        return (f, t);
    }

    private static object ToJournalDto(Domain.GlJournalEntry e) => new
    {
        e.Id, e.EntryNo, e.EntryDate, e.Memo, e.Source, e.SourceRef, e.Status, e.PostedAt, e.LocationId,
        debit = e.Lines.Where(l => !l.IsDeleted).Sum(l => l.Debit),
        credit = e.Lines.Where(l => !l.IsDeleted).Sum(l => l.Credit),
        lines = e.Lines.Where(l => !l.IsDeleted).OrderBy(l => l.Sort)
            .Select(l => new { l.AccountCode, l.AccountName, l.Debit, l.Credit, l.LineMemo }),
    };
}

public record AccountInput(Guid? Id, string? Code, string? Name, string? AccountType, bool? IsActive);
