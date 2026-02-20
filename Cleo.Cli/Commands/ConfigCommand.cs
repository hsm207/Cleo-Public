using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Core.UseCases.BrowseSources;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ConfigCommand : ICommandGroup
{
    private readonly AuthCommand _authCommand;
    private readonly ReposCommand _reposCommand;
    private readonly IHelpProvider _helpProvider;

    public ConfigCommand(AuthCommand authCommand, ReposCommand reposCommand, IHelpProvider helpProvider)
    {
        _authCommand = authCommand;
        _reposCommand = reposCommand;
        _helpProvider = helpProvider;
    }

    public Command Build()
    {
        var command = new Command("config", _helpProvider.GetCommandDescription("Config_Description"));

        command.AddCommand(_authCommand.Build());
        command.AddCommand(_reposCommand.Build());

        return command;
    }
}
