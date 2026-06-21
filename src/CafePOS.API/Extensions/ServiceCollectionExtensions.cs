using Microsoft.Extensions.DependencyInjection;

namespace CafePOS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services;
    }
}
