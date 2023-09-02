using Quartz;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Shared;

namespace SinovadMediaServer.SchedulerJob
{
    public class UpdateMediaFileProfileJob : IJob
    {

        private readonly IMediaFilePlaybackService _mediaFilePlaybackService;


        public UpdateMediaFileProfileJob(IMediaFilePlaybackService mediaFilePlaybackService)
        {
            _mediaFilePlaybackService = mediaFilePlaybackService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _mediaFilePlaybackService.UpdateAllMediaFileProfile();
        }
    }
}
