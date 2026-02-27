using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using System.Text.Json;

namespace Cleo.Infrastructure.Persistence;

internal sealed class RegistryMetadataStore : IMetadataStore
{
    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public RegistryMetadataStore(
        ISessionLayout layout,
        IFileSystem fileSystem,
        DirectorySessionProvisioner provisioner)
    {
        _layout = layout;
        _fileSystem = fileSystem;
        _provisioner = provisioner;
    }

    public async Task<SessionMetadataDto?> LoadAsync(SessionId sessionId, CancellationToken cancellationToken)
    {
        var path = _layout.GetMetadataPath(sessionId);
        if (!_fileSystem.FileExists(path))
        {
            return null;
        }

        var json = await _fileSystem.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<SessionMetadataDto>(json, Options);
    }

    public async Task SaveAsync(SessionMetadataDto metadata, CancellationToken cancellationToken)
    {
        var sessionId = new SessionId(metadata.SessionId);
        var path = _layout.GetMetadataPath(sessionId);
        _provisioner.EnsureSessionDirectory(sessionId);

        var json = JsonSerializer.Serialize(metadata, Options);
        await _fileSystem.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
    }
}
