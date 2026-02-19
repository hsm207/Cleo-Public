using System.Globalization;
using Cleo.Cli.Resources;

namespace Cleo.Cli.Services;

/// <summary>
/// A concrete implementation of the help provider.
/// Wraps the generated CliStrings resource class.
/// </summary>
internal sealed class HelpProvider : IHelpProvider
{
    public string GetCommandDescription(string key)
    {
        return CliStrings.ResourceManager.GetString(key, CultureInfo.CurrentCulture) ?? $"[Missing: {key}]";
    }

    public string GetResource(string key)
    {
        return CliStrings.ResourceManager.GetString(key, CultureInfo.CurrentCulture) ?? $"[Missing: {key}]";
    }
}
