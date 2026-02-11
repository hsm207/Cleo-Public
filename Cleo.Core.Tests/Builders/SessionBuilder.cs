using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Tests.Builders;

internal sealed class SessionBuilder
{
    private SessionId _id = new("sessions/test-123");
    private string _remoteId = "remote-123";
    private TaskDescription _task = new("Fix the universe");
    private SourceContext _source = new("org/repo", "main");
    private SessionPulse _pulse = new(SessionStatus.StartingUp, "Starting...");
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private Uri? _dashboardUri = new("https://jules.com/sessions/test-123");

    public SessionBuilder WithId(string id)
    {
        _id = new SessionId(id);
        return this;
    }

    public SessionBuilder WithTask(string task)
    {
        _task = (TaskDescription)task;
        return this;
    }

    public SessionBuilder WithPulse(SessionStatus status, string detail = "")
    {
        _pulse = new SessionPulse(status, detail);
        return this;
    }

    public Session Build()
    {
        return new Session(_id, _remoteId, _task, _source, _pulse, _createdAt, dashboardUri: _dashboardUri);
    }
}
