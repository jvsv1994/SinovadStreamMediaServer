﻿
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
        public MiddlewareInjectorOptions _middlewareInjectorOptions;
        public Boolean started = false;
        private IOptions<MyConfig> _config;

        public TimedHostedService(ILogger<TimedHostedService> logger, SharedService sharedService, SharedData sharedData, MiddlewareInjectorOptions middlewareInjectorOptions, IOptions<MyConfig> config)
        {
            _config = config;
            _logger = logger;
            _sharedData = sharedData;
            _sharedService = sharedService;
            _middlewareInjectorOptions = middlewareInjectorOptions;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            if (started == false)
            {
                UpdateAccountServer(ServerState.Started);
            }
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new System.Threading.Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
           
            return Task.CompletedTask;
        }

        private void UpdateAccountServer(ServerState serverState)
        {
            if (_sharedData.hostData != null && _sharedData.hostData.accountServer!=null)
            {
                _sharedService.UpdateAccountServer(serverState);
                if(serverState==ServerState.Started)
                {
                    started = true;
                }else{
                    started = false;
                }
            }
        }

        private void DoWork(object state)
        {
            if (_sharedData.hostData == null)
            {
                started = false;
            }else{
                if (started == false)
                {
                    UpdateAccountServer(ServerState.Started);
                }
            }
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            UpdateAccountServer(ServerState.Stopped);

            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            UpdateAccountServer(ServerState.Stopped);
        }
    }
}
