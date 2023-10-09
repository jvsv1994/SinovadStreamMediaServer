using Microsoft.AspNetCore.SignalR.Client;
using Quartz;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Mapping;
using System.Net.NetworkInformation;

namespace SinovadMediaServer.SchedulerJob
{
    public class PingClientsJob : IJob
    {

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        private readonly IAlertService _alertService;

        public PingClientsJob(SharedData sharedData, SharedService sharedService, IAlertService alertService)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
            _alertService = alertService;
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
                        await _alertService.Create("No se obtuvo conexion con el cliente "+ mediaFilePlayback.ClientData.LocalIpAddress,AlertType.Bullhorn);
                        var itemTitle = mediaFilePlayback.ItemData.Subtitle!=null ? mediaFilePlayback.ItemData.Title + " " + mediaFilePlayback.ItemData.Subtitle : mediaFilePlayback.ItemData.Title;
                        await _alertService.Create("Eliminando playback de " + itemTitle +" reproducido por el cliente "+mediaFilePlayback.ClientData.LocalIpAddress, AlertType.Bullhorn);
                        _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.RemoveAll(x=>x.Guid==mediaFilePlayback.Guid);
                        await _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayBack", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                    }
                }
            }
        }
    }
}
