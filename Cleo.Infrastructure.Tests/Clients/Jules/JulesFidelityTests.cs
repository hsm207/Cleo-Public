using System.Text.Json;
using System.Text.Json.Serialization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Entities;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesFidelityTests
{
    private static readonly JsonSerializerOptions Options = new() 
    { 
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact(DisplayName = "Round-Trip: Activity List Response supports perfect, lossless JSON symmetry")]
    public void ActivityListResponseShouldSupportLosslessRoundTrip()
    {
        var path = Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "raw_activities_list.json");
        var jsonContent = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(jsonContent);

        var dto = JsonSerializer.Deserialize<JulesListActivitiesResponseDto>(jsonContent, Options);
        Assert.NotNull(dto);

        var roundTripJson = JsonSerializer.Serialize(dto, Options);
        using var roundTripDoc = JsonDocument.Parse(roundTripJson);
        AssertJsonElementsEqual(doc.RootElement, roundTripDoc.RootElement);
    }

    [Fact(DisplayName = "Round-Trip: Session Response supports perfect, lossless JSON symmetry")]
    public void SessionResponseShouldSupportLosslessRoundTrip()
    {
        var path = Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "session_created.json");
        var jsonContent = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(jsonContent);

        var dto = JsonSerializer.Deserialize<JulesSessionResponseDto>(jsonContent, Options);
        Assert.NotNull(dto);

        var roundTripJson = JsonSerializer.Serialize(dto, Options);
        using var roundTripDoc = JsonDocument.Parse(roundTripJson);
        AssertJsonElementsEqual(doc.RootElement, roundTripDoc.RootElement);
    }

    [Fact(DisplayName = "Mapping: All API fields must reach the Domain (NO DATA LOSS)")]
    public void MappingShouldPreserveAllApiFields()
    {
        var path = Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "session_created.json");
        var jsonContent = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<JulesSessionResponseDto>(jsonContent, Options)!;
        var statusMapper = new DefaultSessionStatusMapper();

        var domain = JulesMapper.Map(dto, statusMapper);

        // Identity
        domain.Id.Value.Should().Be(dto.Name);
        
        // 🛑 FAILING CHECKS (PROVING DATA LOSS):
        domain.RemoteId.Should().Be(dto.Id, "the short ID must be preserved for bug correlation");
        domain.CreatedAt.Should().Be(DateTimeOffset.Parse(dto.CreateTime!), "the session must know when it was born");
    }

    private static void AssertJsonElementsEqual(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.ValueKind, actual.ValueKind);
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                var expectedProps = expected.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var actualProps = actual.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                Assert.True(expectedProps.Count == actualProps.Count, $"Prop count mismatch! Expected [{expectedProps.Count}] Actual [{actualProps.Count}] in {expected.GetRawText()}");
                foreach (var prop in expectedProps)
                {
                    Assert.True(actualProps.ContainsKey(prop.Key), $"Missing property: {prop.Key}");
                    AssertJsonElementsEqual(prop.Value, actualProps[prop.Key]);
                }
                break;
            case JsonValueKind.Array:
                Assert.Equal(expected.GetArrayLength(), actual.GetArrayLength());
                for (int i = 0; i < expected.GetArrayLength(); i++)
                    AssertJsonElementsEqual(expected[i], actual[i]);
                break;
            case JsonValueKind.String:
                Assert.Equal(expected.GetString(), actual.GetString());
                break;
            case JsonValueKind.Number:
                // Handle precision: use GetRawText if numeric comparison is tricky
                Assert.Equal(expected.GetRawText(), actual.GetRawText());
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                break;
        }
    }
}
