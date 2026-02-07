using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryTaskRepositoryTests : IDisposable
{
    private readonly string _tempFile;
    private readonly RegistryTaskRepository _sut;

    public RegistryTaskRepositoryTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), "CleoRegistry_" + Guid.NewGuid().ToString("N") + ".json");
        _sut = new RegistryTaskRepository(_tempFile);
    }

    [Fact(DisplayName = "Registry should save and retrieve multiple tasks across different projects.")]
    public async Task Save_ShouldPersistMultipleTasks()
    {
        // Arrange
        var task1 = CreateSession("sessions/1", "Task 1", "repo/A");
        var task2 = CreateSession("sessions/2", "Task 2", "repo/B");
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.SaveAsync(task1, ct);
        await _sut.SaveAsync(task2, ct);
        
        var retrieved1 = await _sut.GetByIdAsync(task1.Id, ct);
        var retrieved2 = await _sut.GetByIdAsync(task2.Id, ct);
        var list = await _sut.ListAsync(ct);

        // Assert
        retrieved1.Should().NotBeNull();
        retrieved1!.Id.Should().Be(task1.Id);
        
        retrieved2.Should().NotBeNull();
        retrieved2!.Id.Should().Be(task2.Id);

        list.Should().HaveCount(2);
        list.Should().Contain(s => s.Id == task1.Id);
        list.Should().Contain(s => s.Id == task2.Id);
    }

    [Fact(DisplayName = "Registry should update existing tasks if the SessionId matches.")]
    public async Task Save_ShouldUpdateExistingTask()
    {
        // Arrange
        var id = new SessionId("sessions/1");
        var initial = CreateSession(id.Value, "Initial", "repo");
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(initial, ct);

        var updated = new Session(id, (TaskDescription)"Updated", initial.Source, new SessionPulse(SessionStatus.Completed));

        // Act
        await _sut.SaveAsync(updated, ct);
        var result = await _sut.GetByIdAsync(id, ct);

        // Assert
        result.Should().NotBeNull();
        result!.Task.Should().Be((TaskDescription)"Updated");
        result.Pulse.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact(DisplayName = "Registry should return null/empty when no registry file exists.")]
    public async Task ShouldHandleEmptyRegistry()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await _sut.GetByIdAsync(new SessionId("any"), ct);
        var list = await _sut.ListAsync(ct);

        result.Should().BeNull();
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Registry should handle empty or malformed files gracefully.")]
    public async Task ShouldHandleMalformedFile()
    {
        var ct = TestContext.Current.CancellationToken;
        File.WriteAllText(_tempFile, " ");
        
        var list = await _sut.ListAsync(ct);
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Registry should delete specific tasks.")]
    public async Task Delete_ShouldRemoveTask()
    {
        // Arrange
        var session = CreateSession("sessions/1", "Task", "repo");
        var ct = TestContext.Current.CancellationToken;
        await _sut.SaveAsync(session, ct);

        // Act
        await _sut.DeleteAsync(session.Id, ct);
        var list = await _sut.ListAsync(ct);

        // Assert
        list.Should().BeEmpty();
    }

    [Fact(DisplayName = "Registry should handle empty registry files.")]
    public async Task LoadRegistry_ShouldHandleEmptyFile()
    {
        var ct = TestContext.Current.CancellationToken;
        File.WriteAllText(_tempFile, "");
        
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

    [Fact(DisplayName = "Registry should handle existing directories gracefully.")]
    public async Task Save_ShouldWorkWhenDirectoryExists()
    {
        var session = CreateSession("sessions/1", "Task", "repo");
        var ct = TestContext.Current.CancellationToken;
        
        await _sut.SaveAsync(session, ct);
        await _sut.SaveAsync(session, ct);

        var list = await _sut.ListAsync(ct);
        list.Should().ContainSingle();
    }

    [Fact(DisplayName = "Registry should handle paths without directory components.")]
    public async Task Save_ShouldWorkWithBareFilename()
    {
        var sut = new RegistryTaskRepository("bare_file.json");
        var session = CreateSession("sessions/1", "T", "r");
        
        await sut.SaveAsync(session, TestContext.Current.CancellationToken);
        File.Exists("bare_file.json").Should().BeTrue();
        File.Delete("bare_file.json");
    }

    [Fact(DisplayName = "JulesMapper should validate statusMapper argument.")]
    public void Map_ShouldThrowOnNullStatusMapper()
    {
        var dto = new JulesSessionDto("n", "i", "s", "p", new SourceContextDto("r", new GithubRepoContextDto("m")));
        Action act = () => JulesMapper.Map(dto, (TaskDescription)"t", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Registry should validate arguments and constructor.")]
    public async Task Methods_ShouldThrowOnNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetByIdAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAsync(null!, ct));
        Assert.Throws<ArgumentNullException>(() => new RegistryTaskRepository(null!));
    }

    [Fact(DisplayName = "Registry should use default path when created via parameterless constructor.")]
    public void DefaultConstructor_ShouldNotThrow()
    {
        var sut = new RegistryTaskRepository();
        sut.Should().NotBeNull();
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
