using System.Text;
using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Accounting;

/// <summary>
/// General-ledger engine (#73). Double-entry journals: sales (settled orders) and
/// purchases (posted GRNs) are auto-posted into balanced entries; expenses and
/// supplier (AP) payments each post their own. Plus a trial balance, AP aging and
/// a CSV export for the accountant. Every entry balances: Σ debit == Σ credit.
/// </summary>
public class AccountingService(ITenantDbContextFactory factory)
{
    // Well-known system account codes the posting engine references.
    public const string Cash = "1000", Bank = "1010", CardClearing = "1020", AR = "1100",
        InputVat = "1150", Inventory = "1200", AP = "2000", TaxPayable = "2100",
        ServiceChargePayable = "2110", TipsPayable = "2120", CustomerAdvances = "2200",
        SalesRevenue = "4000", SalesDiscounts = "4100", OperatingExpense = "6000", PromoLoyalty = "6010";

    // (code, name, type, isSystem)
    private static readonly (string Code, string Name, string Type)[] DefaultChart =
    {
        (Cash, "Cash on Hand", "asset"), (Bank, "Bank", "asset"), (CardClearing, "Card Clearing", "asset"),
        (AR, "Accounts Receivable", "asset"), (InputVat, "Input VAT Receivable", "asset"),
        (Inventory, "Inventory", "asset"),
        (AP, "Accounts Payable", "liability"), (TaxPayable, "VAT / Tax Payable", "liability"),
        (ServiceChargePayable, "Service Charge Payable", "liability"), (TipsPayable, "Tips Payable", "liability"),
        (CustomerAdvances, "Customer Advances", "liability"),
        (SalesRevenue, "Sales Revenue", "income"), (SalesDiscounts, "Sales Discounts", "income"),
        (OperatingExpense, "Operating Expenses", "expense"), (PromoLoyalty, "Promotions & Loyalty", "expense"),
    };

    /// <summary>Seed the default chart of accounts (idempotent — inserts only missing codes).</summary>
    public async Task EnsureChartAsync(TenantDbContext db, CancellationToken ct)
    {
        var have = await db.GlAccounts.AsNoTracking().Select(a => a.Code).ToListAsync(ct);
        var missing = DefaultChart.Where(d => !have.Contains(d.Code)).ToList();
        if (missing.Count == 0) return;
        foreach (var d in missing)
            db.GlAccounts.Add(new GlAccount { Code = d.Code, Name = d.Name, AccountType = d.Type, IsSystem = true, IsActive = true });
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<GlAccount>> ListAccountsAsync(bool includeInactive, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await EnsureChartAsync(db, ct);
        var q = db.GlAccounts.AsNoTracking();
        if (!includeInactive) q = q.Where(a => a.IsActive);
        return await q.OrderBy(a => a.Code).ToListAsync(ct);
    }

    public async Task<GlAccount> UpsertAccountAsync(Guid? id, string code, string name, string type, bool isActive, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Code and name are required");
        if (type is not ("asset" or "liability" or "equity" or "income" or "expense"))
            throw new InvalidOperationException("Invalid account type");
        GlAccount a;
        if (id is Guid gid)
        {
            a = await db.GlAccounts.FirstOrDefaultAsync(x => x.Id == gid, ct) ?? throw new InvalidOperationException("Account not found");
            a.Name = name.Trim(); a.AccountType = type; a.IsActive = isActive;
            if (!a.IsSystem) a.Code = code.Trim();   // system account codes are fixed (posting engine relies on them)
        }
        else { a = new GlAccount { Code = code.Trim(), Name = name.Trim(), AccountType = type, IsActive = isActive }; db.GlAccounts.Add(a); }
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new InvalidOperationException("An account with that code already exists"); }
        return a;
    }

    // ── posting helpers ──────────────────────────────────────────────────────
    public record LineSpec(string AccountCode, decimal Debit, decimal Credit, string? Memo = null);

    private static async Task<Dictionary<string, GlAccount>> ChartMapAsync(TenantDbContext db, CancellationToken ct) =>
        await db.GlAccounts.AsNoTracking().ToDictionaryAsync(a => a.Code, a => a, ct);

    private async Task<string> NextNoAsync(TenantDbContext db, string prefix, int existingCount) =>
        await Task.FromResult($"{prefix}-{existingCount + 1:D6}");

