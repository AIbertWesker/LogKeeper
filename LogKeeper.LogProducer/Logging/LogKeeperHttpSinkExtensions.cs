using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Net.Http.Json;

namespace LogKeeper.LogProducer.Logging;

public static class LogKeeperHttpSinkExtensions
{
    public static LoggerConfiguration LogKeeperHttp(
        this LoggerSinkConfiguration sinkConfiguration,
        string endpoint,
        string applicationName,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
        HttpMessageHandler? httpMessageHandler = null)
    {
        var httpClient = httpMessageHandler is null
            ? new HttpClient()
            : new HttpClient(httpMessageHandler);

        return sinkConfiguration.Sink(
            new LogKeeperHttpSink(httpClient, endpoint, applicationName),
            restrictedToMinimumLevel);
    }
}

internal sealed class LogKeeperHttpSink : ILogEventSink, IDisposable
{
    private const int MaxBufferedPerLevel = 10;

    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _applicationName;
    private readonly object _sync = new();
    private readonly Dictionary<LogEventLevel, Queue<LogKeeperPayload>> _buffers = new();
    private readonly Timer _flushTimer;

    public LogKeeperHttpSink(HttpClient httpClient, string endpoint, string applicationName)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
        _applicationName = applicationName;
        _flushTimer = new Timer(_ => FlushBuffered(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void Emit(LogEvent logEvent)
    {
        var payload = CreatePayload(logEvent);

        lock (_sync)
        {
            if (!TryFlushBufferedUnsafe())
            {
                EnqueueUnsafe(logEvent.Level, payload);
                return;
            }

            if (!TrySend(payload))
            {
                EnqueueUnsafe(logEvent.Level, payload);
            }
        }
    }

    public void Dispose()
    {
        _flushTimer.Dispose();

        lock (_sync)
        {
            TryFlushBufferedUnsafe();
        }

        _httpClient.Dispose();
    }

    private void FlushBuffered()
    {
        lock (_sync)
        {
            TryFlushBufferedUnsafe();
        }
    }

    private bool TryFlushBufferedUnsafe()
    {
        foreach (var queue in _buffers.Values)
        {
            while (queue.Count > 0)
            {
                var payload = queue.Peek();
                if (!TrySend(payload))
                {
                    return false;
                }

                queue.Dequeue();
            }
        }

        return true;
    }

    private bool TrySend(LogKeeperPayload payload)
    {
        try
        {
            using var response = _httpClient.PostAsJsonAsync(_endpoint, payload).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private void EnqueueUnsafe(LogEventLevel level, LogKeeperPayload payload)
    {
        if (!_buffers.TryGetValue(level, out var queue))
        {
            queue = new Queue<LogKeeperPayload>();
            _buffers[level] = queue;
        }

        if (queue.Count >= MaxBufferedPerLevel)
        {
            queue.Dequeue();
        }

        queue.Enqueue(payload);
    }

    private LogKeeperPayload CreatePayload(LogEvent logEvent)
    {
        return new LogKeeperPayload(
            Guid.CreateVersion7(),
            logEvent.Timestamp.UtcDateTime,
            logEvent.Level.ToString(),
            logEvent.RenderMessage(),
            _applicationName,
            TryGetScalarValue(logEvent, "CorrelationId"),
            TryGetScalarValue(logEvent, "ClientIp"),
            Environment.MachineName,
            logEvent.Exception?.ToString(),
            ToFlatProperties(logEvent));
    }

    private static string? TryGetScalarValue(LogEvent logEvent, string key)
    {
        if (!logEvent.Properties.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value is ScalarValue scalar && scalar.Value is not null)
        {
            return scalar.Value.ToString();
        }

        return value.ToString().Trim('"');
    }

    private static Dictionary<string, object> ToFlatProperties(LogEvent logEvent)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in logEvent.Properties)
        {
            result[key] = value.ToString().Trim('"');
        }

        return result;
    }
}

internal sealed record LogKeeperPayload(
    Guid Id,
    DateTime Timestamp,
    string Level,
    string Message,
    string Application,
    string? CorrelationId,
    string? ClientIp,
    string MachineName,
    string? Exception,
    Dictionary<string, object> Properties);
