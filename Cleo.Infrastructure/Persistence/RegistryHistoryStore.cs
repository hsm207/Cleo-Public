using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

internal sealed class RegistryHistoryStore : IHistoryStore
{
    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner;
    private readonly NdjsonActivitySerializer _serializer;

    public RegistryHistoryStore(
        ISessionLayout layout,
        IFileSystem fileSystem,
        DirectorySessionProvisioner provisioner,
        NdjsonActivitySerializer serializer)
    {
        _layout = layout;
        _fileSystem = fileSystem;
        _provisioner = provisioner;
        _serializer = serializer;
    }

    public async Task AppendAsync(SessionId sessionId, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken)
    {
        _provisioner.EnsureSessionDirectory(sessionId);

        var path = _layout.GetHistoryPath(sessionId);
        var lines = activities.Select(a => _serializer.Serialize(a));
        await _fileSystem.AppendAllLinesAsync(path, lines, cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<SessionActivity> ReadAsync(SessionId sessionId, HistoryCriteria? criteria, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var path = _layout.GetHistoryPath(sessionId);
        if (!_fileSystem.FileExists(path))
        {
            yield break;
        }

        await foreach (var line in _fileSystem.ReadLinesAsync(path, cancellationToken).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var activity = _serializer.Deserialize(line);
            if (activity != null)
            {
                if (criteria == null || criteria.IsSatisfiedBy(activity))
                {
                    yield return activity;
                }
            }
        }
    }
}
