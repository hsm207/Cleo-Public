using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public sealed class SessionCreationOptionsTests
{
    [Fact(DisplayName = "SessionCreationOptions should have expected defaults.")]
    public void ShouldHaveExpectedDefaults()
    {
        var options = new SessionCreationOptions();

        Assert.Equal(AutomationMode.Unspecified, options.Mode);
        Assert.Null(options.Title);
        Assert.True(options.RequirePlanApproval);
    }

    [Fact(DisplayName = "SessionCreationOptions should store custom values.")]
    public void ShouldStoreCustomValues()
    {
        var options = new SessionCreationOptions(AutomationMode.AutoCreatePr, "My Session", false);

        Assert.Equal(AutomationMode.AutoCreatePr, options.Mode);
        Assert.Equal("My Session", options.Title);
        Assert.False(options.RequirePlanApproval);
    }
}
