using FluentAssertions;
using Hms.Api.Domain;
using Hms.Api.Features.Accounting;
using Hms.Api.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

/// <summary>#73 GL / accounting — chart, double-entry posting (sales/purchase),
/// expenses, AP payments, trial balance, AP aging, CSV export.</summary>
[Collection("pg")]
public class AccountingTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private AccountingService Acct() => new(fx.Factory());

    [Fact]
    public async Task Default_chart_is_seeded_on_first_read()
    {
        var accounts = await Acct().ListAccountsAsync(false, default);
        accounts.Should().Contain(a => a.Code == "1000" && a.IsSystem);
        accounts.Should().Contain(a => a.Code == "4000");   // sales revenue
        accounts.Should().Contain(a => a.Code == "2000");   // accounts payable
    }

    [Fact]
    public async Task Manual_journal_must_balance()
    {
        var acct = Acct();
        var bad = async () => await acct.PostManualAsync(new ManualJournalInput(DateOnly.FromDateTime(DateTime.UtcNow), "test", null,
            new() { new ManualLineInput("1000", 100, 0, null), new ManualLineInput("4000", 0, 90, null) }), default);
        await bad.Should().ThrowAsync<InvalidOperationException>().WithMessage("*does not balance*");

        var ok = await acct.PostManualAsync(new ManualJournalInput(DateOnly.FromDateTime(DateTime.UtcNow), "test", null,
            new() { new ManualLineInput("1000", 100, 0, null), new ManualLineInput("4000", 0, 100, null) }), default);
        ok.Status.Should().Be("posted");
        ok.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Settled_sale_posts_a_balanced_journal_and_is_idempotent()
    {
        var (orders, _) = fx.Services();
        var o = await orders.CreateAsync(new CreateOrderInput(fx.LocationId, "dine_in", null, null, "1", 2, null), default);
        await orders.AddItemAsync(o.Id, new AddItemInput(fx.ChickenKottuId, 1, "kitchen", null), default);
        var full = (await orders.GetAsync(o.Id, default))!;
        await orders.SettleAsync(o.Id, new SettleInput(new() { new PaymentInput("cash", full.TotalAmount, null) }), default);

        var acct = Acct();
        var made = await acct.GenerateSalesJournalsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null, default);
        made.Should().Be(1);

        await using (var db = fx.NewTenantContext())
        {
            var je = await db.GlJournalEntries.Include(e => e.Lines).FirstAsync(e => e.Source == "sales" && e.SourceRef == full.OrderNumber);
            je.Lines.Sum(l => l.Debit).Should().Be(je.Lines.Sum(l => l.Credit), "every journal balances");
            je.Lines.First(l => l.AccountCode == AccountingService.Cash).Debit.Should().Be(full.TotalAmount);
            je.Lines.First(l => l.AccountCode == AccountingService.SalesRevenue).Credit.Should().Be(full.SubtotalAmount);
        }

        // Re-running posts nothing new (idempotent on order number).
        (await acct.GenerateSalesJournalsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null, default)).Should().Be(0);
    }

    [Fact]
    public async Task Posted_GRN_posts_a_purchase_journal_and_shows_in_AP_aging()
    {
        Guid supplierId;
        await using (var seed = fx.NewTenantContext())
        {
            var s = new Supplier { Code = "ACME", Name = "Acme Foods", PaymentTermsDays = 30, IsActive = true };
            seed.Suppliers.Add(s);
            await seed.SaveChangesAsync();          // persist first so the FK (supplier_id) resolves
            supplierId = s.Id;
            seed.GoodsReceivedNotes.Add(new GoodsReceivedNote
            {
                LocationId = fx.LocationId, SupplierId = supplierId, GrnNumber = "GRN-T1", Status = "posted",
                ReceivedDate = DateOnly.FromDateTime(DateTime.UtcNow), TotalCost = 1000m, TaxAmount = 180m,
            });
            await seed.SaveChangesAsync();
        }

        var acct = Acct();
        (await acct.GeneratePurchaseJournalsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), default)).Should().Be(1);

        await using (var db = fx.NewTenantContext())
        {
            var je = await db.GlJournalEntries.Include(e => e.Lines).FirstAsync(e => e.Source == "purchase" && e.SourceRef == "GRN-T1");
            je.Lines.First(l => l.AccountCode == AccountingService.Inventory).Debit.Should().Be(1000m);
            je.Lines.First(l => l.AccountCode == AccountingService.InputVat).Debit.Should().Be(180m);
            je.Lines.First(l => l.AccountCode == AccountingService.AP).Credit.Should().Be(1180m);
        }

        var aging = await acct.ApAgingAsync(default);
        aging.Should().ContainSingle(r => r.SupplierId == supplierId).Which.Balance.Should().Be(1180m);

        // A partial payment reduces the AP balance.
        var payAcct = (await acct.ListAccountsAsync(false, default)).First(a => a.Code == AccountingService.Cash);
        await acct.CreateApPaymentAsync(new ApPaymentInput(DateOnly.FromDateTime(DateTime.UtcNow), supplierId, 500m, payAcct.Id, "chq#1", null), default);
        (await acct.ApAgingAsync(default)).First(r => r.SupplierId == supplierId).Balance.Should().Be(680m);
    }

    [Fact]
    public async Task Expense_posts_and_trial_balance_balances()
    {
        var acct = Acct();
        var accounts = await acct.ListAccountsAsync(false, default);
        var expAcct = accounts.First(a => a.Code == AccountingService.OperatingExpense);
        var cash = accounts.First(a => a.Code == AccountingService.Cash);

        var e = await acct.CreateExpenseAsync(new ExpenseInput(DateOnly.FromDateTime(DateTime.UtcNow), expAcct.Id, 250m,
            "Gas station", cash.Id, "cash", "LPG refill", null), default);
        e.JournalEntryId.Should().NotBeNull();

        var tb = await acct.TrialBalanceAsync(null, null, default);
        tb.TotalDebit.Should().Be(tb.TotalCredit, "the trial balance always balances");
        tb.Rows.First(r => r.Code == AccountingService.OperatingExpense).Debit.Should().Be(250m);
        tb.Rows.First(r => r.Code == AccountingService.Cash).Credit.Should().Be(250m);
    }

    [Fact]
    public async Task Csv_export_has_header_and_posted_lines()
    {
        var acct = Acct();
        await acct.PostManualAsync(new ManualJournalInput(DateOnly.FromDateTime(DateTime.UtcNow), "reclass cash→bank", null,
            new() { new ManualLineInput("1000", 500, 0, null), new ManualLineInput("1010", 0, 500, null) }), default);

        var csv = await acct.ExportCsvAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), default);
        csv.Should().StartWith("entry_no,date,source,reference,account_code,account_name,debit,credit,memo");
        csv.Should().Contain("1010");
    }

    [Fact]
    public async Task Unknown_account_code_is_rejected()
    {
        var act = async () => await Acct().PostManualAsync(new ManualJournalInput(DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            new() { new ManualLineInput("1000", 50, 0, null), new ManualLineInput("9999", 0, 50, null) }), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not in the chart*");
    }
}
