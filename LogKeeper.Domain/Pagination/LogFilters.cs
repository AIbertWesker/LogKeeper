namespace LogKeeper.Domain;

public sealed class LogFilters
{
    public string? Level { get; init; }
    public string? Application { get; init; }
    public string? CorrelationId { get; init; }
    public string? ClientIp { get; init; }
    public string? MessageContains { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