    /// <summary>Build + add a balanced journal entry from line specs. Throws if unbalanced or empty.</summary>
    private static GlJournalEntry BuildEntry(TenantDbContext db, Dictionary<string, GlAccount> chart,
        string entryNo, DateOnly date, string source, string? sourceRef, string? memo, Guid? locationId,
        IReadOnlyList<LineSpec> specs)
    {
        var live = specs.Where(s => Math.Round(s.Debit, 4) != 0 || Math.Round(s.Credit, 4) != 0).ToList();
        if (live.Count == 0) throw new InvalidOperationException("A journal needs at least one non-zero line");
        var dr = live.Sum(s => s.Debit); var cr = live.Sum(s => s.Credit);
        if (Math.Abs(Math.Round(dr - cr, 2)) > 0.001m)
            throw new InvalidOperationException($"Journal does not balance: debits {dr:F2} ≠ credits {cr:F2}");

        var entry = new GlJournalEntry
        {
            EntryNo = entryNo, EntryDate = date, Source = source, SourceRef = sourceRef, Memo = memo,
            LocationId = locationId, Status = "posted", PostedAt = DateTime.UtcNow,
        };
        var sort = 0;
        foreach (var s in live)
        {
            if (!chart.TryGetValue(s.AccountCode, out var acct))
                throw new InvalidOperationException($"Account {s.AccountCode} is not in the chart of accounts");
            entry.Lines.Add(new GlJournalLine
            {
                TenantId = db.TenantId, AccountId = acct.Id, AccountCode = acct.Code, AccountName = acct.Name,
                Debit = Math.Round(s.Debit, 4), Credit = Math.Round(s.Credit, 4), LineMemo = s.Memo,
                Sort = sort++, CreatedAt = DateTime.UtcNow,
            });
        }
        db.GlJournalEntries.Add(entry);
        return entry;
    }

    /// <summary>Post a manual, accountant-typed balanced journal.</summary>
    public async Task<GlJournalEntry> PostManualAsync(ManualJournalInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await PeriodGuard.EnsureOpenAsync(db, input.EntryDate.ToDateTime(TimeOnly.MinValue), ct);
        await EnsureChartAsync(db, ct);
        var chart = await ChartMapAsync(db, ct);
        var count = await db.GlJournalEntries.CountAsync(ct);
        var specs = (input.Lines ?? new()).Select(l => new LineSpec(l.AccountCode, l.Debit, l.Credit, l.Memo)).ToList();
        var entry = BuildEntry(db, chart, await NextNoAsync(db, "JE", count), input.EntryDate, "manual", null, input.Memo, input.LocationId, specs);
        await db.SaveChangesAsync(ct);
        return entry;
    }

    /// <summary>Map a POS tender type to the GL account it debits.</summary>
    private static string TenderAccount(string payType) => payType switch
    {
        "cash" => Cash,
        "card" => CardClearing,
        "ubereats_prepaid" or "pickme_prepaid" => AR,
        "credit" => AR,
        "loyalty" => PromoLoyalty,
        "advance" => CustomerAdvances,
        _ => Cash,
    };

