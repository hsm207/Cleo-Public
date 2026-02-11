using System.Text.Json;
using System.Text.Json.Serialization;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
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

    [Fact(DisplayName = "JulesActivityDto should support a perfect, lossless round-trip (JSON -> DTO -> JSON) for all 188 activities")]
    public void ShouldSupportLosslessJsonRoundTripForEntireHistory()
    {
        // Arrange: Load the "Oracle of Jules" (the raw JSON)
        var path = Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "raw_activities_history.json");
        var jsonContent = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(jsonContent);

        Assert.Equal(188, doc.RootElement.GetArrayLength());

        // Act & Assert: The Sovereign Round-Trip
        foreach (var originalElement in doc.RootElement.EnumerateArray())
        {
            // 1. Get the raw JSON of a single activity (A)
            var originalJson = originalElement.GetRawText();

            // 2. Deserialize into our sectioned DTO
            var dto = JsonSerializer.Deserialize<JulesActivityDto>(originalJson, Options);
            Assert.NotNull(dto);

            // 3. Serialize back to JSON (B)
            var roundTripJson = JsonSerializer.Serialize(dto, Options);

            // 4. Assert A == B (Comparing structural equality)
            using var roundTripDoc = JsonDocument.Parse(roundTripJson);
            AssertJsonElementsEqual(originalElement, roundTripDoc.RootElement);
        }
    }

    private static void AssertJsonElementsEqual(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.ValueKind, actual.ValueKind);

        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                var expectedProps = expected.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var actualProps = actual.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                
                Assert.True(expectedProps.Count == actualProps.Count, $"Property count mismatch! \nExpected [{expectedProps.Count}]: {string.Join(", ", expectedProps.Keys)}\nActual [{actualProps.Count}]: {string.Join(", ", actualProps.Keys)}\nOriginal JSON: {expected.GetRawText()}\nActual JSON: {actual.GetRawText()}");
                
                foreach (var prop in expectedProps)
                {
                    Assert.True(actualProps.ContainsKey(prop.Key), $"Missing property: {prop.Key} in {expected.GetRawText()}");
                    AssertJsonElementsEqual(prop.Value, actualProps[prop.Key]);
                }
                
                // Also check for extra properties in actual that aren't in expected
                foreach (var prop in actualProps)
                {
                    Assert.True(expectedProps.ContainsKey(prop.Key), $"Extra property found in actual: {prop.Key} in {actual.GetRawText()}");
                }
                break;

            case JsonValueKind.Array:
                Assert.Equal(expected.GetArrayLength(), actual.GetArrayLength());
                for (int i = 0; i < expected.GetArrayLength(); i++)
                {
                    AssertJsonElementsEqual(expected[i], actual[i]);
                }
                break;

            case JsonValueKind.String:
                Assert.Equal(expected.GetString(), actual.GetString());
                break;

            case JsonValueKind.Number:
                Assert.Equal(expected.GetDecimal(), actual.GetDecimal());
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                // ValueKind check above handles these
                break;

            default:
                Assert.Equal(expected.GetRawText(), actual.GetRawText());
                break;
        }
    }
}
