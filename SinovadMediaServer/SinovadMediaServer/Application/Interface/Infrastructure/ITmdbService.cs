using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Application.Interface.Infrastructure
{
    public interface ITmdbService
    {
        MediaItemDto SearchMovie(string movieName, string year);
        MediaItemDto SearchTvShow(string name);
        MediaSeasonDto GetTvSeason(int tvShowId, int seasonNumber);
        MediaEpisodeDto GetTvEpisode(int tvShowId, int seasonNumber, int episodeNumber);

    }
}
