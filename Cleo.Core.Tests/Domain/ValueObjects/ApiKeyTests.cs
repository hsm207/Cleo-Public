using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class ApiKeyTests
{
    [Theory(DisplayName = "ApiKey should enforce validity invariants (cannot be null or empty).")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldEnforceInvariants(string? invalidValue)
    {
        // 1. Constructor Invariant
        Assert.Throws<ArgumentException>(() => new ApiKey(invalidValue!));
        
        // 2. Implicit Cast Invariant
        ApiKey? nullKey = null;
        Assert.Throws<ArgumentNullException>(() => (string)nullKey!);
    }

    [Fact(DisplayName = "ApiKey should behave as a valid value object (Storage, Conversion, Equality).")]
    public void ShouldBehaveAsValidValue()
    {
        var raw = "AQ.TestKey_123";
        
        // 1. Creation
        var key = new ApiKey(raw);
        
        // 2. Equality (Structural)
        Assert.Equal(raw, key.Value);
        Assert.Equal(key, new ApiKey(raw)); // Value Equality
        
        // 3. Conversion (Explicit & Implicit)
        var castedKey = (ApiKey)raw;
        string backToString = key;
        
        Assert.Equal(key, castedKey);
        Assert.Equal(raw, backToString);
        
        // 4. String Representation
        Assert.Equal(raw, key.ToString());
    }

    [Fact(DisplayName = "ApiKey should trim whitespace.")]
    public void ShouldTrimWhitespace()
    {
        var raw = "  my-secret-key  ";
        var expected = "my-secret-key";
        var key = new ApiKey(raw);
        Assert.Equal(expected, key.Value);
    }
}
