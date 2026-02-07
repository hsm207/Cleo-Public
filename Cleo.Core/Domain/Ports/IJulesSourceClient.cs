using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for retrieving available source repositories from Jules.
/// </summary>
public interface IJulesSourceClient
{
    /// <summary>
    /// Lists the available sources in the Jules account.
    /// </summary>
    Task<IReadOnlyCollection<SessionSource>> ListSourcesAsync(CancellationToken cancellationToken = default);
}
