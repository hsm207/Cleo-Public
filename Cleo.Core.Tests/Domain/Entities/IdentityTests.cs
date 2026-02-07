using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class IdentityTests
{
    private readonly ApiKey _testKey = new("my-key");

    [Fact(DisplayName = "An Identity should be initialized with a valid ApiKey and an 'Unknown' status.")]
    public void ConstructorShouldInitializeCorrectly()
    {
        var identity = new Identity(_testKey);

        Assert.Equal(_testKey, identity.ApiKey);
        Assert.Equal(IdentityStatus.Unknown, identity.Status);
    }

    [Fact(DisplayName = "An Identity should throw ArgumentNullException if the ApiKey is missing.")]
    public void ConstructorShouldEnforceTransactionalIntegrity()
    {
        Assert.Throws<ArgumentNullException>(() => new Identity(null!));
    }

    [Fact(DisplayName = "An Identity's status should be updatable to reflect validation results.")]
    public void UpdateStatusShouldUpdateStatus()
    {
        var identity = new Identity(_testKey);
        
        identity.UpdateStatus(IdentityStatus.Valid);
        Assert.Equal(IdentityStatus.Valid, identity.Status);

        identity.UpdateStatus(IdentityStatus.Invalid);
        Assert.Equal(IdentityStatus.Invalid, identity.Status);
    }
}
