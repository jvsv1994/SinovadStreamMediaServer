
using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.Configuration;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Mapping;
using TMDbLib.Client;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace SinovadMediaServer.Infrastructure.Tmdb
{
    public class TmdbService : ITmdbService
    {

        private readonly SharedService _sharedService;

        private readonly TMDbClient _tmdbClient;

        public TmdbService(SharedService sharedService, IOptions<MyConfig> options)
        {
            _sharedService = sharedService;
            _tmdbClient = new TMDbClient(options.Value.TMDbApiKey);
        }

        public MediaItemDto SearchMovie(string movieName, string year)
        {
            TMDbLib.Objects.Movies.Movie movieFinded = null;
            string language = "es-MX";
            TMDbLib.Objects.General.SearchContainer<SearchMovie> search = _tmdbClient.SearchMovieAsync(movieName, language, 1, false, int.Parse(year)).Result;
            if (search.Results.Count > 0)
            {
                movieFinded = GetValidMovie(movieName, search.Results, language);
            }
            else
            {
                language = "es-ES";
                search = _tmdbClient.SearchMovieAsync(movieName, language, 1, false, int.Parse(year)).Result;
                if (search.Results.Count > 0)
                {
                    movieFinded = GetValidMovie(movieName, search.Results, language);
                }
                else
                {
                    language = "en-US";
                    search = _tmdbClient.SearchMovieAsync(movieName, language, 1, false, int.Parse(year)).Result;
                    if (search.Results.Count > 0)
                    {
                        movieFinded = GetValidMovie(movieName, search.Results, language);
                    }
                }
            }
            MediaItemDto movieDto = null;
            if (movieFinded != null)
            {
                movieDto = new MediaItemDto();
                movieDto.Title = movieFinded.Title;
                movieDto.ExtendedTitle = movieFinded.Title + " (" + movieFinded.ReleaseDate.Value.Year + ")";
                movieDto.ReleaseDate = movieFinded.ReleaseDate;
                movieDto.SourceId = movieFinded.Id.ToString();
                movieDto.Overview = movieFinded.Overview;
                movieDto.PosterPath = movieFinded.PosterPath;
                movieDto.MediaTypeId = MediaType.Movie;
                movieDto.MetadataAgentsId = MetadataAgents.TMDb;
                movieDto.SearchQuery = movieName;
                TMDbLib.Objects.Movies.Credits credits = _tmdbClient.GetMovieCreditsAsync(movieFinded.Id).Result;
                if (credits != null && credits.Cast!=null)
                {
                    movieDto.Actors = string.Join(", ", credits.Cast.Select(x => x.Name).Take(10));
                }
                if (credits != null && credits.Crew != null)
                {
                    movieDto.Directors = string.Join(", ", credits.Crew.Select(x => x.Name).Take(10));
                }
                if (movieFinded.Genres != null && movieFinded.Genres.Count > 0)
                {
                    movieDto.Genres = string.Join(", ", movieFinded.Genres.Select(x => x.Name));
                    movieDto.ListGenres = movieFinded.Genres.MapTo<List<MediaGenreDto>>();
                }
            }
            return movieDto;
        }

        public MediaItemDto SearchTvShow(string name)
        {
            MediaItemDto tvSerieDto = null;
            TvShow tvShow = null;
            string language = "es-MX";
            TMDbLib.Objects.General.SearchContainer<SearchTv> search = _tmdbClient.SearchTvShowAsync(name, language, 1, false).Result;
            if (search.Results.Count > 0)
            {
                tvShow = GetValidTvSerie(name, search.Results, language);
            }
            else
            {
                language = "es-ES";
                search = _tmdbClient.SearchTvShowAsync(name, language, 1, false).Result;
                if (search.Results.Count > 0)
                {
                    tvShow = GetValidTvSerie(name, search.Results, language);
                }
                else
                {
                    language = "en-US";
                    search = _tmdbClient.SearchTvShowAsync(name, language, 1, false).Result;
                    if (search.Results.Count > 0)
                    {
                        tvShow = GetValidTvSerie(name, search.Results, language);
                    }
                }
            }
            if (tvShow != null)
            {
                tvSerieDto = new MediaItemDto();
                tvSerieDto.Title = tvShow.Name;
                tvSerieDto.ExtendedTitle = tvShow.Name + (tvShow.LastAirDate.Value.Year > tvShow.FirstAirDate.Value.Year ? " (" + tvShow.FirstAirDate.Value.Year + "-" + tvShow.LastAirDate.Value.Year + ")" : " (" + tvShow.FirstAirDate.Value.Year + ")");
                tvSerieDto.Overview = tvShow.Overview;
                tvSerieDto.PosterPath = tvShow.PosterPath;
                tvSerieDto.SourceId = tvShow.Id.ToString();
                tvSerieDto.FirstAirDate = (DateTime)tvShow.FirstAirDate;
                tvSerieDto.LastAirDate = (DateTime)tvShow.LastAirDate;
                tvSerieDto.MediaTypeId = MediaType.TvSerie;
                tvSerieDto.MetadataAgentsId = MetadataAgents.TMDb;
                tvSerieDto.SearchQuery = name;
                TMDbLib.Objects.Movies.Credits credits = _tmdbClient.GetMovieCreditsAsync(tvShow.Id).Result;
                if (credits != null && credits.Cast != null)
                {
                    tvSerieDto.Actors = string.Join(", ", credits.Cast.Select(x => x.Name).Take(10));
                }
                if (credits != null && credits.Crew != null)
                {
                    tvSerieDto.Directors = string.Join(", ", credits.Crew.Select(x => x.Name).Take(10));
                }
                if (tvShow.Genres != null && tvShow.Genres.Count > 0)
                {
                    tvSerieDto.Genres = string.Join(", ", tvShow.Genres.Select(x => x.Name));
                    tvSerieDto.ListGenres = tvShow.Genres.MapTo<List<MediaGenreDto>>();
                }
            }
            return tvSerieDto;
        }

        public MediaEpisodeDto GetTvEpisode(int tvShowId, int seasonNumber, int episodeNumber)
        {
            var episode = _tmdbClient.GetTvEpisodeAsync(tvShowId, seasonNumber, episodeNumber, TvEpisodeMethods.Undefined, "es-MX").Result;
            if (episode != null)
            {
                var episodeDto = new MediaEpisodeDto();
                episodeDto.Name=episode.Name;
                episodeDto.Overview = episode.Overview;
                episodeDto.EpisodeNumber=episodeNumber;
                episodeDto.SeasonNumber=seasonNumber;
                episodeDto.PosterPath = episode.StillPath;
                episodeDto.SourceId = episode.Id.ToString();
                return episodeDto;
            }
            return null;
        }

        public MediaSeasonDto GetTvSeason(int tvShowId, int seasonNumber)
        {
            var season = _tmdbClient.GetTvSeasonAsync(tvShowId, seasonNumber, TvSeasonMethods.Undefined, "es-MX").Result;
            if (season != null)
            {
                var seasonDto = new MediaSeasonDto();
                seasonDto.Name = season.Name;
                seasonDto.Overview = season.Overview;
                seasonDto.PosterPath = season.PosterPath;
                seasonDto.SourceId = season.Id.ToString();
                seasonDto.SeasonNumber = season.SeasonNumber;
                return seasonDto;
            }
            return null;
        }

        private TMDbLib.Objects.Movies.Movie GetValidMovie(string movieName, List<SearchMovie> listSearchTv, string language)
        {
            TMDbLib.Objects.Movies.Movie movieFinded = null;
            TMDbLib.Objects.Movies.Movie firstMovieFinded = null;
            for (int i = 0; i < listSearchTv.Count; i++)
            {
                var searched = listSearchTv[i];
                TMDbLib.Objects.Movies.Movie movie = _tmdbClient.GetMovieAsync(searched.Id, language).Result;
                if (i == 0)
                {
                    firstMovieFinded = movie;
                }
                if (movie.Genres != null && movie.Genres.Count > 0 && _sharedService.GetFormattedText(movieName) == _sharedService.GetFormattedText(movie.Title))
                {
                    movieFinded = movie;
                    break;
                }
            }
            if (movieFinded == null)
            {
                movieFinded = firstMovieFinded;
            }
            return movieFinded;
        }

        private TvShow GetValidTvSerie(string tvSerieName, List<SearchTv> listSearchTv, string language)
        {
            TvShow tvShowFinded = null;
            TvShow firstTvShowFinded = null;
            for (int i = 0; i < listSearchTv.Count; i++)
            {
                var searched = listSearchTv[i];
                TvShow tvshow = _tmdbClient.GetTvShowAsync(searched.Id, TvShowMethods.Undefined, language).Result;
                if (i == 0)
                {
                    firstTvShowFinded = tvshow;
                }
                if (tvshow.Genres != null && tvshow.Genres.Count > 0 && _sharedService.GetFormattedText(tvSerieName) == _sharedService.GetFormattedText(tvshow.Name))
                {
                    tvShowFinded = tvshow;
                    break;
                }
            }
            if (tvShowFinded == null)
            {
                tvShowFinded = firstTvShowFinded;
            }
            return tvShowFinded;
        }

    }
}
