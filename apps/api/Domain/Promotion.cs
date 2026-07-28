namespace Hms.Api.Domain;

/// <summary>An auto-applied, schedulable promotion (happy hour, spend-and-save, BOGO…).</summary>
public class Promotion : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string PromoType { get; set; } = "product_discount";   // product_discount | bill_value | buy_x_get_y | bundle
    public bool IsActive { get; set; } = true;
    public bool AutoApply { get; set; } = true;
    public int Priority { get; set; }
    public DateOnly? StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public int DaysMask { get; set; } = 127;          // bit0=Mon … bit6=Sun
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? AppliesToOrderType { get; set; }   // dine_in | takeaway | delivery; null = any
    public Guid? AppliesToCategoryId { get; set; }     // customer category scope (#65); null = any
    public string? DisplayMessage { get; set; }
    public List<PromotionLine> Lines { get; set; } = new();

    /// <summary>True if this promotion is live for the given local timestamp + order type.</summary>
    public bool IsLive(DateTime nowLocal, string orderType)
    {
        if (!IsActive) return false;
        var d = DateOnly.FromDateTime(nowLocal);
        if (StartsOn is { } s && d < s) return false;
        if (EndsOn is { } e && d > e) return false;
        // DayOfWeek: Sunday=0..Saturday=6 → our bit0=Mon..bit6=Sun
        var bit = ((int)nowLocal.DayOfWeek + 6) % 7;
        if ((DaysMask & (1 << bit)) == 0) return false;
        var t = TimeOnly.FromDateTime(nowLocal);
        if (StartTime is { } st && EndTime is { } et && !(t >= st && t <= et)) return false;
        if (!string.IsNullOrEmpty(AppliesToOrderType) && AppliesToOrderType != orderType) return false;
        return true;
    }
}

/// <summary>Type-specific rule row for a promotion (interpreted per Promotion.PromoType).</summary>
public class PromotionLine : BaseEntity
{
    public Guid PromotionId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }   // product_discount: target a whole category (+ sub-categories) instead of one product
    public decimal MinQty { get; set; }
    public decimal BillFrom { get; set; }
    public decimal? BillTo { get; set; }
    public Guid? GetProductId { get; set; }
    public decimal GetQty { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? BundlePrice { get; set; }
}

/// <summary>A promotion that actually fired on an order (bill display + usage reporting).</summary>
public class OrderPromotion : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid PromotionId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal DiscountAmount { get; set; }
}
