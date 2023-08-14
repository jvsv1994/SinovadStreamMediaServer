using Microsoft.AspNetCore.SignalR;

namespace SinovadMediaServer.SignailIR
{
    public class CustomHub : Hub
    {
        public async Task RefreshLibraries()
        {
            await Clients.All.SendAsync("RefreshLibraries");
        }
        public async Task RefreshMediaItems()
        {
            await Clients.All.SendAsync("RefreshMediaItems");
        }
    }
}
