using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;

namespace SinovadMediaServer.Strategies
{
    public class SearchVideoFilesStrategy
    {   

        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public SearchVideoFilesStrategy(IOptions<MyConfig> config, SharedData sharedData)
        {
            _config = config;
            _sharedData = sharedData;
        }

        public void UpdateVideosInListStorages(UpdateStorageVideosDto mediaRequest)
        {
            if (mediaRequest.LogIdentifier == null)
            {
                mediaRequest.LogIdentifier = System.Guid.NewGuid().ToString();
            }
            IEnumerable<string> filesToAdd = new List<string>();

            foreach (var storage in mediaRequest.ListStorages)
            {
                var listPaths = Directory.GetFiles(storage.PhysicalPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mkv") || s.EndsWith(".mp4") || s.EndsWith(".avi")).ToList();
                storage.ListPaths = listPaths;
            } 
            var restService = new RestService<Object>(_config, _sharedData);
            var res = restService.ExecuteHttpMethodAsync(HttpMethodType.POST, "/videos/UpdateVideosInListStorages", mediaRequest).Result;
        }

    }
}
