using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;

namespace SinovadMediaServer.Strategies
{
    public class SearchVideoFilesStrategy
    {   

        private readonly RestService _restService;

        public SearchVideoFilesStrategy(RestService restService)
        {
            _restService= restService;
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
            var res = _restService.ExecuteHttpMethodAsync<object>(HttpMethodType.POST, "/videos/UpdateVideosInListStorages", mediaRequest).Result;
        }

    }
}
