using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class PlanIdTests
{
    [Theory(DisplayName = "PlanId should enforce validity invariants (cannot be null or empty).")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldEnforceInvariants(string? invalidValue)
    {
        Assert.Throws<ArgumentException>(() => new PlanId(invalidValue!));

        PlanId? nullId = null;
        Assert.Throws<ArgumentNullException>(() => (string)nullId!);
    }

    [Theory(DisplayName = "PlanId should accept any non-empty string (Raw Truth).")]
    [InlineData("123")]
    [InlineData("plan/123")]
    [InlineData("sessions/123")]
    [InlineData("random-string")]
    public void ShouldAcceptAnyNonEmptyString(string validValue)
    {
        var id = new PlanId(validValue);
        Assert.Equal(validValue, id.Value);
    }

    [Fact(DisplayName = "PlanId should behave as a valid value object.")]
    public void ShouldBehaveAsValidValue()
    {
        var raw = "plans/abc-123";

        var id = new PlanId(raw);
        var fromFactory = PlanId.FromString(raw);

        Assert.Equal(raw, id.Value);
        Assert.Equal(id, fromFactory);

        var casted = (PlanId)raw;
        string backToString = id;

        Assert.Equal(id, casted);
        Assert.Equal(raw, backToString);
        Assert.Equal(raw, id.ToString());
    }
}
