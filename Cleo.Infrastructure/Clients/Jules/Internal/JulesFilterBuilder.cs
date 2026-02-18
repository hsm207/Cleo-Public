using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Clients.Jules.Internal;

internal static class JulesFilterBuilder
{
    public static string? Build(RemoteFetchOptions options)
    {
        var clauses = new List<string>();

        if (options.Since.HasValue)
        {
            clauses.Add($"create_time >= \"{options.Since.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}\"");
        }

        if (options.Until.HasValue)
        {
            clauses.Add($"create_time <= \"{options.Until.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}\"");
        }

        if (clauses.Count == 0) return null;

        return string.Join(" AND ", clauses);
    }
}
