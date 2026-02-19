namespace Cleo.Core.Domain.ValueObjects;

public record RemoteActivityOptions(
    DateTimeOffset? Since,
    DateTimeOffset? Until,
    int? PageSize,
    string? PageToken
);
