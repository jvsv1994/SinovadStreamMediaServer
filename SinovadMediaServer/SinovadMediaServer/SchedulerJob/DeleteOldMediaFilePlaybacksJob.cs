using Quartz;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.SchedulerJob
{
    public class DeleteOldMediaFilePlaybacksJob : IJob
    {

        private readonly IMediaFilePlaybackService _mediaFilePlaybackService;

        public DeleteOldMediaFilePlaybacksJob(IMediaFilePlaybackService mediaFilePlaybackService)
        {
            _mediaFilePlaybackService = mediaFilePlaybackService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _mediaFilePlaybackService.DeleteOldTranscodedMediaFiles();
        }
    }
}
