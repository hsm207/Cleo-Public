using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

public interface IMetadataStore
{
    Task<SessionMetadataDto?> LoadAsync(SessionId sessionId, CancellationToken cancellationToken);
    Task SaveAsync(SessionMetadataDto metadata, CancellationToken cancellationToken);
}
