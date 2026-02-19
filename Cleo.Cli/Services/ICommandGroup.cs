using System.CommandLine;

namespace Cleo.Cli.Services;

/// <summary>
/// Represents a group of commands (domain) that can be built and added to the root command.
/// </summary>
internal interface ICommandGroup
{
    Command Build();
}
