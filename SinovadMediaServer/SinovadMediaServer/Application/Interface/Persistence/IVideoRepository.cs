using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IVideoRepository : IGenericRepository<Video>
    {
        List<Video> GetVideoByLibraryId(int libraryId);

        //object GetVideosByTvSerieAndUser(int tvSerieId, int userId);
    }
}
