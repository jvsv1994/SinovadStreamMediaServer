using Microsoft.AspNetCore.Builder;

namespace SinovadMediaServer.MiddlewareInjector
{
    public static class MiddlewareInjectorExtensions
    {
        public static IApplicationBuilder UseMiddlewareInjector(this IApplicationBuilder builder, MiddlewareInjectorOptions options)
        {
            return builder.UseMiddleware<MiddlewareInjectorMiddleware>(builder.New(), options);
        }
    }
}
