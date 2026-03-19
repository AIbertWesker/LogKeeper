using Microsoft.Extensions.DependencyInjection;

namespace LogKeeper.Logic;

public static class DependencyInjection
{
    public static IServiceCollection AddLogic(this IServiceCollection services)
    {
        return services;
    }
}