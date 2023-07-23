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
        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public MiddlewareInjectorOptions _middlewareInjectorOptions;

        private readonly RestService _restService;

        public SharedService(IOptions<MyConfig> config, SharedData sharedData, MiddlewareInjectorOptions middlewareInjectorOptions, RestService restService)
        {
            _config = config;
            _sharedData = sharedData;
            _middlewareInjectorOptions = middlewareInjectorOptions;
            _restService= restService;
        }

        public async Task<Response<AuthenticateMediaServerResponseDto>> AuthenticateMediaServer()
        {
            var response= await _restService.ExecuteHttpMethodAsync<AuthenticateMediaServerResponseDto>(HttpMethodType.POST, "/authentication/AuthenticateMediaServer", _config.Value.SecurityIdentifier);         
            return response;
        }

        public async Task<Response<AuthenticateUserResponseDto>> ValidateUser(string username,string password)
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
            if (_sharedData.UserData!=null)
            {
                mediaServerDto.UserId = _sharedData.UserData.Id;
            }
            mediaServerDto.SecurityIdentifier= _config.Value.SecurityIdentifier;
            mediaServerDto.DeviceName=_config.Value.DeviceName;
            mediaServerDto.Url = _config.Value.WebUrl;
            mediaServerDto.IpAddress = _config.Value.IpAddress;
            mediaServerDto.Port = int.Parse(_config.Value.PortNumber);
            mediaServerDto.PublicIpAddress = _config.Value.PublicIpAddress;
            var response = await _restService.ExecuteHttpMethodAsync<MediaServerDto>(HttpMethodType.PUT, "/mediaServers/Save", mediaServerDto);
            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }


        public async Task<TranscoderSettingsDto> GetTranscoderSettings()
        {
       
            var response = await _restService.ExecuteHttpMethodAsync<TranscoderSettingsDto>(HttpMethodType.GET, "/transcoderSettings/GetByMediaServerAsync/"+this._sharedData.MediaServerData.Id);
            if (response.IsSuccess && response.Data!=null)
            {
                return response.Data;
            }else
            {
               return await CreateTranscoderSettings();
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
            _middlewareInjectorOptions.InjectMiddleware(app =>
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
            });
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>());
                await httpClient.PostAsync(_config.Value.WebUrl + "/Main/StartApplication", content);
            }
        }

    }
}
