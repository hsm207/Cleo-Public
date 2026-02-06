using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionIdTests
{
    [Fact(DisplayName = "A SessionId should store its value correctly when valid.")]
    public void ConstructorShouldSetValueWhenValid()
    {
        var idValue = "sessions/123";
        var id = new SessionId(idValue);
        Assert.Equal(idValue, id.Value);
    }

    [Theory(DisplayName = "A SessionId should throw an error if the value is empty or null.")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ConstructorShouldThrowWhenInvalid(string? invalidValue)
    {
        Assert.Throws<ArgumentException>(() => new SessionId(invalidValue!));
    }

    [Fact(DisplayName = "A SessionId should support explicit conversion from string and implicit conversion to string.")]
    public void ConversionShouldWork()
    {
        var originalValue = "sessions/abc";
        var id = (SessionId)originalValue;
        string value = id;
        Assert.Equal(originalValue, value);
    }

    [Fact(DisplayName = "A SessionId should throw ArgumentNullException when implicitly converting a null SessionId to string.")]
    public void ImplicitStringConversionShouldThrowOnNull()
    {
        SessionId? id = null;
        Assert.Throws<ArgumentNullException>(() => (string)id!);
    }

    [Fact(DisplayName = "A SessionId can be created using the static FromString factory method.")]
    public void FromStringShouldCreateValidSessionId()
    {
        var id = SessionId.FromString("sessions/xyz");
        Assert.Equal("sessions/xyz", id.Value);
    }

    [Fact(DisplayName = "A SessionId's ToString should return its internal value.")]
    public void ToStringShouldReturnInternalValue()
    {
        var id = new SessionId("sessions/test");
        Assert.Equal("sessions/test", id.ToString());
    }
}
