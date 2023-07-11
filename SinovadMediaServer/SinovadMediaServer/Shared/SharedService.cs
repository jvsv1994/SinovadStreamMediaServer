using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Proxy;

namespace SinovadMediaServer.Shared
{
    public  class SharedService
    {
        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public MiddlewareInjectorOptions _middlewareInjectorOptions;

        public SharedService(IOptions<MyConfig> config, SharedData sharedData, MiddlewareInjectorOptions middlewareInjectorOptions)
        {
            _config = config;
            _sharedData = sharedData;
            _middlewareInjectorOptions = middlewareInjectorOptions;
        }

        public void UpdateAccountServer(ServerState serverState)
        {
            var accountServer = _sharedData.hostData.accountServer;
            accountServer.StateCatalogDetailId = (int)serverState;
            accountServer.HostUrl = _config.Value.WebUrl;
            var restService = new RestService<Object>(_config, _sharedData);
            var res=restService.ExecuteHttpMethodAsync(HttpMethodType.PUT, "/accountServers/Update", accountServer).Result; 
        }

        public void InjectTranscodeMiddleware()
        {
            _middlewareInjectorOptions.InjectMiddleware(app =>
            {
                if (_sharedData.hostData != null)
                {
                    var restService = new RestService<TranscodeSettingDto>(_config, _sharedData);
                    var transcodeSetting = restService.ExecuteHttpMethodAsync(HttpMethodType.GET, "/transcodeSettings/GetByAccountServerAsync/" + _sharedData.hostData.accountServerId).Result;
                    if (transcodeSetting != null)
                    {
                        var fileOptions = new FileServerOptions
                        {
                            FileProvider = new PhysicalFileProvider(transcodeSetting.DirectoryPhysicalPath),
                            RequestPath = new PathString("/transcoded"),
                            EnableDirectoryBrowsing = true,
                            EnableDefaultFiles = false
                        };
                        fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
                        app.UseFileServer(fileOptions);
                    }
                }
            });
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>());
                httpClient.PostAsync(_config.Value.WebUrl + "/Main/StartApplication", content);
            }
        }

        public void CheckAndRegisterGenres()
        {
            if (_sharedData.hostData != null)
            {
                var restService = new RestService<Object>(_config, _sharedData);
                var res=restService.ExecuteHttpMethodAsync(HttpMethodType.POST, "/genres/CheckAndRegisterGenres");
            }
        }

    }
}
