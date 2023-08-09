using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Application.Interface.Infrastructure
{
    public interface ITmdbService
    {
        ItemDetailDto GetTvSerieData(int tmdbId, List<SeasonDto> listSeasons, List<VideoDto> listVideos);
        MovieDto SearchMovie(string movieName, string year);
        TvSerieDto SearchTvShow(string name);
        ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail);
        string GetEpisodeName(int tvShowId, int seasonNumber, int episodeNumber);
        List<GenreDto> GetListGenres();

    }
}
