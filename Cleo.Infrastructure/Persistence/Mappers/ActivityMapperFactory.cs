using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

/// <summary>
/// A factory for orchestrating the polymorphic mapping of session activities.
/// Following the Plugin Strategy to ensure OCP compliance.
/// </summary>
internal sealed class ActivityMapperFactory
{
    private readonly IEnumerable<IActivityPersistenceMapper> _mappers;

    public ActivityMapperFactory(IEnumerable<IActivityPersistenceMapper> mappers)
    {
        _mappers = mappers;
    }

    public ActivityEnvelopeDto ToEnvelope(SessionActivity activity)
    {
        var mapper = _mappers.FirstOrDefault(m => m.CanHandle(activity))
            ?? throw new InvalidOperationException($"No persistence mapper found for activity type: {activity.GetType().Name}");

        return new ActivityEnvelopeDto
        {
            Type = mapper.TypeKey,
            Id = activity.Id,
            Timestamp = activity.Timestamp,
            Originator = activity.Originator.ToString(),
            PayloadJson = mapper.SerializePayload(activity)
        };
    }

    public SessionActivity FromEnvelope(ActivityEnvelopeDto envelope)
    {
        var mapper = _mappers.FirstOrDefault(m => m.TypeKey == envelope.Type)
            ?? throw new InvalidOperationException($"No persistence mapper found for stored type: {envelope.Type}");

        if (!Enum.TryParse<ActivityOriginator>(envelope.Originator, out var originator))
        {
            originator = ActivityOriginator.System;
        }

        return mapper.DeserializePayload(envelope.Id, envelope.Timestamp, originator, envelope.PayloadJson);
    }
}
