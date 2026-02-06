using Cleo.Core;
using Xunit;

namespace Cleo.Core.Tests;

public class CalculatorTests
{
    [Fact]
    public void AddShouldReturnSumOfTwoNumbers()
    {
        // Act
        var result = Calculator.Add(2, 2);
        
        // Assert
        Assert.Equal(4, result);
    }
}
