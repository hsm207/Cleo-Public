using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// A structured, sectioned DTO for Jules activities.
/// Acts as an Anti-Corruption Layer (ACL) by grouping flat API fields into Metadata and Payload.
/// </summary>
[JsonConverter(typeof(JulesActivityConverter))]
public sealed record JulesActivityDto(
    JulesActivityMetadata Metadata,
    JulesActivityPayload Payload
)
{
    // Preserves unknown properties for perfect round-trip fidelity 🛡️💎
    [JsonIgnore]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Custom converter to map flat Jules API activity JSON into the structured JulesActivityDto.
/// </summary>
internal sealed class JulesActivityConverter : JsonConverter<JulesActivityDto>
{
    public override JulesActivityDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use a temporary flat DTO to deserialize the raw JSON
        var flat = JsonSerializer.Deserialize<FlatActivityDto>(ref reader, options);
        if (flat == null) return null;

        var metadata = new JulesActivityMetadata(
            flat.Id,
            flat.Name,
            flat.Description,
            flat.CreateTime,
            flat.Originator,
            flat.Artifacts);

        var payload = new JulesActivityPayload(
            flat.ProgressUpdated,
            flat.PlanGenerated,
            flat.PlanApproved,
            flat.UserMessaged,
            flat.AgentMessaged,
            flat.SessionCompleted,
            flat.SessionFailed,
            flat.CodeChanges,
            flat.BashOutput,
            flat.Media);

        return new JulesActivityDto(metadata, payload) { ExtensionData = flat.ExtensionData };
    }

    public override void Write(Utf8JsonWriter writer, JulesActivityDto value, JsonSerializerOptions options)
    {
        // We don't typically need to write these back to the API, but for completeness:
        var flat = new FlatActivityDto(
            value.Metadata.Name,
            value.Metadata.Id,
            value.Metadata.Description,
            value.Metadata.CreateTime,
            value.Metadata.Originator,
            value.Metadata.Artifacts?.ToList(),
            value.Payload.PlanGenerated,
            value.Payload.PlanApproved,
            value.Payload.UserMessaged,
            value.Payload.AgentMessaged,
            value.Payload.ProgressUpdated,
            value.Payload.SessionCompleted,
            value.Payload.SessionFailed,
            value.Payload.CodeChanges,
            value.Payload.BashOutput,
            value.Payload.Media)
        {
            ExtensionData = value.ExtensionData
        };

        JsonSerializer.Serialize(writer, flat, options);
    }

    // Internal flat representation to match the wire format
    private sealed record FlatActivityDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("createTime")] string CreateTime,
        [property: JsonPropertyName("originator")] string Originator,
        [property: JsonPropertyName("artifacts")] List<ArtifactDto>? Artifacts,
        [property: JsonPropertyName("planGenerated")] PlanGeneratedDto? PlanGenerated,
        [property: JsonPropertyName("planApproved")] PlanApprovedDto? PlanApproved,
        [property: JsonPropertyName("userMessaged")] UserMessagedDto? UserMessaged,
        [property: JsonPropertyName("agentMessaged")] AgentMessagedDto? AgentMessaged,
        [property: JsonPropertyName("progressUpdated")] ProgressUpdatedDto? ProgressUpdated,
        [property: JsonPropertyName("sessionCompleted")] SessionCompletedDto? SessionCompleted,
        [property: JsonPropertyName("sessionFailed")] SessionFailedDto? SessionFailed,
        [property: JsonPropertyName("codeChanges")] ChangeSetDto? CodeChanges,
        [property: JsonPropertyName("bashOutput")] BashOutputDto? BashOutput,
        [property: JsonPropertyName("media")] MediaDto? Media
    )
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; init; }
    }
}
