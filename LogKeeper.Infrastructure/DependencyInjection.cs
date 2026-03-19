using LogKeeper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LogKeeper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILogKeeperClock, SystemClock>();
        return services;
    }
}