    /// <summary>
    /// Auto-post a balanced sales journal for every settled order in the period
    /// that doesn't already have one. Returns how many entries were created.
    /// </summary>
    public async Task<int> GenerateSalesJournalsAsync(DateTime from, DateTime to, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await EnsureChartAsync(db, ct);
        var chart = await ChartMapAsync(db, ct);
        var fromU = OrderService.AsUtc(from); var toU = OrderService.AsUtc(to);

        var ordersQ = db.Orders.AsNoTracking().Where(o => o.Status == "settled" && o.SettledAt >= fromU && o.SettledAt < toU);
        if (locationId is Guid lid) ordersQ = ordersQ.Where(o => o.LocationId == lid);
        var orders = await ordersQ.OrderBy(o => o.SettledAt).ToListAsync(ct);
        if (orders.Count == 0) return 0;

        var already = (await db.GlJournalEntries.AsNoTracking()
            .Where(e => e.Source == "sales").Select(e => e.SourceRef).ToListAsync(ct)).ToHashSet();
        var orderIds = orders.Select(o => o.Id).ToList();
        var paymentsByOrder = (await db.Payments.AsNoTracking().Where(p => orderIds.Contains(p.OrderId)).ToListAsync(ct))
            .GroupBy(p => p.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        var count = await db.GlJournalEntries.CountAsync(ct);
        var made = 0;
        foreach (var o in orders)
        {
            if (already.Contains(o.OrderNumber)) continue;
            var pays = paymentsByOrder.GetValueOrDefault(o.Id) ?? new List<Payment>();
            var specs = new List<LineSpec>();
            // Debits: tenders (base-currency value), grouped by account.
            foreach (var grp in pays.GroupBy(p => TenderAccount(p.PayType)))
                specs.Add(new LineSpec(grp.Key, grp.Sum(p => p.BaseAmount > 0 ? p.BaseAmount : p.Amount), 0, "Tender"));
            // Debit: discounts (contra-revenue).
            var disc = o.DiscountAmount + o.PromotionDiscountAmount;
            if (disc > 0) specs.Add(new LineSpec(SalesDiscounts, disc, 0, "Discounts"));
            // Credits: revenue, service charge, tax, tips.
            specs.Add(new LineSpec(SalesRevenue, 0, o.SubtotalAmount, "Sales"));
            if (o.ServiceChargeAmount > 0) specs.Add(new LineSpec(ServiceChargePayable, 0, o.ServiceChargeAmount, "Service charge"));
            if (o.TaxAmount > 0) specs.Add(new LineSpec(TaxPayable, 0, o.TaxAmount, "VAT/SSCL"));
            if (o.TipAmount > 0) specs.Add(new LineSpec(TipsPayable, 0, o.TipAmount, "Tip"));

            try
            {
                BuildEntry(db, chart, await NextNoAsync(db, "JE", count + made), o.SettledAt is { } s ? DateOnly.FromDateTime(s) : DateOnly.FromDateTime(o.OpenedAt),
                    "sales", o.OrderNumber, $"Sale {o.InvoiceNumber ?? o.OrderNumber}", o.LocationId, specs);
                made++;
            }
            catch (InvalidOperationException) { /* skip a malformed/zero order rather than abort the batch */ }
        }
        if (made > 0) await db.SaveChangesAsync(ct);
        return made;
    }

    /// <summary>Auto-post a purchase journal (Dr Inventory + Input VAT, Cr AP) for each posted GRN in the period.</summary>
    public async Task<int> GeneratePurchaseJournalsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await EnsureChartAsync(db, ct);
        var chart = await ChartMapAsync(db, ct);
        var fromD = DateOnly.FromDateTime(OrderService.AsUtc(from)); var toD = DateOnly.FromDateTime(OrderService.AsUtc(to));

        var grns = await db.GoodsReceivedNotes.AsNoTracking()
            .Where(g => g.Status == "posted" && g.ReceivedDate >= fromD && g.ReceivedDate <= toD)
            .OrderBy(g => g.ReceivedDate).ToListAsync(ct);
        if (grns.Count == 0) return 0;
        var already = (await db.GlJournalEntries.AsNoTracking()
            .Where(e => e.Source == "purchase").Select(e => e.SourceRef).ToListAsync(ct)).ToHashSet();

        var count = await db.GlJournalEntries.CountAsync(ct);
        var made = 0;
        foreach (var g in grns)
        {
            if (already.Contains(g.GrnNumber)) continue;
            var specs = new List<LineSpec> { new(Inventory, g.TotalCost, 0, "Goods received") };
            if (g.TaxAmount > 0) specs.Add(new LineSpec(InputVat, g.TaxAmount, 0, "Input VAT"));
            specs.Add(new LineSpec(AP, 0, g.TotalCost + g.TaxAmount, "Supplier payable"));
            try { BuildEntry(db, chart, await NextNoAsync(db, "JE", count + made), g.ReceivedDate, "purchase", g.GrnNumber, $"GRN {g.GrnNumber}", g.LocationId, specs); made++; }
            catch (InvalidOperationException) { }
        }
        if (made > 0) await db.SaveChangesAsync(ct);
        return made;
    }

