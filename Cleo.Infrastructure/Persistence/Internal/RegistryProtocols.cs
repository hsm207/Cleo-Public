using Cleo.Core.Domain.Entities;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Internal;

// 2. The Session Mapper (What are we saving?) 🔄
public interface IRegistryTaskMapper
{
    SessionMetadataDto MapToMetadataDto(Session session);
    Session MapFromMetadataDto(SessionMetadataDto dto);
}

public sealed record RegisteredPullRequestDto(
    Uri Url,
    string Title,
    string Description,
    string HeadRef,
    string BaseRef);
