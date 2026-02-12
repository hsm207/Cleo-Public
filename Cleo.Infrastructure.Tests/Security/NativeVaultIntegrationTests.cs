using System.Security.Cryptography;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Security;
using Xunit;

namespace Cleo.Infrastructure.Tests.Security;

public class NativeVaultIntegrationTests : IDisposable
{
    private readonly string _tempFile;
    private readonly NativeVault _vault;
    private readonly AesGcmEncryptionStrategy _strategy;

    public NativeVaultIntegrationTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"cleo_vault_{Guid.NewGuid():N}.dat");
        _strategy = new AesGcmEncryptionStrategy();
        _vault = new NativeVault(_tempFile, _strategy);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "NativeVault should perform real encryption round-trip on disk.")]
    public async Task ShouldPerformRoundTripEncryption()
    {
        var secret = "MySecretToken123!";
        var identity = new Identity((ApiKey)secret);

        // 1. Store (Encrypt) 🔐
        await _vault.StoreAsync(identity, CancellationToken.None);

        // 2. Verify file exists on disk (The Metal) 💿
        Assert.True(File.Exists(_tempFile));

        // 3. Verify content is NOT plain text 🕵️‍♀️
        var fileContent = await File.ReadAllTextAsync(_tempFile);
        Assert.DoesNotContain(secret, fileContent);

        // 4. Retrieve (Decrypt) 🔓
        var retrieved = await _vault.RetrieveAsync(CancellationToken.None);
        Assert.NotNull(retrieved);
        Assert.Equal(secret, (string)retrieved!.ApiKey);
    }

    [Fact(DisplayName = "NativeVault should throw InvalidOperationException if data is tampered.")]
    public async Task ShouldDetectTampering()
    {
        // 1. Store valid secret
        await _vault.StoreAsync(new Identity((ApiKey)"Valid"), CancellationToken.None);

        // 2. Tamper with file (flip a bit in the middle) 🔨
        var bytes = await File.ReadAllBytesAsync(_tempFile);
        bytes[bytes.Length / 2] ^= 0xFF;
        await File.WriteAllBytesAsync(_tempFile, bytes);

        // 3. Attempt Retrieve -> Should Fail 🛑
        // Note: NativeVault wraps VaultSecurityException in InvalidOperationException
        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => _vault.RetrieveAsync(CancellationToken.None));
    }

    [Fact(DisplayName = "NativeVault should create directory if missing.")]
    public async Task ShouldCreateDirectoryIfMissing()
    {
        var nestedDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var nestedFile = Path.Combine(nestedDir, "secret.dat");
        var vault = new NativeVault(nestedFile, new AesGcmEncryptionStrategy());

        try
        {
            await vault.StoreAsync(new Identity((ApiKey)"Secret"), CancellationToken.None);
            Assert.True(Directory.Exists(nestedDir));
            Assert.True(File.Exists(nestedFile));
        }
        finally
        {
            if (Directory.Exists(nestedDir)) Directory.Delete(nestedDir, true);
        }
    }

    [Fact(DisplayName = "NativeVault should clear secrets from disk.")]
    public async Task ShouldClearSecrets()
    {
        await _vault.StoreAsync(new Identity((ApiKey)"Secret"), CancellationToken.None);
        Assert.True(File.Exists(_tempFile));

        await _vault.ClearAsync(CancellationToken.None);
        Assert.False(File.Exists(_tempFile));
    }

    [Fact(DisplayName = "NativeVault should implement explicit ICredentialStore methods.")]
    public async Task ShouldImplementICredentialStore()
    {
        ICredentialStore store = _vault;
        var identity = new Identity((ApiKey)"Secret");

        // Save
        await store.SaveIdentityAsync(identity, CancellationToken.None);
        Assert.True(File.Exists(_tempFile));

        // Get
        var retrieved = await store.GetIdentityAsync(CancellationToken.None);
        Assert.NotNull(retrieved);
        Assert.Equal("Secret", (string)retrieved!.ApiKey);

        // Clear
        await store.ClearIdentityAsync(CancellationToken.None);
        Assert.False(File.Exists(_tempFile));
    }
}
