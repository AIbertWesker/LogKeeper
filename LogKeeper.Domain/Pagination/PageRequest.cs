namespace LogKeeper.Domain;

public sealed class PageRequest
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public int PageSize { get; init; } = DefaultPageSize;

    public int NormalizedPageSize
    {
        get
        {
            if (PageSize <= 0)
            {
                return DefaultPageSize;
            }

            return PageSize > MaxPageSize ? MaxPageSize : PageSize;
        }
    }
}
