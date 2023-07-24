using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Common;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Proxy;

namespace SinovadMediaServer.Shared
{
    public  class SharedService
    {

        private SharedData _sharedData;

        public MiddlewareInjectorOptions _middlewareInjectorOptions;

        private readonly RestService _restService;

        public SharedService(SharedData sharedData, MiddlewareInjectorOptions middlewareInjectorOptions, RestService restService)
        {
            _sharedData = sharedData;
            _middlewareInjectorOptions = middlewareInjectorOptions;
            _restService= restService;
        }

   

        public void Get(MediaServerState serverState)
        {

            //mediaServer.StateCatalogDetailId = (int)serverState;
            //mediaServer.Url = _config.Value.WebUrl;
            //var restService = new RestService<Object>(_config, _sharedData);
            //try
            //{
            //    var res = restService.ExecuteHttpMethodAsync(HttpMethodType.PUT, "/mediaServers/Update", mediaServer).Result;
            //}
            //catch (Exception ex) {
            //    Console.WriteLine(ex.Message);
            //}
        }

        public void UpdateMediaServer(MediaServerState serverState)
        {

                //mediaServer.StateCatalogDetailId = (int)serverState;
                //mediaServer.Url = _config.Value.WebUrl;
                //var restService = new RestService<Object>(_config, _sharedData);
                //try
                //{
                //    var res = restService.ExecuteHttpMethodAsync(HttpMethodType.PUT, "/mediaServers/Update", mediaServer).Result;
                //}
                //catch (Exception ex) {
                //    Console.WriteLine(ex.Message);
                //}
        }

        public async void InjectTranscodeMiddleware()
        {
        //    _middlewareInjectorOptions.InjectMiddleware(app =>
        //    {      
        //        var fileOptions = new FileServerOptions
        //        {
        //            FileProvider = new PhysicalFileProvider(_sharedData.TranscoderSettingsData.TemporaryFolder),
        //            RequestPath = new PathString("/transcoded"),
        //            EnableDirectoryBrowsing = true,
        //            EnableDefaultFiles = false
        //        };
        //        fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
        //        app.UseFileServer(fileOptions);              
        //    });
        //    using (var httpClient = new HttpClient())
        //    {
        //        var content = new FormUrlEncodedContent(new Dictionary<string, string>());
        //        await httpClient.PostAsync(_config.Value.WebUrl + "/Main/StartApplication", content);
        //    }
        }

    }
}
