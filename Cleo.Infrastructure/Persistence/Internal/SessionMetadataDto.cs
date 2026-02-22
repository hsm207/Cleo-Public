using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Internal;

public sealed record SessionMetadataDto(
    string SessionId,
    string RemoteId,
    string TaskDescription,
    string Repository,
    string SourceBranch,
    SessionStatus PulseStatus,
    Uri? DashboardUri,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt = null,
    RegisteredPullRequestDto? PullRequest = null);
