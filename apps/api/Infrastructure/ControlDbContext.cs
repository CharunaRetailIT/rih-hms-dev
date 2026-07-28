using Hms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Control plane: tracks tenants, subscriptions, billing. Single DB shared
/// across all customers. NOT a tenant-scoped context.
/// </summary>
public class ControlDbContext(DbContextOptions<ControlDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionItem> SubscriptionItems => Set<SubscriptionItem>();
    public DbSet<BillingTax> BillingTaxes => Set<BillingTax>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("control");

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash);
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        });

        b.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Slug).HasMaxLength(60).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.DatabaseName).HasMaxLength(100).IsRequired();
            e.Property(x => x.DatabaseHost).HasMaxLength(255).IsRequired();
            e.Property(x => x.Plan).HasMaxLength(40);
            e.Property(x => x.OwnerEmail).HasMaxLength(255);
            e.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
            e.Property(x => x.DefaultCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.TimeZone).HasMaxLength(64).IsRequired();
            e.Property(x => x.Status).HasConversion<int>();
        });

        b.Entity<Plan>(e =>
        {
            e.ToTable("plans");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3);
        });

        b.Entity<Addon>(e =>
        {
            e.ToTable("addons");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(40).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3);
        });

        b.Entity<Subscription>(e =>
        {
            e.ToTable("subscriptions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.Property(x => x.Provider).HasMaxLength(40).IsRequired();
            e.Property(x => x.Plan).HasMaxLength(40).IsRequired();
            e.Property(x => x.Status).HasMaxLength(40).IsRequired();
            e.Property(x => x.CardBrand).HasMaxLength(40);
            e.Property(x => x.CardLast4).HasMaxLength(8);
        });

        b.Entity<SubscriptionItem>(e =>
        {
            e.ToTable("subscription_items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SubscriptionId);
            e.Property(x => x.ItemType).HasMaxLength(40).IsRequired();
            e.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3);
        });

        b.Entity<PlatformSetting>(e =>
        {
            e.ToTable("platform_settings");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(80);
            e.Property(x => x.Value).IsRequired();
        });

        b.Entity<BillingTax>(e =>
        {
            e.ToTable("billing_taxes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Scope).HasMaxLength(20).IsRequired();
            e.Property(x => x.RatePercent).HasPrecision(6, 3);
        });
    }

    public override int SaveChanges()
    {
        StampTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(ct);
    }

    private void StampTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var e in ChangeTracker.Entries<ControlEntity>())
        {
            if (e.State == EntityState.Added)
            {
                if (e.Entity.Id == Guid.Empty) e.Entity.Id = Guid.NewGuid();
                e.Entity.CreatedAt = now;
                e.Entity.UpdatedAt = now;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.UpdatedAt = now;
            }
        }
    }
}
