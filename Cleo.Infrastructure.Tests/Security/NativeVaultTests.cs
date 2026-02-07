using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Security;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Security;

public class NativeVaultTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly Mock<IEncryptionStrategy> _mockStrategy = new();

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "NativeVault should use the encryption strategy to store the API Key.")]
    public async Task StoreAsyncShouldEncryptAndSave()
    {
        var vault = new NativeVault(_tempFile, _mockStrategy.Object);
        var identity = new Identity(new ApiKey("secret"));
        var encryptedBytes = new byte[] { 1, 2, 3 };

        _mockStrategy.Setup(s => s.Encrypt("secret")).Returns(encryptedBytes);

        await vault.StoreAsync(identity, TestContext.Current.CancellationToken);

        var savedBytes = await File.ReadAllBytesAsync(_tempFile, TestContext.Current.CancellationToken);
        Assert.Equal(encryptedBytes, savedBytes);
    }

    [Fact(DisplayName = "NativeVault should return the decrypted identity if it exists.")]
    public async Task RetrieveAsyncShouldDecryptAndReturn()
    {
        var vault = new NativeVault(_tempFile, _mockStrategy.Object);
        var encryptedBytes = new byte[] { 1, 2, 3 };
        await File.WriteAllBytesAsync(_tempFile, encryptedBytes, TestContext.Current.CancellationToken);

        _mockStrategy.Setup(s => s.Decrypt(encryptedBytes)).Returns("decrypted-secret");

        var result = await vault.RetrieveAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("decrypted-secret", (string)result.ApiKey);
    }

    [Fact(DisplayName = "NativeVault should throw InvalidOperationException with a helpful message if decryption fails.")]
    public async Task RetrieveAsyncShouldThrowHelpfulErrorIfDecryptionFails()
    {
        var vault = new NativeVault(_tempFile, _mockStrategy.Object);
        var encryptedBytes = new byte[] { 1, 2, 3 };
        await File.WriteAllBytesAsync(_tempFile, encryptedBytes, TestContext.Current.CancellationToken);

        _mockStrategy.Setup(s => s.Decrypt(encryptedBytes)).Throws(new System.Security.Cryptography.CryptographicException("Stale key!"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => vault.RetrieveAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Unable to decrypt your Jules API Key", ex.Message);
        Assert.Contains("Stale key!", ex.Message);
    }

    [Fact(DisplayName = "NativeVault should throw IOException if file access is blocked by an unexpected error.")]
    public async Task RetrieveAsyncShouldThrowOnUnexpectedIoError()
    {
        // We use a mock strategy that throws an IO exception (simulating locked file or disk error)
        _mockStrategy.Setup(s => s.Decrypt(It.IsAny<byte[]>())).Throws(new IOException("Disk failure!"));
        
        await File.WriteAllBytesAsync(_tempFile, new byte[] { 1, 2, 3 }, TestContext.Current.CancellationToken);
        var vault = new NativeVault(_tempFile, _mockStrategy.Object);

        await Assert.ThrowsAsync<IOException>(() => vault.RetrieveAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "NativeVault should return null if the storage file is missing.")]
    public async Task RetrieveAsyncShouldReturnNullIfFileMissing()
    {
        var vault = new NativeVault("non-existent-file", _mockStrategy.Object);
        var result = await vault.RetrieveAsync(TestContext.Current.CancellationToken);
        Assert.Null(result);
    }

    [Fact(DisplayName = "NativeVault should delete the storage file when cleared.")]
    public async Task ClearAsyncShouldDeleteFile()
    {
        await File.WriteAllTextAsync(_tempFile, "trash", TestContext.Current.CancellationToken);
        var vault = new NativeVault(_tempFile, _mockStrategy.Object);

        await vault.ClearAsync(TestContext.Current.CancellationToken);

        Assert.False(File.Exists(_tempFile));
    }

    [Fact(DisplayName = "NativeVault public constructor should initialize correctly on the current platform.")]
    public void PublicConstructorShouldInitialize()
    {
        // This test ensures the strategy selection branch is covered.
        var vault = new NativeVault();
        Assert.NotNull(vault);
    }

    [Fact(DisplayName = "NativeVault should create the storage directory if it does not exist.")]
    public void ConstructorShouldCreateDirectory()
    {
        var uniqueSubDir = Guid.NewGuid().ToString();
        var baseDir = Path.Combine(Path.GetTempPath(), uniqueSubDir);
        var originalLocalApp = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        
        try
        {
            // We simulate a custom environment to test directory creation
            // (Note: This is a bit tricky with Environment variables, 
            // but since NativeVault uses SpecialFolder.LocalApplicationData, 
            // we'll just verify the directory exists after instantiation in a new path).
            
            var testPath = Path.Combine(baseDir, "identity.dat");
            var vault = new NativeVault(testPath, _mockStrategy.Object);
            
            Assert.True(Directory.Exists(baseDir));
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }
}
