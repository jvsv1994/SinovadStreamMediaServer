using Microsoft.AspNetCore.SignalR.Client;
using Quartz;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;

namespace SinovadMediaServer.SchedulerJob
{
    public class DeleteOldMediaFilePlaybacksJob : IJob
    {

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        private readonly IAppLogger<DeleteOldMediaFilePlaybacksJob> _logger;

        public DeleteOldMediaFilePlaybacksJob(SharedData sharedData, IAppLogger<DeleteOldMediaFilePlaybacksJob> logger)
        {
            _sharedData = sharedData;
            _logger = logger;
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
                        _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                        _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                    }
                }
            }
            catch (Exception ex)
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
