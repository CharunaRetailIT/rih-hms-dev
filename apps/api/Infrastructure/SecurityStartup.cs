namespace Hms.Api.Infrastructure;

/// <summary>
/// Boot-time secret validation. In Production the JWT signing key and the
/// AES-GCM master key MUST come from a real secret store (set via env vars
/// <c>Jwt__SigningKey</c> / <c>Secrets__MasterKey</c>, sourced from Key Vault /
/// Doppler / etc.). If they're missing — or still the checked-in dev defaults —
/// we fail fast rather than boot an insecure server. No-op outside Production.
/// </summary>
public static class SecurityStartup
{
    // The values shipped in appsettings.json / SecretProtector for local dev.
    public const string DevSigningKey = "dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars";
    public const string DevMasterKeyB64 = "ZGV2LW9ubHktbWFzdGVyLWtleS0zMmJ5dGVzLXJpdC1obXM="; // dev default (see SecretProtector)

    /// <summary>Throws <see cref="InvalidOperationException"/> if production secrets are unsafe.</summary>
    public static void AssertProductionSecrets(bool isProduction, string? signingKey, string? masterKey)
    {
        if (!isProduction) return;

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(signingKey) || signingKey == DevSigningKey)
            problems.Add("Jwt:SigningKey is missing or still the dev default — set Jwt__SigningKey from your secret store.");
        else if (signingKey.Length < 32)
            problems.Add("Jwt:SigningKey must be at least 32 characters.");

        if (string.IsNullOrWhiteSpace(masterKey) || masterKey == DevMasterKeyB64)
            problems.Add("Secrets:MasterKey is missing or still the dev default — set Secrets__MasterKey (base64 of 32 bytes) from your secret store.");

        if (problems.Count > 0)
            throw new InvalidOperationException(
                "Refusing to start in Production with insecure secrets:\n  - " + string.Join("\n  - ", problems));
    }
}
