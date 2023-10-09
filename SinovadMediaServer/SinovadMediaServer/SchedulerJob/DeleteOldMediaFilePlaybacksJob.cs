using Microsoft.AspNetCore.SignalR.Client;
using Quartz;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;

namespace SinovadMediaServer.SchedulerJob
{
    public class DeleteOldMediaFilePlaybacksJob : IJob
    {

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        private readonly IAppLogger<DeleteOldMediaFilePlaybacksJob> _logger;

        private readonly IAlertService _alertService;

        public DeleteOldMediaFilePlaybacksJob(SharedData sharedData, SharedService sharedService, IAppLogger<DeleteOldMediaFilePlaybacksJob> logger, IAlertService alertService)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
            _logger = logger;
            _alertService = alertService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DeleteOldTranscodedMediaFiles();
        }

        public void DeleteOldTranscodedMediaFiles()
        {
            try
            {
                List<MediaFilePlaybackDto> listMediaFilePlaybackForDelete = new List<MediaFilePlaybackDto>();
                foreach (var mediaFilePlayback in _sharedData.ListMediaFilePlayback)
                {
                    bool forceDelete = CheckIfDeleteTranscodedFile(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    if (forceDelete)
                    {
                        var itemTitle = mediaFilePlayback.ItemData.Subtitle != null ? mediaFilePlayback.ItemData.Title + " " + mediaFilePlayback.ItemData.Subtitle : mediaFilePlayback.ItemData.Title;
                        _alertService.Create("Forzando eliminacion de playback antiguo de " + itemTitle + " reproducido por el cliente " + mediaFilePlayback.ClientData.LocalIpAddress, AlertType.Bullhorn);
                        _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                        _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                    }
                }
            }catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        private bool CheckIfDeleteTranscodedFile(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
        {
            var forceDelete = false;
            var currentMilisecond = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var tvpMilisecond = mediaFilePlaybackTranscodingProcess.Created.Ticks / TimeSpan.TicksPerMillisecond;
            if (currentMilisecond - tvpMilisecond > 86400000)
            {
                forceDelete = true;
            }
            return forceDelete;
        }

    }
}
