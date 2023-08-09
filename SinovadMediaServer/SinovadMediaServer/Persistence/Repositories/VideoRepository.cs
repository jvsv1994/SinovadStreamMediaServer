using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Persistence.Contexts;

namespace SinovadMediaServer.Persistence.Repositories
{
    public class VideoRepository : GenericRepository<Video>, IVideoRepository
    {
        private ApplicationDbContext _context;
        public VideoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Video> GetVideoByLibraryId(int libraryId)
        {
            return _context.Videos.Where(video => video.LibraryId == libraryId).ToList();
        }

        //public object GetVideosByTvSerieAndUser(int tvSerieId, int userId)
        //{
        //    var result = _context.TvSeries.Join(_context.Videos, tvSerie => tvSerie.Id, video => video.TvSerieId, (tvserie, video) => new { tvserie, video })
        //        .Join(_context.Libraries, video => video.video.LibraryId, library => library.Id, (video, library) => new { video, library })
        //        .Join(_context.MediaServers, library => library.library.MediaServerId, mediaServer => mediaServer.Id, (library, mediaServer) =>
        //        new VideoDto
        //        {
        //            UserId = (int)mediaServer.UserId,
        //            MediaServerId = mediaServer.Id,
        //            MediaServerUrl = mediaServer.Url,
        //            MediaType = (MediaType)library.library.MediaTypeCatalogDetailId,
        //            TmdbId = library.video.tvserie.TmdbId != null ? (int)library.video.tvserie.TmdbId : 0,
        //            SeasonNumber = library.video.video.SeasonNumber != null ? (int)library.video.video.SeasonNumber : 0,
        //            EpisodeNumber = library.video.video.EpisodeNumber != null ? (int)library.video.video.EpisodeNumber : 0,
        //            VideoId = library.video.video.Id,
        //            TvSerieId = library.video.video.TvSerieId != null ? (int)library.video.video.TvSerieId : 0,
        //            PhysicalPath = library.video.video.PhysicalPath
        //        })
        //        .Where(item => item.MediaType == MediaType.TvSerie && item.UserId == userId && item.TvSerieId == tvSerieId).ToList();
        //    return result;
        //}
    }
}
