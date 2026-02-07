using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryTaskRepositoryTests : IDisposable
{
    private readonly string _tempFile;
    private readonly RegistryTaskRepository _sut;
    private readonly Mock<IRegistryPathProvider> _mockPath = new();

    public RegistryTaskRepositoryTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), "CleoRegistry_" + Guid.NewGuid().ToString("N") + ".json");
        _mockPath.Setup(p => p.GetRegistryPath()).Returns(_tempFile);
        
        _sut = new RegistryTaskRepository(_mockPath.Object, new RegistryTaskMapper(), new JsonRegistrySerializer());
    }

    [Fact(DisplayName = "Registry should save and retrieve multiple tasks.")]
    public async Task Save_ShouldPersistMultipleTasks()
    {
        var task1 = CreateSession("sessions/1", "Task 1", "repo/A");
        var task2 = CreateSession("sessions/2", "Task 2", "repo/B");
        var ct = TestContext.Current.CancellationToken;

        await _sut.SaveAsync(task1, ct);
        await _sut.SaveAsync(task2, ct);
        
        var list = await _sut.ListAsync(ct);

        list.Should().HaveCount(2);
        list.Should().Contain(s => s.Id == task1.Id);
        list.Should().Contain(s => s.Id == task2.Id);
    }

    [Fact(DisplayName = "Registry should update existing tasks.")]
    public async Task Save_ShouldUpdateExistingTask()
    {
        var id = new SessionId("sessions/1");
        var initial = CreateSession(id.Value, "Initial", "repo");
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(initial, ct);

        var updated = new Session(id, (TaskDescription)"Updated", initial.Source, new SessionPulse(SessionStatus.Completed));

        await _sut.SaveAsync(updated, ct);
        var result = await _sut.GetByIdAsync(id, ct);

        result!.Task.Should().Be((TaskDescription)"Updated");
    }

    [Fact(DisplayName = "Registry should handle empty registry files.")]
    public async Task LoadRegistry_ShouldHandleEmptyFile()
    {
        var ct = TestContext.Current.CancellationToken;
        File.WriteAllText(_tempFile, " ");
        
        var list = await _sut.ListAsync(ct);
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Registry should handle 'null' json content.")]
    public async Task LoadRegistry_ShouldHandleNullContent()
    {
        var ct = TestContext.Current.CancellationToken;
        File.WriteAllText(_tempFile, "null");
        
        var list = await _sut.ListAsync(ct);
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Registry should handle bare filenames.")]
    public async Task Save_ShouldHandleBareFilenames()
    {
        var barePath = "bare_file.json";
        var mockBare = new Mock<IRegistryPathProvider>();
        mockBare.Setup(p => p.GetRegistryPath()).Returns(barePath);
        
        var sut = new RegistryTaskRepository(mockBare.Object, new RegistryTaskMapper(), new JsonRegistrySerializer());
        await sut.SaveAsync(CreateSession("sessions/1", "T", "r"), TestContext.Current.CancellationToken);
        
        File.Exists(barePath).Should().BeTrue();
        File.Delete(barePath);
    }

    [Fact(DisplayName = "Registry should validate arguments and constructor.")]
    public async Task Methods_ShouldThrowOnNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetByIdAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAsync(null!, ct));
        
        Assert.Throws<ArgumentNullException>(() => new RegistryTaskRepository(null!, new RegistryTaskMapper(), new JsonRegistrySerializer()));
        Assert.Throws<ArgumentNullException>(() => new RegistryTaskRepository(_mockPath.Object, null!, new JsonRegistrySerializer()));
        Assert.Throws<ArgumentNullException>(() => new RegistryTaskRepository(_mockPath.Object, new RegistryTaskMapper(), null!));
    }

    [Fact(DisplayName = "Registry should provide a parameterless constructor.")]
    public void DefaultConstructor_ShouldNotThrow()
    {
        var sut = new RegistryTaskRepository();
        sut.Should().NotBeNull();
    }

    [Fact(DisplayName = "Registry should handle missing context file.")]
    public async Task ShouldHandleMissingFile()
    {
        var ct = TestContext.Current.CancellationToken;
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
        
        var result = await _sut.GetByIdAsync(new SessionId("any"), ct);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Registry should delete specific tasks.")]
    public async Task Delete_ShouldRemoveTask()
    {
        var session = CreateSession("sessions/1", "Task", "repo");
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(session, ct);

        await _sut.DeleteAsync(session.Id, ct);
        var list = await _sut.ListAsync(ct);

        list.Should().BeEmpty();
    }

    private static Session CreateSession(string id, string task, string repo)
    {
        return new Session(
            new SessionId(id),
            (TaskDescription)task,
            new SourceContext(repo, "main"),
            new SessionPulse(SessionStatus.InProgress, "Working...")
        );
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }
}
