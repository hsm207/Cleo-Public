using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public class HighFidelityArchaeologyTests
{
    private readonly IServiceProvider _serviceProvider;

    public HighFidelityArchaeologyTests()
    {
        var services = new ServiceCollection();

        // Register High-Fidelity Persistence Plugins (South Boundary) 🔌💎
        services.AddSingleton<ArtifactMapperFactory>();
        services.AddSingleton<IArtifactPersistenceMapper, BashOutputMapper>();
        services.AddSingleton<IArtifactPersistenceMapper, ChangeSetMapper>();
        services.AddSingleton<IArtifactPersistenceMapper, MediaMapper>();

        services.AddSingleton<ActivityMapperFactory>();
        services.AddSingleton<IActivityPersistenceMapper, PlanningActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, MessageActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, ApprovalActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, ProgressActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, CompletionActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, FailureActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, SessionAssignedActivityMapper>();

        services.AddSingleton<IRegistryTaskMapper, RegistryTaskMapper>();
        services.AddSingleton<IRegistrySerializer, JsonRegistrySerializer>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact(DisplayName = "PlanningActivity preserves all nested steps during round-trip 🏺📜")]
    public void PlanningActivity_PreservesSteps_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var originalSteps = new List<PlanStep>
        {
            new("step-1", 0, "Step 1", "Description 1"),
            new("step-2", 1, "Step 2", "Description 2")
        };
        // Raw Truth: Use TestFactory.CreatePlanId (which now returns bare "plan-456")
        var original = new PlanningActivity("act-123", "remote-123", DateTimeOffset.UtcNow, ActivityOriginator.Agent, TestFactory.CreatePlanId("plan-456"), originalSteps);

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (PlanningActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.PlanId.Value.Should().Be("plan-456");
        hydrated.Steps.Should().BeEquivalentTo(originalSteps);
    }

    [Fact(DisplayName = "Deep History: Activities preserve attached hierarchical artifacts 📎💎")]
    public void ProgressActivity_PreservesEvidence_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var evidence = new List<Artifact>
        {
            new BashOutput("ls -la", "total 0", 0),
            new ChangeSet("repo", new GitPatch("diff", "base", "msg")),
            new MediaArtifact("image/png", "base64-data")
        };
        var original = new ProgressActivity("act-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Testing with evidence", null, evidence);

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (ProgressActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.Evidence.Should().HaveCount(3);
        hydrated.Evidence.OfType<BashOutput>().First().Command.Should().Be("ls -la");
        hydrated.Evidence.OfType<ChangeSet>().First().Patch.UniDiff.Should().Be("diff");
        hydrated.Evidence.OfType<MediaArtifact>().First().MimeType.Should().Be("image/png");
    }

    [Fact(DisplayName = "Persistence is decoupled from C# class names via stable discriminators 🧱🛡️")]
    public void Persistence_UsesStableDiscriminators_RegardlessOfClassNames()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var activity = new PlanningActivity("act-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, TestFactory.CreatePlanId("p1"), new List<PlanStep>());

        // Act
        var envelope = factory.ToEnvelope(activity);

        // Assert
        envelope.Type.Should().Be("PLAN_GENERATED");
    }

    [Fact(DisplayName = "Truth-Sensing: Logical SessionState Override identifies blocked sessions 🧠⚖️")]
    public void Session_EvaluatesSessionStateLogically_WhenIdleButBlockedOnPlan()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var task = (TaskDescription)"Fix bug";
        var source = TestFactory.CreateSourceContext("repo");

        // A session that is physically IDLE (Completed) but has a Plan and NO PR
        var pulse = new SessionPulse(SessionStatus.Completed);
        var session = new Session(sessionId, "remote-123", task, source, pulse, DateTimeOffset.UtcNow);

        session.AddActivity(new PlanningActivity("act-plan", "remote-plan", DateTimeOffset.UtcNow, ActivityOriginator.Agent, TestFactory.CreatePlanId("plan-1"), new List<PlanStep> { new("s1", 0, "Do it", "Now") }));

        // Act & Assert
        session.Pulse.Status.Should().Be(SessionStatus.Completed);
        session.State.Should().Be(SessionState.AwaitingPlanApproval); // Logical Override! 🧠🔥
    }

    [Fact(DisplayName = "Round-Trip: CompletionActivity preserves success signal 🏁")]
    public void CompletionActivity_PreservesSuccessSignal_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var evidence = new List<Artifact> { new BashOutput("echo done", "done", 0) };
        var original = new CompletionActivity("comp-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, evidence);

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (CompletionActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.Evidence.Should().HaveCount(1);
        hydrated.Evidence.First().Should().BeOfType<BashOutput>();
    }

    [Fact(DisplayName = "Round-Trip: FailureActivity preserves reason and context 💥")]
    public void FailureActivity_PreservesReason_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var original = new FailureActivity("fail-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Critical Error 500");

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (FailureActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.Reason.Should().Be("Critical Error 500");
    }

    [Fact(DisplayName = "Round-Trip: MessageActivity preserves dialogue 💬")]
    public void MessageActivity_PreservesDialogue_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var original = new MessageActivity("msg-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.User, "Hello Cleo!");

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (MessageActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.Text.Should().Be("Hello Cleo!");
    }

    [Fact(DisplayName = "Round-Trip: ApprovalActivity preserves plan reference ✅")]
    public void ApprovalActivity_PreservesPlanReference_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        // Raw Truth: Use TestFactory.CreatePlanId (which now returns bare "plan-123")
        var original = new ApprovalActivity("app-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.User, TestFactory.CreatePlanId("plan-123"));

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (ApprovalActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.PlanId.Value.Should().Be("plan-123");
    }

    [Fact(DisplayName = "Round-Trip: SessionAssignedActivity preserves task 🏺")]
    public void SessionAssignedActivity_PreservesTask_DuringRoundTrip()
    {
        // Arrange
        var factory = _serviceProvider.GetRequiredService<ActivityMapperFactory>();
        var original = new SessionAssignedActivity("init-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.System, (TaskDescription)"Start Mission");

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = (SessionAssignedActivity)factory.FromEnvelope(envelope);

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.Task.Should().Be((TaskDescription)"Start Mission");
    }

    [Fact(DisplayName = "Round-Trip: Session status is preserved in the registry 🏺💓")]
    public void SessionStatus_IsPreserved_DuringRoundTrip()
    {
        // Arrange
        var mapper = _serviceProvider.GetRequiredService<IRegistryTaskMapper>();
        var original = new Session(
            TestFactory.CreateSessionId("1"),
            "remote-1",
            (TaskDescription)"Mission",
            TestFactory.CreateSourceContext("repo"),
            new SessionPulse(SessionStatus.Completed),
            DateTimeOffset.UtcNow);

        // Act
        var dto = mapper.MapToDto(original);
        var hydrated = mapper.MapToDomain(dto);

        // Assert
        hydrated.Pulse.Status.Should().Be(SessionStatus.Completed);
        hydrated.State.Should().Be(SessionState.Idle);
    }
}
