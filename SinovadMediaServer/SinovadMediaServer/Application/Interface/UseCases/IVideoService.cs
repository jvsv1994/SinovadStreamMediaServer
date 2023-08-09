using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IVideoService
    {
        Response<object> UpdateVideoProfile(VideoProfileDto videoProfileDto);
        Response<object> UpdateVideosInListLibraries(SearchFilesDto dto);
        Response<List<ItemsGroupDto>> GetVideosOrganized(int userId, int profileId, bool searchMovies, bool searchTvSeries);
        Response<List<ItemDto>> GetVideosByFilters(int userId, bool searchMovies, bool searchTvSeries, string searchText);
        Task<Response<ItemDetailDto>> GetMovieDetail(int movieId);
        Task<Response<ItemDetailDto>> GetMovieDataByUser(int userId, int movieId);
        Task<Response<ItemDetailDto>> GetTvSerieDetail(int userId, int tvSerieId);
        Task<Response<ItemDetailDto>> GetTvSerieDataByUser(int userId, int tvSerieId);

    }
}
