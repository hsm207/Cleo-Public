using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class CompositeJulesActivityMapperTests
{
    [Fact(DisplayName = "Map should delegate to matching mapper based on payload type.")]
    public void ShouldDelegateToMatchingMapper()
    {
        // Arrange
        var payload = new JulesUserMessagedPayloadDto("Hello");
        var metadata = CreateMetadata("msg-1");
        var dto = new JulesActivityDto(metadata, payload);

        var expectedActivity = new MessageActivity("id", "rid", DateTimeOffset.UtcNow, ActivityOriginator.User, "msg");

        // Mock the specific generic interface
        var subMapper = new Mock<IJulesActivityMapper<JulesUserMessagedPayloadDto>>();
        subMapper.Setup(m => m.Map(dto)).Returns(expectedActivity);

        var composite = new CompositeJulesActivityMapper(new[] { subMapper.Object });

        // Act
        var result = composite.Map(dto);

        // Assert
        result.Should().Be(expectedActivity);
        subMapper.Verify(m => m.Map(dto), Times.Once);
    }

    [Fact(DisplayName = "Map should fallback to MessageActivity when no mapper matches.")]
    public void ShouldFallbackWhenNoMapperMatches()
    {
        // Arrange
        // Create a payload type that has NO mapper registered
        // We use JulesUnknownPayloadDto but provide NO mapper for it to the composite.

        var payload = new JulesUnknownPayloadDto("SomeType", "{}");
        var metadata = CreateMetadata("unknown-1");
        var dto = new JulesActivityDto(metadata, payload);

        // Pass an empty list of mappers
        var composite = new CompositeJulesActivityMapper(Enumerable.Empty<IJulesActivityMapper>());

        // Act
        var result = composite.Map(dto);

        // Assert
        result.Should().BeOfType<MessageActivity>();
        var msg = (MessageActivity)result;
        msg.Text.Should().Contain("Unknown activity type");
        msg.Id.Should().Be("unknown-1"); // Metadata.Name -> Domain.Id
    }

    [Fact(DisplayName = "Map should handle Failure payload using FailureActivityMapper.")]
    public void ShouldMapFailurePayload()
    {
        // Arrange
        var payload = new JulesSessionFailedPayloadDto("Something went wrong");
        var metadata = CreateMetadata("fail-1");
        var dto = new JulesActivityDto(metadata, payload);

        var expectedActivity = new FailureActivity("id", "rid", DateTimeOffset.UtcNow, ActivityOriginator.System, "Reason");

        // Mock mapper for Failure payload
        var failureMapper = new Mock<IJulesActivityMapper<JulesSessionFailedPayloadDto>>();
        failureMapper.Setup(m => m.Map(dto)).Returns(expectedActivity);

        var composite = new CompositeJulesActivityMapper(new[] { failureMapper.Object });

        // Act
        var result = composite.Map(dto);

        // Assert
        result.Should().Be(expectedActivity);
    }

    private static JulesActivityMetadataDto CreateMetadata(string name)
    {
        return new JulesActivityMetadataDto(
            Id: "remote-1",
            Name: name,
            Description: "Desc",
            CreateTime: DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Originator: "agent",
            Artifacts: null
        );
    }
}
