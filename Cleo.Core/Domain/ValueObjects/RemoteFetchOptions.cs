namespace Cleo.Core.Domain.ValueObjects;

public record RemoteFetchOptions(
    DateTimeOffset? Since,
    DateTimeOffset? Until,
    int? PageSize
);
