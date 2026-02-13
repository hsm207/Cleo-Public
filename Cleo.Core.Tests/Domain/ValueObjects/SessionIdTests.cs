using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionIdTests
{
    [Theory(DisplayName = "SessionId should enforce validity invariants (cannot be null or empty).")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldEnforceInvariants(string? invalidValue)
    {
        Assert.Throws<ArgumentException>(() => new SessionId(invalidValue!));
        
        SessionId? nullId = null;
        Assert.Throws<ArgumentNullException>(() => (string)nullId!);
    }

    [Fact(DisplayName = "SessionId should behave as a valid value object (Factory, Conversion, Equality).")]
    public void ShouldBehaveAsValidValue()
    {
        var raw = "sessions/123-abc";
        
        // 1. Creation
        var id = new SessionId(raw);
        var fromFactory = SessionId.FromString(raw);
        
        // 2. Equality
        Assert.Equal(raw, id.Value);
        Assert.Equal(id, fromFactory);
        
        // 3. Conversion
        var casted = (SessionId)raw;
        string backToString = id;
        
        Assert.Equal(id, casted);
        Assert.Equal(raw, backToString);
        
        // 4. String Representation
        Assert.Equal(raw, id.ToString());
    }
}
