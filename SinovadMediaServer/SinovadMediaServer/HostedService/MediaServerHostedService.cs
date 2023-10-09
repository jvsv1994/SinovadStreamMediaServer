using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;
using Timer = System.Threading.Timer;

namespace SinovadMediaServer.HostedService
{
    public class MediaServerHostedService : IHostedService, IDisposable
    {
        private Timer _timer;

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        private readonly IAppLogger<MediaServerHostedService> _logger;

        private readonly IAlertService _alertService;

        //private string _hubUrl = "http://localhost:53363/mediaServerHub";

        private string _hubUrl = "https://streamapi.sinovad.com/mediaServerHub";

        public MediaServerHostedService(SharedData sharedData, SharedService sharedService, IAppLogger<MediaServerHostedService> logger, IAlertService alertService)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
            _logger = logger;
            _alertService = alertService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _alertService.Create("Inicio del servicio alojado", AlertType.Bullhorn);
            _sharedData.HubConnection = new HubConnectionBuilder().WithUrl(_hubUrl).WithAutomaticReconnect().Build();
            _sharedData.HubConnection.Closed += async (error) =>
            {
                await _alertService.Create("Se cerró la conexión a Signal IR - " + (error!=null?error.Message:""), AlertType.Bullhorn);
            };
            _sharedData.HubConnection.Reconnecting += async (error) =>
            {
                await _alertService.Create("Reconectando a Signal IR", AlertType.Bullhorn);
            };
            _sharedData.HubConnection.Reconnected += async (connectionId) =>
            {
                await _alertService.Create($"Reconexión satisfactoria a Signal IR. El Id de la conexión es: {connectionId}", AlertType.Bullhorn);
                try
                {
                    //await DeleteAllTranscodedMediaFiles();
                    System.Threading.Thread.Sleep(1000);
                    RemoveHubHandlerMethods();
                    System.Threading.Thread.Sleep(1000);
                    await _sharedData.HubConnection.InvokeAsync("AddConnectionToUserClientsGroup", _sharedData.UserData.Guid);
                    AddHubHandlerMethods();
                }catch (Exception exception)
                {
                    _logger.LogError(exception.Message);
                }
            };
            await TryStartHubConnection(cancellationToken);
            _timer = new Timer(UpdateMediaServerConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void RemoveHubHandlerMethods()
        {
            _alertService.Create("Eliminando controladores para los métodos del Hub ", AlertType.Bullhorn);
            _sharedData.HubConnection.Remove("UpdateCurrentTimeMediaFilePlayBack");
            _sharedData.HubConnection.Remove("RemoveMediaFilePlayBack");
            _sharedData.HubConnection.Remove("RemoveLastTranscodedMediaFileProcess");
            _sharedData.HubConnection.Remove("UpdateCurrentTimeMediaFilePlayBack");
        }


        public async Task TryStartHubConnection(CancellationToken cancellationToken)
        {
            try
            {
                await _alertService.Create("Intentando conectarse a Signal IR ", AlertType.Bullhorn);
                await _sharedData.HubConnection.StartAsync(cancellationToken);
                await _alertService.Create($"Conexión satisfactoria a Signal IR. El Id de la conexión es: {_sharedData.HubConnection.ConnectionId}", AlertType.Bullhorn);
                System.Threading.Thread.Sleep(1000);
                RemoveHubHandlerMethods();
                System.Threading.Thread.Sleep(1000);
                await _sharedData.HubConnection.InvokeAsync("AddConnectionToUserClientsGroup", _sharedData.UserData.Guid);
                AddHubHandlerMethods();
            }catch (Exception e){
                await _alertService.Create("Error al intentar conectarse a Signal IR - "+e.Message, AlertType.Bullhorn);
                System.Threading.Thread.Sleep(1000);
                await TryStartHubConnection(cancellationToken);
            }
        }

        public void AddHubHandlerMethods()
        {
            _alertService.Create("Agregando controladores para los métodos del Hub ", AlertType.Bullhorn);
            _sharedData.HubConnection.On("UpdateCurrentTimeMediaFilePlayBack", (string mediaServerGuid, string mediaFilePlaybackGuid, double currentTime, bool isPlaying) =>
            {
                if (mediaServerGuid == _sharedData.MediaServerData.Guid.ToString())
                {
                    var mediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == mediaFilePlaybackGuid).FirstOrDefault();
                    if (mediaFilePlayback != null)
                    {
                        mediaFilePlayback.ClientData.CurrentTime = currentTime;
                        mediaFilePlayback.ClientData.IsPlaying = isPlaying;
                    }
                }
            });
            _sharedData.HubConnection.On("RemoveMediaFilePlayBack", (string mediaServerGuid, string mediaFilePlaybackGuid) =>
            {
                if (_sharedData.MediaServerData.Guid.ToString() == mediaServerGuid)
                {
                    var mediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == mediaFilePlaybackGuid).FirstOrDefault();
                    if (mediaFilePlayback != null)
                    {
                        _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                        _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayBack", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                    }
                }
            });
            _sharedData.HubConnection.On("RemoveLastTranscodedMediaFileProcess", (string mediaServerGuid, string mediaFilePlaybackGuid) =>
            {
                if (_sharedData.MediaServerData.Guid.ToString() == mediaServerGuid)
                {
                    var MediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == mediaFilePlaybackGuid).FirstOrDefault();
                    if (MediaFilePlayback != null)
                    {
                        _sharedService.KillProcessAndRemoveDirectory(MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    }
                }
            });
        }

        public Task DeleteAllTranscodedMediaFiles()
        {
            try
            {
                _alertService.Create("Eliminando todos los archivos transcodificados ", AlertType.Bullhorn);
                foreach (var mediaFilePlayback in _sharedData.ListMediaFilePlayback)
                {
                    _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                    _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
            return Task.CompletedTask;
        }

        private void UpdateMediaServerConnection(object state) {
            if (_sharedData.UserData!=null && _sharedData.MediaServerData!=null)
            {
               _sharedData.HubConnection.InvokeAsync("UpdateMediaServerLastConnection", _sharedData.UserData.Guid.ToString(), _sharedData.MediaServerData.Guid.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _alertService.Create("Se detuvo el servicio alojado", AlertType.Bullhorn);
            DeleteAllTranscodedMediaFiles();
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
