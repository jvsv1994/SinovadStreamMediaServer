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

        public ItemDetailDto GetTvSerieData(ItemDetailDto tvSerieDetail, List<MediaSeasonDto> listSeasons, List<MediaFileDto> listMediaFiles)
        {
            TMDbClient client = new TMDbClient(_sharedService._config.TMDbApiKey);
            TvShow tvSerie = client.GetTvShowAsync(int.Parse(tvSerieDetail.SourceId), TvShowMethods.Undefined, "es-MX").Result;
            Credits credits = client.GetTvShowCreditsAsync(int.Parse(tvSerieDetail.SourceId), "es-MX").Result;
            tvSerieDetail = GetDetailTvSerieByTMDB(tvSerie.Genres, credits);
            tvSerieDetail.PosterPath = tvSerie.PosterPath;
            tvSerieDetail.Title = tvSerie.Name;
            for (var i = 0; i < listSeasons.Count; i++)
            {
                var season = listSeasons[i];
                TvSeason seasontmdb = client.GetTvSeasonAsync(int.Parse(tvSerieDetail.SourceId), (int)season.SeasonNumber, TvSeasonMethods.Undefined, "es-MX").Result;
                season.Overview = seasontmdb.Overview;
                season.Name = seasontmdb.Name;
                season.PosterPath = seasontmdb.PosterPath;
                List<TvSeasonEpisode> listtse = seasontmdb.Episodes;
                List<MediaEpisodeDto> listEpisodesBySeason = new List<MediaEpisodeDto>();
                List<MediaFileDto> listMediaFilesBySeason = listMediaFiles.FindAll(item => item.SeasonNumber == season.SeasonNumber);
                for (var j = 0; j < listMediaFilesBySeason.Count; j++)
                {
                    var mediaFile = listMediaFilesBySeason[j];
                    var tse = listtse.Find(item => item.EpisodeNumber == mediaFile.EpisodeNumber);
                    if (tse != null)
                    {
                        var indexEpisode = listEpisodesBySeason.FindIndex(item => item.EpisodeNumber == mediaFile.EpisodeNumber);
                        if (indexEpisode == -1)
                        {
                            var episode = new MediaEpisodeDto();
                            episode.EpisodeNumber = tse.EpisodeNumber;
                            episode.SeasonNumber = tse.SeasonNumber;
                            episode.Name = tse.Name;
                            episode.Overview = tse.Overview;
                            listEpisodesBySeason.Add(episode);
                        }
                    }
                }
                List<MediaEpisodeDto> listEpisodesOrdered = listEpisodesBySeason.OrderBy(o => o.EpisodeNumber).ToList();
                season.ListEpisodes = listEpisodesOrdered;
            }
            List<MediaSeasonDto> listSeasonsOrdered = listSeasons.OrderBy(o => o.SeasonNumber).ToList();
            tvSerieDetail.ListSeasons = listSeasonsOrdered;
            return tvSerieDetail;
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
                movieDto.ReleaseDate = movieFinded.ReleaseDate;
                movieDto.SourceId = movieFinded.Id.ToString();
                movieDto.Overview = movieFinded.Overview;
                movieDto.PosterPath = movieFinded.PosterPath;
                movieDto.MediaTypeId = MediaType.Movie;
                movieDto.MetadataAgentsId = MetadataAgents.TMDb;
                movieDto.SearchQuery = movieName;
                if (movieFinded.Credits!=null && movieFinded.Credits.Cast!=null)
                {
                    movieDto.Actors = string.Join(", ", movieFinded.Credits.Cast.Select(x => x.Name));
                }
                if (movieFinded.Credits != null && movieFinded.Credits.Crew != null)
                {
                    movieDto.Directors = string.Join(", ", movieFinded.Credits.Crew.Select(x => x.Name));
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
                tvSerieDto.Overview = tvShow.Overview;
                tvSerieDto.PosterPath = tvShow.PosterPath;
                tvSerieDto.SourceId = tvShow.Id.ToString();
                tvSerieDto.FirstAirDate = (DateTime)tvShow.FirstAirDate;
                tvSerieDto.LastAirDate = (DateTime)tvShow.LastAirDate;
                tvSerieDto.MediaTypeId = MediaType.TvSerie;
                tvSerieDto.MetadataAgentsId = MetadataAgents.TMDb;
                tvSerieDto.SearchQuery = name;
                if (tvShow.Credits != null && tvShow.Credits.Cast != null)
                {
                    tvSerieDto.Actors = string.Join(", ", tvShow.Credits.Cast.Select(x => x.Name));
                }
                if (tvShow.Credits != null && tvShow.Credits.Crew != null)
                {
                    tvSerieDto.Directors = string.Join(", ", tvShow.Credits.Crew.Select(x => x.Name));
                }
                if (tvShow.Genres != null && tvShow.Genres.Count > 0)
                {
                    tvSerieDto.Genres = string.Join(", ", tvShow.Genres.Select(x => x.Name));
                    tvSerieDto.ListGenres = tvShow.Genres.MapTo<List<MediaGenreDto>>();
                }
            }
            return tvSerieDto;
        }

        public TvEpisode SearchEpisode(int tvShowId, int seasonNumber, int episodeNumber)
        {
            return _tmdbClient.GetTvEpisodeAsync(tvShowId, seasonNumber, episodeNumber, TvEpisodeMethods.Undefined, "es-MX").Result;
        }

        public string GetEpisodeName(int tvShowId, int seasonNumber, int episodeNumber)
        {
            var episode = _tmdbClient.GetTvEpisodeAsync(tvShowId, seasonNumber, episodeNumber, TvEpisodeMethods.Undefined, "es-MX").Result;
            if (episode != null)
            {
                return episode.Name;
            }
            return null;
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

        public List<MediaGenreDto> GetListGenres()
        {
            List<TMDbLib.Objects.General.Genre> movieGenres = _tmdbClient.GetMovieGenresAsync("es-MX").Result;
            List<int> movieGenresIds = movieGenres.Select(it => it.Id).ToList();
            List<TMDbLib.Objects.General.Genre> tvSerieGenres = _tmdbClient.GetTvGenresAsync("es-MX").Result;
            tvSerieGenres = tvSerieGenres.Where(it => !movieGenresIds.Contains(it.Id)).ToList();
            var allGenres = movieGenres.Concat(tvSerieGenres);
            List<MediaGenreDto> listGenres = new List<MediaGenreDto>();
            foreach (var genre in allGenres)
            {
                var genreDto = new MediaGenreDto();
                genreDto.Name = genre.Name;
                listGenres.Add(genreDto);
            }
            return listGenres;
        }

        public ItemDetailDto GetMovieDetail(string movieId)
        {
            var itemDetailDto = new ItemDetailDto();
            TMDbLib.Objects.Movies.Movie movie = _tmdbClient.GetMovieAsync(int.Parse(movieId), "es-MX").Result;
            TMDbLib.Objects.Movies.Credits credits = _tmdbClient.GetMovieCreditsAsync(int.Parse(movieId)).Result;
            itemDetailDto.Genres = string.Join(", ", movie.Genres.Select(item => item.Name));
            if (credits != null)
            {
                itemDetailDto.Actors = string.Join(", ", credits.Cast.Select(item => item.Name).Take(10));
                itemDetailDto.Directors = string.Join(", ", credits.Crew.Select(item => item.Name).Take(10));
            }
            return itemDetailDto;
        }

        public ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail)
        {
            TMDbLib.Objects.Movies.Movie movie = _tmdbClient.GetMovieAsync(int.Parse(movieDetail.SourceId), "es-MX").Result;
            TMDbLib.Objects.Movies.Credits credits = _tmdbClient.GetMovieCreditsAsync(int.Parse(movieDetail.SourceId)).Result;
            movieDetail.Genres = string.Join(", ", movie.Genres.Select(item => item.Name));
            if (credits != null)
            {
                movieDetail.Actors = string.Join(", ", credits.Cast.Select(item => item.Name).Take(10));
                movieDetail.Directors = string.Join(", ", credits.Crew.Select(item => item.Name).Take(10));
            }
            return movieDetail;
        }

        private ItemDetailDto GetDetailTvSerieByTMDB(List<TMDbLib.Objects.General.Genre> genreList, Credits credits)
        {
            ItemDetailDto detail = new ItemDetailDto();
            if (genreList != null && genreList.Count > 0)
            {
                detail.Genres = string.Join(",", genreList.Select(x => x.Name));
            }
            if (credits != null)
            {
                if (credits.Cast != null && credits.Cast.Count > 0)
                {
                    detail.Actors = string.Join(",", credits.Cast.Select(x => x.Name).Take(10));
                }
                if (credits.Crew != null && credits.Crew.Count > 0)
                {
                    detail.Directors = string.Join(",", credits.Crew.Select(x => x.Name).Take(10));
                }
            }
            return detail;
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
