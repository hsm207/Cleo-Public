using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Tests.Common;

public static class TestFactory
{
    public static class Constants
    {
        public const string DefaultBranch = "main";
    }

    public static SessionId CreateSessionId(string suffix) => new($"sessions/{suffix}");

    public static PlanId CreatePlanId(string suffix) => new($"plans/{suffix}");

    public static SourceContext CreateSourceContext(string repo, string branch = Constants.DefaultBranch)
        => new($"sources/{repo}", branch);

    /// <summary>
    /// Provides raw data for testing "Dirty" scenarios and Self-Healing.
    /// </summary>
    public static class Data
    {
        public const string LegacySessionId = "123";
        public const string LegacyPlanId = "456";
        public const string LegacyRepo = "user/repo";
    }
}
