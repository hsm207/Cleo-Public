namespace Cleo.Infrastructure.Common;

/// <summary>
/// A centralized logic selector for implementing the Strategy Pattern.
/// Eliminates code cloning in Mapper Factories and Clients.
/// </summary>
internal static class StrategySelector
{
    /// <summary>
    /// Selects the first strategy that matches the criteria, or returns default/null.
    /// </summary>
    public static TStrategy? Select<TStrategy, TCandidate>(
        IEnumerable<TStrategy> strategies,
        TCandidate candidate,
        Func<TStrategy, TCandidate, bool> predicate)
    {
        return strategies.FirstOrDefault(s => predicate(s, candidate));
    }

    /// <summary>
    /// Selects the first strategy that matches the criteria, or throws if none found.
    /// </summary>
    public static TStrategy SelectOrThrow<TStrategy, TCandidate>(
        IEnumerable<TStrategy> strategies,
        TCandidate candidate,
        Func<TStrategy, TCandidate, bool> predicate,
        Func<string> errorMessageFactory)
    {
        var strategy = Select(strategies, candidate, predicate);
        if (strategy == null)
        {
            throw new InvalidOperationException(errorMessageFactory());
        }
        return strategy;
    }
}
