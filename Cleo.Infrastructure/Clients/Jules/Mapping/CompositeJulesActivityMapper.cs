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
    private readonly Dictionary<Type, IJulesActivityMapper> _mapperMap;

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public CompositeJulesActivityMapper(IEnumerable<IJulesActivityMapper> mappers)
    {
        _mapperMap = new Dictionary<Type, IJulesActivityMapper>();

        foreach (var mapper in mappers)
        {
            // Inspect the interface to find the TPayload type(s)
            var interfaceTypes = mapper.GetType().GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJulesActivityMapper<>));

            foreach (var interfaceType in interfaceTypes)
            {
                var payloadType = interfaceType.GetGenericArguments()[0];
                _mapperMap[payloadType] = mapper;
            }
        }
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (_mapperMap.TryGetValue(dto.Payload.GetType(), out var mapper))
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
