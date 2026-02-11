using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal static class ActivityOriginatorMapper
{
    public static ActivityOriginator Map(string? originator) => originator?.ToUpperInvariant() switch
    {
        "USER" => ActivityOriginator.User,
        "AGENT" => ActivityOriginator.Agent,
        "SYSTEM" => ActivityOriginator.System,
        _ => ActivityOriginator.User // Default fallback
    };
}
