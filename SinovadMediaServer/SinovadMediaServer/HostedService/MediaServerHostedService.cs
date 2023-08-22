using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using SinovadMediaServer.Shared;
using Timer = System.Threading.Timer;

namespace SinovadMediaServer.HostedService
{
    public class MediaServerHostedService : IHostedService, IDisposable
    {
        private Timer _timer;

        private readonly SharedData _sharedData;


        public MediaServerHostedService(SharedData sharedData)
        {
            _sharedData = sharedData;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdateMediaServerConnection,null,TimeSpan.Zero,TimeSpan.FromSeconds(3));
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
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
