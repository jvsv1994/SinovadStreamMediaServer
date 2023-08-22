using Microsoft.AspNetCore.Builder;

namespace SinovadMediaServer.MiddlewareInjector
{
    public class MiddlewareInjectorOptions
    {
        private Action<IApplicationBuilder> _injector;

        public void InjectMiddleware(Action<IApplicationBuilder> builder)
        {
            Interlocked.Exchange(ref _injector, builder);
        }

        internal Action<IApplicationBuilder> GetInjector()
        {
            return Interlocked.Exchange(ref _injector, null);
        }
    }
}
