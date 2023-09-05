using Microsoft.AspNetCore.SignalR.Client;
using Quartz;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Mapping;
using System.Net.NetworkInformation;

namespace SinovadMediaServer.SchedulerJob
{
    public class PingClientsJob : IJob
    {

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        public PingClientsJob(SharedData sharedData, SharedService sharedService)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var listMediaFilePlaybacks= _sharedData.ListMediaFilePlayback.MapTo<List<MediaFilePlaybackDto>>();
            if(listMediaFilePlaybacks != null && listMediaFilePlaybacks.Count>0)
            {
                foreach (var mediaFilePlayback in listMediaFilePlaybacks)
                {
                    using var ping = new Ping();
                    string url = mediaFilePlayback.ClientData.LocalIpAddress;
                    PingReply res = ping.Send(url);
                    if(res.Status==IPStatus.TimedOut)
                    {
                        _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.RemoveAll(x=>x.Guid==mediaFilePlayback.Guid);
                        await _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayBack", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                    }
                }
            }
        }
    }
}
