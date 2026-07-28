using System.Security.Cryptography;

namespace Hms.Api.Features.Auth;

/// <summary>
/// Hashes staff login PINs with PBKDF2-SHA256 (per-row random salt). PINs are
/// never stored in plaintext, so a DB leak yields nothing usable. Format:
/// <c>pbkdf2.{iterations}.{base64 salt}.{base64 hash}</c>.
/// </summary>
public static class PinHasher
{
    private const int Iterations = 100_000;
    private const int SaltLen = 16;
    private const int HashLen = 32;

    public static string Hash(string pin)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLen);
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, HashLen);
        return $"pbkdf2.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string pin, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;
        var parts = stored.Split('.');
        if (parts.Length != 4 || parts[0] != "pbkdf2") return false;
        if (!int.TryParse(parts[1], out var iter)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iter, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch { return false; }
    }

    /// <summary>A login PIN must be 4–8 digits.</summary>
    public static bool IsValidFormat(string? pin) =>
        !string.IsNullOrEmpty(pin) && pin.Length is >= 4 and <= 8 && pin.All(char.IsDigit);
}
