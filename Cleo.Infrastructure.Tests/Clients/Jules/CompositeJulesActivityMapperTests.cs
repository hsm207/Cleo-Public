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
    [Fact(DisplayName = "Map should delegate to matching mapper.")]
    public void ShouldDelegateToMatchingMapper()
    {
        // Arrange
        var subMapper = new Mock<IJulesActivityMapper>();
        var dto = CreateDto("known");
        var expectedActivity = new MessageActivity("id", "rid", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "msg");

        subMapper.Setup(m => m.CanMap(dto)).Returns(true);
        subMapper.Setup(m => m.Map(dto)).Returns(expectedActivity);

        var composite = new CompositeJulesActivityMapper(new[] { subMapper.Object });

        // Act
        var result = composite.Map(dto);

        // Assert
        result.Should().Be(expectedActivity);
    }

    [Fact(DisplayName = "Map should fallback to MessageActivity when no mapper matches.")]
    public void ShouldFallbackWhenNoMapperMatches()
    {
        // Arrange
        var subMapper = new Mock<IJulesActivityMapper>();
        var dto = CreateDto("unknown");

        subMapper.Setup(m => m.CanMap(dto)).Returns(false); // No match

        var composite = new CompositeJulesActivityMapper(new[] { subMapper.Object });

        // Act
        var result = composite.Map(dto);

        // Assert
        result.Should().BeOfType<MessageActivity>();
        var msg = (MessageActivity)result;
        msg.Text.Should().Contain("Unknown activity type");
        msg.Id.Should().Be("name-1"); // Metadata.Name -> Domain.Id
    }

    private static JulesActivityDto CreateDto(string name)
    {
        var metadata = new JulesActivityMetadataDto(
            Id: "remote-1",
            Name: "name-1",
            Description: "Desc",
            CreateTime: DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Originator: "agent",
            Artifacts: null
        );
        return new JulesActivityDto(metadata, new JulesProgressUpdatedPayloadDto("T", "D"));
    }
}
