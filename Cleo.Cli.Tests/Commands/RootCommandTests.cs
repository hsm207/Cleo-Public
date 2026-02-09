using Cleo.Cli.Commands;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Core.UseCases.ListSessions;
using Cleo.Core.UseCases.ForgetSession;
using Cleo.Core.UseCases.AuthenticateUser;
using Cleo.Core.UseCases.BrowseSources;
using Cleo.Core.Domain.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class RootCommandTests
{
    private readonly LogCommand _logCommand;
    private readonly StatusCommand _statusCommand;
    private readonly ListCommand _listCommand;
    private readonly ForgetCommand _forgetCommand;
    private readonly AuthCommand _authCommand;
    private readonly ReposCommand _reposCommand;

    public RootCommandTests()
    {
        _logCommand = new LogCommand(new Mock<IBrowseHistoryUseCase>().Object, new Mock<ILogger<LogCommand>>().Object);
        _statusCommand = new StatusCommand(new Mock<IRefreshPulseUseCase>().Object, new Mock<ILogger<StatusCommand>>().Object);
        _listCommand = new ListCommand(new Mock<IListSessionsUseCase>().Object, new Mock<ILogger<ListCommand>>().Object);
        _forgetCommand = new ForgetCommand(new Mock<IForgetSessionUseCase>().Object, new Mock<ILogger<ForgetCommand>>().Object);
        _authCommand = new AuthCommand(new Mock<IAuthenticateUserUseCase>().Object, new Mock<ICredentialStore>().Object, new Mock<ILogger<AuthCommand>>().Object);
        _reposCommand = new ReposCommand(new Mock<IBrowseSourcesUseCase>().Object, new Mock<ILogger<ReposCommand>>().Object);
    }

    [Fact(DisplayName = "Given the root command, when viewing help, then it should use the authoritative Glossary terms.")]
    public void Build_ShouldUseAuthoritativeGlossaryTerms()
    {
        // Act
        // This test checks individual command descriptions as they would appear in the root command's help.

        var log = _logCommand.Build();
        var status = _statusCommand.Build();
        var list = _listCommand.Build();
        var forget = _forgetCommand.Build();
        var auth = _authCommand.Build();
        var repos = _reposCommand.Build();

        // Assert
        log.Description.Should().Contain("audit trail");
        status.Description.Should().Contain("Pulse");
        status.Description.Should().Contain("Stance");
        list.Description.Should().Contain("Session Registry");
        forget.Description.Should().Contain("Session Registry");
        auth.Description.Should().Contain("Identity");
        auth.Description.Should().Contain("Vault");
        repos.Description.Should().Contain("GitHub repositories");
    }
}
