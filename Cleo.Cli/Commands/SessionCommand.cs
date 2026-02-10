using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class SessionCommand
{
    private readonly NewCommand _newCommand;
    private readonly ListCommand _listCommand;
    private readonly StatusCommand _statusCommand;
    private readonly ForgetCommand _forgetCommand;

    public SessionCommand(
        NewCommand newCommand,
        ListCommand listCommand,
        StatusCommand statusCommand,
        ForgetCommand forgetCommand)
    {
        _newCommand = newCommand;
        _listCommand = listCommand;
        _statusCommand = statusCommand;
        _forgetCommand = forgetCommand;
    }

    public Command Build()
    {
        var command = new Command("session", "Lifecycle Management. Use this to initiate, list, recover, or evaluate the pulse and stance of an engineering collaboration. More specialized subcommands available. Use --help to explore further.");

        command.AddCommand(_newCommand.Build());
        command.AddCommand(_listCommand.Build());
        command.AddCommand(_statusCommand.Build());
        command.AddCommand(_forgetCommand.Build());

        return command;
    }
}
