using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.InitiateSession;
using Xunit;

namespace Cleo.Core.Tests.UseCases.InitiateSession;

public class InitiateSessionUseCaseTests
{
    private readonly FakeJulesClient _julesClient = new();
    private readonly FakeSessionWriter _sessionWriter = new();
    private readonly InitiateSessionUseCase _sut;

    public InitiateSessionUseCaseTests()
    {
        _sut = new InitiateSessionUseCase(_julesClient, _sessionWriter);
    }

    [Fact(DisplayName = "InitiateSessionUseCase should enforce the Auto-PR policy and save the session.")]
    public async Task ExecuteAsyncShouldEnforceAutoPrAndSave()
    {
        // Arrange
        var request = new InitiateSessionRequest(
            TaskDescription: "Fix the bug",
            RepoContext: "my/repo",
            StartingBranch: "main"
        );

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsPrAutomated);
        Assert.Equal(new Uri("https://fake.jules.com/123"), result.DashboardUri);
        
        // Verify the actual state of the writer
        var savedSession = _sessionWriter.SavedSessions.Values.Single();
        Assert.Equal(result.Id, savedSession.Id);
        Assert.Equal("Fix the bug", _julesClient.LastOptions?.Title);
        Assert.Equal(AutomationMode.AutoCreatePr, _julesClient.LastOptions?.Mode);
    }

    [Fact(DisplayName = "InitiateSessionUseCase should use the user-provided title if available.")]
    public async Task ExecuteAsyncWithTitleShouldUseProvidedTitle()
    {
        // Arrange
        var request = new InitiateSessionRequest(
            TaskDescription: "Fix the bug",
            RepoContext: "my/repo",
            StartingBranch: "main",
            UserProvidedTitle: "My Cute Title"
        );

        // Act
        await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("My Cute Title", _julesClient.LastOptions?.Title);
    }

    [Fact(DisplayName = "InitiateSessionUseCase should truncate long tasks when generating a title.")]
    public async Task ExecuteAsyncLongTaskShouldTruncateTitle()
    {
        // Arrange
        var longTask = new string('a', 100);
        var request = new InitiateSessionRequest(longTask, "repo", "branch");

        // Act
        await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var title = _julesClient.LastOptions?.Title;
        Assert.NotNull(title);
        Assert.EndsWith("...", title, StringComparison.Ordinal);
        Assert.Equal(53, title.Length); // 50 chars + "..."
    }

    // --- Fakes ---

    private sealed class FakeSessionWriter : ISessionWriter
    {
        public Dictionary<SessionId, Session> SavedSessions { get; } = new();

        public Task RememberAsync(Session session, CancellationToken cancellationToken = default)
        {
            SavedSessions[session.Id] = session;
            return Task.CompletedTask;
        }

        public Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            SavedSessions.Remove(id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJulesClient : IJulesSessionClient
    {
        public SessionCreationOptions? LastOptions { get; private set; }

        public Task<Session> CreateSessionAsync(TaskDescription task, SourceContext source, SessionCreationOptions options, CancellationToken cancellationToken = default)
        {
            LastOptions = options;
            var session = new Session(
                new SessionId("sessions/fake-123"),
                "remote-fake-123",
                task,
                source,
                new SessionPulse(SessionStatus.StartingUp, "Fake Start"),
                DateTimeOffset.UtcNow,
                dashboardUri: new Uri("https://fake.jules.com/123")
            );
            return Task.FromResult(session);
        }

        public Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public Task SendMessageAsync(SessionId id, string feedback, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();
    }
}
