namespace LogKeeper;

using LogKeeper.Abstractions;
using LogKeeper.Domain;
using LogKeeper.Infrastructure;
using LogKeeper.Logic;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddLogic();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("Frontend");
        app.UseHttpsRedirection();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
            .WithName("Health")
            .AddOpenApiOperationTransformer((opperation, context, ct) =>
            {
                opperation.Summary = "App health check.";
                opperation.Description = "Returns app health status.";
                return Task.CompletedTask;
            });

        app.MapGet("/logs/{id:guid}", async (Guid id, ILogRepository logRepository, CancellationToken cancellationToken) =>
        {
            var log = await logRepository.GetByIdAsync(id, cancellationToken);
            return log is null ? Results.NotFound() : Results.Ok(log);
        });

        app.MapGet("/logs", async (
            int? pageSize,
            DateTime? cursorTimestamp,
            string? level,
            string? application,
            string? correlationId,
            string? clientIp,
            string? messageContains,
            DateTime? from,
            DateTime? to,
            ILogRepository logRepository,
            CancellationToken cancellationToken) =>
        {
            var query = new LogPageQuery
            {
                Page = new PageRequest { PageSize = pageSize ?? PageRequest.DefaultPageSize },
                CursorTimestamp = cursorTimestamp,
                Filters = new LogFilters
                {
                    Level = level,
                    Application = application,
                    CorrelationId = correlationId,
                    ClientIp = clientIp,
                    MessageContains = messageContains,
                    From = from,
                    To = to
                }
            };

            var page = await logRepository.GetPageAsync(query, cancellationToken);
            return Results.Ok(page);
        });

        app.MapPost("/logs", async (LogEntry logEntry, ILogRepository logRepository, CancellationToken cancellationToken) =>
        {
            if (logEntry.Id == Guid.Empty)
            {
                logEntry.Id = Guid.CreateVersion7();
            }

            if (logEntry.Timestamp == default)
            {
                logEntry.Timestamp = DateTime.UtcNow;
            }

            await logRepository.SaveAsync(logEntry);
            return Results.Ok();
        });

        app.Run();
    }
}
