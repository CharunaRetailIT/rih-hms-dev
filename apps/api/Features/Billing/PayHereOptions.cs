using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Hms.Api.Features.Billing;

/// <summary>
/// PayHere configuration (#110). Bound from the "PayHere" config section, fed by
/// /opt/hms/hms.env (PayHere__AppId, PayHere__MerchantSecret, …) — secrets never live in code.
///
/// Two credential pairs, two jobs:
///  • App ID + App Secret  → OAuth + Charging API (server-to-server billing of a saved card).
///  • Merchant ID + Secret → checkout / preapproval hash + notify-signature verification.
/// Both are required to move money end-to-end; with only the App pair we can authenticate
/// but can't capture a card, so the system stays on the manual provider until both are set.
/// </summary>
public sealed class PayHereOptions
{
    public string Mode { get; set; } = "sandbox";          // sandbox | live
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }
    public string? MerchantId { get; set; }
    public string? MerchantSecret { get; set; }
    public string? AllowedDomain { get; set; }             // e.g. hms.retailit.lk (PayHere-whitelisted)

    public bool IsLive => string.Equals(Mode, "live", StringComparison.OrdinalIgnoreCase);
    public string ApiBase => IsLive ? "https://www.payhere.lk" : "https://sandbox.payhere.lk";

    public bool HasChargingCreds => !string.IsNullOrWhiteSpace(AppId) && !string.IsNullOrWhiteSpace(AppSecret);
    public bool HasCheckoutCreds => !string.IsNullOrWhiteSpace(MerchantId) && !string.IsNullOrWhiteSpace(MerchantSecret);
    /// <summary>Both pairs present → PayHere can both capture a card and charge it. Drives provider auto-select.</summary>
    public bool IsFullyConfigured => HasChargingCreds && HasCheckoutCreds;

    /// <summary>Checkout/preapproval request hash:
    /// UPPER(md5(merchant_id + order_id + amount(2dp) + currency + UPPER(md5(merchant_secret)))).</summary>
    public string CheckoutHash(string orderId, decimal amount, string currency)
        => Md5Upper($"{MerchantId}{orderId}{amount.ToString("F2", CultureInfo.InvariantCulture)}{currency}{Md5Upper(MerchantSecret ?? string.Empty)}");

    /// <summary>Verify the notify md5sig PayHere posts to notify_url:
    /// UPPER(md5(merchant_id + order_id + payhere_amount + payhere_currency + status_code + UPPER(md5(merchant_secret)))).</summary>
    public bool VerifyNotifySig(string orderId, string payhereAmount, string payhereCurrency, string statusCode, string? md5sig)
    {
        if (string.IsNullOrWhiteSpace(md5sig) || !HasCheckoutCreds) return false;
        var local = Md5Upper($"{MerchantId}{orderId}{payhereAmount}{payhereCurrency}{statusCode}{Md5Upper(MerchantSecret ?? string.Empty)}");
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(local), Encoding.ASCII.GetBytes(md5sig.ToUpperInvariant()));
    }

    public static string Md5Upper(string s) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(s))); // ToHexString is upper-case
}
