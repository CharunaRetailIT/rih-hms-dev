using FluentAssertions;
using Hms.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hms.Api.IntegrationTests;

[Collection("pg")]
public class SecurityTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Encrypt_then_Decrypt_round_trips()
    {
        var protector = fx.Protector();
        var cipher = protector.Encrypt("ue_secret_123");

        cipher.Should().NotBeNullOrEmpty();
        cipher.Should().NotContain("ue_secret_123");
        protector.Decrypt(cipher).Should().Be("ue_secret_123");
    }

    [Fact]
    public void Encrypt_same_plaintext_twice_yields_different_ciphertext_but_equal_decrypt()
    {
        var protector = fx.Protector();
        var a = protector.Encrypt("ue_secret_123");
        var b = protector.Encrypt("ue_secret_123");

        a.Should().NotBe(b);   // random nonce per encryption
        protector.Decrypt(a).Should().Be("ue_secret_123");
        protector.Decrypt(b).Should().Be("ue_secret_123");
        protector.Decrypt(a).Should().Be(protector.Decrypt(b));
    }

    [Fact]
    public void Mask_shows_only_last_four_and_never_full_plaintext()
    {
        var protector = fx.Protector();
        var cipher = protector.Encrypt("ue_secret_123");

        var masked = protector.Mask(cipher);

        masked.Should().Be("••••••_123");          // bullets + last-4
        masked.Should().StartWith("••••••");
        masked.Should().EndWith("_123");
        masked.Should().NotContain("ue_secret_123");
        masked.Should().NotContain("ue_secret");
    }

    [Fact]
    public void Encrypt_and_Decrypt_of_null_return_null()
    {
        var protector = fx.Protector();
        protector.Encrypt(null).Should().BeNull();
        protector.Decrypt(null).Should().BeNull();
    }

    [Fact]
    public async Task DB_stores_ciphertext_at_rest_not_plaintext()
    {
        var protector = fx.Protector();
        var credentialId = Guid.NewGuid();

        try
        {
            await using (var db = fx.NewTenantContext())
            {
                db.Set<AggregatorCredential>().Add(new AggregatorCredential
                {
                    Id = credentialId,
                    TenantId = fx.TenantId,
                    Aggregator = "ubereats",
                    ClientSecretEnc = protector.Encrypt("SUPER_SENSITIVE_9999"),
                    Environment = "sandbox",
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
                await db.SaveChangesAsync();
            }

            await using (var db = fx.NewTenantContext())
            {
                var stored = await db.Set<AggregatorCredential>().IgnoreQueryFilters()
                    .FirstAsync(c => c.Id == credentialId);

                stored.ClientSecretEnc.Should().NotBeNullOrEmpty();
                stored.ClientSecretEnc.Should().NotContain("SUPER_SENSITIVE");
                protector.Decrypt(stored.ClientSecretEnc).Should().Be("SUPER_SENSITIVE_9999");
            }
        }
        finally
        {
            await using var cleanup = fx.NewTenantContext();
            var row = await cleanup.Set<AggregatorCredential>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == credentialId);
            if (row is not null)
            {
                cleanup.Set<AggregatorCredential>().Remove(row);
                await cleanup.SaveChangesAsync();
            }
        }
    }
}
