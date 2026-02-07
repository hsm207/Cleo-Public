using Cleo.Infrastructure.Security;
using Xunit;

namespace Cleo.Infrastructure.Tests.Security;

public class AesGcmEncryptionStrategyTests
{
    [Fact(DisplayName = "AesGcmEncryptionStrategy should correctly encrypt and decrypt a secret string.")]
    public void RoundTripShouldSucceed()
    {
        var strategy = new AesGcmEncryptionStrategy();
        var secret = "Hello World!";

        var encrypted = strategy.Encrypt(secret);
        var decrypted = strategy.Decrypt(encrypted);

        Assert.Equal(secret, decrypted);
        Assert.NotEqual(secret, System.Text.Encoding.UTF8.GetString(encrypted));
    }

    [Fact(DisplayName = "AesGcmEncryptionStrategy should throw ArgumentNullException for null inputs.")]
    public void ShouldThrowOnNull()
    {
        var strategy = new AesGcmEncryptionStrategy();
        Assert.Throws<ArgumentNullException>(() => strategy.Encrypt(null!));
        Assert.Throws<ArgumentNullException>(() => strategy.Decrypt(null!));
    }

    [Fact(DisplayName = "AesGcmEncryptionStrategy should throw VaultSecurityException for invalid data length during decryption.")]
    public void DecryptShouldThrowOnInvalidLength()
    {
        var strategy = new AesGcmEncryptionStrategy();
        var invalidData = new byte[5]; // Less than Nonce + Tag size
        Assert.Throws<VaultSecurityException>(() => strategy.Decrypt(invalidData));
    }
}
