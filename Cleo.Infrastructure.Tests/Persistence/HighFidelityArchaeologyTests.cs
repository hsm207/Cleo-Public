using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
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
            new(0, "Step 1", "Description 1"),
            new(1, "Step 2", "Description 2")
        };
        var original = new PlanningActivity("act-123", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "plan-456", originalSteps);

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = factory.FromEnvelope(envelope) as PlanningActivity;

        // Assert
        hydrated.Should().NotBeNull();
        hydrated!.PlanId.Should().Be("plan-456");
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
        var original = new ProgressActivity("act-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Testing with evidence", null, evidence);

        // Act
        var envelope = factory.ToEnvelope(original);
        var hydrated = factory.FromEnvelope(envelope) as ProgressActivity;

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
        var activity = new PlanningActivity("act-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "p1", new List<PlanStep>());

        // Act
        var envelope = factory.ToEnvelope(activity);

        // Assert
        envelope.Type.Should().Be("PLAN_GENERATED"); 
    }

    [Fact(DisplayName = "Truth-Sensing: Logical Stance Override identifies blocked sessions 🧠⚖️")]
    public void Session_EvaluatesStanceLogically_WhenIdleButBlockedOnPlan()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        var task = (TaskDescription)"Fix bug";
        var source = new SourceContext("repo", "main");
        
        // A session that is physically IDLE (Completed) but has a Plan and NO PR
        var pulse = new SessionPulse(SessionStatus.Completed, "Done (technical)");
        var session = new Session(sessionId, task, source, pulse);
        
        session.AddActivity(new PlanningActivity("act-plan", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "plan-1", new List<PlanStep> { new(0, "Do it", "Now") }));

        // Act & Assert
        session.Pulse.Status.Should().Be(SessionStatus.Completed); 
        session.EvaluatedStance.Should().Be(Stance.AwaitingPlanApproval); // Logical Override! 🧠🔥
        session.DeliveryStatus.Should().Be(DeliveryStatus.Unfulfilled); // The Truth! 💎
    }
}
