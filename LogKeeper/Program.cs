namespace LogKeeper;

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

        builder.Services.AddLogic();
        builder.Services.AddInfrastructure();

        var app = builder.Build();

        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
            .WithName("Health")
            .AddOpenApiOperationTransformer((opperation, context, ct) =>
            {
                opperation.Summary = "App health check.";
                opperation.Description = "Returns app health status.";
                return Task.CompletedTask;
            });

        app.Run();
    }
}