    /// <summary>Capture an expense and post its journal (Dr expense account, Cr cash/bank).</summary>
    public async Task<GlExpense> CreateExpenseAsync(ExpenseInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (input.Amount <= 0) throw new InvalidOperationException("Amount must be greater than zero");
        await PeriodGuard.EnsureOpenAsync(db, input.ExpenseDate.ToDateTime(TimeOnly.MinValue), ct);
        await EnsureChartAsync(db, ct);
        var chart = await ChartMapAsync(db, ct);
        var acct = await db.GlAccounts.FirstOrDefaultAsync(a => a.Id == input.AccountId, ct)
            ?? throw new InvalidOperationException("Expense account not found");
        var pay = await db.GlAccounts.FirstOrDefaultAsync(a => a.Id == input.PaymentAccountId, ct)
            ?? throw new InvalidOperationException("Payment account not found");

        var eCount = await db.GlExpenses.CountAsync(ct);
        var jCount = await db.GlJournalEntries.CountAsync(ct);
        var entry = BuildEntry(db, chart, await NextNoAsync(db, "JE", jCount), input.ExpenseDate, "expense", null,
            $"Expense — {input.Payee ?? acct.Name}", input.LocationId,
            new List<LineSpec> { new(acct.Code, input.Amount, 0, input.Memo), new(pay.Code, 0, input.Amount, input.Payee) });

        var exp = new GlExpense
        {
            ExpenseNo = await NextNoAsync(db, "EXP", eCount), ExpenseDate = input.ExpenseDate, AccountId = acct.Id,
            Amount = Math.Round(input.Amount, 4), Payee = input.Payee, PaymentAccountId = pay.Id,
            PaymentMethod = input.PaymentMethod, Memo = input.Memo, LocationId = input.LocationId, JournalEntryId = entry.Id,
        };
        db.GlExpenses.Add(exp);
        await db.SaveChangesAsync(ct);
        return exp;
    }

    /// <summary>Record a supplier payment and post its journal (Dr AP, Cr cash/bank).</summary>
    public async Task<ApPayment> CreateApPaymentAsync(ApPaymentInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (input.Amount <= 0) throw new InvalidOperationException("Amount must be greater than zero");
        await PeriodGuard.EnsureOpenAsync(db, input.PaymentDate.ToDateTime(TimeOnly.MinValue), ct);
        await EnsureChartAsync(db, ct);
        var chart = await ChartMapAsync(db, ct);
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == input.SupplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found");
        var pay = await db.GlAccounts.FirstOrDefaultAsync(a => a.Id == input.PaymentAccountId, ct)
            ?? throw new InvalidOperationException("Payment account not found");

        var pCount = await db.ApPayments.CountAsync(ct);
        var jCount = await db.GlJournalEntries.CountAsync(ct);
        var entry = BuildEntry(db, chart, await NextNoAsync(db, "JE", jCount), input.PaymentDate, "payment", null,
            $"Payment to {supplier.Name}", null,
            new List<LineSpec> { new(AP, input.Amount, 0, supplier.Name), new(pay.Code, 0, input.Amount, input.Reference) });

        var p = new ApPayment
        {
            PaymentNo = await NextNoAsync(db, "APP", pCount), SupplierId = supplier.Id, PaymentDate = input.PaymentDate,
            Amount = Math.Round(input.Amount, 4), PaymentAccountId = pay.Id, Reference = input.Reference,
            Memo = input.Memo, JournalEntryId = entry.Id,
        };
        db.ApPayments.Add(p);
        await db.SaveChangesAsync(ct);
        return p;
    }

    /// <summary>Trial balance: per-account debit/credit totals + balance from posted lines up to <paramref name="to"/>.</summary>
    public async Task<TrialBalance> TrialBalanceAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await EnsureChartAsync(db, ct);
        var toD = to is { } t ? DateOnly.FromDateTime(OrderService.AsUtc(t)) : DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly? fromD = from is { } f ? DateOnly.FromDateTime(OrderService.AsUtc(f)) : null;

