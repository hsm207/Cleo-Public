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
    private readonly ActivitiesCommand _activitiesCommand;
    private readonly StatusCommand _statusCommand;
    private readonly ListCommand _listCommand;
    private readonly ForgetCommand _forgetCommand;
    private readonly AuthCommand _authCommand;
    private readonly SourcesCommand _sourcesCommand;

    public RootCommandTests()
    {
        _activitiesCommand = new ActivitiesCommand(new Mock<IBrowseHistoryUseCase>().Object, new Mock<ILogger<ActivitiesCommand>>().Object);
        _statusCommand = new StatusCommand(new Mock<IRefreshPulseUseCase>().Object, new Mock<ILogger<StatusCommand>>().Object);
        _listCommand = new ListCommand(new Mock<IListSessionsUseCase>().Object, new Mock<ILogger<ListCommand>>().Object);
        _forgetCommand = new ForgetCommand(new Mock<IForgetSessionUseCase>().Object, new Mock<ILogger<ForgetCommand>>().Object);
        _authCommand = new AuthCommand(new Mock<IAuthenticateUserUseCase>().Object, new Mock<ICredentialStore>().Object, new Mock<ILogger<AuthCommand>>().Object);
        _sourcesCommand = new SourcesCommand(new Mock<IBrowseSourcesUseCase>().Object, new Mock<ILogger<SourcesCommand>>().Object);
    }

    [Fact(DisplayName = "Given the root command, when viewing help, then it should use the authoritative Glossary terms.")]
    public void Build_ShouldUseAuthoritativeGlossaryTerms()
    {
        // Act
        // This test checks individual command descriptions as they would appear in the root command's help.
        // It's not testing "the root command" directly because I don't have access to Program.cs here easily,
        // but I can verify the components that make up the root help.

        var activities = _activitiesCommand.Build();
        var status = _statusCommand.Build();
        var list = _listCommand.Build();
        var forget = _forgetCommand.Build();
        var auth = _authCommand.Build();
        var sources = _sourcesCommand.Build();

        // Assert
        activities.Description.Should().Contain("Session Log");
        status.Description.Should().Contain("Pulse");
        status.Description.Should().Contain("Stance");
        list.Description.Should().Contain("Session Registry");
        forget.Description.Should().Contain("Session Registry");
        auth.Description.Should().Contain("Identity");
        auth.Description.Should().Contain("Vault");
        sources.Description.Should().Contain("GitHub repositories"); // As per user update for sources
    }
}
