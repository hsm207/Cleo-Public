namespace Cleo.Infrastructure.Common;

/// <summary>
/// Provides information about the current execution environment.
/// Abstracted to allow for platform-agnostic testing.
/// </summary>
public interface IPlatformProvider
{
    bool IsWindows();
}

/// <summary>
/// The default, real-world implementation of the platform provider.
/// </summary>
internal sealed class DefaultPlatformProvider : IPlatformProvider
{
    public bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
}
