using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Tests.Common;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules.Internal;

public class JulesQueryBuilderTests
{
    private readonly SessionId _id = TestFactory.CreateSessionId("test-123");

    [Fact(DisplayName = "Given no options, when building the activities URI, then it should return a basic path without query parameters.")]
    public void ShouldConstructBasicUri_WhenNoOptionsProvided()
    {
        // Arrange
        var options = new RemoteActivityOptions(null, null, null, null);

        // Act
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        // Assert
        Assert.Equal($"v1alpha/{_id.Value}/activities", uri);
    }

    [Fact(DisplayName = "Given comprehensive fetch options, when building the activities URI, then it should include correctly escaped filter, pageSize, and pageToken.")]
    public void ShouldConstructCorrectUri_WithAllParameters()
    {
        // Arrange
        var since = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var until = since.AddDays(1);
        var options = new RemoteActivityOptions(since, until, 100, "token-456");

        // Act
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        // Assert
        Assert.StartsWith($"v1alpha/{_id.Value}/activities?", uri);
        Assert.Contains("pageSize=100", uri);
        Assert.Contains("pageToken=token-456", uri);
        
        // Verify Escaped Filter
        var expectedFilter = "create_time >= \"2024-01-01T12:00:00Z\" AND create_time <= \"2024-01-02T12:00:00Z\"";
        Assert.Contains($"filter={Uri.EscapeDataString(expectedFilter)}", uri);
    }
}
