using SinovadMediaServer.Configuration;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using Microsoft.Extensions.Options;
using Quartz;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {
        public static IOptions<MyConfig> _config { get; set; }
        private SharedData _sharedData { get; set; }

        public BackgroundJob(IOptions<MyConfig> config, SharedData sharedData)
        {
            _config = config;
            _sharedData = sharedData;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var genericStrategy = new GenericStrategy(_config, _sharedData);
            genericStrategy.deleteOldTranscodeVideoProcess();
        }
    }
}
