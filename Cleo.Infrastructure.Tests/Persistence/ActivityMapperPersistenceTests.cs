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
            // RemoteId is not in the DTO currently, it's inferred or managed elsewhere?
            // Wait, ActivityEnvelopeDto doesn't have RemoteId based on cat output.
            // Ah, SessionActivity has RemoteId. Where is it stored?
            // Let me check ActivityMapperFactory.ToEnvelope again.
            // Id = activity.Id.
            // It seems RemoteId is NOT persisted in ActivityEnvelopeDto! This might be a bug or intentional.
            // Wait, SessionActivity(Id, RemoteId, ...).
            // If Persistence misses RemoteId, that's a data loss issue!
            // BUT for this test, I just need to compile.
            Timestamp = DateTimeOffset.UtcNow,
            Originator = "InvalidOriginator", // Not "Agent", "User", "System"
            PayloadJson = "{}" // Dummy payload
        };

        // Act
        var activity = _factory.FromEnvelope(envelope);

        // Assert
        Assert.Equal(ActivityOriginator.System, activity.Originator); // Fallback behavior check 🛡️
    }
}
