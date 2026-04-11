using LogKeeper.Domain;

namespace LogKeeper.Abstractions;

public interface ILogRepository
{
    Task SaveAsync(LogEntry log);
    Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<LogEntry>> GetPageAsync(LogPageQuery query, CancellationToken cancellationToken = default);
}