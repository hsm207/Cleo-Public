using System.Text.Json;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesDtoSerializationTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    [Fact(DisplayName = "Given a JSON with 'agentMessaged' payload, it should deserialize correctly.")]
    public void JulesActivityConverter_ShouldDeserialize_AgentMessaged()
    {
        // Arrange
        var json = """
        {
          "id": "act-1",
          "name": "sessions/123/activities/act-1",
          "createTime": "2023-10-27T12:00:00Z",
          "originator": "agent",
          "agentMessaged": {
            "agentMessage": "Hello human!"
          }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Id.Should().Be("act-1");

        var payload = result.Payload.Should().BeOfType<JulesAgentMessagedPayloadDto>().Subject;
        payload.AgentMessage.Should().Be("Hello human!");
    }

    [Fact(DisplayName = "Given a JSON with 'bashOutput' payload, it should deserialize correctly.")]
    public void JulesActivityConverter_ShouldDeserialize_BashOutput()
    {
        // Arrange
        var json = """
        {
          "id": "act-2",
          "name": "rem-2",
          "createTime": "2023-10-27T12:05:00Z",
          "originator": "agent",
          "bashOutput": {
            "command": "ls -la",
            "output": "total 0",
            "exitCode": 0
          }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);

        // Assert
        result.Should().NotBeNull();
        var payload = result!.Payload.Should().BeOfType<JulesBashOutputPayloadDto>().Subject;
        payload.Command.Should().Be("ls -la");
        payload.Output.Should().Be("total 0");
        payload.ExitCode.Should().Be(0);
    }

    [Fact(DisplayName = "Given a JSON with 'codeChanges' payload, it should deserialize correctly.")]
    public void JulesActivityConverter_ShouldDeserialize_CodeChanges()
    {
        // Arrange
        var json = """
        {
          "id": "act-3",
          "name": "rem-3",
          "createTime": "2023-10-27T12:10:00Z",
          "originator": "agent",
          "codeChanges": {
            "source": "src/main.cs",
            "gitPatch": {
                "unidiffPatch": "diff",
                "baseCommitId": "sha123",
                "suggestedCommitMessage": "fix bug"
            }
          }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);

        // Assert
        result.Should().NotBeNull();
        var payload = result!.Payload.Should().BeOfType<JulesCodeChangesPayloadDto>().Subject;
        payload.Source.Should().Be("src/main.cs");
        payload.GitPatch.Should().NotBeNull();
        payload.GitPatch!.UnidiffPatch.Should().Be("diff");
        payload.GitPatch.BaseCommitId.Should().Be("sha123");
        payload.GitPatch.SuggestedCommitMessage.Should().Be("fix bug");
    }

    [Fact(DisplayName = "Given a JSON with 'media' payload, it should deserialize correctly.")]
    public void JulesActivityConverter_ShouldDeserialize_Media()
    {
        // Arrange
        var json = """
        {
          "id": "act-4",
          "name": "rem-4",
          "createTime": "2023-10-27T12:15:00Z",
          "originator": "agent",
          "media": {
            "mimeType": "image/png",
            "data": "base64data"
          }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);

        // Assert
        result.Should().NotBeNull();
        var payload = result!.Payload.Should().BeOfType<JulesMediaPayloadDto>().Subject;
        payload.MimeType.Should().Be("image/png");
        payload.Data.Should().Be("base64data");
    }

    [Fact(DisplayName = "Given a DTO, it should serialize back to the correct JSON structure (Round-Trip).")]
    public void JulesActivityConverter_ShouldSerialize_Correctly()
    {
        // Arrange
        var metadata = new JulesActivityMetadataDto("act-5", "rem-5", null, DateTimeOffset.UtcNow.ToString("O"), "agent", null);
        var payload = new JulesUserMessagedPayloadDto("Hi Agent!");
        var dto = new JulesActivityDto(metadata, payload);

        // Act
        var json = JsonSerializer.Serialize(dto, Options);

        // Assert
        json.Should().Contain("userMessaged");
        json.Should().Contain("Hi Agent!");

        // Round trip check
        var deserialized = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);
        deserialized.Should().NotBeNull();
        deserialized!.Payload.Should().BeOfType<JulesUserMessagedPayloadDto>();
    }

    [Fact(DisplayName = "Given an unknown payload type, it should fallback to a generic message payload.")]
    public void JulesActivityConverter_ShouldHandle_UnknownPayload()
    {
        // Arrange
        var json = """
        {
          "id": "act-6",
          "name": "rem-6",
          "createTime": "2023-10-27T12:20:00Z",
          "originator": "system",
          "futureEvent": {
            "data": "something new"
          }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<JulesActivityDto>(json, Options);

        // Assert
        result.Should().NotBeNull();
        // Should fallback to Progress/Message payload with "Unknown Event" title
        var payload = result!.Payload.Should().BeOfType<JulesProgressUpdatedPayloadDto>().Subject;
        payload.Title.Should().Be("Unknown Event");
    }
}
