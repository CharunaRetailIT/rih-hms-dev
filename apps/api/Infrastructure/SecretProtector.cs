using System.Security.Cryptography;
using System.Text;

namespace Hms.Api.Infrastructure;

/// <summary>
/// Encrypts/decrypts per-tenant secrets (aggregator API keys, etc.) for storage
/// in the database. The MASTER key is the single infrastructure secret — it
/// lives in Azure Key Vault / an env var, NEVER in the DB. Everything a merchant
/// configures (their Uber/PickMe keys) is stored encrypted with it.
/// </summary>
public interface ISecretProtector
{
    string? Encrypt(string? plaintext);
    string? Decrypt(string? ciphertext);
    /// <summary>Safe display form, e.g. "••••••1234" — never reveals the secret.</summary>
    string? Mask(string? ciphertext);
}

public sealed class AesGcmSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesGcmSecretProtector(IConfiguration config)
    {
        // Master key: base64 of 32 bytes. Set Secrets:MasterKey (env
        // Secrets__MasterKey) in prod via Key Vault. Dev default below is for
        // local only and must be overridden in any real deployment.
        var b64 = config["Secrets:MasterKey"]
            ?? "ZGV2LW9ubHktbWFzdGVyLWtleS0zMmJ5dGVzLXJpdC1obXM="; // "dev-only-master-key-32bytes-rit-hms"
        _key = SHA256.HashData(Convert.FromBase64String(b64)); // normalise to 32 bytes
    }

    public string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return null;
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plain.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plain, cipher, tag);
        // packed = nonce | tag | cipher  (base64)
        var packed = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, packed, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, packed, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, packed, nonce.Length + tag.Length, cipher.Length);
        return Convert.ToBase64String(packed);
    }

    public string? Decrypt(string? ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return null;
        var packed = Convert.FromBase64String(ciphertext);
        var nonceLen = AesGcm.NonceByteSizes.MaxSize;
        var tagLen = AesGcm.TagByteSizes.MaxSize;
        var nonce = packed[..nonceLen];
        var tag = packed[nonceLen..(nonceLen + tagLen)];
        var cipher = packed[(nonceLen + tagLen)..];
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    public string? Mask(string? ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return null;
        var plain = Decrypt(ciphertext);
        if (string.IsNullOrEmpty(plain)) return null;
        return plain.Length <= 4 ? "••••" : "••••••" + plain[^4..];
    }
}
