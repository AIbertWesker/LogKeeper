namespace LogKeeper.Domain;

public sealed class LogPageQuery
{
    public PageRequest Page { get; init; } = new();
    public LogFilters Filters { get; init; } = new();
    public DateTime? CursorTimestamp { get; init; }
}
