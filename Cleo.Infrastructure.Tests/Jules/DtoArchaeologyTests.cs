/*
using System.Text.Json;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class DtoArchaeologyTests
{
    [Fact(DisplayName = "JulesActivityDto should reflect the sectioned structure (Metadata/Payload).")]
    public void JulesActivityDto_ShouldHaveSectionedStructure()
    {
        var json = @"{
            ""name"": ""n"",
            ""id"": ""i"",
            ""createTime"": ""2024-01-01T00:00:00Z"",
            ""originator"": ""agent"",
            ""progressUpdated"": { ""title"": ""T"", ""description"": ""D"" }
        }";

        var dto = JsonSerializer.Deserialize<JulesActivityDto>(json);

        dto.Should().NotBeNull();
        dto!.Metadata.Id.Should().Be("i");
        dto.Payload.ProgressUpdated.Should().NotBeNull();
        dto.Payload.ProgressUpdated!.Title.Should().Be("T");
    }
}
*/