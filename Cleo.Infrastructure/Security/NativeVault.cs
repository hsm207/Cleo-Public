using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Security;

/// <summary>
/// A secure vault implementation that uses OS-native protection where possible.
/// </summary>
public sealed class NativeVault : IVault, ICredentialStore
{
    private readonly string _storagePath;
    private readonly IEncryptionStrategy _strategy;

    public NativeVault(string storagePath, IEncryptionStrategy strategy)
    {
        _storagePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

        // Ensure directory exists for whichever path we are using
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task StoreAsync(Identity identity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var apiKey = (string)identity.ApiKey;
        var encrypted = _strategy.Encrypt(apiKey);

        await File.WriteAllBytesAsync(_storagePath, encrypted, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the developer's identity from secure storage.
    /// </summary>
    public async Task<Identity?> RetrieveAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_storagePath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(_storagePath, cancellationToken).ConfigureAwait(false);
            var decrypted = _strategy.Decrypt(encrypted);
            
            var identity = new Identity((ApiKey)decrypted);
            return identity;
        }
        catch (VaultSecurityException ex)
        {
            throw new InvalidOperationException(
                $"❌ Critical Error: Unable to decrypt your Jules API Key. " +
                $"This usually happens if your machine configuration changed or the storage file was corrupted. " +
                $"Please re-authenticate by running 'cleo auth login'. " +
                $"Details: {ex.Message}", ex);
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_storagePath))
        {
            File.Delete(_storagePath);
        }
        return Task.CompletedTask;
    }

    async Task ICredentialStore.SaveIdentityAsync(Identity identity, CancellationToken cancellationToken)
    {
        await StoreAsync(identity, cancellationToken).ConfigureAwait(false);
    }

    async Task ICredentialStore.ClearIdentityAsync(CancellationToken cancellationToken)
    {
        await ClearAsync(cancellationToken).ConfigureAwait(false);
    }

    async Task<Identity?> ICredentialStore.GetIdentityAsync(CancellationToken cancellationToken)
    {
        return await RetrieveAsync(cancellationToken).ConfigureAwait(false);
    }
}
