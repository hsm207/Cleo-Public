using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Entities;

/// <summary>
/// Represents the authenticated developer persona.
/// </summary>
public class Identity
{
    public ApiKey ApiKey { get; }
    public IdentityStatus Status { get; private set; }

    public Identity(ApiKey apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ApiKey = apiKey;
        Status = IdentityStatus.Unknown;
    }

    public void UpdateStatus(IdentityStatus newStatus)
    {
        Status = newStatus;
    }
}
