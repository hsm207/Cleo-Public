using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class ApiKeyTests
{
    [Fact(DisplayName = "An ApiKey should store its value correctly when valid.")]
    public void ConstructorShouldSetValueWhenValid()
    {
        var keyValue = "AQ.RealApiKey";
        var apiKey = new ApiKey(keyValue);
        Assert.Equal(keyValue, apiKey.Value);
    }

    [Theory(DisplayName = "An ApiKey should throw an error if the key is empty or null.")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ConstructorShouldThrowWhenInvalid(string? invalidValue)
    {
        Assert.Throws<ArgumentException>(() => new ApiKey(invalidValue!));
    }

    [Fact(DisplayName = "An ApiKey should support explicit conversion from string and implicit conversion to string.")]
    public void ConversionShouldWork()
    {
        var originalValue = "super-secret-token";
        var key = (ApiKey)originalValue;
        string value = key;
        Assert.Equal(originalValue, value);
    }

    [Fact(DisplayName = "An ApiKey should throw ArgumentNullException when implicitly converting a null ApiKey to string.")]
    public void ImplicitStringConversionShouldThrowOnNull()
    {
        ApiKey? key = null;
        Assert.Throws<ArgumentNullException>(() => (string)key!);
    }

    [Fact(DisplayName = "An ApiKey's ToString should return its internal value.")]
    public void ToStringShouldReturnInternalValue()
    {
        var key = new ApiKey("test-key");
        Assert.Equal("test-key", key.ToString());
    }
}
