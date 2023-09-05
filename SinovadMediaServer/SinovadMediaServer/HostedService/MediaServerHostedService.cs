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

        public MediaServerHostedService(SharedData sharedData, SharedService sharedService, IAppLogger<MediaServerHostedService> logger, IAlertService alertService)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
            _logger = logger;
            _alertService = alertService;
        }

        public async Task RetryHubConnection(CancellationToken cancellationToken)
        {
            try
            {
                System.Threading.Thread.Sleep(5000);
                await _alertService.Create("Reintentando la conexión en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
                _logger.LogInformation("Hub Connection Before Start Again");
                await TryStartHubConnection(cancellationToken);
                _logger.LogInformation("Hub Connection After Start Again");
                await _alertService.Create("Conexión reestablecida en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error - "+ exception.Message);
                await RetryHubConnection(cancellationToken);
            }
        }

        public Task RemoveHubHandlerMethods(CancellationToken cancellationToken)
        {
            _sharedData.HubConnection.Remove("UpdateCurrentTimeMediaFilePlayBack");
            _sharedData.HubConnection.Remove("RemoveMediaFilePlayBack");
            _sharedData.HubConnection.Remove("RemoveLastTranscodedMediaFileProcess");
            _sharedData.HubConnection.Remove("UpdateCurrentTimeMediaFilePlayBack");
            _logger.LogInformation("Remove Hub Handler Methods");
            return Task.CompletedTask;
        }


        public Task TryStartHubConnection(CancellationToken cancellationToken)
        {
            _sharedData.HubConnection.StartAsync();
            _sharedData.HubConnection.InvokeAsync("AddConnectionToUserClientsGroup", _sharedData.UserData.Guid);
            _sharedData.HubConnection.Closed += async (error) =>
            {
                await _alertService.Create("Se cerró la conexión en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
                try
                {
                    _logger.LogInformation("Delete All Transcode Video Process");
                    DeleteAllTranscodedMediaFiles();
                    System.Threading.Thread.Sleep(1000);
                    await RemoveHubHandlerMethods(cancellationToken);
                    await RetryHubConnection(cancellationToken);
                }catch (Exception exception){
                    _logger.LogError(exception.Message);
                }
            };
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
            _sharedData.HubConnection.Reconnected += async (res) =>
            {
                _logger.LogInformation("Hub Connection Reconnected");
            };
            _sharedData.HubConnection.Reconnecting += async (res) =>
            {
                _logger.LogInformation("Hub Connection Reconnecting");
            };
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted Service Start");
            _alertService.Create("Conexión establecida en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
            TryStartHubConnection(cancellationToken);
            _timer = new Timer(UpdateMediaServerConnection,null,TimeSpan.Zero,TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        public void DeleteAllTranscodedMediaFiles()
        {
            try
            {
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
        }

        private void UpdateMediaServerConnection(object state) {
            if (_sharedData.UserData!=null && _sharedData.MediaServerData!=null)
            {
                _sharedData.HubConnection.InvokeAsync("UpdateMediaServerLastConnection", _sharedData.UserData.Guid.ToString(), _sharedData.MediaServerData.Guid.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _alertService.Create("Se detuvo la conexión en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
            _logger.LogInformation("Hosted Service Stop");
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
