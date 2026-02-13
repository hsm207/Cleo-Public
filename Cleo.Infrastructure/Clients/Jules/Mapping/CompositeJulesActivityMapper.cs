using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// A Composite Mapper that encapsulates the strategy selection and fallback logic.
/// This restores SRP to the Client and ensures OCP by hiding the mapper list.
/// </summary>
public sealed class CompositeJulesActivityMapper : IJulesActivityMapper
{
    private readonly IEnumerable<IJulesActivityMapper> _mappers;

    public CompositeJulesActivityMapper(IEnumerable<IJulesActivityMapper> mappers)
    {
        _mappers = mappers;
    }

    public bool CanMap(JulesActivityDto dto)
    {
        // The composite can theoretically map anything via fallback, so this is always true?
        // Or should it delegate?
        // Given it encapsulates fallback, it effectively claims it can map any activity.
        return true;
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public SessionActivity Map(JulesActivityDto dto)
    {
        var mapper = _mappers.FirstOrDefault(m => m.CanMap(dto));
        if (mapper != null)
        {
            return mapper.Map(dto);
        }

        // Fallback Logic: Clothed inside the Composite! 👚
        return new MessageActivity(
            dto.Metadata.Name, // Maps to Domain.Id
            dto.Metadata.Id,   // Maps to Domain.RemoteId
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture),
            ActivityOriginator.System,
            $"Unknown activity type '{dto.Metadata.Name}' received.");
    }
}
