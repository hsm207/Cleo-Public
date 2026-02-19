using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseSources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class ReposCommandTests
{
    private readonly Mock<IBrowseSourcesUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly ReposCommand _command;

    public ReposCommandTests()
    {
        _useCaseMock = new Mock<IBrowseSourcesUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new ReposCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<ReposCommand>>().Object);
    }

    [Fact(DisplayName = "Repos should call UseCase and PresentRepositories.")]
    public async Task Repos_Valid_PresentsRepos()
    {
        // Arrange
        var sources = new[] { new SessionSource("repo1", "owner", "repo1") };
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseSourcesResponse(sources));

        // Act
        await _command.Build().InvokeAsync("repos");

        // Assert
        _presenterMock.Verify(x => x.PresentRepositories(It.Is<IEnumerable<string>>(l => l.Contains("repo1"))), Times.Once);
    }

    [Fact(DisplayName = "Repos should call PresentEmptyRepositories when none found.")]
    public async Task Repos_Empty_PresentsEmpty()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseSourcesResponse(Array.Empty<SessionSource>()));

        // Act
        await _command.Build().InvokeAsync("repos");

        // Assert
        _presenterMock.Verify(x => x.PresentEmptyRepositories(), Times.Once);
    }

    [Fact(DisplayName = "Repos should call PresentError on exception.")]
    public async Task Repos_Error_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseSourcesRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync("repos");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}
