using LogKeeper.Abstractions;
using LogKeeper.Infrastructure.Options;
using LogKeeper.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogKeeper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration dupa)
    {
        services.AddSingleton<ILogKeeperClock, SystemClock>();

        services.Configure<MongoSettings>(dupa.GetSection(MongoSettings.SectionName));
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<ILogRepository, LogRepository>(); 

        return services;
    }
}

