using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;
using Timer = System.Threading.Timer;

namespace SinovadMediaServer.HostedService
{
    public class MediaServerHostedService : IHostedService, IDisposable
    {
        private Timer _timer;

        private readonly SharedData _sharedData;

        private readonly IAppLogger<MediaServerHostedService> _logger;

        private readonly ITranscodingProcessService _transcodingProcessService;

        public MediaServerHostedService(SharedData sharedData,IAppLogger<MediaServerHostedService> logger, ITranscodingProcessService transcodingProcessService)
        {
            _sharedData = sharedData;
            _logger = logger;
            _transcodingProcessService = transcodingProcessService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted Service Start");
            _sharedData.HubConnection.Closed += async (error) =>
            {
                try
                {
                    _logger.LogInformation("Delete All Transcode Video Process");
                    await _transcodingProcessService.DeleteAllTranscodeVideoProcess();
                    _logger.LogInformation("Hub Connection Closed");
                    System.Threading.Thread.Sleep(5000);
                    _logger.LogInformation("Hub Connection Before Start Again");
                    await _sharedData.HubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Hub Connection After Start Again");

                }catch(Exception exception)
                {
                    _logger.LogError(exception.Message);
                }
            };
            _sharedData.HubConnection.Reconnected += async (res) =>
            {
               _logger.LogInformation("Hub Connection Reconnected");    
            };
            _sharedData.HubConnection.Reconnecting += async (res) =>
            {
                _logger.LogInformation("Hub Connection Reconnecting");
            };
            _timer = new Timer(UpdateMediaServerConnection,null,TimeSpan.Zero,TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        private void UpdateMediaServerConnection(object state) {
            if (_sharedData.UserData!=null && _sharedData.MediaServerData!=null)
            {
                _sharedData.HubConnection.InvokeAsync("UpdateMediaServerLastConnection", _sharedData.UserData.Guid.ToString(), _sharedData.MediaServerData.Guid.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted Service Stop");
            _transcodingProcessService.DeleteAllTranscodeVideoProcess();
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
