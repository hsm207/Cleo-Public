using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class TaskDescriptionTests
{
    [Fact(DisplayName = "A TaskDescription should store its value correctly when valid.")]
    public void ConstructorShouldSetValueWhenValid()
    {
        var description = "Fix the login bug";
        var task = new TaskDescription(description);
        Assert.Equal(description, task.Value);
    }

    [Theory(DisplayName = "A TaskDescription should throw an error if the value is empty or null.")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ConstructorShouldThrowWhenInvalid(string? invalidValue)
    {
        Assert.Throws<ArgumentException>(() => new TaskDescription(invalidValue!));
    }

    [Fact(DisplayName = "A TaskDescription should support explicit conversion from string and implicit conversion to string.")]
    public void ConversionShouldWork()
    {
        var originalValue = "Build Feature X";
        var task = (TaskDescription)originalValue; // Explicit cast
        string value = task; // Still implicit string conversion
        Assert.Equal(originalValue, value);
    }

    [Fact(DisplayName = "A TaskDescription can be created using the static FromString factory method.")]
    public void FromStringShouldCreateValidTaskDescription()
    {
        var task = TaskDescription.FromString("Valid Task");
        Assert.Equal("Valid Task", task.Value);
    }

    [Fact(DisplayName = "A TaskDescription's ToString should return its internal value.")]
    public void ToStringShouldReturnInternalValue()
    {
        var task = new TaskDescription("Hello Jules");
        Assert.Equal("Hello Jules", task.ToString());
    }
}
