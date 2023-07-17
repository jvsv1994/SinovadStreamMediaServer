using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SinovadMediaServer.Background;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.SchedulerJob;
using SinovadMediaServer.Shared;

namespace SinovadMediaServer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var builderTmp = new ConfigurationBuilder().AddCommandLine(args);
            IConfiguration config = builderTmp.Build();
            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddressesAsync(hostName);
            var listFindedIps = ips.Result.Where(a => a.IsIPv6LinkLocal == false).ToList();
            List<String> listUrls = new List<String>();
            var httpUrl = "";
            var httpsUrl = "";
            var defaultIpAddress = "";
            if (listFindedIps.Count > 0)
            {
                var fip = listFindedIps[0];           
                defaultIpAddress = fip.ToString();
                httpUrl = "http://" + defaultIpAddress + ":5179";
                httpsUrl = "https://" + defaultIpAddress + ":5180";
                listUrls.Add(httpUrl);
                //listUrls.Add(httpsUrl);           
            }

            config["IpAddress"] = defaultIpAddress;
            config["WebUrl"] = httpUrl;
            config["RestApiUrl"] = "http://streamapi.sinovad.com/api/v1";
            //config["RestApiUrl"] = "http://localhost:53363/api/v1";  
            Task.Run(() => StartWebServer(config, listUrls, args));
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(config));
        }

        private static void StartWebServer(IConfiguration config, List<String> listUrls,string[] args)
        {
   
            var builder = WebHost.CreateDefaultBuilder();
            var app = builder
              .UseConfiguration(config)
              .UseKestrel()
              .UseUrls(listUrls.ToArray())
              .UseContentRoot(Directory.GetCurrentDirectory())
              .UseWebRoot(Path.Combine("wwwroot"))
              .ConfigureServices((services) =>
              {
                  services.AddQuartz(q =>
                  {
                      q.UseMicrosoftDependencyInjectionScopedJobFactory();
                      // Just use the name of your job that you created in the Jobs folder.
                      var jobKey = new JobKey("SendEmailJob");
                      q.AddJob<BackgroundJob>(opts => opts.WithIdentity(jobKey));

                      q.AddTrigger(opts => opts
                          .ForJob(jobKey)
                          .WithIdentity("SendEmailJob-trigger")
                          //This Cron interval can be described as "run every minute" (when second is zero)
                          .WithCronSchedule("0 /8 * ? * *")
                      );
                  });
                  services.AddHostedService<TimedHostedService>();
                  services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                  services.AddControllers().AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                  );
                  services.AddSingleton<MiddlewareInjectorOptions>();
                  services.AddSingleton<SharedData>();
                  services.AddSingleton<SharedService>();
                  services.Configure<MyConfig>(config);
                  services.AddMemoryCache();
                  services.AddCors(options => options.AddPolicy("AllowAnyOrigin",
                  builder =>
                  {
                      builder
                      .AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
                  }));
              })
              .Configure(app =>
              {
                  app.UseCors("AllowAnyOrigin");
                  app.UseHttpsRedirection();
                  app.UseStaticFiles();
                  var injectorOptions = app.ApplicationServices.GetService<MiddlewareInjectorOptions>();
                  app.UseMiddlewareInjector(injectorOptions);
                  app.Use(async (context, next) =>
                  {
                      // Forward to the next one.
                      await next.Invoke();
                  });
                  app.UseRouting();
                  app.UseAuthorization();
                  app.UseEndpoints(endpoints =>
                  {
                      endpoints.MapControllers();
                  });
              }).Build();
            app.Run();
        }

    }
}