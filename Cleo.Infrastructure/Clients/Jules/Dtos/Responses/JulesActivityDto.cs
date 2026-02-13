using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// A structured, sectioned DTO for Jules activities.
/// Acts as an Anti-Corruption Layer (ACL) by grouping flat API fields into Metadata and a Polymorphic Payload.
/// </summary>
[JsonConverter(typeof(JulesActivityConverter))]
public sealed record JulesActivityDto(
    JulesActivityMetadataDto Metadata,
    JulesActivityPayloadDto Payload
);

/// <summary>
/// Elegant .NET 10 converter that maps flat Jules API activity JSON into structured sections.
/// Leverages JsonNode for dynamic property sniffing and strictly-typed polymorphic payloads.
/// </summary>
internal sealed class JulesActivityConverter : JsonConverter<JulesActivityDto>
{
    private static readonly Dictionary<string, Type> PayloadTypeMap = new()
    {
        ["progressUpdated"] = typeof(JulesProgressUpdatedPayloadDto),
        ["planGenerated"] = typeof(JulesPlanGeneratedPayloadDto),
        ["planApproved"] = typeof(JulesPlanApprovedPayloadDto),
        ["userMessaged"] = typeof(JulesUserMessagedPayloadDto),
        ["agentMessaged"] = typeof(JulesAgentMessagedPayloadDto),
        ["sessionCompleted"] = typeof(JulesSessionCompletedPayloadDto),
        ["sessionFailed"] = typeof(JulesSessionFailedPayloadDto),
        ["codeChanges"] = typeof(JulesCodeChangesPayloadDto),
        ["bashOutput"] = typeof(JulesBashOutputPayloadDto),
        ["media"] = typeof(JulesMediaPayloadDto)
    };

    public override JulesActivityDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 1. Parse the entire object as a JsonNode for dynamic sniffing 🕵️‍♀️
        var node = JsonNode.Parse(ref reader)?.AsObject();
        if (node == null) return null;

        // 2. Deserialize the common Metadata (Envelope) fields 🧱
        var metadata = node.Deserialize<JulesActivityMetadataDto>(options);
        if (metadata == null) return null;

        // 3. Detect which Payload is present based on property keys 👃💎
        JulesActivityPayloadDto? payload = null;
        foreach (var entry in PayloadTypeMap)
        {
            if (node.ContainsKey(entry.Key))
            {
                payload = node[entry.Key].Deserialize(entry.Value, options) as JulesActivityPayloadDto;
                break;
            }
        }

        // 4. Fallback to a generic payload if none found (keeps things robust) 🛡️
        payload ??= new JulesUnknownPayloadDto("Unknown", node.ToJsonString());

        return new JulesActivityDto(metadata, payload);
    }

    public override void Write(Utf8JsonWriter writer, JulesActivityDto value, JsonSerializerOptions options)
    {
        // 1. Start with the Metadata fields 🧱
        var node = JsonObject.Create(JsonSerializer.SerializeToElement(value.Metadata, options))!;

        // 2. Find the correct key for the polymorphic payload 🎨
        var payloadKey = PayloadTypeMap.FirstOrDefault(x => x.Value == value.Payload.GetType()).Key;
        if (payloadKey != null)
        {
            // 3. Serialize the payload and attach it to the root node under its API key
            node[payloadKey] = JsonSerializer.SerializeToNode(value.Payload, value.Payload.GetType(), options);
        }

        // 4. Write the final flattened structure back to the wire 📡
        node.WriteTo(writer, options);
    }
}
