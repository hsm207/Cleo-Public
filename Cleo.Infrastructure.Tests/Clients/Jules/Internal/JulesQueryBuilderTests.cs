using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Tests.Common;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules.Internal;

public class JulesQueryBuilderTests
{
    private readonly SessionId _id = TestFactory.CreateSessionId("test-123");

    [Fact(DisplayName = "BuildListActivitiesUri should construct basic URI.")]
    public void BuildsBasicUri()
    {
        var options = new RemoteActivityOptions(null, null, null, null);
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        Assert.Equal($"v1alpha/{_id.Value}/activities", uri);
    }

    [Fact(DisplayName = "BuildListActivitiesUri should include pageSize.")]
    public void BuildsPageSize()
    {
        var options = new RemoteActivityOptions(null, null, 50, null);
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        Assert.Contains("pageSize=50", uri);
    }

    [Fact(DisplayName = "BuildListActivitiesUri should include pageToken from options.")]
    public void BuildsPageToken()
    {
        var options = new RemoteActivityOptions(null, null, null, "token-1");
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        Assert.Contains("pageToken=token-1", uri);
    }

    [Fact(DisplayName = "BuildListActivitiesUri should prefer passed pageToken over options.")]
    public void BuildsNextPageToken()
    {
        var options = new RemoteActivityOptions(null, null, null, "token-1");
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options, "token-2");

        Assert.Contains("pageToken=token-2", uri);
    }

    [Fact(DisplayName = "BuildListActivitiesUri should build filter.")]
    public void BuildsFilter()
    {
        var since = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var options = new RemoteActivityOptions(since, null, null, null);
        var uri = JulesQueryBuilder.BuildListActivitiesUri(_id, options);

        Assert.Contains("filter=", uri);
        Assert.Contains(Uri.EscapeDataString("create_time >= \"2024-01-01T00:00:00Z\""), uri);
    }
}
