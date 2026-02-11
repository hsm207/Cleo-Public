using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// A fallback mapper that handles unrecognized Jules API activities safely.
/// </summary>
internal sealed class UnknownActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => true;

    public SessionActivity Map(JulesActivityDto dto)
    {
        // For unknown activities, we'll just reuse the Id as RemoteId for now,
        // or we could use Name if we had it easily accessible without casting.
        // Actually JulesActivityMetadataDto has both Id and Name.
        return new MessageActivity(
            dto.Metadata.Name,
            dto.Metadata.Id,
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture),
            ActivityOriginator.System,
            $"Unknown activity type received: {dto.Metadata.Description}"
        );
    }
}
