using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Security;
using Xunit;

namespace Cleo.Infrastructure.Tests.Security;

public class NativeVaultTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly IEncryptionStrategy _strategy = new AesGcmEncryptionStrategy();

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "NativeVault should use the encryption strategy to store the API Key.")]
    public async Task StoreAsyncShouldEncryptAndSave()
    {
        var vault = new NativeVault(_tempFile, _strategy);
        var identity = new Identity(new ApiKey("secret-api-key"));

        // Act
        await vault.StoreAsync(identity, TestContext.Current.CancellationToken);

        // Assert
        var savedBytes = await File.ReadAllBytesAsync(_tempFile, TestContext.Current.CancellationToken);
        Assert.NotEmpty(savedBytes);
        
        // Decrypt manually to verify it's actually correct
        var decrypted = _strategy.Decrypt(savedBytes);
        Assert.Equal("secret-api-key", decrypted);
    }

    [Fact(DisplayName = "NativeVault should return the decrypted identity if it exists.")]
    public async Task RetrieveAsyncShouldDecryptAndReturn()
    {
        var vault = new NativeVault(_tempFile, _strategy);
        var identity = new Identity(new ApiKey("real-vibes-only"));
        
        // Arrange: Store real encrypted data
        var encrypted = _strategy.Encrypt("real-vibes-only");
        await File.WriteAllBytesAsync(_tempFile, encrypted, TestContext.Current.CancellationToken);

        // Act
        var result = await vault.RetrieveAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("real-vibes-only", (string)result.ApiKey);
    }

    [Fact(DisplayName = "NativeVault should throw InvalidOperationException with a helpful message if decryption fails.")]
    public async Task RetrieveAsyncShouldThrowHelpfulErrorIfDecryptionFails()
    {
        var vault = new NativeVault(_tempFile, _strategy);
        // Write garbage data that satisfies the length check (28+ bytes) but is invalid AES-GCM
        var garbage = new byte[32];
        new Random().NextBytes(garbage);
        await File.WriteAllBytesAsync(_tempFile, garbage, TestContext.Current.CancellationToken);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => vault.RetrieveAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Unable to decrypt your Jules API Key", ex.Message);
    }

    [Fact(DisplayName = "NativeVault should return null if the storage file is missing.")]
    public async Task RetrieveAsyncShouldReturnNullIfFileMissing()
    {
        var vault = new NativeVault("non-existent-file", _strategy);
        var result = await vault.RetrieveAsync(TestContext.Current.CancellationToken);
        Assert.Null(result);
    }

    [Fact(DisplayName = "NativeVault should delete the storage file when cleared.")]
    public async Task ClearAsyncShouldDeleteFile()
    {
        await File.WriteAllTextAsync(_tempFile, "trash", TestContext.Current.CancellationToken);
        var vault = new NativeVault(_tempFile, _strategy);

        await vault.ClearAsync(TestContext.Current.CancellationToken);

        Assert.False(File.Exists(_tempFile));
    }

    [Fact(DisplayName = "NativeVault should create the storage directory if it does not exist.")]
    public void ConstructorShouldCreateDirectory()
    {
        var uniqueSubDir = Guid.NewGuid().ToString("N");
        var baseDir = Path.Combine(Path.GetTempPath(), uniqueSubDir);
        var testPath = Path.Combine(baseDir, "identity.dat");
        
        try
        {
            var vault = new NativeVault(testPath, _strategy);
            Assert.True(Directory.Exists(baseDir));
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }
}
