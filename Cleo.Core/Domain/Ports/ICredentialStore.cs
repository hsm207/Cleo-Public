using Cleo.Core.Domain.Entities;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The secure vault for the developer's persona. Responsible for persisting and retrieving Identity details.
/// </summary>
public interface ICredentialStore
{
    Task SaveIdentityAsync(Identity identity, CancellationToken cancellationToken = default);
    Task ClearIdentityAsync(CancellationToken cancellationToken = default);
    Task<Identity?> GetIdentityAsync(CancellationToken cancellationToken = default);
}
