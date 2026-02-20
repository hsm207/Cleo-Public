using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

internal sealed class RegistryHistoryStore : IHistoryStore
{
    private static readonly string[] NewLineSeparators = ["\r\n", "\r", "\n"];

    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;
    private readonly NdjsonActivitySerializer _serializer;

    public RegistryHistoryStore(
        ISessionLayout layout,
        IFileSystem fileSystem,
        NdjsonActivitySerializer serializer)
    {
        _layout = layout;
        _fileSystem = fileSystem;
        _serializer = serializer;
    }

    public async Task AppendAsync(SessionId sessionId, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken)
    {
        var path = _layout.GetHistoryPath(sessionId);
        var dir = Path.GetDirectoryName(path);

        if (dir != null && !_fileSystem.DirectoryExists(dir))
        {
            _fileSystem.CreateDirectory(dir);
        }

        var lines = activities.Select(a => _serializer.Serialize(a));
        await _fileSystem.AppendAllLinesAsync(path, lines, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SessionActivity>> ReadAsync(SessionId sessionId, HistoryCriteria? criteria, CancellationToken cancellationToken)
    {
        var path = _layout.GetHistoryPath(sessionId);
        if (!_fileSystem.FileExists(path))
        {
            return Array.Empty<SessionActivity>();
        }

        var content = await _fileSystem.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        var lines = content.Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);

        var activities = new List<SessionActivity>();
        foreach (var line in lines)
        {
            var activity = _serializer.Deserialize(line);
            if (activity != null)
            {
                if (criteria == null || criteria.IsSatisfiedBy(activity))
                {
                    activities.Add(activity);
                }
            }
        }

        return activities.AsReadOnly();
    }
}
