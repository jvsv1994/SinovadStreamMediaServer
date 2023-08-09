using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
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

        public void UpdateVideosInListLibraries(UpdateLibraryVideosDto mediaRequest)
        {
            if (mediaRequest.LogIdentifier == null)
            {
                mediaRequest.LogIdentifier = System.Guid.NewGuid().ToString();
            }
            IEnumerable<string> filesToAdd = new List<string>();

            foreach (var library in mediaRequest.ListLibraries)
            {
                var listPaths = Directory.GetFiles(library.PhysicalPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mkv") || s.EndsWith(".mp4") || s.EndsWith(".avi")).ToList();
                library.ListPaths = listPaths;
            } 
            var res = _restService.ExecuteHttpMethodAsync<object>(HttpMethodType.POST, "/videos/UpdateVideosInListLibraries", mediaRequest).Result;
        }

    }
}
