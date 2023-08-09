using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.Configuration;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Shared;
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

        public ItemDetailDto GetTvSerieData(int tmdbId, List<SeasonDto> listSeasons, List<VideoDto> listVideos)
        {
            var tvSerieDetail = new ItemDetailDto();
            TMDbClient client = new TMDbClient(_sharedService._config.TMDbApiKey);
            TvShow tvSerie = client.GetTvShowAsync(tmdbId, TvShowMethods.Undefined, "es-MX").Result;
            Credits credits = client.GetTvShowCreditsAsync(tmdbId, "es-MX").Result;
            tvSerieDetail = GetDetailTvSerieByTMDB(tvSerie.Genres, credits);
            tvSerieDetail.TmdbId = tmdbId;
            tvSerieDetail.PosterPath = tvSerie.PosterPath;
            tvSerieDetail.Title = tvSerie.Name;
            for (var i = 0; i < listSeasons.Count; i++)
            {
                var season = listSeasons[i];
                TvSeason seasontmdb = client.GetTvSeasonAsync(season.TvSerieTmdbId, (int)season.SeasonNumber, TvSeasonMethods.Undefined, "es-MX").Result;
                season.Overview = seasontmdb.Overview;
                season.Name = seasontmdb.Name;
                season.PosterPath = seasontmdb.PosterPath;
                List<TvSeasonEpisode> listtse = seasontmdb.Episodes;
                List<EpisodeDto> listEpisodesBySeason = new List<EpisodeDto>();
                List<VideoDto> listVideosBySeason = listVideos.FindAll(item => item.SeasonNumber == season.SeasonNumber);
                for (var j = 0; j < listVideosBySeason.Count; j++)
                {
                    var video = listVideosBySeason[j];
                    var tse = listtse.Find(item => item.EpisodeNumber == video.EpisodeNumber);
                    if (tse != null)
                    {
                        var indexEpisode = listEpisodesBySeason.FindIndex(item => item.EpisodeNumber == video.EpisodeNumber);
                        if (indexEpisode == -1)
                        {
                            var episode = new EpisodeDto();
                            episode.MediaServerId = video.MediaServerId;
                            episode.MediaServerUrl = video.MediaServerUrl;
                            episode.TvSerieName = tvSerie.Name;
                            episode.EpisodeNumber = tse.EpisodeNumber;
                            episode.SeasonNumber = tse.SeasonNumber;
                            episode.Name = tse.Name;
                            episode.Overview = tse.Overview;
                            episode.PhysicalPath = video.PhysicalPath;
                            episode.TmdbId = tse.Id;
                            episode.StillPath = tse.StillPath;
                            episode.VideoId = video.VideoId;
                            listEpisodesBySeason.Add(episode);
                        }
                    }
                }
                List<EpisodeDto> listEpisodesOrdered = listEpisodesBySeason.OrderBy(o => o.EpisodeNumber).ToList();
                season.ListEpisodes = listEpisodesOrdered;
            }
            List<SeasonDto> listSeasonsOrdered = listSeasons.OrderBy(o => o.SeasonNumber).ToList();
            tvSerieDetail.ListSeasons = listSeasonsOrdered;
            return tvSerieDetail;
        }

        public MovieDto SearchMovie(string movieName, string year)
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
            MovieDto movieDto = null;
            if (movieFinded != null)
            {
                movieDto = new MovieDto();
                movieDto.TmdbId = movieFinded.Id;
                movieDto.Adult = movieFinded.Adult;
                movieDto.OriginalLanguage = movieFinded.OriginalLanguage;
                movieDto.OriginalTitle = movieFinded.OriginalTitle;
                movieDto.Overview = movieFinded.Overview;
                movieDto.Popularity = (double)movieFinded.Popularity;
                movieDto.PosterPath = movieFinded.PosterPath;
                movieDto.BackdropPath = movieFinded.BackdropPath;
                movieDto.ReleaseDate = (DateTime)movieFinded.ReleaseDate;
                movieDto.Title = movieFinded.Title;
                if (movieFinded.Genres != null && movieFinded.Genres.Count > 0)
                {
                    movieDto.ListGenreNames = movieFinded.Genres.Select(g => g.Name).ToList();
                }
            }
            return movieDto;
        }

        public TvSerieDto SearchTvShow(string name)
        {
            TvSerieDto tvSerieDto = null;
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
                tvSerieDto = new TvSerieDto();
                tvSerieDto.TmdbId = tvShow.Id;
                tvSerieDto.OriginalLanguage = tvShow.OriginalLanguage;
                tvSerieDto.OriginalName = tvShow.OriginalName;
                tvSerieDto.Overview = tvShow.Overview;
                tvSerieDto.Popularity = (double)tvShow.Popularity;
                tvSerieDto.PosterPath = tvShow.PosterPath;
                tvSerieDto.BackdropPath = tvShow.BackdropPath;
                tvSerieDto.FirstAirDate = (DateTime)tvShow.FirstAirDate;
                tvSerieDto.LastAirDate = (DateTime)tvShow.LastAirDate;
                tvSerieDto.Name = tvShow.Name;
                if (tvShow.Genres != null && tvShow.Genres.Count > 0)
                {
                    tvSerieDto.ListGenreNames = tvShow.Genres.Select(it => it.Name).ToList();

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


        public List<GenreDto> GetListGenres()
        {
            List<TMDbLib.Objects.General.Genre> movieGenres = _tmdbClient.GetMovieGenresAsync("es-MX").Result;
            List<int> movieGenresIds = movieGenres.Select(it => it.Id).ToList();
            List<TMDbLib.Objects.General.Genre> tvSerieGenres = _tmdbClient.GetTvGenresAsync("es-MX").Result;
            tvSerieGenres = tvSerieGenres.Where(it => !movieGenresIds.Contains(it.Id)).ToList();
            var allGenres = movieGenres.Concat(tvSerieGenres);
            List<GenreDto> listGenres = new List<GenreDto>();
            foreach (var genre in allGenres)
            {
                var genreDto = new GenreDto();
                genreDto.TmdbId = genre.Id;
                genreDto.Name = genre.Name;
                listGenres.Add(genreDto);
            }
            return listGenres;
        }

        public ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail)
        {
            TMDbLib.Objects.Movies.Movie movie = _tmdbClient.GetMovieAsync((int)movieDetail.TmdbId, "es-MX").Result;
            TMDbLib.Objects.Movies.Credits credits = _tmdbClient.GetMovieCreditsAsync((int)movieDetail.TmdbId).Result;
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
