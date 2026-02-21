using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class DefaultSessionPathResolver : ISessionPathResolver
{
    public string GetSessionsRoot()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Cleo", "sessions");
    }
}
