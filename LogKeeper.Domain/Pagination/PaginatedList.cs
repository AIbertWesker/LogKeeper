namespace LogKeeper.Domain;

public sealed class PaginatedList<T>
{
    public PaginatedList(IReadOnlyList<T> items, long totalCount, int currentSize, DateTime? nextCursorTimestamp, bool hasMore)
    {
        Items = items;
        TotalCount = totalCount;
        CurrentSize = currentSize;
        NextCursorTimestamp = nextCursorTimestamp;
        HasMore = hasMore;
    }

    public IReadOnlyList<T> Items { get; }
    public long TotalCount { get; }
    public int CurrentSize { get; }
    public DateTime? NextCursorTimestamp { get; }
    public bool HasMore { get; }
}
