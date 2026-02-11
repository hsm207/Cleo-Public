using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Requests;

/// <summary>
/// Data transfer object for sending a message to an active session.
/// </summary>
internal sealed record JulesSendMessageRequestDto(
    [property: JsonPropertyName("prompt")] string Prompt
);
