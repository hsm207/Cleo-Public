#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record PlanDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("steps")] PlanStepDto[] Steps,
    [property: JsonPropertyName("createTime")] DateTimeOffset CreateTime
);
