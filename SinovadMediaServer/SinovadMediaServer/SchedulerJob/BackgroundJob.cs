using Quartz;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.SchedulerJob
{
    public class BackgroundJob : IJob
    {

        private readonly ITranscodingProcessService _transcodingProcessService;

        public BackgroundJob(ITranscodingProcessService transcodingProcessService)
        {
            _transcodingProcessService = transcodingProcessService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _transcodingProcessService.DeleteOldTranscodeVideoProcess();
        }
    }
}
