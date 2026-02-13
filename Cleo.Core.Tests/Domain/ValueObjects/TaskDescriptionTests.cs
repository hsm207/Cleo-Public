using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class TaskDescriptionTests
{
    [Theory(DisplayName = "TaskDescription should enforce validity invariants (cannot be null or empty).")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldEnforceInvariants(string? invalidValue)
    {
        // 1. Constructor Invariant
        Assert.Throws<ArgumentException>(() => new TaskDescription(invalidValue!));
        
        // 2. Implicit Cast Invariant
        TaskDescription? nullTask = null;
        Assert.Throws<ArgumentNullException>(() => (string)nullTask!);
    }

    [Fact(DisplayName = "TaskDescription should behave as a valid value object (Factory, Conversion, Equality).")]
    public void ShouldBehaveAsValidValue()
    {
        var raw = "Refactor the monolith";
        
        // 1. Creation (Constructor & Factory)
        var task = new TaskDescription(raw);
        var fromFactory = TaskDescription.FromString(raw);
        
        // 2. Equality
        Assert.Equal(raw, task.Value);
        Assert.Equal(task, fromFactory); // Value Equality
        
        // 3. Conversion
        var casted = (TaskDescription)raw;
        string backToString = task;
        
        Assert.Equal(task, casted);
        Assert.Equal(raw, backToString);
        
        // 4. String Representation
        Assert.Equal(raw, task.ToString());
    }
}
