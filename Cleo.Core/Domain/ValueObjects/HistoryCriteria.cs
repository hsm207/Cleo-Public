using Cleo.Core.Domain.Entities;

namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Criteria for filtering session history.
/// </summary>
public sealed record HistoryCriteria(
    IReadOnlyCollection<Type>? ActivityTypes = null,
    DateTimeOffset? Since = null,
    DateTimeOffset? Until = null,
    string? SearchText = null
)
{
    public static readonly HistoryCriteria None = new();

    public bool IsSatisfiedBy(SessionActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (ActivityTypes != null && ActivityTypes.Count > 0 && !ActivityTypes.Contains(activity.GetType()))
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

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var content = activity.GetContentSummary();
            var thoughts = string.Join(" ", activity.GetThoughts());

            // Simple containment check across summary and thoughts
            if (!content.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                !thoughts.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
