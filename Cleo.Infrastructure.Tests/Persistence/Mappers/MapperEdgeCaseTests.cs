using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence.Mappers;

public class MapperEdgeCaseTests
{
    private readonly ActivityMapperFactory _factory;

    public MapperEdgeCaseTests()
    {
        var artifactMapper = new ArtifactMapperFactory(new IArtifactPersistenceMapper[]
        {
            new BashOutputMapper(),
            new ChangeSetMapper(),
            new MediaMapper()
        });

        var activityMappers = new IActivityPersistenceMapper[]
        {
            new Cleo.Infrastructure.Persistence.Mappers.MessageActivityMapper(artifactMapper),
            new Cleo.Infrastructure.Persistence.Mappers.FailureActivityMapper(artifactMapper),
            // Add others if needed for coverage
        };
        _factory = new ActivityMapperFactory(activityMappers);
    }

    [Fact(DisplayName = "MessageActivityMapper should handle missing payload gracefully.")]
    public void MessageActivityMapper_HandlesMissingPayload()
    {
        var envelope = new ActivityEnvelopeDto
        {
            Type = "MESSAGE",
            Id = "msg-1",
            Timestamp = DateTimeOffset.UtcNow,
            Originator = "Agent",
            PayloadJson = "{}" // Missing Text field
        };

        var activity = _factory.FromEnvelope(envelope);

        activity.Should().BeOfType<MessageActivity>();
        // Assuming default or empty string if missing?
        // Need to check MessageActivityMapper implementation to be sure.
        // Usually JSON deserializer leaves it null or default.
        // If required, it might throw. The goal is to verify behavior.
    }

    [Fact(DisplayName = "FailureActivityMapper should handle missing Reason gracefully.")]
    public void FailureActivityMapper_HandlesMissingReason()
    {
        var envelope = new ActivityEnvelopeDto
        {
            Type = "FAILED",
            Id = "fail-1",
            Timestamp = DateTimeOffset.UtcNow,
            Originator = "System",
            PayloadJson = "{}" // Missing Reason
        };

        var activity = _factory.FromEnvelope(envelope);

        activity.Should().BeOfType<FailureActivity>();
    }
}
