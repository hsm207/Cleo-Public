using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionPulseTests
{
    [Fact(DisplayName = "A SessionPulse should correctly store the status and optional details.")]
    public void ConstructorShouldSetValues()
    {
        var status = SessionStatus.InProgress;
        var detail = "Working on it...";
        var pulse = new SessionPulse(status, detail);

        Assert.Equal(status, pulse.Status);
        Assert.Equal(detail, pulse.Detail);
    }

    [Fact(DisplayName = "A SessionPulse should work perfectly fine with just a status.")]
    public void ConstructorShouldWorkWithDefaultDetail()
    {
        var pulse = new SessionPulse(SessionStatus.StartingUp);
        
        Assert.Equal(SessionStatus.StartingUp, pulse.Status);
        Assert.Null(pulse.Detail);
    }
}
