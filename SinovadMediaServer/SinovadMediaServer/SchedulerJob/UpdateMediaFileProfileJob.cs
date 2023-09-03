using Quartz;
using SinovadMediaServer.Application.Interface.UseCases;
using System.Net.NetworkInformation;

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
            using var ping = new Ping();
            string url = "example.com";
            PingReply res = ping.Send(url);
            Console.WriteLine(res.Status);
            _mediaFilePlaybackService.UpdateAllMediaFileProfile();
        }
    }
}
