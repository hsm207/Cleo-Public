using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class FileContextSessionRepositoryTests : IDisposable
{
    private readonly string _tempPath;
    private readonly FileContextSessionRepository _sut;

    public FileContextSessionRepositoryTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "CleoTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);
        _sut = new FileContextSessionRepository(_tempPath);
    }

    [Fact(DisplayName = "Repository should save and retrieve session project context.")]
    public async Task Save_ShouldPersistToDisk()
    {
        // Arrange
        var session = CreateCuteSession();
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.SaveAsync(session, ct);
        var retrieved = await _sut.GetByIdAsync(session.Id, ct);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(session.Id);
        retrieved.Task.Should().Be(session.Task);
        retrieved.Source.Repository.Should().Be(session.Source.Repository);
        retrieved.Pulse.Status.Should().Be(session.Pulse.Status);
    }

    [Fact(DisplayName = "Repository should list the active session if context exists.")]
    public async Task ListAsync_ShouldReturnActiveSession()
    {
        // Arrange
        var session = CreateCuteSession();
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(session, ct);

        // Act
        var list = await _sut.ListAsync(ct);

        // Assert
        list.Should().ContainSingle();
        list.First().Id.Should().Be(session.Id);
    }

    [Fact(DisplayName = "Repository should return null/empty when no context exists.")]
    public async Task ShouldHandleMissingContext()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _sut.GetByIdAsync(new SessionId("any"), ct);
        var list = await _sut.ListAsync(ct);

        result.Should().BeNull();
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Repository should handle ID mismatches gracefully.")]
    public async Task GetById_ShouldReturnNull_WhenIdDoesNotMatch()
    {
        // Arrange
        var session = CreateCuteSession();
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(session, ct);

        // Act
        var result = await _sut.GetByIdAsync(new SessionId("sessions/wrong-id"), ct);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Repository should delete context file.")]
    public async Task Delete_ShouldRemoveFile()
    {
        // Arrange
        var session = CreateCuteSession();
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(session, ct);

        // Act
        await _sut.DeleteAsync(session.Id, ct);
        var retrieved = await _sut.GetByIdAsync(session.Id, ct);

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact(DisplayName = "Repository should validate arguments.")]
    public async Task Methods_ShouldThrowOnNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetByIdAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAsync(null!, ct));
        Assert.Throws<ArgumentNullException>(() => new FileContextSessionRepository(null!));
    }

    private static Session CreateCuteSession()
    {
        return new Session(
            new SessionId("sessions/cookie-session-777"),
            (TaskDescription)"Integrate the rainbow sprinkle dispenser. 🌈🍪",
            new SourceContext("sources/github/cleo-lover/cookie-bakery", "main"),
            new SessionPulse(SessionStatus.InProgress, "Mixing the batter...")
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }
}
