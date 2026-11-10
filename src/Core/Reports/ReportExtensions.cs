namespace Core.Reports;

public static class ReportExtensions
{
    public static IEnumerable<T> TopBy<T>(
    this IEnumerable<T> source,
    Func<T, int> selector,
    int n,
    Func<T, string> tieBreaker)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(tieBreaker);

        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        return source
            .OrderByDescending(selector)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n);

    }
}
