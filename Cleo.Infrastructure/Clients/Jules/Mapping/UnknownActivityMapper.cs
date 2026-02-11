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
        return new MessageActivity(dto.Metadata.Id, DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), ActivityOriginator.System, $"Unknown activity type received: {dto.Metadata.Description}");
    }
}
