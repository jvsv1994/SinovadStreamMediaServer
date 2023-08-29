using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Application.UseCases.Alerts;
using SinovadMediaServer.Application.UseCases.Libraries;
using SinovadMediaServer.Application.UseCases.TranscoderSetting;
using SinovadMediaServer.Application.UseCases.TranscodingProcesses;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.HostedService;
using SinovadMediaServer.Infrastructure;
using SinovadMediaServer.Infrastructure.Imdb;
using SinovadMediaServer.Infrastructure.Tmdb;
using SinovadMediaServer.MiddlewareInjector;
using SinovadMediaServer.Persistence.Contexts;
using SinovadMediaServer.Persistence.Interceptors;
using SinovadMediaServer.Persistence.Repositories;
using SinovadMediaServer.SchedulerJob;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using SinovadMediaServer.Transversal.Logger;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Reflection;

namespace SinovadMediaServer
{
    public partial class Form1 : Form
    {
        private string _hubUrl = "https://streamapi.sinovad.com/mediaServerHub";

        private static SharedData _sharedData;

        private static SinovadApiService _sinovadApiService;

        private static MediaServerConfig _mediaServerConfig;

        private static List<String> _listUrls;

        HubConnection _hubConnection;

        public Form1(string[] args)
        {            
            _sharedData = new SharedData();
            _sinovadApiService = new SinovadApiService(_sharedData);
            SetMediaServerConfiguration();
            InitializeComponent();
            ValidateMediaServer();
        }
        private void StartWebServer()
        {
            _hubConnection = new HubConnectionBuilder().WithUrl(_hubUrl).Build();
            _hubConnection.StartAsync();
            _hubConnection.InvokeAsync("AddConnectionToUserClientsGroup",_sharedData.UserData.Guid);
            _sharedData.HubConnection = _hubConnection;
            var builder = WebHost.CreateDefaultBuilder();
            var app = builder
            .UseKestrel()
            .UseUrls(_listUrls.ToArray())
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseWebRoot(Path.Combine("wwwroot"))
            .ConfigureServices((services) =>
            {
                services.AddScoped<AuditableEntitySaveChangesInterceptor>();
                services.AddDbContext<ApplicationDbContext>();
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionScopedJobFactory();
                    // Just use the name of your job that you created in the Jobs folder.
                    var jobKey = new JobKey("CheckFilesToDelete");
                    q.AddJob<BackgroundJob>(opts => opts.WithIdentity(jobKey));

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("CheckFilesToDelete-trigger")
                        //This Cron interval can be described as "run every minute" (when second is zero)
                        //.WithCronSchedule("0 /8 * ? * *")
                        //At minute o past every 3rd hour.
                        .WithCronSchedule("0 * /3 ? * *")
                    );
                });
                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
                services.AddHostedService<MediaServerHostedService>();
                services.AddSingleton<MiddlewareInjectorOptions>();
                services.AddLogging();
                services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));

                //Shared
                services.AddSingleton<SharedData>();
                services.AddAutoMapper(Assembly.GetExecutingAssembly());
                services.AddScoped<SinovadApiService>();
                services.AddScoped<SharedService>();
                //Repositories
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                //Infrastructure
                services.AddScoped<ITmdbService, TmdbService>();
                services.AddScoped<IImdbService, ImdbService>();
                //Services
                services.AddScoped<ITranscoderSettingsService, TranscoderSettingsService>();
                services.AddScoped<ITranscodingProcessService, TranscodingProcessService>();
                services.AddScoped<ILibraryService, LibraryService>();
                services.AddScoped<IAlertService, AlertService>();

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
                  using (var scope = app.ApplicationServices.CreateScope())
                  using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                      context.Database.EnsureCreatedAsync();
                  app.UseCors("AllowAnyOrigin");
                  app.UseHttpsRedirection();
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
                  var transcoderSettingsService = app.ApplicationServices.GetService<ITranscoderSettingsService>();
                  var result=  transcoderSettingsService.GetAsync().Result;
                  if(result.Data == null)
                  {
                      var tsDto = new TranscoderSettingsDto();
                      tsDto.MediaServerId = _sharedData.MediaServerData.Id;
                      tsDto.ConstantRateFactor = 18;
                      tsDto.PresetCatalogId = (int)Catalog.TranscoderPreset;
                      tsDto.PresetCatalogDetailId = (int)TranscoderPreset.Ultrafast;
                      tsDto.VideoTransmissionTypeCatalogId = (int)Catalog.VideoTransmissionType;
                      tsDto.VideoTransmissionTypeCatalogDetailId = (int)VideoTransmissionType.HLS;
                      tsDto.TemporaryFolder = System.IO.Path.GetTempPath();
                      transcoderSettingsService.Save(tsDto);
                      _sharedData.TranscoderSettingsData = tsDto;
                  }else{
                      _sharedData.TranscoderSettingsData = result.Data;
                  }
                  var injectorOptions = app.ApplicationServices.GetService<MiddlewareInjectorOptions>();
                  app.UseMiddlewareInjector(injectorOptions);
                  injectorOptions.InjectMiddleware(app =>
                  {
                      if (_sharedData.TranscoderSettingsData != null)
                      {
                          var fileOptions = new FileServerOptions
                          {
                              FileProvider = new PhysicalFileProvider(_sharedData.TranscoderSettingsData.TemporaryFolder),
                              RequestPath = new PathString("/transcoded"),
                              EnableDirectoryBrowsing = true,
                              EnableDefaultFiles = false
                          };
                          fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
                          app.UseFileServer(fileOptions);
                      }
                  });
                  app.UseStatusCodePagesWithReExecute("/");//to fix angular routing issues
                  app.UseDefaultFiles();
                  app.UseStaticFiles();
                  var sharedData = app.ApplicationServices.GetService<SharedData>();
                  sharedData.ApiToken=_sharedData.ApiToken;
                  sharedData.UserData = _sharedData.UserData;
                  sharedData.MediaServerData = _sharedData.MediaServerData;
                  sharedData.TranscoderSettingsData = _sharedData.TranscoderSettingsData;
                  sharedData.ListPresets=_sharedData.ListPresets;
                  sharedData.WebUrl=_sharedData.WebUrl;
                  sharedData.HubConnection = _sharedData.HubConnection;
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
            var localUrl = "";
            var defaultIpAddress = "";
            var portNumber = "5179";
            if (listFindedIps.Count > 0)
            {
                var fip = listFindedIps[0];
                defaultIpAddress = fip.ToString();
                httpUrl = "http://" + defaultIpAddress + ":"+ portNumber;
                localUrl = "http://127.0.0.1:"+ portNumber;
                _listUrls.Add(httpUrl);
                _listUrls.Add(localUrl);
            }
            _mediaServerConfig.PortNumber = "5179";
            _mediaServerConfig.PublicIpAddress = GetPublicIp();
            _mediaServerConfig.DeviceName= Environment.MachineName;
            _mediaServerConfig.IpAddress = defaultIpAddress;
            UserPrincipal user = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
            String sid = user.Sid.Value;
            _mediaServerConfig.SecurityIdentifier = sid;
            _mediaServerConfig.WebUrl = httpUrl;
            _mediaServerConfig.LocalUrl = localUrl;
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
            var response = await _sinovadApiService.ExecuteHttpMethodAsync<AuthenticateMediaServerResponseDto>(HttpMethodType.POST, "/authentication/AuthenticateMediaServer", _mediaServerConfig.SecurityIdentifier);
            return response;
        }

        public async Task<Response<AuthenticateUserResponseDto>> ValidateUser(string username, string password)
        {
            var accessUserDto = new AccessUserDto();
            accessUserDto.UserName = username;
            accessUserDto.Password = password;
            var response = await _sinovadApiService.ExecuteHttpMethodAsync<AuthenticateUserResponseDto>(HttpMethodType.POST, "/authentication/AuthenticateUser", accessUserDto);
            return response;
        }

        public async Task<bool> SetupMediaServer()
        {
            var resultMediaServer = await SaveMediaServer();
            if (resultMediaServer != null)
            {
                _sharedData.MediaServerData = resultMediaServer;           
                var list = await GetPresets();
                if (list != null && list.Count > 0)
                {
                    _sharedData.ListPresets = list;
                    return true;
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
            var response = await _sinovadApiService.ExecuteHttpMethodAsync<MediaServerDto>(HttpMethodType.PUT, "/mediaServers/Save", mediaServerDto);
            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }

        public async Task<List<CatalogDetailDto>> GetPresets()
        {
            var response = await _sinovadApiService.ExecuteHttpMethodAsync<List<CatalogDetailDto>>(HttpMethodType.GET, "/catalogs/GetDetailsByCatalogAsync/" + (int)Catalog.TranscoderPreset);
            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            else
            {
                return null;
            }
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
            Process.Start(new ProcessStartInfo(_mediaServerConfig.WebUrl + "/settings/server/" + _sharedData.MediaServerData.Guid + "/manage/libraries?apiToken=" + _sharedData.ApiToken) { UseShellExecute = true });
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