using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Tests.Common;

namespace Cleo.Core.Tests.Builders;

internal sealed class SessionBuilder
{
    private SessionId _id = TestFactory.CreateSessionId("test-123");
    private string _remoteId = "remote-123";
    private TaskDescription _task = new("Fix the universe");
    private SourceContext _source = TestFactory.CreateSourceContext("org/repo");
    private SessionPulse _pulse = new(SessionStatus.StartingUp);
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow.AddHours(-1);
    private Uri? _dashboardUri = new("https://jules.com/sessions/test-123");

    public SessionBuilder WithId(string id)
    {
        // Allow raw strings if they are already prefixed, otherwise use factory logic?
        // Ideally tests should pass valid IDs. But if the test passes "sessions/foo", we can just use new SessionId.
        // If the test passes "foo", we should use the factory.
        // For Builder pattern, usually the test provides the *full* value.
        // Let's assume tests using WithId might pass raw strings needing prefixes or valid ones.
        // Given we are refactoring tests, we should update the call sites too.
        _id = new SessionId(id);
        return this;
    }

    public SessionBuilder WithTask(string task)
    {
        _task = (TaskDescription)task;
        return this;
    }

    public SessionBuilder WithPulse(SessionStatus status)
    {
        _pulse = new SessionPulse(status);
        return this;
    }

    public Session Build()
    {
        // If we want total control, we should allow passing history.
        // But for now, let's just make sure the initial activity (created inside Session ctor if history is null) uses _createdAt.
        // And _createdAt is set to -1 hour by default.
        return new Session(_id, _remoteId, _task, _source, _pulse, _createdAt, dashboardUri: _dashboardUri);
    }
}
