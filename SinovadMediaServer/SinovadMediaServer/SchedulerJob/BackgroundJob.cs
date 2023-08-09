using Quartz;
using SinovadMediaServer.Infrastructure;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {
        private readonly SharedData _sharedData;

        private readonly SinovadApiService _sinovadApiService;

        public BackgroundJob(SharedData sharedData, SinovadApiService restService)
        {
            _sharedData = sharedData;
            _sinovadApiService = restService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var genericStrategy = new TranscodeProcessStrategy(_sharedData,_sinovadApiService);
            await genericStrategy.DeleteOldTranscodeVideoProcess();
        }
    }
}
