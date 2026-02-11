using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseSources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ReposCommandTests : IDisposable
{
    private readonly Mock<IBrowseSourcesUseCase> _useCaseMock;
    private readonly Mock<ILogger<ReposCommand>> _loggerMock;
    private readonly ReposCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ReposCommandTests()
    {
        _useCaseMock = new Mock<IBrowseSourcesUseCase>();
        _loggerMock = new Mock<ILogger<ReposCommand>>();
        _command = new ReposCommand(_useCaseMock.Object, _loggerMock.Object);

        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given available sources, when running 'repos', then it should list them.")]
    public async Task Repos_WithSources_ListsThem()
    {
        // Arrange
        // SessionSource(Name, Owner, Repo)
        var sources = new List<SessionSource>
        {
            new("my-repo", "owner", "my-repo")
        };

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseSourcesResponse(sources));

        // Act
        var exitCode = await _command.Build().InvokeAsync("repos");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();
        output.Should().Contain("🛰️ Available Repositories:");
        output.Should().Contain("my-repo");
    }

    [Fact(DisplayName = "Given no sources, when running 'repos', then it should display empty message.")]
    public async Task Repos_NoSources_DisplaysEmptyMessage()
    {
        // Arrange
        var sources = new List<SessionSource>();

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseSourcesResponse(sources));

        // Act
        await _command.Build().InvokeAsync("repos");

        // Assert
        _stringWriter.ToString().Should().Contain("📭 No sources found");
    }

    [Fact(DisplayName = "Given an error, when running 'repos', then it should handle exception.")]
    public async Task Repos_Error_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Down"));

        // Act
        await _command.Build().InvokeAsync("repos");

        // Assert
        _stringWriter.ToString().Should().Contain("💔 Error: API Down");
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}
