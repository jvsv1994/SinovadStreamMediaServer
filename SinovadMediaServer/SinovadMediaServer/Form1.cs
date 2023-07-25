using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using SinovadMediaServer.Common;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.SchedulerJob;
using SinovadMediaServer.Shared;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;

namespace SinovadMediaServer
{
    public partial class Form1 : Form
    {

        private static SharedData _sharedData;

        private static RestService _restService;

        private static MediaServerConfig _mediaServerConfig;

        private static List<String> _listUrls;

        public Form1()
        {
            _sharedData = new SharedData();
            _restService = new RestService(_sharedData);
            SetMediaServerConfiguration();
            InitializeComponent();
            ValidateMediaServer();
        }

        private static void StartWebServer()
        {
            var builder = WebHost.CreateDefaultBuilder();
            var app = builder
            .UseKestrel()
              //.ConfigureKestrel(options =>
              //{
              //    var port = 5179;
              //    var pfxFilePath = @"kestrel.pfx";
              //    // The password you specified when exporting the PFX file using OpenSSL.
              //    // This would normally be stored in configuration or an environment variable;
              //    // I've hard-coded it here just to make it easier to see what's going on.
              //    var pfxPassword = "changeit";

              //    options.Listen(IPAddress.Any, port, listenOptions =>
              //    {
              //        // Enable support for HTTP1 and HTTP2 (required if you want to host gRPC endpoints)
              //        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
              //        // Configure Kestrel to use a certificate from a local .PFX file for hosting HTTPS
              //        listenOptions.UseHttps(pfxFilePath, pfxPassword);
              //    });
              //})
              .UseUrls(_listUrls.ToArray())
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
                  var fileOptions = new FileServerOptions
                  {
                      FileProvider = new PhysicalFileProvider(_sharedData.TranscoderSettingsData.TemporaryFolder),
                      RequestPath = new PathString("/transcoded"),
                      EnableDirectoryBrowsing = true,
                      EnableDefaultFiles = false
                  };
                  fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
                  app.UseFileServer(fileOptions);
                  var sharedData = app.ApplicationServices.GetService<SharedData>();
                  sharedData.ApiToken=_sharedData.ApiToken;
                  sharedData.UserData = _sharedData.UserData;
                  sharedData.MediaServerData = _sharedData.MediaServerData;
                  sharedData.TranscoderSettingsData = _sharedData.TranscoderSettingsData;
                  sharedData.ListPresets=_sharedData.ListPresets;
                  sharedData.WebUrl=_sharedData.WebUrl;
              }).Build();
            Task.Run(() =>
            {
                app.Run();
            });
        }

        private static void SetMediaServerConfiguration()
        {
            _mediaServerConfig = new MediaServerConfig();
            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddressesAsync(hostName);
            var listFindedIps = ips.Result.Where(a => a.IsIPv6LinkLocal == false).ToList();
            _listUrls = new List<string>();
            var httpUrl = "";
            var httpsUrl = "";
            var defaultIpAddress = "";
            if (listFindedIps.Count > 0)
            {
                var fip = listFindedIps[0];
                defaultIpAddress = fip.ToString();
                httpUrl = "http://" + defaultIpAddress + ":5179";
                httpsUrl = "https://" + defaultIpAddress + ":5180";
                _listUrls.Add(httpUrl);
                //_listUrls.Add(httpsUrl);           
            }
            _mediaServerConfig.PortNumber = "5179";
            _mediaServerConfig.PublicIpAddress = GetPublicIp();
            _mediaServerConfig.DeviceName= Environment.MachineName;
            _mediaServerConfig.IpAddress = defaultIpAddress;
            UserPrincipal user = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
            String sid = user.Sid.Value;
            _mediaServerConfig.SecurityIdentifier = sid;
            _mediaServerConfig.WebUrl = httpUrl;
            _sharedData.WebUrl = httpUrl;
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

        public async Task<Response<AuthenticateMediaServerResponseDto>> AuthenticateMediaServer()
        {
            var response = await _restService.ExecuteHttpMethodAsync<AuthenticateMediaServerResponseDto>(HttpMethodType.POST, "/authentication/AuthenticateMediaServer", _mediaServerConfig.SecurityIdentifier);
            return response;
        }

        public async Task<Response<AuthenticateUserResponseDto>> ValidateUser(string username, string password)
        {
            var accessUserDto = new AccessUserDto();
            accessUserDto.UserName = username;
            accessUserDto.Password = password;
            var response = await _restService.ExecuteHttpMethodAsync<AuthenticateUserResponseDto>(HttpMethodType.POST, "/authentication/AuthenticateUser", accessUserDto);
            return response;
        }

        public async Task<bool> SetupMediaServer()
        {
            var resultMediaServer = await SaveMediaServer();
            if (resultMediaServer != null)
            {
                _sharedData.MediaServerData = resultMediaServer;
                var resultTranscoderSettings = await GetTranscoderSettings();
                if (resultTranscoderSettings != null)
                {
                    _sharedData.TranscoderSettingsData = resultTranscoderSettings;
                    var list = await GetPresets();
                    if (list != null && list.Count > 0)
                    {
                        _sharedData.ListPresets = list;
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<MediaServerDto> SaveMediaServer()
        {
            var mediaServerDto = new MediaServerDto();
            if (_sharedData.MediaServerData != null)
            {
                mediaServerDto = _sharedData.MediaServerData;
            }
            if (_sharedData.UserData != null)
            {
                mediaServerDto.UserId = _sharedData.UserData.Id;
            }
            mediaServerDto.SecurityIdentifier = _mediaServerConfig.SecurityIdentifier;
            mediaServerDto.DeviceName = _mediaServerConfig.DeviceName;
            mediaServerDto.Url = _mediaServerConfig.WebUrl;
            mediaServerDto.IpAddress = _mediaServerConfig.IpAddress;
            mediaServerDto.Port = int.Parse(_mediaServerConfig.PortNumber);
            mediaServerDto.PublicIpAddress = _mediaServerConfig.PublicIpAddress;
            var response = await _restService.ExecuteHttpMethodAsync<MediaServerDto>(HttpMethodType.PUT, "/mediaServers/Save", mediaServerDto);
            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }

        public async Task<TranscoderSettingsDto> GetTranscoderSettings()
        {

            var response = await _restService.ExecuteHttpMethodAsync<TranscoderSettingsDto>(HttpMethodType.GET, "/transcoderSettings/GetByMediaServerAsync/" + _sharedData.MediaServerData.Id);
            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            else
            {
                return await CreateTranscoderSettings();
            }
        }

        public async Task<List<CatalogDetailDto>> GetPresets()
        {
            var response = await _restService.ExecuteHttpMethodAsync<List<CatalogDetailDto>>(HttpMethodType.GET, "/catalogs/GetDetailsByCatalogAsync/" + (int)Catalog.TranscoderPreset);
            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            else
            {
                return null;
            }
        }

        public async Task<TranscoderSettingsDto> CreateTranscoderSettings()
        {
            var tsDto = new TranscoderSettingsDto();
            tsDto.MediaServerId = _sharedData.MediaServerData.Id;
            tsDto.ConstantRateFactor = 18;
            tsDto.PresetCatalogId = (int)Catalog.TranscoderPreset;
            tsDto.PresetCatalogDetailId = (int)TranscoderPreset.Ultrafast;
            tsDto.VideoTransmissionTypeCatalogId = (int)Catalog.VideoTransmissionType;
            tsDto.VideoTransmissionTypeCatalogDetailId = (int)VideoTransmissionType.HLS;
            tsDto.TemporaryFolder = System.IO.Path.GetTempPath();
            var response = await _restService.ExecuteHttpMethodAsync<TranscoderSettingsDto>(HttpMethodType.PUT, "/transcoderSettings/Save", tsDto);
            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }

        private async void ValidateMediaServer()
        {
            var response = await AuthenticateMediaServer();
            if (response.IsSuccess && response.Data != null)
            {
                var data = response.Data;
                _sharedData.ApiToken = data.ApiToken;
                _sharedData.MediaServerData = data.MediaServer;
                if (data.User != null)
                {
                    _sharedData.UserData = data.User;
                    var completed = await SetupMediaServer();
                    if (completed)
                    {
                        usernameLabel.Text = _sharedData.UserData.UserName;
                        ipAddressAndPortLabel.Text = _sharedData.MediaServerData.IpAddress + " : " + _sharedData.MediaServerData.Port;
                        deviceNameLabel.Text = _sharedData.MediaServerData.DeviceName;
                        familyNameLabel.Text = _sharedData.MediaServerData.FamilyName != null && _sharedData.MediaServerData.FamilyName != "" ? _sharedData.MediaServerData.FamilyName : "No especificado";
                        StartWebServer();
                        panelMediaServerInfo.Visible = true;
                    }
                }
                else
                {
                    panelValidateCredentials.Visible = true;
                }
            }
            else
            {
                panelValidateCredentials.Visible = true;
            }
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void validateUserButton_Click(object sender, EventArgs e)
        {
            validateUserButton.Enabled = false;
            textBoxUser.Enabled = false;
            textBoxPassword.Enabled = false;
            authenticationMessageLabel.Visible = false;
            var res = await ValidateUser(textBoxUser.Text, textBoxPassword.Text);
            if (res.IsSuccess)
            {
                var data = res.Data;
                _sharedData.ApiToken = data.ApiToken;
                _sharedData.UserData = data.User;
                authenticationMessageLabel.ForeColor = Color.Green;
                authenticationMessageLabel.Text = "Autenticación exitosa";
                authenticationMessageLabel.Visible = true;
                authenticationMessageLabel.Text = "Configurando Servidor multimedia, espere un momento por favor";
                var completed = await SetupMediaServer();
                if (completed)
                {
                    usernameLabel.Text = _sharedData.UserData.UserName;
                    ipAddressAndPortLabel.Text = _sharedData.MediaServerData.IpAddress + " : " + _sharedData.MediaServerData.Port;
                    deviceNameLabel.Text = _sharedData.MediaServerData.DeviceName;
                    familyNameLabel.Text = _sharedData.MediaServerData.FamilyName != null && _sharedData.MediaServerData.FamilyName != "" ? _sharedData.MediaServerData.FamilyName : "No especificado";
                    StartWebServer();
                    panelValidateCredentials.Visible = false;
                    panelMediaServerInfo.Visible = true;
                }
            }
            else
            {
                authenticationMessageLabel.ForeColor = Color.Red;
                authenticationMessageLabel.Text = res.Message;
                authenticationMessageLabel.Visible = true;
                validateUserButton.Enabled = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panelValidateCredentials_Paint(object sender, PaintEventArgs e)
        {

        }

        private void manageLibrariesButton_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://webstream.sinovad.com/settings/server/" + _sharedData.MediaServerData.Guid + "/manage/libraries?apiToken=" + _sharedData.ApiToken) { UseShellExecute = true });
        }

        private void authenticationMessageLabel_Click(object sender, EventArgs e)
        {

        }

        private void buttonRegisterUser_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://webstream.sinovad.com/register") { UseShellExecute = true });
        }
    }
}