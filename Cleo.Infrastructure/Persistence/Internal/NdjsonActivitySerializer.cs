using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Mappers;
using System.Text.Json;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class NdjsonActivitySerializer
{
    private readonly ActivityMapperFactory _mapperFactory;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    public NdjsonActivitySerializer(ActivityMapperFactory mapperFactory)
    {
        _mapperFactory = mapperFactory;
    }

    public string Serialize(SessionActivity activity)
    {
        var dto = _mapperFactory.ToEnvelope(activity);
        return JsonSerializer.Serialize(dto, Options);
    }

    public SessionActivity? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var dto = JsonSerializer.Deserialize<ActivityEnvelopeDto>(json, Options);
            return dto != null ? _mapperFactory.FromEnvelope(dto) : null;
        }
        catch (JsonException)
        {
            // Corrupt line? Skip.
            return null;
        }
    }
}
