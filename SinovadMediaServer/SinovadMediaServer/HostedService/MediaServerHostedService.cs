﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Timer = System.Threading.Timer;

namespace SinovadMediaServer.HostedService
{
    public class MediaServerHostedService : IHostedService, IDisposable
    {
        private Timer _timer;

        private readonly SharedData _sharedData;

        private readonly IAppLogger<MediaServerHostedService> _logger;

        private readonly IMediaFilePlaybackService _mediaFilePlaybackService;

        private readonly IAlertService _alertService;

        public MediaServerHostedService(SharedData sharedData, IAppLogger<MediaServerHostedService> logger, IMediaFilePlaybackService mediaFilePlaybackService, IAlertService alertService)
        {
            _sharedData = sharedData;
            _logger = logger;
            _mediaFilePlaybackService = mediaFilePlaybackService;
            _alertService = alertService;
        }

        public async Task RetryHubConnection(CancellationToken cancellationToken)
        {
            try
            {
                System.Threading.Thread.Sleep(5000);
                await _alertService.Create("Reintentando la conexión en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
                _logger.LogInformation("Hub Connection Before Start Again");
                await _sharedData.HubConnection.StartAsync(cancellationToken);
                _logger.LogInformation("Hub Connection After Start Again");
                await _alertService.Create("Conexión reestablecida en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error - "+ exception.Message);
                await RetryHubConnection(cancellationToken);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
             _alertService.Create("Conexión establecida en " + (_sharedData.MediaServerData.FamilyName!=null?_sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName),AlertType.Bullhorn);
            _logger.LogInformation("Hosted Service Start");
            _sharedData.HubConnection.Closed += async (error) =>
            {
                await _alertService.Create("Se cerró la conexión en " + (_sharedData.MediaServerData.FamilyName != null ? _sharedData.MediaServerData.FamilyName : _sharedData.MediaServerData.DeviceName), AlertType.Bullhorn);
                try
                {
                    _logger.LogInformation("Delete All Transcode Video Process");
                    DeleteAllTranscodedMediaFiles();
                    _logger.LogInformation("Hub Connection Closed");
                    await RetryHubConnection(cancellationToken);
                }
                catch(Exception exception)
                {
                    _logger.LogError(exception.Message);
                }
            };
            _sharedData.HubConnection.On("UpdateCurrentTimeMediaFilePlayback", (string mediaServerGuid, string MediaFilePlaybackGuid,double currentTime,bool isPlaying) =>
            {
                if(mediaServerGuid==_sharedData.MediaServerData.Guid.ToString())
                {
                    var mediaFilePlayback=_sharedData.ListMediaFilePlayback.Where(x => x.Guid == MediaFilePlaybackGuid).FirstOrDefault();
                    if(mediaFilePlayback!=null) {
                       mediaFilePlayback.ClientData.CurrentTime= currentTime;
                       mediaFilePlayback.ClientData.IsPlaying=isPlaying;
                    }
                }
            });
            _sharedData.HubConnection.On("RemoveMediaFilePlayback", (string mediaServerGuid, string MediaFilePlaybackGuid) =>
            {
                if(_sharedData.MediaServerData.Guid.ToString()==mediaServerGuid)
                {
                    var MediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == MediaFilePlaybackGuid).FirstOrDefault();
                    if (MediaFilePlayback != null)
                    {
                        _mediaFilePlaybackService.KillProcessAndRemoveDirectory(MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.Remove(MediaFilePlayback);
                    }
                }
            });
            _sharedData.HubConnection.On("RemoveLastTranscodedMediaFileProcess", (string mediaServerGuid, string MediaFilePlaybackGuid) =>
            {
                if (_sharedData.MediaServerData.Guid.ToString() == mediaServerGuid)
                {
                    var MediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == MediaFilePlaybackGuid).FirstOrDefault();
                    if (MediaFilePlayback != null)
                    {
                        _mediaFilePlaybackService.KillProcessAndRemoveDirectory(MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
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
            _timer = new Timer(UpdateMediaServerConnection,null,TimeSpan.Zero,TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        public void DeleteAllTranscodedMediaFiles()
        {
            try
            {
                foreach (var MediaFilePlayback in _sharedData.ListMediaFilePlayback)
                {
                    _mediaFilePlaybackService.KillProcessAndRemoveDirectory(MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    _sharedData.ListMediaFilePlayback.Remove(MediaFilePlayback);
                }
                _sharedData.ListMediaFilePlayback.Clear();
            }catch (Exception ex)
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
