using Quartz;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {
        private SharedData _sharedData { get; set; }

        private readonly RestService _restService;

        public BackgroundJob(SharedData sharedData, RestService restService)
        {
            _sharedData = sharedData;
            _restService = restService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var genericStrategy = new GenericStrategy(_sharedData, _restService);
            genericStrategy.deleteOldTranscodeVideoProcess();
        }
    }
}
