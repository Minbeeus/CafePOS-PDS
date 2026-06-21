using Microsoft.AspNetCore.Builder;

namespace CafePOS.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        return app;
    }
}
