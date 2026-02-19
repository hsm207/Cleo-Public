namespace Cleo.Cli.Services;

/// <summary>
/// Provides localized help strings and command descriptions.
/// Acts as a translator for the CLI.
/// </summary>
internal interface IHelpProvider
{
    string GetCommandDescription(string key);
    string GetResource(string key);
}
