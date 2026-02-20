using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class DirectorySessionProvisioner
{
    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;

    public DirectorySessionProvisioner(ISessionLayout layout, IFileSystem fileSystem)
    {
        _layout = layout;
        _fileSystem = fileSystem;
    }

    public void EnsureSessionDirectory(SessionId sessionId)
    {
        var path = _layout.GetSessionDirectory(sessionId);
        if (!_fileSystem.DirectoryExists(path))
        {
            _fileSystem.CreateDirectory(path);
        }
    }
}
