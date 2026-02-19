using Cleo.Cli.Services;
using FluentAssertions;
using Xunit;

namespace Cleo.Cli.Tests.Services;

public sealed class HelpProviderTests
{
    private readonly HelpProvider _sut = new();

    [Fact(DisplayName = "GetCommandDescription should return the string from resources.")]
    public void GetCommandDescription_ReturnsResource()
    {
        // Act
        // Assuming "New_Description" exists in CliStrings.resx
        var result = _sut.GetCommandDescription("New_Description");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotStartWith("[Missing:");
    }

    [Fact(DisplayName = "GetCommandDescription should return [Missing: key] if key is not found.")]
    public void GetCommandDescription_MissingKey_ReturnsFallback()
    {
        // Act
        var result = _sut.GetCommandDescription("NonExistentKey");

        // Assert
        result.Should().Be("[Missing: NonExistentKey]");
    }

    [Fact(DisplayName = "GetResource should return the string from resources.")]
    public void GetResource_ReturnsResource()
    {
        // Act
        // Assuming "New_Success" exists in CliStrings.resx
        var result = _sut.GetResource("New_Success");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotStartWith("[Missing:");
    }

    [Fact(DisplayName = "GetResource should return [Missing: key] if key is not found.")]
    public void GetResource_MissingKey_ReturnsFallback()
    {
        // Act
        var result = _sut.GetResource("NonExistentKey");

        // Assert
        result.Should().Be("[Missing: NonExistentKey]");
    }
}
