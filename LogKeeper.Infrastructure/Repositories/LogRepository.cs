using LogKeeper.Abstractions;
using LogKeeper.Domain;
using LogKeeper.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogKeeper.Infrastructure.Repositories;

internal sealed class LogRepository : ILogRepository
{
    private const string _collectionName = "logs";

    private readonly IMongoCollection<LogEntry> _collection;
    private readonly MongoSettings _mongoSettings;

    public LogRepository(IMongoClient client, IOptions<MongoSettings> mongoSettings)
    {
        _mongoSettings = mongoSettings.Value;
        var database = client.GetDatabase(_mongoSettings.DatabaseName);
        _collection = database.GetCollection<LogEntry>(_collectionName);
    }

    public async Task SaveAsync(LogEntry log)
    {
        await _collection.InsertOneAsync(log);
    }

    public async Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LogEntry>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedList<LogEntry>> GetPageAsync(LogPageQuery query, CancellationToken cancellationToken = default)
    {
        var safeQuery = query ?? new LogPageQuery();
        var baseFilter = BuildBaseFilter(safeQuery.Filters);
        var totalCountTask = _collection.CountDocumentsAsync(baseFilter, cancellationToken: cancellationToken);

        var pageFilter = baseFilter;
        if (safeQuery.CursorTimestamp.HasValue)
        {
            var cursorFilter = Builders<LogEntry>.Filter.Lt(x => x.Timestamp, safeQuery.CursorTimestamp.Value);
            pageFilter = Builders<LogEntry>.Filter.And(baseFilter, cursorFilter);
        }

        var limit = safeQuery.Page.NormalizedPageSize + 1;
        var pageItems = await _collection
            .Find(pageFilter)
            .SortByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        var hasMore = pageItems.Count > safeQuery.Page.NormalizedPageSize;
        if (hasMore)
        {
            pageItems.RemoveAt(pageItems.Count - 1);
        }

        DateTime? nextCursorTimestamp = hasMore ? pageItems[^1].Timestamp : null;
        var totalCount = await totalCountTask;

        return new PaginatedList<LogEntry>(pageItems, totalCount, pageItems.Count, nextCursorTimestamp, hasMore);
    }

    private static FilterDefinition<LogEntry> BuildBaseFilter(LogFilters? filters)
    {
        if (filters is null)
        {
            return FilterDefinition<LogEntry>.Empty;
        }

        var filterBuilder = Builders<LogEntry>.Filter;
        var filterDefinitions = new List<FilterDefinition<LogEntry>>();

        if (!string.IsNullOrWhiteSpace(filters.Level))
        {
            filterDefinitions.Add(filterBuilder.Eq(x => x.Level, filters.Level));
        }

        if (!string.IsNullOrWhiteSpace(filters.Application))
        {
            filterDefinitions.Add(filterBuilder.Eq(x => x.Application, filters.Application));
        }

        if (!string.IsNullOrWhiteSpace(filters.CorrelationId))
        {
            filterDefinitions.Add(filterBuilder.Eq(x => x.CorrelationId, filters.CorrelationId));
        }

        if (!string.IsNullOrWhiteSpace(filters.ClientIp))
        {
            filterDefinitions.Add(filterBuilder.Eq(x => x.ClientIp, filters.ClientIp));
        }

        if (!string.IsNullOrWhiteSpace(filters.MessageContains))
        {
            filterDefinitions.Add(filterBuilder.Regex(x => x.Message, new BsonRegularExpression(filters.MessageContains, "i")));
        }

        if (filters.From.HasValue)
        {
            filterDefinitions.Add(filterBuilder.Gte(x => x.Timestamp, filters.From.Value));
        }

        if (filters.To.HasValue)
        {
            filterDefinitions.Add(filterBuilder.Lte(x => x.Timestamp, filters.To.Value));
        }

        return filterDefinitions.Count == 0
            ? FilterDefinition<LogEntry>.Empty
            : filterBuilder.And(filterDefinitions);
    }

}
