using Hms.Api.Domain;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Customers;

/// <summary>
/// CRM / customers (#70): customer master + categories, a per-customer (or
/// per-category) default discount, and credit customers (AR) — a credit limit, a
/// running outstanding balance, a ledger (visits + receipts), and AR receipts that
/// pay the balance down. Credit settlement itself (the charge) lives in OrderService.
/// </summary>
public static class CustomersEndpoint
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        // Operations (cashiers attach/quick-add at the till). Category admin → BackOffice.
        var g = app.MapGroup("/api/v1/customers").WithTags("Customers").RequireAuthorization("Operations");

        g.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct, string? search, bool? activeOnly, bool? creditOnly) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.Customers.AsNoTracking().AsQueryable();
            if (activeOnly == true) q = q.Where(c => c.IsActive);
            if (creditOnly == true) q = q.Where(c => c.IsCreditCustomer);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(c => c.Name.ToLower().Contains(s) || c.Code.ToLower().Contains(s)
                              || (c.Phone != null && c.Phone.Contains(s)));
            }
            var cats = await db.CustomerCategories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            var list = await q.OrderBy(c => c.Name).Take(200).ToListAsync(ct);
            return Results.Ok(list.Select(c => ToDto(c, c.CategoryId is { } cid && cats.TryGetValue(cid, out var n) ? n : null)));
        }).WithName("Customers.List").WithSummary("Search / list customers (POS picker + CRM screen).");

        // Server-side paginated grid for the Customers admin screen (large customer bases).
        g.MapGet("/paged", async (
            ITenantDbContextFactory factory, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, Guid? categoryId = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.Customers.AsNoTracking().AsQueryable();
            if (categoryId.HasValue) q = q.Where(c => c.CategoryId == categoryId.Value);
            if (isActive.HasValue) q = q.Where(c => c.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(c => EF.Functions.ILike(c.Name, s) || EF.Functions.ILike(c.Code, s) || EF.Functions.ILike(c.Phone ?? "", s));
            }

            var totalCount = await q.CountAsync(ct);
            var cats = await db.CustomerCategories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            var rows = await q.OrderBy(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return Results.Ok(new
            {
                data = rows.Select(c => ToDto(c, c.CategoryId is { } cid && cats.TryGetValue(cid, out var n) ? n : null)),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Customers.Paged").WithSummary("Server-side paginated customer grid.");

        g.MapGet("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return c is null ? Results.NotFound() : Results.Ok(ToDto(c, null));
        }).WithName("Customers.Get");

        g.MapPut("", async (CustomerInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Name)) return Results.BadRequest(new { error = "Customer name is required" });
            await using var db = await factory.CreateForCurrentAsync(ct);

            var c = i.Id is Guid id ? await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct) : null;
            if (i.Id is { } && c is null) return Results.NotFound();
            if (c is null)
            {
                // Find-or-create by phone: the same number should be ONE customer, so the
                // till stops minting a duplicate "Mubeen 077…" on every visit. If it exists,
                // attach the existing record (filling in any new detail) instead of creating.
                var phone = Trim(i.Phone);
                if (phone is not null)
                {
                    var existing = await db.Customers.FirstOrDefaultAsync(x => x.Phone == phone, ct);
                    if (existing is not null)
                        // Phone already registered — don't silently create OR silently attach
                        // (the typed name may differ). Tell the caller who owns it so the POS can
                        // offer to attach that existing customer.
                        return Results.Conflict(new { error = $"This phone is already registered to {existing.Name}.", existingId = existing.Id, existingName = existing.Name, phone });
                }
                var code = string.IsNullOrWhiteSpace(i.Code) ? $"C-{Guid.NewGuid():N}"[..8].ToUpperInvariant() : i.Code.Trim().ToUpperInvariant();
                if (await db.Customers.AnyAsync(x => x.Code == code, ct))
                    return Results.BadRequest(new { error = $"Customer code {code} already exists" });
                c = new Customer { Code = code };
                db.Customers.Add(c);
            }
            else if (!string.IsNullOrWhiteSpace(i.Code))
            {
                c.Code = i.Code.Trim().ToUpperInvariant();
            }

            c.Name = i.Name.Trim();
            c.CategoryId = i.CategoryId;
            c.Phone = Trim(i.Phone); c.Email = Trim(i.Email); c.Address = Trim(i.Address); c.TaxNo = Trim(i.TaxNo);
            c.CountryCode = Trim(i.CountryCode); c.Province = Trim(i.Province); c.District = Trim(i.District); c.PostalCode = Trim(i.PostalCode);
            c.DiscountPercent = i.DiscountPercent is > 0 ? i.DiscountPercent : null;
            c.IsCreditCustomer = i.IsCreditCustomer;
            c.CreditLimit = Math.Max(0, i.CreditLimit);
            c.LoyaltyCardNo = Trim(i.LoyaltyCardNo);
            c.IsActive = i.IsActive;
            c.Notes = Trim(i.Notes);
            c.DateOfBirth = ParseDob(i.DateOfBirth);
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDto(c, null));
        }).WithName("Customers.Upsert").WithSummary("Create or edit a customer.");

        // Bulk CSV import (#master-data-import): upsert customers by Code (else Phone, else Name);
        // auto-create categories by name; auto-generate a code when blank. Per-row error report.
        g.MapPost("/import", async (List<CustomerImportRow> rows, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (rows is null || rows.Count == 0) return Results.BadRequest(new { error = "No rows to import" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var cats = await db.CustomerCategories.ToListAsync(ct);
            var all = await db.Customers.ToListAsync(ct);
            int created = 0, updated = 0; var errors = new List<object>(); var rn = 0;
            foreach (var row in rows)
            {
                rn++;
                try
                {
                    if (string.IsNullOrWhiteSpace(row.Name)) { errors.Add(new { row = rn, name = row.Name, error = "Name is required" }); continue; }
                    var phone = Trim(row.Phone);
                    Customer? c = null;
                    if (!string.IsNullOrWhiteSpace(row.Code))
                        c = all.FirstOrDefault(x => string.Equals(x.Code, row.Code!.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (c is null && phone is not null) c = all.FirstOrDefault(x => x.Phone == phone);
                    c ??= all.FirstOrDefault(x => string.Equals(x.Name, row.Name!.Trim(), StringComparison.OrdinalIgnoreCase));

                    Guid? catId = null;
                    if (!string.IsNullOrWhiteSpace(row.Category))
                    {
                        var cat = cats.FirstOrDefault(x => string.Equals(x.Name, row.Category!.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (cat is null) { cat = new CustomerCategory { Name = row.Category!.Trim(), Code = row.Category!.Trim().ToUpperInvariant()[..Math.Min(12, row.Category!.Trim().Length)], IsActive = true }; db.CustomerCategories.Add(cat); cats.Add(cat); }
                        catId = cat.Id;
                    }

                    if (c is null)
                    {
                        var code = string.IsNullOrWhiteSpace(row.Code) ? $"C-{Guid.NewGuid():N}"[..8].ToUpperInvariant() : row.Code!.Trim().ToUpperInvariant();
                        if (all.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase)))
                        { errors.Add(new { row = rn, name = row.Name, error = $"Customer code {code} already exists" }); continue; }
                        c = new Customer { Code = code, IsActive = true };
                        db.Customers.Add(c); all.Add(c); created++;
                    }
                    else updated++;
                    c.Name = row.Name!.Trim();
                    if (catId is not null) c.CategoryId = catId;
                    if (phone is not null) c.Phone = phone;
                    if (Trim(row.Email) is { } em) c.Email = em;
                    if (Trim(row.Address) is { } ad) c.Address = ad;
                    if (Trim(row.TaxNo) is { } tn) c.TaxNo = tn;
                    if (Trim(row.CountryCode) is { } cc) c.CountryCode = cc;
                    if (Trim(row.LoyaltyCardNo) is { } lc) c.LoyaltyCardNo = lc;
                    if (row.DiscountPercent is > 0) c.DiscountPercent = row.DiscountPercent;
                    if (row.CreditLimit is { } cl && cl > 0) { c.IsCreditCustomer = true; c.CreditLimit = cl; }
                }
                catch (Exception ex) { errors.Add(new { row = rn, name = row.Name, error = ex.Message }); }
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { created, updated, errors });
        }).WithName("Customers.Import").WithSummary("Bulk CSV import of customers (upsert by code/phone/name).");

        g.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();
            if (c.CurrentBalance > 0.009m) return Results.BadRequest(new { error = "Customer has an outstanding balance — settle it first" });
            c.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Customers.Delete");

        // Ledger: the customer's settled orders (visit history) + AR receipts + balance.
        g.MapGet("/{id:guid}/ledger", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();

            var orders = await db.Orders.AsNoTracking()
                .Where(o => o.CustomerId == id && o.Status == "settled")
                .OrderByDescending(o => o.SettledAt)
                .Select(o => new { o.Id, o.OrderNumber, o.InvoiceNumber, o.SettledAt, o.TotalAmount })
                .Take(100).ToListAsync(ct);
            var creditOrderIds = await db.Payments.AsNoTracking()
                .Where(p => p.PayType == "credit" && orders.Select(o => o.Id).Contains(p.OrderId))
                .Select(p => p.OrderId).ToListAsync(ct);
            var receipts = await db.CustomerPayments.AsNoTracking()
                .Where(p => p.CustomerId == id)
                .OrderByDescending(p => p.ReceivedAt)
                .Select(p => new { p.Id, p.Amount, p.PayType, p.Reference, p.ReceivedAt, p.Notes })
                .Take(100).ToListAsync(ct);
            var loyalty = await db.LoyaltyTransactions.AsNoTracking()
                .Where(t => t.CustomerId == id)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new { t.Id, t.TxnType, t.Points, t.BalanceAfter, t.Note, t.CreatedAt })
                .Take(100).ToListAsync(ct);

            return Results.Ok(new
            {
                customer = ToDto(c, null),
                balance = c.CurrentBalance,
                loyaltyPoints = c.LoyaltyPoints,
                orders = orders.Select(o => new { o.Id, o.OrderNumber, o.InvoiceNumber, o.SettledAt, o.TotalAmount, onCredit = creditOrderIds.Contains(o.Id) }),
                receipts, loyalty,
            });
        }).WithName("Customers.Ledger").WithSummary("Visit history + AR receipts + outstanding balance.");

        // AR receipt — a credit customer pays down their balance.
        g.MapPost("/{id:guid}/payments", async (Guid id, CustomerPaymentInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (i.Amount <= 0) return Results.BadRequest(new { error = "Amount must be positive" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();
            var amount = Math.Round(i.Amount, 4);
            if (amount > c.CurrentBalance + 0.009m)
                return Results.BadRequest(new { error = $"Receipt {amount:F2} exceeds the outstanding balance {c.CurrentBalance:F2}" });

            db.CustomerPayments.Add(new CustomerPayment
            {
                CustomerId = id, Amount = amount, PayType = string.IsNullOrWhiteSpace(i.PayType) ? "cash" : i.PayType.Trim(),
                Reference = Trim(i.Reference), Notes = Trim(i.Notes), ReceivedAt = DateTime.UtcNow,
            });
            c.CurrentBalance = Math.Round(c.CurrentBalance - amount, 4);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { c.Id, balance = c.CurrentBalance });
        }).WithName("Customers.RecordPayment").WithSummary("Record an AR receipt against the customer's balance.");

        // Attach-by-card (#66): find a customer by their scannable loyalty card number.
        g.MapGet("/by-card/{card}", async (string card, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.LoyaltyCardNo == card.Trim() && x.IsActive, ct);
            return c is null ? Results.NotFound() : Results.Ok(ToDto(c, null));
        }).WithName("Customers.ByCard").WithSummary("Look up a customer by loyalty card number.");

        // Advance / deposit (#70): customer prepays; drawn at the till as an "advance" tender.
        g.MapPost("/{id:guid}/advance", async (Guid id, CustomerPaymentInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (i.Amount <= 0) return Results.BadRequest(new { error = "Amount must be positive" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();
            var amount = Math.Round(i.Amount, 4);
            c.AdvanceBalance = Math.Round(c.AdvanceBalance + amount, 4);
            db.CustomerPayments.Add(new CustomerPayment   // ledger visibility (not an AR receipt)
            {
                CustomerId = id, Amount = amount, PayType = "advance",
                Reference = Trim(i.Reference), Notes = Trim(i.Notes), ReceivedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { c.Id, advanceBalance = c.AdvanceBalance });
        }).WithName("Customers.RecordAdvance").WithSummary("Record a customer advance/deposit (drawable at the till).");

        // ── contract prices (#70): a fixed price this customer pays for a product ──
        g.MapGet("/{id:guid}/prices", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var rows = await db.CustomerProductPrices.AsNoTracking().Where(p => p.CustomerId == id)
                .Join(db.Products.AsNoTracking(), p => p.ProductId, pr => pr.Id, (p, pr) => new { p.ProductId, pr.Name, pr.Sku, pr.BasePrice, p.Price })
                .OrderBy(x => x.Name).ToListAsync(ct);
            return Results.Ok(rows);
        }).WithName("Customers.ListPrices");

        g.MapPut("/{id:guid}/prices", async (Guid id, CustomerPriceInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (i.Price < 0) return Results.BadRequest(new { error = "Price cannot be negative" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            if (!await db.Customers.AnyAsync(c => c.Id == id, ct)) return Results.NotFound();
            var row = await db.CustomerProductPrices.FirstOrDefaultAsync(p => p.CustomerId == id && p.ProductId == i.ProductId, ct);
            if (row is null) { row = new CustomerProductPrice { CustomerId = id, ProductId = i.ProductId }; db.CustomerProductPrices.Add(row); }
            row.Price = Math.Round(i.Price, 4); row.IsDeleted = false;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.ProductId, row.Price });
        }).WithName("Customers.SetPrice").RequireAuthorization("BackOffice");

        g.MapDelete("/{id:guid}/prices/{productId:guid}", async (Guid id, Guid productId, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.CustomerProductPrices.FirstOrDefaultAsync(p => p.CustomerId == id && p.ProductId == productId, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Customers.DeletePrice").RequireAuthorization("BackOffice");

        // ── statement (#70): AR charges + receipts over a period with a running balance ──
        g.MapGet("/{id:guid}/statement", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct, DateTime? from, DateTime? to) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();

            var fromD = from is { } f ? OrderService.AsUtc(f) : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var toD = (to is { } t ? OrderService.AsUtc(t).Date : DateTime.UtcNow.Date).AddDays(1);   // inclusive end-of-day

            // Charges = credit-tendered settlements (these raise AR); receipts = AR payments.
            var charges = await db.Payments.AsNoTracking().Where(p => p.PayType == "credit")
                .Join(db.Orders.AsNoTracking().Where(o => o.CustomerId == id && o.SettledAt != null),
                    p => p.OrderId, o => o.Id, (p, o) => new { When = o.SettledAt!.Value, p.Amount, Ref = o.InvoiceNumber ?? o.OrderNumber })
                .ToListAsync(ct);
            var receipts = await db.CustomerPayments.AsNoTracking().Where(p => p.CustomerId == id)
                .Select(p => new { When = p.ReceivedAt, p.Amount, p.Reference, p.PayType }).ToListAsync(ct);

            var opening = charges.Where(x => x.When < fromD).Sum(x => x.Amount) - receipts.Where(x => x.When < fromD).Sum(x => x.Amount);

            var rows = charges.Where(x => x.When >= fromD && x.When < toD)
                .Select(x => new { date = x.When, kind = "charge", reference = x.Ref, debit = x.Amount, credit = 0m })
              .Concat(receipts.Where(x => x.When >= fromD && x.When < toD)
                .Select(x => new { date = x.When, kind = "receipt", reference = x.Reference ?? x.PayType, debit = 0m, credit = x.Amount }))
                .OrderBy(x => x.date).ToList();

            var running = opening;
            var lines = rows.Select(r => { running += r.debit - r.credit; return new { r.date, r.kind, r.reference, r.debit, r.credit, balance = running }; }).ToList();

            return Results.Ok(new
            {
                customer = ToDto(c, null), periodFrom = fromD, periodTo = toD,
                opening, closing = running, currentBalance = c.CurrentBalance,
                totalCharges = rows.Sum(r => r.debit), totalReceipts = rows.Sum(r => r.credit),
                lines,
            });
        }).WithName("Customers.Statement").WithSummary("Customer AR statement for a period (opening + running balance + closing).");

        // ── categories ──────────────────────────────────────────────────────────
        var cg = app.MapGroup("/api/v1/customer-categories").WithTags("Customers");
        cg.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var list = await db.CustomerCategories.AsNoTracking().OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Code, c.Name, c.DiscountPercent, c.IsActive, c.Notes, c.IsVat }).ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("CustomerCategories.List").RequireAuthorization("Operations");

        cg.MapPut("", async (CustomerCategoryInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Name)) return Results.BadRequest(new { error = "Category name is required" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var code = string.IsNullOrWhiteSpace(i.Code) ? i.Name.Trim().ToUpperInvariant().Replace(' ', '_') : i.Code.Trim().ToUpperInvariant();
            var c = i.Id is Guid id
                ? await db.CustomerCategories.FirstOrDefaultAsync(x => x.Id == id, ct)
                : await db.CustomerCategories.FirstOrDefaultAsync(x => x.Code == code, ct);
            if (c is null) { c = new CustomerCategory { Code = code }; db.CustomerCategories.Add(c); }
            c.Code = code; c.Name = i.Name.Trim(); c.DiscountPercent = Math.Max(0, i.DiscountPercent);
            c.IsActive = i.IsActive; c.Notes = Trim(i.Notes); c.IsVat = i.IsVat;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { c.Id, c.Code, c.Name, c.DiscountPercent, c.IsActive, c.IsVat });
        }).WithName("CustomerCategories.Upsert").RequireAuthorization("BackOffice");

        cg.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.CustomerCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();
            c.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("CustomerCategories.Delete").RequireAuthorization("BackOffice");

        // Category-level contract prices (apply to every customer in the category).
        cg.MapGet("/{id:guid}/prices", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var rows = await db.CustomerProductPrices.AsNoTracking().Where(p => p.CategoryId == id)
                .Join(db.Products.AsNoTracking(), p => p.ProductId, pr => pr.Id, (p, pr) => new { p.ProductId, pr.Name, pr.Sku, pr.BasePrice, p.Price })
                .OrderBy(x => x.Name).ToListAsync(ct);
            return Results.Ok(rows);
        }).WithName("CustomerCategories.ListPrices").RequireAuthorization("Operations");

        cg.MapPut("/{id:guid}/prices", async (Guid id, CustomerPriceInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (i.Price < 0) return Results.BadRequest(new { error = "Price cannot be negative" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            if (!await db.CustomerCategories.AnyAsync(c => c.Id == id, ct)) return Results.NotFound();
            var row = await db.CustomerProductPrices.FirstOrDefaultAsync(p => p.CategoryId == id && p.ProductId == i.ProductId, ct);
            if (row is null) { row = new CustomerProductPrice { CategoryId = id, ProductId = i.ProductId }; db.CustomerProductPrices.Add(row); }
            row.Price = Math.Round(i.Price, 4); row.IsDeleted = false;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.ProductId, row.Price });
        }).WithName("CustomerCategories.SetPrice").RequireAuthorization("BackOffice");

        cg.MapDelete("/{id:guid}/prices/{productId:guid}", async (Guid id, Guid productId, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.CustomerProductPrices.FirstOrDefaultAsync(p => p.CategoryId == id && p.ProductId == productId, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("CustomerCategories.DeletePrice").RequireAuthorization("BackOffice");

        // POS-facing config (Operations): loyalty redemption + KOT workflow mode.
        // The POS reads this to decide whether to show the points tender and how to
        // route kitchen tickets (KDS board, auto-print on send). The sidebar reads
        // it to hide the Kitchen (KOT) screen for venues that opt out of a display.
        app.MapGet("/api/v1/pos/config", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            return Results.Ok(new {
                enabled = s?.LoyaltyEnabled ?? false, earnRate = s?.LoyaltyEarnRate ?? 0m, redeemValue = s?.LoyaltyRedeemValue ?? 1m,
                kdsEnabled = s?.KdsEnabled ?? true, kotAutoPrint = s?.KotAutoPrint ?? false,
                logoUrl = s?.LogoUrl, baseCurrency = s?.BaseCurrency ?? "LKR",
                taxLabel = string.IsNullOrWhiteSpace(s?.TaxLabel) ? "VAT" : s!.TaxLabel, vatEnabled = s?.VatEnabled ?? true,
                planCode = s?.PlanCode, features = Hms.Api.Infrastructure.PlanFeatures.Map(s?.PlanCode),   // #6 Pro/Lite gating
                ereceipt = new { channels = s?.EReceiptChannels ?? "", quota = s?.EReceiptQuota ?? 0, used = s?.EReceiptUsed ?? 0 },   // #79
            });
        }).WithTags("POS").RequireAuthorization("Operations").WithName("Pos.Config");

        // Back-compat alias (older clients call /loyalty/config).
        app.MapGet("/api/v1/loyalty/config", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            return Results.Ok(new {
                enabled = s?.LoyaltyEnabled ?? false, earnRate = s?.LoyaltyEarnRate ?? 0m, redeemValue = s?.LoyaltyRedeemValue ?? 1m,
                kdsEnabled = s?.KdsEnabled ?? true, kotAutoPrint = s?.KotAutoPrint ?? false,
                logoUrl = s?.LogoUrl, baseCurrency = s?.BaseCurrency ?? "LKR",
                taxLabel = string.IsNullOrWhiteSpace(s?.TaxLabel) ? "VAT" : s!.TaxLabel, vatEnabled = s?.VatEnabled ?? true,
            });
        }).WithTags("Loyalty").RequireAuthorization("Operations").WithName("Loyalty.Config");

        // ── Loyalty customers: customers enrolled with a card scheme / card number ──
        var lc = app.MapGroup("/api/v1/loyalty/customers").WithTags("Loyalty");

        lc.MapGet("/paged", async (
            ITenantDbContextFactory factory, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, Guid? schemeId = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.Customers.AsNoTracking()
                .Where(c => c.LoyaltyCardSchemeId != null || c.LoyaltyCardNo != null);
            if (schemeId.HasValue) q = q.Where(c => c.LoyaltyCardSchemeId == schemeId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(c => EF.Functions.ILike(c.Name, s) || EF.Functions.ILike(c.Code, s) || EF.Functions.ILike(c.LoyaltyCardNo ?? "", s));
            }

            var totalCount = await q.CountAsync(ct);
            var schemes = await db.LoyaltyCardSchemes.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, ct);
            var rows = await q.OrderByDescending(c => c.LoyaltyLastActivityAt ?? c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return Results.Ok(new
            {
                data = rows.Select(c => new
                {
                    c.Id, c.Code, c.Name, c.Phone, c.LoyaltyCardNo, c.LoyaltyCardSchemeId,
                    schemeName = c.LoyaltyCardSchemeId is { } sid && schemes.TryGetValue(sid, out var sn) ? sn : null,
                    c.LoyaltyPoints, c.LoyaltyLifetimePoints, c.IsActive,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).RequireAuthorization("Operations").WithName("Loyalty.Customers.Paged");

        lc.MapPut("/{id:guid}", async (Guid id, LoyaltyCustomerAssignInput body, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return Results.NotFound();
            if (body.SchemeId is Guid sid && !await db.LoyaltyCardSchemes.AnyAsync(x => x.Id == sid, ct))
                return Results.BadRequest(new { error = "Selected loyalty card scheme was not found" });
            c.LoyaltyCardSchemeId = body.SchemeId;
            c.LoyaltyCardNo = Trim(body.LoyaltyCardNo);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { c.Id, c.LoyaltyCardSchemeId, c.LoyaltyCardNo });
        }).RequireAuthorization("BackOffice").WithName("Loyalty.Customers.Assign");

        // ── Loyalty tiers (#66) ──────────────────────────────────────────────────
        var lt = app.MapGroup("/api/v1/loyalty/tiers").WithTags("Loyalty");
        lt.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var rows = await db.LoyaltyTiers.AsNoTracking().OrderBy(t => t.MinLifetimePoints)
                .Select(t => new { t.Id, t.Name, t.MinLifetimePoints, t.EarnMultiplier, t.DiscountPercent, t.SortOrder, t.IsActive }).ToListAsync(ct);
            return Results.Ok(rows);
        }).RequireAuthorization("Operations").WithName("Loyalty.Tiers.List");

        lt.MapPut("", async (LoyaltyTierInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Name)) return Results.BadRequest(new { error = "Tier name is required" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var t = i.Id is Guid id ? await db.LoyaltyTiers.FirstOrDefaultAsync(x => x.Id == id, ct) : null;
            if (t is null) { t = new LoyaltyTier(); db.LoyaltyTiers.Add(t); }
            t.Name = i.Name.Trim();
            t.MinLifetimePoints = Math.Max(0, i.MinLifetimePoints);
            t.EarnMultiplier = i.EarnMultiplier <= 0 ? 1m : i.EarnMultiplier;
            t.DiscountPercent = Math.Clamp(i.DiscountPercent, 0, 100);
            t.SortOrder = i.SortOrder; t.IsActive = i.IsActive;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { t.Id, t.Name });
        }).RequireAuthorization("BackOffice").WithName("Loyalty.Tiers.Upsert");

        lt.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var t = await db.LoyaltyTiers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (t is null) return Results.NotFound();
            t.IsDeleted = true; t.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization("BackOffice").WithName("Loyalty.Tiers.Delete");

        // Points expiring soon (#66) — feeds a reminder job. Delivery (SMS/email/push)
        // needs a notifications channel; this surfaces who/when for that future sender.
        app.MapGet("/api/v1/loyalty/expiring", async (ITenantDbContextFactory factory, CancellationToken ct, int? days) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            var window = s?.LoyaltyExpiryDays ?? 0;
            if (window <= 0) return Results.Ok(new { expiryEnabled = false, soon = Array.Empty<object>() });
            var within = Math.Clamp(days ?? 14, 1, 365);
            var cutoffLast = DateTime.UtcNow.AddDays(within - window);   // expires within `within` days
            var rows = await db.Customers.AsNoTracking()
                .Where(c => c.LoyaltyPoints > 0 && c.LoyaltyLastActivityAt != null && c.LoyaltyLastActivityAt <= cutoffLast)
                .OrderBy(c => c.LoyaltyLastActivityAt)
                .Select(c => new { c.Id, c.Name, c.Phone, c.LoyaltyCardNo, c.LoyaltyPoints,
                    expiresOn = c.LoyaltyLastActivityAt!.Value.AddDays(window) })
                .Take(500).ToListAsync(ct);
            return Results.Ok(new { expiryEnabled = true, windowDays = window, soon = rows });
        }).WithTags("Loyalty").RequireAuthorization("BackOffice").WithName("Loyalty.Expiring");

        // Expire stale balances now (inactivity model). A scheduled job calls this; ops can too.
        app.MapPost("/api/v1/loyalty/expire", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var s = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            var days = s?.LoyaltyExpiryDays ?? 0;
            if (days <= 0) return Results.Ok(new { expiredCustomers = 0, expiredPoints = 0m, note = "Expiry is disabled" });
            var cutoff = DateTime.UtcNow.AddDays(-days);
            var stale = await db.Customers.Where(c => c.LoyaltyPoints > 0 && c.LoyaltyLastActivityAt != null && c.LoyaltyLastActivityAt < cutoff).ToListAsync(ct);
            decimal total = 0;
            foreach (var c in stale)
            {
                total += c.LoyaltyPoints;
                db.LoyaltyTransactions.Add(new LoyaltyTransaction { CustomerId = c.Id, TxnType = "expire", Points = -c.LoyaltyPoints, BalanceAfter = 0m, Note = $"Expired — {days} days inactive" });
                c.LoyaltyPoints = 0m;
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { expiredCustomers = stale.Count, expiredPoints = total });
        }).WithTags("Loyalty").RequireAuthorization("BackOffice").WithName("Loyalty.Expire");

        return app;
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static DateOnly? ParseDob(string? s) => DateOnly.TryParse(s, out var d) ? d : null;

    private static object ToDto(Customer c, string? categoryName) => new
    {
        c.Id, c.Code, c.Name, c.CategoryId, categoryName,
        c.Phone, c.Email, c.Address, c.CountryCode, c.Province, c.District, c.PostalCode, c.TaxNo,
        c.DiscountPercent, c.IsCreditCustomer, c.CreditLimit, c.CurrentBalance,
        creditAvailable = Math.Max(0, c.CreditLimit - c.CurrentBalance),
        c.AdvanceBalance,
        c.LoyaltyPoints, c.LoyaltyLifetimePoints, c.LoyaltyCardNo,
        c.IsActive, c.Notes,
        dateOfBirth = c.DateOfBirth?.ToString("yyyy-MM-dd"),
    };
}

