using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public class ActivityMapperPersistenceTests
{
    private readonly ActivityMapperFactory _factory;

    public ActivityMapperPersistenceTests()
    {
        var artifactMapperFactory = new ArtifactMapperFactory(new IArtifactPersistenceMapper[]
        {
            new BashOutputMapper(),
            new ChangeSetMapper(),
            new MediaMapper()
        });

        var activityMappers = new IActivityPersistenceMapper[]
        {
            new Cleo.Infrastructure.Persistence.Mappers.PlanningActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.MessageActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ApprovalActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ProgressActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.CompletionActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.FailureActivityMapper(artifactMapperFactory)
        };
        _factory = new ActivityMapperFactory(activityMappers);
    }

    [Fact(DisplayName = "ActivityMapperFactory should handle corrupted/unknown originator by defaulting to System.")]
    public void ShouldHandleUnknownOriginator()
    {
        // Arrange: Create an envelope with an invalid integer string for Originator
        var envelope = new ActivityEnvelopeDto
        {
            Type = "PROGRESS", // TypeKey for ProgressActivity
            Id = "act-1",
            Timestamp = DateTimeOffset.UtcNow,
            Originator = "InvalidOriginator", // Not "Agent", "User", "System"
            PayloadJson = "{\"RemoteId\":\"remote-1\"}" // Provided required RemoteId
        };

        // Act
        var activity = _factory.FromEnvelope(envelope);

        // Assert
        Assert.Equal(ActivityOriginator.System, activity.Originator);
    }
}
