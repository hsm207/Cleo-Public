using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using System.Text.Json;

namespace Cleo.Infrastructure.Persistence;

internal sealed class RegistryMetadataStore : IMetadataStore
{
    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public RegistryMetadataStore(ISessionLayout layout, IFileSystem fileSystem)
    {
        _layout = layout;
        _fileSystem = fileSystem;
    }

    public async Task<SessionMetadataDto?> LoadAsync(SessionId sessionId, CancellationToken cancellationToken)
    {
        var path = _layout.GetMetadataPath(sessionId);
        if (!_fileSystem.FileExists(path))
        {
            return null;
        }

        var json = await _fileSystem.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        try
        {
            return JsonSerializer.Deserialize<SessionMetadataDto>(json, Options);
        }
        catch (JsonException)
        {
            // Corrupt metadata?
            return null;
        }
    }

    public async Task SaveAsync(SessionMetadataDto metadata, CancellationToken cancellationToken)
    {
        // SessionMetadataDto stores ID as string, need to convert back to SessionId to get path.
        // Assuming metadata.SessionId is valid (it should be "sessions/123" or "123"?).
        // SessionId constructor requires "sessions/" prefix.

        SessionId sessionId;
        if (metadata.SessionId.StartsWith("sessions/", StringComparison.OrdinalIgnoreCase))
        {
            sessionId = new SessionId(metadata.SessionId);
        }
        else
        {
             // This case should ideally not happen if we map correctly, but defensively:
             sessionId = new SessionId($"sessions/{metadata.SessionId}");
        }

        var path = _layout.GetMetadataPath(sessionId);

        // Ensure directory exists - implicit responsibility of writer?
        // But for safety:
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !_fileSystem.DirectoryExists(dir))
        {
            _fileSystem.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(metadata, Options);
        await _fileSystem.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
    }
}
