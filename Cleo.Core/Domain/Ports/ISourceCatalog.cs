using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The librarian of possibilities. Responsible for listing available repository sources for a new mission.
/// </summary>
public interface ISourceCatalog
{
    Task<IReadOnlyList<SessionSource>> GetAvailableSourcesAsync(CancellationToken cancellationToken = default);
}