public record CustomerInput(
    Guid? Id, string? Code, string Name, Guid? CategoryId,
    string? Phone, string? Email, string? Address, string? TaxNo,
    decimal? DiscountPercent, bool IsCreditCustomer, decimal CreditLimit, bool IsActive, string? Notes, string? LoyaltyCardNo = null,
    string? DateOfBirth = null,
    string? CountryCode = null, string? Province = null, string? District = null, string? PostalCode = null);

public record CustomerImportRow(string? Code, string? Name, string? Phone, string? Email, string? Address, string? TaxNo,
    string? Category, decimal? DiscountPercent, decimal? CreditLimit, string? LoyaltyCardNo, string? CountryCode);

public record CustomerCategoryInput(Guid? Id, string? Code, string Name, decimal DiscountPercent, bool IsActive, string? Notes, bool IsVat = false);

public record CustomerPaymentInput(decimal Amount, string? PayType, string? Reference, string? Notes);
public record CustomerPriceInput(Guid ProductId, decimal Price);
public record LoyaltyTierInput(Guid? Id, string Name, decimal MinLifetimePoints, decimal EarnMultiplier = 1, decimal DiscountPercent = 0, int SortOrder = 0, bool IsActive = true);
public record LoyaltyCustomerAssignInput(Guid? SchemeId, string? LoyaltyCardNo);
