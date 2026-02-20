using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class DirectorySessionLayout : ISessionLayout
{
    private readonly ISessionPathResolver _pathResolver;
    private const string MetadataFileName = "session.json";
    private const string HistoryFileName = "activities.jsonl";

    public DirectorySessionLayout(ISessionPathResolver pathResolver)
    {
        _pathResolver = pathResolver;
    }

    public string GetSessionDirectory(SessionId sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        var sessionsRoot = _pathResolver.GetSessionsRoot();
        // The SessionId value object handles the "sessions/" prefix internally (e.g. "sessions/123"),
        // but for folder storage we only want the ID part ("123").
        // We rely on SessionId.Value being the raw ID if it follows the "sessions/" convention,
        // or we need to extract it.
        // Wait, SessionId.Value usually is "sessions/123".
        // The folder should be "123".

        var rawId = ExtractRawId(sessionId);
        return Path.Combine(sessionsRoot, rawId);
    }

    public string GetMetadataPath(SessionId sessionId)
    {
        return Path.Combine(GetSessionDirectory(sessionId), MetadataFileName);
    }

    public string GetHistoryPath(SessionId sessionId)
    {
        return Path.Combine(GetSessionDirectory(sessionId), HistoryFileName);
    }

    private static string ExtractRawId(SessionId sessionId)
    {
        // SessionId ensures the prefix "sessions/" is present.
        // We want the part after "sessions/".
        const string prefix = "sessions/";
        if (sessionId.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return sessionId.Value[prefix.Length..];
        }
        // Fallback for edge cases (though SessionId should enforce prefix)
        return sessionId.Value;
    }
}
