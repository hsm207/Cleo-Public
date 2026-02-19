using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class AuthCommand
{
    private readonly IAuthenticateUserUseCase _authenticateUseCase;
    private readonly IVault _vault;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<AuthCommand> _logger;

    public AuthCommand(
        IAuthenticateUserUseCase authenticateUseCase,
        IVault vault,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<AuthCommand> logger)
    {
        _authenticateUseCase = authenticateUseCase;
        _vault = vault;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var authCommand = new Command("auth", _helpProvider.GetCommandDescription("Auth_Description"));

        var loginCommand = new Command("login", _helpProvider.GetCommandDescription("Auth_Login_Description"));
        var keyArgument = new Argument<string>("key", _helpProvider.GetCommandDescription("Auth_Key_Description"));
        loginCommand.AddArgument(keyArgument);
        loginCommand.SetHandler(async (key) => await ExecuteLoginAsync(key), keyArgument);

        var logoutCommand = new Command("logout", _helpProvider.GetCommandDescription("Auth_Logout_Description"));
        logoutCommand.SetHandler(async () => await ExecuteLogoutAsync());

        authCommand.AddCommand(loginCommand);
        authCommand.AddCommand(logoutCommand);

        return authCommand;
    }

    private async Task ExecuteLoginAsync(string key)
    {
        try
        {
            var request = new AuthenticateUserRequest(key);
            var response = await _authenticateUseCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.Success)
            {
                _presenter.PresentSuccess(response.Message);
            }
            else
            {
                _presenter.PresentError(response.Message);
            }
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to login.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await _vault.ClearAsync(CancellationToken.None).ConfigureAwait(false);
        _presenter.PresentSuccess(_helpProvider.GetResource("Auth_Logout_Success"));
    }
}
