using SinovadMediaServer.Configuration;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using Microsoft.Extensions.Options;
using Quartz;
using SinovadMediaServer.Proxy;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {
        public static IOptions<MyConfig> _config { get; set; }
        private SharedData _sharedData { get; set; }

        private readonly RestService _restService;

        public BackgroundJob(IOptions<MyConfig> config, SharedData sharedData, RestService restService)
        {
            _config = config;
            _sharedData = sharedData;
            _restService = restService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var genericStrategy = new GenericStrategy(_config, _sharedData, _restService);
            genericStrategy.deleteOldTranscodeVideoProcess();
        }
    }
}
