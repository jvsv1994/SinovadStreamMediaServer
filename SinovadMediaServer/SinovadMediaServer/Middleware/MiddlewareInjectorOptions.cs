using SinovadMediaServer.Configuration;
using SinovadMediaServer.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace SinovadMediaServer.Middleware
{
		public static class MiddlewareInjectorExtensions
		{
			public static IApplicationBuilder UseMiddlewareInjector(this IApplicationBuilder builder, MiddlewareInjectorOptions options)
			{
				return builder.UseMiddleware<MiddlewareInjectorMiddleware>(builder.New(), options);
			}
		}

		public class MiddlewareInjectorMiddleware
		{
			private readonly RequestDelegate _next;
			private readonly IApplicationBuilder _builder;
			private readonly MiddlewareInjectorOptions _options;
			private RequestDelegate _subPipeline;
			public static IOptions<MyConfig> _config { get; set; }
			public SharedData _sharedData;

		public MiddlewareInjectorMiddleware(RequestDelegate next, IApplicationBuilder builder, MiddlewareInjectorOptions options, IOptions<MyConfig> config, SharedData sharedData)
			{
				
				_config = config;
				_sharedData = sharedData;
				_next = next ?? throw new ArgumentNullException(nameof(next));
				_builder = builder ?? throw new ArgumentNullException(nameof(builder));
				_options = options ?? throw new ArgumentNullException(nameof(options));
			}

			public Task Invoke(HttpContext httpContext)
			{
				var injector = _options.GetInjector();
				if (injector != null)
				{
					var builder = _builder.New();
					injector(builder);
					builder.Run(_next);
					_subPipeline = builder.Build();
				}
				if (_subPipeline != null)
				{
					return _subPipeline(httpContext);
				}
				return _next(httpContext);
			}

		}

		public class MiddlewareInjectorOptions
		{
			private Action<IApplicationBuilder> _injector;

			public MiddlewareInjectorOptions()
			{

			}

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
