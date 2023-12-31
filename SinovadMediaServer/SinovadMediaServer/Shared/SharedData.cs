﻿using Microsoft.AspNetCore.SignalR.Client;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.DTOs.MediaServer;

#nullable disable

namespace SinovadMediaServer.Shared
{
    public class SharedData
    {
        public String WebUrl { get; set; }
        public String ApiToken { get; set; }
        public MediaServerDto MediaServerData { get; set; }
        public TranscoderSettingsDto TranscoderSettingsData { get; set; }
        public UserDto UserData { get; set; }
        public List<CatalogDetailDto> ListPresets { get; set; }
        public HubConnection HubConnection { get; set; }
        public List<MediaFileDto> ListMediaFiles { get; set; }
        public List<MediaFilePlaybackDto> ListMediaFilePlayback { get; set; }

    }
}
