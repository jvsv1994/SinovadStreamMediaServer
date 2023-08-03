using Quartz;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {
        private readonly SharedData _sharedData;

        private readonly RestService _restService;

        public BackgroundJob(SharedData sharedData, RestService restService)
        {
            _sharedData = sharedData;
            _restService = restService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var genericStrategy = new TranscodeProcessStrategy(_sharedData,_restService);
            await genericStrategy.DeleteOldTranscodeVideoProcess();
        }
    }
}
