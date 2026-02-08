namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Defines the level of autonomy granted to Jules during a session.
/// </summary>
public enum AutomationMode
{
    /// <summary>
    /// Jules will only propose a plan and wait for manual approval.
    /// </summary>
    None,

    /// <summary>
    /// Jules will automatically create a Pull Request upon successful task completion.
    /// </summary>
    AutoCreatePullRequest
}
