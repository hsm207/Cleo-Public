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
            PayloadJson = "{\"RemoteId\":\"remote-123\"}" // Provided required RemoteId
        };

        var activity = _factory.FromEnvelope(envelope);

        activity.Should().BeOfType<MessageActivity>();
    }

    [Fact(DisplayName = "FailureActivityMapper should handle missing Reason gracefully.")]
    public void FailureActivityMapper_HandlesMissingReason()
    {
        var envelope = new ActivityEnvelopeDto
        {
            Type = "FAILURE",
            Id = "fail-1",
            Timestamp = DateTimeOffset.UtcNow,
            Originator = "System",
            PayloadJson = "{\"RemoteId\":\"remote-fail\"}" // Provided required RemoteId
        };

        var activity = _factory.FromEnvelope(envelope);

        activity.Should().BeOfType<FailureActivity>();
    }
}