        var postedIds = db.GlJournalEntries.Where(e => e.Status == "posted" && e.EntryDate <= toD
            && (fromD == null || e.EntryDate >= fromD)).Select(e => e.Id);
        var rows = await db.GlJournalLines.AsNoTracking().Where(l => postedIds.Contains(l.EntryId))
            .GroupBy(l => new { l.AccountCode, l.AccountName })
            .Select(g => new { g.Key.AccountCode, g.Key.AccountName, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
            .ToListAsync(ct);

        var lines = rows.OrderBy(r => r.AccountCode)
            .Select(r => new TrialBalanceRow(r.AccountCode, r.AccountName, r.Debit, r.Credit, r.Debit - r.Credit))
            .ToList();
        return new TrialBalance(fromD, toD, lines, lines.Sum(l => l.Debit), lines.Sum(l => l.Credit));
    }

    /// <summary>AP aging: per-supplier balance = Σ posted GRN (goods + input VAT) − Σ AP payments.</summary>
    public async Task<List<ApAgingRow>> ApAgingAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var billed = await db.GoodsReceivedNotes.AsNoTracking().Where(g => g.Status == "posted")
            .GroupBy(g => g.SupplierId)
            .Select(g => new { SupplierId = g.Key, Amount = g.Sum(x => x.TotalCost + x.TaxAmount) }).ToListAsync(ct);
        var paid = await db.ApPayments.AsNoTracking().GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Amount = g.Sum(x => x.Amount) }).ToListAsync(ct);
        var suppliers = await db.Suppliers.AsNoTracking().Select(s => new { s.Id, s.Code, s.Name, s.PaymentTermsDays }).ToListAsync(ct);

        var paidById = paid.ToDictionary(x => x.SupplierId, x => x.Amount);
        return billed.Select(b =>
            {
                var s = suppliers.FirstOrDefault(x => x.Id == b.SupplierId);
                var bal = Math.Round(b.Amount - paidById.GetValueOrDefault(b.SupplierId), 4);
                return new ApAgingRow(b.SupplierId, s?.Code ?? "?", s?.Name ?? "(deleted)", s?.PaymentTermsDays ?? 0,
                    b.Amount, paidById.GetValueOrDefault(b.SupplierId), bal);
            })
            .Where(r => Math.Abs(r.Balance) > 0.009m)
            .OrderByDescending(r => r.Balance).ToList();
    }

    public async Task<List<GlJournalEntry>> ListJournalsAsync(DateTime? from, DateTime? to, string? source, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.GlJournalEntries.AsNoTracking().Include(e => e.Lines).AsQueryable();
        if (from is { } f) { var fd = DateOnly.FromDateTime(OrderService.AsUtc(f)); q = q.Where(e => e.EntryDate >= fd); }
        if (to is { } t) { var td = DateOnly.FromDateTime(OrderService.AsUtc(t)); q = q.Where(e => e.EntryDate <= td); }
        if (!string.IsNullOrWhiteSpace(source)) q = q.Where(e => e.Source == source);
        return await q.OrderByDescending(e => e.EntryDate).ThenByDescending(e => e.EntryNo).Take(500).ToListAsync(ct);
    }

    /// <summary>Flatten posted journal lines into a CSV for import into external accounting.</summary>
    public async Task<string> ExportCsvAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var entries = await ListJournalsAsync(from, to, null, ct);
        var sb = new StringBuilder();
        sb.AppendLine("entry_no,date,source,reference,account_code,account_name,debit,credit,memo");
        static string Q(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        foreach (var e in entries.Where(e => e.Status == "posted").OrderBy(e => e.EntryDate).ThenBy(e => e.EntryNo))
            foreach (var l in e.Lines.Where(l => !l.IsDeleted).OrderBy(l => l.Sort))
                sb.AppendLine(string.Join(',', new[]
                {
                    Q(e.EntryNo), e.EntryDate.ToString("yyyy-MM-dd"), Q(e.Source), Q(e.SourceRef),
                    Q(l.AccountCode), Q(l.AccountName), l.Debit.ToString("0.00"), l.Credit.ToString("0.00"), Q(l.LineMemo),
                }));
        return sb.ToString();
    }
}

// ── DTOs ──
public record ManualJournalInput(DateOnly EntryDate, string? Memo, Guid? LocationId, List<ManualLineInput>? Lines);
public record ManualLineInput(string AccountCode, decimal Debit, decimal Credit, string? Memo);
public record ExpenseInput(DateOnly ExpenseDate, Guid AccountId, decimal Amount, string? Payee,
    Guid PaymentAccountId, string? PaymentMethod, string? Memo, Guid? LocationId);
public record ApPaymentInput(DateOnly PaymentDate, Guid SupplierId, decimal Amount, Guid PaymentAccountId, string? Reference, string? Memo);

public record TrialBalance(DateOnly? From, DateOnly To, List<TrialBalanceRow> Rows, decimal TotalDebit, decimal TotalCredit);
public record TrialBalanceRow(string Code, string Name, decimal Debit, decimal Credit, decimal Balance);
public record ApAgingRow(Guid SupplierId, string Code, string Name, int TermsDays, decimal Billed, decimal Paid, decimal Balance);
