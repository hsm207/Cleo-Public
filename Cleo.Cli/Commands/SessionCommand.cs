using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class SessionCommand : ICommandGroup
{
    private readonly NewCommand _newCommand;
    private readonly ListCommand _listCommand;
    private readonly CheckinCommand _checkinCommand;
    private readonly ForgetCommand _forgetCommand;
    private readonly IHelpProvider _helpProvider;

    public SessionCommand(
        NewCommand newCommand,
        ListCommand listCommand,
        CheckinCommand checkinCommand,
        ForgetCommand forgetCommand,
        IHelpProvider helpProvider)
    {
        _newCommand = newCommand;
        _listCommand = listCommand;
        _checkinCommand = checkinCommand;
        _forgetCommand = forgetCommand;
        _helpProvider = helpProvider;
    }

    public Command Build()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_Session_Name"), _helpProvider.GetCommandDescription("Session_Description"));

        command.AddCommand(_newCommand.Build());
        command.AddCommand(_listCommand.Build());
        command.AddCommand(_checkinCommand.Build());
        command.AddCommand(_forgetCommand.Build());

        return command;
    }
}
