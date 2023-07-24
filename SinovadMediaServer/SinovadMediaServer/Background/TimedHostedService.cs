
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Shared;
using System.Text;
using System.Text.Json;

namespace SinovadMediaServer.Background
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private System.Threading.Timer _timer;
        private readonly SharedData _sharedData;
        private SharedService _sharedService;
        public Boolean started = false;

        public TimedHostedService(ILogger<TimedHostedService> logger, SharedService sharedService, SharedData sharedData)
        {
            _logger = logger;
            _sharedData = sharedData;
            _sharedService = sharedService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            if (started == false)
            {
                UpdateMediaServer(MediaServerState.Started);
            }
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new System.Threading.Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
           
            return Task.CompletedTask;
        }

        private void UpdateMediaServer(MediaServerState serverState)
        {
            _sharedService.UpdateMediaServer(serverState);
            if(serverState== MediaServerState.Started)
            {
                started = true;
            }else{
                started = false;
            }
        }

        private void DoWork(object state)
        {
            //if (_sharedData.MediaServerData == null)
            //{
            //    started = false;
            //}else{
            //    if (started == false)
            //    {
            //        UpdateMediaServer(MediaServerState.Started);
            //    }
            //}
            //var count = Interlocked.Increment(ref executionCount);

            //_logger.LogInformation(
            //    "Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            UpdateMediaServer(MediaServerState.Stopped);

            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            UpdateMediaServer(MediaServerState.Stopped);
        }
    }
}
