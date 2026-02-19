using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Clients.Jules.Internal;

internal static class JulesQueryBuilder
{
    public static string BuildListActivitiesUri(SessionId id, RemoteActivityOptions options)
    {
        var uri = $"v1alpha/{id.Value}/activities";
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(options.PageToken))
        {
            queryParams.Add($"pageToken={options.PageToken}");
        }

        if (options.PageSize.HasValue)
        {
            queryParams.Add($"pageSize={options.PageSize.Value}");
        }

        var filter = BuildFilter(options);
        if (!string.IsNullOrEmpty(filter))
        {
            queryParams.Add($"filter={Uri.EscapeDataString(filter)}");
        }

        if (queryParams.Count > 0)
        {
            uri += "?" + string.Join("&", queryParams);
        }

        return uri;
    }

    private static string? BuildFilter(RemoteActivityOptions options)
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
