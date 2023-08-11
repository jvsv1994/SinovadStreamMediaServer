using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Application.Interface.Infrastructure
{
    public interface ITmdbService
    {
        ItemDetailDto GetTvSerieData(ItemDetailDto tvSerieDetail, List<MediaSeasonDto> listSeasons, List<MediaFileDto> listMediaFiles);
        MediaItemDto SearchMovie(string movieName, string year);
        MediaItemDto SearchTvShow(string name);
        ItemDetailDto GetMovieDetail(string movieId);
        ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail);
        MediaSeasonDto GetTvSeason(int tvShowId, int seasonNumber);
        MediaEpisodeDto GetTvEpisode(int tvShowId, int seasonNumber, int episodeNumber);
        string GetEpisodeName(int tvShowId, int seasonNumber, int episodeNumber);
        List<MediaGenreDto> GetListGenres();

    }
}
