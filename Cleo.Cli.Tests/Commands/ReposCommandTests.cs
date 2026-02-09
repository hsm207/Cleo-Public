using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseSources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

// Disable parallelization to avoid Console output capture issues
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
    }

    [Fact(DisplayName = "Given available sources, when running 'repos', then it should list them.")]
    public async Task Repos_WithSources_ListsThem()
    {
        // Arrange
        var sources = new List<SessionSource>
        {
            new("GitHub", "owner", "cleo-cli")
        };

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseSourcesResponse(sources));

        var command = _command.Build();

        // Act
        // SetHandler might be asynchronous and System.CommandLine execution might not await it properly in unit test context if not careful?
        // But await InvokeAsync should wait.
        // Let's verify the mock call first.
        var exitCode = await command.InvokeAsync("");

        _useCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()), Times.Once);

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        // If output is empty, maybe Console.WriteLine isn't being captured because System.CommandLine is using a different console abstraction?
        // Wait, ReposCommand uses Console.WriteLine.
        // But PlanCommandTests worked and it uses Console.WriteLine too.
        // Maybe inconsistent state in the test runner due to static Console usage?

        // Let's retry asserting on output.
        output.Should().Contain("🛰️ Available Repositories:");
        output.Should().Contain("GitHub"); // Check for "GitHub" which is the Source Name in SessionSource(Name, Owner, Repo)
    }
}
