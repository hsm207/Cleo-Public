using Cleo.Core.Domain.Entities;

namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Criteria for filtering session history.
/// </summary>
public sealed record HistoryCriteria(
    IReadOnlyCollection<Type>? Types = null,
    DateTimeOffset? Since = null,
    DateTimeOffset? Until = null,
    string? Text = null
)
{
    public static readonly HistoryCriteria None = new();

    public bool IsSatisfiedBy(SessionActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (Types != null && Types.Count > 0 && !Types.Contains(activity.GetType()))
        {
            return false;
        }

        if (Since.HasValue && activity.Timestamp < Since.Value)
        {
            return false;
        }

        if (Until.HasValue && activity.Timestamp > Until.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var content = activity.GetContentSummary();
            var thoughts = string.Join(" ", activity.GetThoughts());

            // Simple containment check across summary and thoughts
            if (!content.Contains(Text, StringComparison.OrdinalIgnoreCase) &&
                !thoughts.Contains(Text, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
