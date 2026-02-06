using Cleo.Core.Domain.Entities;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A secure portal for managing the developer's identity.
/// </summary>
public interface IVault
{
    /// <summary>
    /// Securely stores the developer's identity.
    /// </summary>
    Task StoreAsync(Identity identity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the developer's identity from secure storage.
    /// </summary>
    /// <returns>The stored identity, or null if none exists.</returns>
    Task<Identity?> RetrieveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forgets the stored identity.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
