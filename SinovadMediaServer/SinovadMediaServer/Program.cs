using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.SchedulerJob;
using SinovadMediaServer.Shared;
using System.DirectoryServices.AccountManagement;

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
            var listUrls = SetConfigurationData(config);
            var webHost=StartWebServer(config, listUrls, args);
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(webHost));
        }


        private static List<string> SetConfigurationData(IConfiguration config)
        {
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
            config["PortNumber"] = "5179";
            config["PublicIpAddress"] = GetPublicIp();
            config["DeviceName"] = Environment.MachineName;
            config["IpAddress"] = defaultIpAddress;
            UserPrincipal user = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
            String sid = user.Sid.Value;
            config["SecurityIdentifier"]= sid;
            config["WebUrl"] = httpUrl;
            config["RestApiUrl"] = "http://streamapi.sinovad.com/api/v1";
            //config["RestApiUrl"] = "http://localhost:53363/api/v1";  
            return listUrls;
        }


        private static string GetPublicIp()
        {
            return "";
            //string url = "http://checkip.dyndns.org";
            //System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            //System.Net.WebResponse resp = req.GetResponse();
            //System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            //string response = sr.ReadToEnd().Trim();
            //string[] ipAddressWithText = response.Split(':');
            //string ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
            //string[] ipAddress = ipAddressWithHTMLEnd.Split('<');
            //string mainIP = ipAddress[0];
            //return mainIP;
        }

        private static IWebHost StartWebServer(IConfiguration config, List<String> listUrls,string[] args)
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
                  //services.AddHostedService<TimedHostedService>();
                  services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                  services.AddControllers().AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                  );
                  services.AddSingleton<SharedData>();
                  services.AddSingleton<MiddlewareInjectorOptions>();
                  services.AddScoped<RestService>();
                  services.AddScoped<SharedService>();
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
                  var sharedService = app.ApplicationServices.GetService<SharedService>();
                  var sharedData = app.ApplicationServices.GetService<SharedData>();
              }).Build();
            return app;
        }

    }
}