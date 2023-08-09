using SinovadMediaServer.Application.Builder;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System.Linq.Expressions;


namespace SinovadMediaServer.Application.UseCases.Videos
{
    public class VideoService : IVideoService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;
        private SearchMediaLog? _searchMediaLog { get; set; }
        private readonly SearchMediaLogBuilder _searchMediaBuilder;

        private readonly ITmdbService _tmdbService;

        private readonly IImdbService _imdbService;

        public VideoService(IUnitOfWork unitOfWork, ITmdbService tmdbService, IImdbService imdbService, SharedService sharedService, SearchMediaLogBuilder searchMediaBuilder)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
            _searchMediaBuilder = searchMediaBuilder;
            _tmdbService = tmdbService;
            _imdbService = imdbService;
        }

        public Response<object> UpdateVideoProfile(VideoProfileDto videoProfileDto)
        {
            var response = new Response<object>();
            try
            {
                VideoProfile videoProfile = _unitOfWork.VideoProfiles.GetByExpression(a => a.VideoId == videoProfileDto.VideoId && a.ProfileId == videoProfileDto.ProfileId);
                if (videoProfile != null)
                {
                    videoProfile.CurrentTime = videoProfileDto.CurrentTime;
                    videoProfile.DurationTime = videoProfileDto.DurationTime;
                    _unitOfWork.VideoProfiles.Update(videoProfile);
                }
                else
                {
                    videoProfile = videoProfileDto.MapTo<VideoProfile>();
                    _unitOfWork.VideoProfiles.Add(videoProfile);
                }
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        private Response<object> RegisterVideos(MediaType mediaType, int libraryId, string paths, string logIdentifier)
        {
            var response = new Response<object>();
            try
            {
                _searchMediaLog = new SearchMediaLog(logIdentifier);
                _searchMediaBuilder.AddSearchMediaLog(_searchMediaLog);
                if (mediaType == MediaType.Movie)
                {
                    RegisterMoviesAndVideos(libraryId, paths);
                }
                if (mediaType == MediaType.TvSerie)
                {
                    //RegisterTvSeriesAndVideos(libraryId, paths);
                }
                RemoveSearchMediaLog();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> UpdateVideosInListLibraries(UpdateLibraryVideosDto dto)
        {
            var response = new Response<object>();
            try
            {
                foreach (var library in dto.ListLibraries)
                {
                    if (library.MediaTypeCatalogDetailId == (int)MediaType.Movie)
                    {
                        var paths = string.Join(",", library.ListPaths);
                        RegisterMoviesAndVideos(library.Id, paths);
                    }
                    if (library.MediaTypeCatalogDetailId == (int)MediaType.TvSerie)
                    {
                        var paths = string.Join(",", library.ListPaths);
                        //RegisterTvSeriesAndVideos(library.Id, paths);
                    }
                }
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<List<ItemsGroupDto>> GetVideosOrganized(int userId, int profileId, bool searchMovies, bool searchTvSeries)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = new List<ItemDto>();
                List<ItemDto> listLastAddedItems = new List<ItemDto>();
                List<ItemDto> listItemsWatched = new List<ItemDto>();
                if (searchMovies)
                {

                    //List<ItemDto> listMovies = _unitOfWork.Movies.GetMoviesByUser(userId).MapTo<List<ItemDto>>();
                    //listItems = listItems.Concat(listMovies).ToList();

                    //List<ItemDto> listLastAddedMovies = _unitOfWork.Movies.GetRecentlyAddedMoviesByUser(userId).MapTo<List<ItemDto>>();
                    //listLastAddedItems = listLastAddedItems.Concat(listLastAddedMovies).ToList();

                    //List<ItemDto> listMoviesWatched = _unitOfWork.Movies.GetMoviesByProfile(profileId).MapTo<List<ItemDto>>();
                    //listItemsWatched = listMoviesWatched.Concat(listItemsWatched).ToList();
                }
                if (searchTvSeries)
                {
                    //List<ItemDto> listTvSeries = _unitOfWork.TvSeries.GetTvSeriesByUserId(userId).MapTo<List<ItemDto>>();
                    //listItems = listItems.Concat(listTvSeries).ToList();

                    //List<ItemDto> listLastAddedTvSeries = _unitOfWork.TvSeries.GetRecentlyTvSeriesAdded(userId).MapTo<List<ItemDto>>();
                    //listLastAddedItems = listLastAddedItems.Concat(listLastAddedTvSeries).ToList();

                    //List<ItemDto> listTvSeriesWatched = _unitOfWork.TvSeries.GetTvSeriesByProfileId(profileId).MapTo<List<ItemDto>>();
                    //listItemsWatched = listTvSeriesWatched.Concat(listItemsWatched).ToList();
                }
                listLastAddedItems = listLastAddedItems.OrderByDescending(c => c.Created).ToList();
                if (listItemsWatched != null && listItemsWatched.Count > 0)
                {
                    listItemsWatched = listItemsWatched.OrderByDescending(c => c.LastModified).ToList();
                }
                //var genres = _unitOfWork.Genres.GetAllAsync().Result.ToList().MapTo<List<GenreDto>>(); ;

                var listItemsGroup = new List<ItemsGroupDto>();
                if (listItemsWatched != null && listItemsWatched.Count > 0)
                {
                    var itemsGroup = new ItemsGroupDto();
                    itemsGroup.Id = -1;
                    itemsGroup.Name = "Continuar viendo";
                    itemsGroup.ListItems = listItemsWatched;
                    listItemsGroup.Add(itemsGroup);
                }
                if (listLastAddedItems != null && listLastAddedItems.Count > 0)
                {
                    var itemsGroup = new ItemsGroupDto();
                    itemsGroup.Id = 0;
                    itemsGroup.Name = "Agregados recientemente";
                    itemsGroup.ListItems = listLastAddedItems;
                    listItemsGroup.Add(itemsGroup);
                }
                //if (genres != null && genres.Count > 0)
                //{
                //    foreach (var genre in genres)
                //    {
                //        var list = listItems.Where(ele => ele.GenreId == genre.Id).ToList();
                //        if (list != null && list.Count > 0)
                //        {
                //            var itemsGroup = new ItemsGroupDto();
                //            itemsGroup.Id = genre.Id;
                //            itemsGroup.Name = genre.Name;
                //            itemsGroup.ListItems = list;
                //            listItemsGroup.Add(itemsGroup);
                //        }
                //    }
                //}
                response.Data = listItemsGroup;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<List<ItemDto>> GetVideosByFilters(int userId, bool searchMovies, bool searchTvSeries, string searchText)
        {
            var response = new Response<List<ItemDto>>();
            try
            {
                //List<ItemDto> listItems = new List<ItemDto>();
                //if (searchMovies)
                //{
                //    List<ItemDto> listMovies = _unitOfWork.Movies.GetMoviesByUserAndSearchText(userId, searchText).MapTo<List<ItemDto>>();
                //    listItems = listItems.Concat(listMovies).ToList();
                //}
                //if (searchTvSeries)
                //{
                //    List<ItemDto> listTvSeries = _unitOfWork.TvSeries.GetTvSeriesByUserIdAndSerchText(userId, searchText).MapTo<List<ItemDto>>();
                //    listItems = listItems.Concat(listTvSeries).ToList();
                //}
                //response.Data = listItems;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<ItemDetailDto>> GetMovieDetail(int movieId)
        {
            var response = new Response<ItemDetailDto>();
            //try
            //{
            //    ItemDetailDto movieDetail = new ItemDetailDto();
            //    var result = await _unitOfWork.Movies.GetAsync(movieId);
            //    if (result == null)
            //    {
            //        throw new Exception("Movie Not found");
            //    }
            //    movieDetail = result.MapTo<ItemDetailDto>();
            //    if (movieDetail.TmdbId != null && movieDetail.TmdbId > 0)
            //    {
            //        movieDetail = _tmdbService.GetMovieDetail(movieDetail);
            //    }
            //    else if (movieDetail.Imdbid != null)
            //    {
            //        movieDetail = _imdbService.GetMovieDetail(movieDetail);
            //    }
            //    response.Data = movieDetail;
            //    response.IsSuccess = true;
            //    response.Message = "Successful";
            //}
            //catch (Exception ex)
            //{
            //    response.Message = ex.Message;
            //    _sharedService._tracer.LogError(ex.StackTrace);
            //}
            return response;
        }

        public async Task<Response<ItemDetailDto>> GetMovieDataByUser(int userId, int movieId)
        {
            var response = new Response<ItemDetailDto>();
            //try
            //{
            //    ItemDetailDto movieDetail = new ItemDetailDto();
            //    var result = await _unitOfWork.Movies.GetAsync(movieId);
            //    if (result == null)
            //    {
            //        throw new Exception("Movie Not found");
            //    }
            //    movieDetail = result.MapTo<ItemDetailDto>();
            //    if (movieDetail.TmdbId != null && movieDetail.TmdbId > 0)
            //    {
            //        movieDetail = _tmdbService.GetMovieDetail(movieDetail);
            //    }
            //    else if (movieDetail.Imdbid != null)
            //    {
            //        movieDetail = _imdbService.GetMovieDetail(movieDetail);
            //    }
            //    movieDetail.Item = _unitOfWork.Movies.GetMovieDataByUser(userId, movieId);
            //    response.Data = movieDetail;
            //    response.IsSuccess = true;
            //    response.Message = "Successful";
            //}
            //catch (Exception ex)
            //{
            //    response.Message = ex.Message;
            //    _sharedService._tracer.LogError(ex.StackTrace);
            //}
            return response;
        }

        public async Task<Response<ItemDetailDto>> GetTvSerieDetail(int userId, int tvSerieId)
        {
            var response = new Response<ItemDetailDto>();
            try
            {
                //TvSerie emstvserie = await _unitOfWork.TvSeries.GetAsync(tvSerieId);
                //ItemDetailDto tvSerieDetail = emstvserie.MapTo<ItemDetailDto>();
                //List<VideoDto> listVideos = _unitOfWork.Videos.GetVideosByTvSerieAndUser(tvSerieId, userId).MapTo<List<VideoDto>>();
                //var seasonService = new SeasonService(_unitOfWork, _sharedService);
                //List<SeasonDto> listSeasons = seasonService.GetSeasonsByVideos(listVideos);
                //if (emstvserie.TmdbId != null && emstvserie.TmdbId > 0)
                //{
                //    tvSerieDetail = _tmdbService.GetTvSerieData((int)tvSerieDetail.TmdbId, listSeasons, listVideos);
                //}
                //else
                //{
                //    var tvSerieService = new TvSerieService(_unitOfWork, _sharedService);
                //    tvSerieDetail = tvSerieService.GetTvSerieData(tvSerieDetail, listSeasons, listVideos);
                //    tvSerieDetail.PosterPath = emstvserie.PosterPath;
                //}
                //response.Data = tvSerieDetail;
                //response.IsSuccess = true;
                //response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<ItemDetailDto>> GetTvSerieDataByUser(int userId, int tvSerieId)
        {
            var response = new Response<ItemDetailDto>();
            try
            {
                //TvSerie emstvserie = await _unitOfWork.TvSeries.GetAsync(tvSerieId);
                //ItemDetailDto tvSerieDetail = emstvserie.MapTo<ItemDetailDto>();
                //List<VideoDto> listVideos = _unitOfWork.Videos.GetVideosByTvSerieAndUser(tvSerieId, userId).MapTo<List<VideoDto>>();
                //var seasonService = new SeasonService(_unitOfWork, _sharedService);
                //List<SeasonDto> listSeasons = seasonService.GetSeasonsByVideos(listVideos);
                //if (emstvserie.TmdbId != null && emstvserie.TmdbId > 0)
                //{
                //    tvSerieDetail = _tmdbService.GetTvSerieData((int)tvSerieDetail.TmdbId, listSeasons, listVideos);
                //}
                //else
                //{
                //    var tvSerieService = new TvSerieService(_unitOfWork, _sharedService);
                //    tvSerieDetail = tvSerieService.GetTvSerieData(tvSerieDetail, listSeasons, listVideos);
                //    tvSerieDetail.PosterPath = emstvserie.PosterPath;
                //}
                //tvSerieDetail.Item = _unitOfWork.TvSeries.GetTvSerieDataByUser(userId, tvSerieId);
                //response.Data = tvSerieDetail;
                //response.IsSuccess = true;
                //response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        private void AddMessage(LogType logType, string message)
        {
            if (logType == LogType.Information)
            {
                _sharedService._tracer.LogInformation(message); ;
                message = "<span style=" + "\"" + "color:white;" + "\">" + message + "</span>";
            }
            if (logType == LogType.Error)
            {
                _sharedService._tracer.LogError(message);
                message = "<span style=" + "\"" + "color:red;" + "\">" + message + "</span>";
            }
            if (_searchMediaLog != null)
            {
                _searchMediaLog._textLines.Add(message);
            }
        }

        private void RemoveSearchMediaLog()
        {
            if (_searchMediaLog != null)
            {
                _searchMediaBuilder.RemoveSearchMediaLog(_searchMediaLog._logIdentifier);
            }
        }

        private void RegisterMoviesAndVideos(int libraryId, string paths)
        {
            AddMessage(LogType.Information, "Starting search movies");
            try
            {
                var filesToAdd = GetFilesToAdd(libraryId, paths);
                DeleteVideosNotFoundInLibrary(libraryId, paths);
                if (filesToAdd != null && filesToAdd.Count > 0)
                {
                    var listMoviesFinded = new List<MovieDto>();
                    var listVideosTmp = new List<VideoDto>();
                    for (int i = 0; i < filesToAdd.Count; i++)
                    {
                        try
                        {
                            var path = filesToAdd[i];
                            AddMessage(LogType.Information, "Processing new video with path " + path);
                            var splitted = path.Split("\\");
                            var fileName = splitted[splitted.Length - 1];
                            var physicalPath = path;
                            var fileNameWithoutExtension = fileName;
                            if (fileName.ToLower().EndsWith(".mp4") || fileName.ToLower().EndsWith(".mkv") || fileName.ToLower().EndsWith(".avi"))
                            {
                                fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
                            }
                            var partsMovieNameWithoutExtension = fileNameWithoutExtension.Split(" ");
                            var movieName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 5);
                            var year = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 4);
                            MovieDto movie = GetMovieFromExternalDataBase(movieName, year);
                            if (movie != null)
                            {
                                var newVideo = new VideoDto();
                                newVideo.PhysicalPath = physicalPath;
                                newVideo.Title = movie.Title;
                                newVideo.LibraryId = libraryId;
                                newVideo.TmdbId = movie.TmdbId;
                                newVideo.Imdbid = movie.Imdbid;
                                listVideosTmp.Add(newVideo);
                                listMoviesFinded.Add(movie);
                                AddMessage(LogType.Information, "Movie finded in TMDb Data Base " + newVideo.Title + " " + year);
                            }
                        }
                        catch (Exception e)
                        {
                            AddMessage(LogType.Error, e.Message);
                        }
                    }
                    RegisterMoviesFromExternalDataBaseAndVideos(listMoviesFinded, listVideosTmp);
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
            AddMessage(LogType.Information, "Ending search movies");
        }

        private MovieDto GetMovieFromExternalDataBase(string movieName, string year)
        {
            MovieDto movie = _tmdbService.SearchMovie(movieName, year);
            if (movie != null && movie.ListGenreNames != null && movie.ListGenreNames.Count > 0)
            {
                AddMessage(LogType.Information, "Movie finded in TMDb Data Base " + movie.Title + " " + year);
                return movie;
            }
            else
            {
                movie = _imdbService.SearchMovie(movieName, year);
                if (movie != null && movie.ListGenreNames != null && movie.ListGenreNames.Count > 0)
                {
                    AddMessage(LogType.Information, "Movie finded in Imdb Data Base " + movie.Title + " " + year);
                    return movie;
                }
                else
                {
                    AddMessage(LogType.Information, "Movie not found in any database");
                }
            }
            return null;
        }

        private void RegisterMoviesFromExternalDataBaseAndVideos(List<MovieDto> listMoviesFinded, List<VideoDto> listVideosDto)
        {
            //List<Genre> listGenres = _unitOfWork.Genres.GetAllAsync().Result.ToList();
            //List<string> listImdbIds = listMoviesFinded.Where(it => it.Imdbid != null).Select(it => it.Imdbid).ToList();
            //List<string> listImdbIdsFinded = _unitOfWork.Movies.GetListImdbMovieIdsFinded(listImdbIds);
            //List<int> listTmdbIds = listMoviesFinded.Where(it => it.TmdbId != null && it.TmdbId > 0).Select(it => (int)it.TmdbId).ToList();
            //List<int> listTmdbIdsFinded = _unitOfWork.Movies.GetListTMDdMovieIdsFinded(listTmdbIds);
            //IEnumerable<MovieDto> listMoviesToRegister = listMoviesFinded.Where(it => !listImdbIdsFinded.Contains(it.Imdbid) && !listTmdbIdsFinded.Contains((int)it.TmdbId));
            //if (listMoviesToRegister != null && listMoviesToRegister.Count() > 0)
            //{
            //    foreach (var movieDto in listMoviesToRegister)
            //    {
            //        movieDto.MovieGenres = SetMovieGenres(movieDto, listGenres);
            //    }
            //    var listMovies = listMoviesToRegister.MapTo<List<Movie>>();
            //    _unitOfWork.Movies.AddList(listMovies);
            //    _unitOfWork.Save();
            //}
            //if (listVideosDto.Count > 0)
            //{
            //    var videosToAdd = new List<VideoDto>();
            //    var listRelatedMovies = _unitOfWork.Movies.GetAllByExpression(item => item.TmdbId != null && listTmdbIds.Contains((int)item.TmdbId) || item.Imdbid != null && listImdbIds.Contains(item.Imdbid));
            //    foreach (var videoDto in listVideosDto)
            //    {
            //        if (videoDto.MovieId == null || videoDto.MovieId == 0)
            //        {
            //            var relatedMovie = listRelatedMovies.FirstOrDefault(it => videoDto.TmdbId != null && it.TmdbId == videoDto.TmdbId || videoDto.Imdbid != null && it.Imdbid == videoDto.Imdbid);
            //            if (relatedMovie != null)
            //            {
            //                videoDto.MovieId = relatedMovie.Id;
            //                videosToAdd.Add(videoDto);
            //            }
            //        }
            //        else
            //        {
            //            videosToAdd.Add(videoDto);
            //        }
            //    }
            //    if (videosToAdd.Count > 0)
            //    {
            //        var listVideos = listVideosDto.MapTo<List<Video>>();
            //        AddMessage(LogType.Information, "Saving all new movie videos");
            //        _unitOfWork.Videos.AddList(listVideos);
            //        _unitOfWork.Save();
            //    }
            //}
        }

        //private List<MovieGenreDto> SetMovieGenres(MovieDto movie, List<Genre> listGenres)
        //{
        //    List<MovieGenreDto> listMovieGenreToAdd = new List<MovieGenreDto>();
        //    if (movie.ListGenreNames != null && movie.ListGenreNames.Count > 0)
        //    {
        //        for (int j = 0; j < movie.ListGenreNames.Count; j++)
        //        {
        //            var genreName = movie.ListGenreNames[j];
        //            var newMovieGenre = new MovieGenreDto();
        //            newMovieGenre.GenreId = listGenres.Find(genre => genre.Name.ToLower().Trim() == genreName.ToLower().Trim()).Id;
        //            listMovieGenreToAdd.Add(newMovieGenre);
        //        }
        //    }
        //    return listMovieGenreToAdd;
        //}

        //private void RegisterTvSeriesAndVideos(int libraryId, string paths)
        //{
        //    AddMessage(LogType.Information, "Starting search tv series");
        //    try
        //    {
        //        var filesToAdd = GetFilesToAdd(libraryId, paths);
        //        DeleteVideosNotFoundInLibrary(libraryId, paths);
        //        var listTvSeriesTMDFinded = new List<TvSerieDto>();
        //        var listVideosTmp = new List<VideoDto>();
        //        object queryEpisodes = _unitOfWork.Episodes.GetEpisodesFromOwnDataBase();
        //        List<EpisodeDto> listAllEpisodes = queryEpisodes.MapTo<List<EpisodeDto>>();
        //        if (filesToAdd != null && filesToAdd.Count > 0)
        //        {
        //            for (int i = 0; i < filesToAdd.Count; i++)
        //            {
        //                try
        //                {
        //                    var path = filesToAdd[i];
        //                    AddMessage(LogType.Information, "Processing new video with path " + path);
        //                    var splitted = path.Split("\\");
        //                    var fileName = splitted[splitted.Length - 1];
        //                    var physicalPath = path;
        //                    var tvSerieNameTmp = fileName.Split(".")[0];
        //                    var tvSerieNameTmp2 = tvSerieNameTmp.Split(" ");
        //                    var seasonEpisodeText = tvSerieNameTmp2[tvSerieNameTmp2.Length - 1];
        //                    var tvSerieName = tvSerieNameTmp.Replace(seasonEpisodeText, "").Trim();
        //                    if (seasonEpisodeText.IndexOf("E") != -1)
        //                    {
        //                        var seasonepisodeArray = seasonEpisodeText.Split("E");
        //                        var seasonText = seasonepisodeArray[0].Replace("S", "");
        //                        var episodeText = seasonepisodeArray[1];
        //                        var seasonNumber = int.Parse(seasonText);
        //                        var episodeNumber = int.Parse(episodeText);
        //                        EpisodeDto episodeBeanFinded = listAllEpisodes.Find(item => _sharedService.GetFormattedText(item.TvSerieName) == _sharedService.GetFormattedText(tvSerieName) && item.SeasonNumber == seasonNumber && item.EpisodeNumber == episodeNumber);
        //                        if (episodeBeanFinded != null)
        //                        {
        //                            var newVideo = new VideoDto();
        //                            newVideo.PhysicalPath = physicalPath;
        //                            newVideo.Title = episodeBeanFinded.TvSerieName;
        //                            newVideo.Subtitle = "T" + seasonNumber + ":E" + episodeNumber + " " + episodeBeanFinded.Name;
        //                            newVideo.LibraryId = libraryId;
        //                            newVideo.EpisodeNumber = episodeNumber;
        //                            newVideo.SeasonNumber = seasonNumber;
        //                            newVideo.EpisodeName = episodeBeanFinded.Name;
        //                            newVideo.TvSerieId = episodeBeanFinded.TvSerieId;
        //                            newVideo.EpisodeId = episodeBeanFinded.EpisodeId;
        //                            listVideosTmp.Add(newVideo);
        //                            AddMessage(LogType.Information, "Episode finded in Innova Stream Data Base " + newVideo.Title + " " + newVideo.Subtitle);
        //                        }
        //                        else
        //                        {
        //                            TvSerieDto tvShow = null;
        //                            if (listTvSeriesTMDFinded != null && listTvSeriesTMDFinded.Count > 0)
        //                            {
        //                                tvShow = listTvSeriesTMDFinded.Find(item => _sharedService.GetFormattedText(item.Name) == _sharedService.GetFormattedText(tvSerieName));
        //                            }
        //                            if (tvShow == null)
        //                            {
        //                                tvShow = _tmdbService.SearchTvShow(tvSerieName);
        //                                if (tvShow != null)
        //                                {
        //                                    listTvSeriesTMDFinded.Add(tvShow);
        //                                }
        //                            }
        //                            if (tvShow != null && tvShow.TmdbId != null && tvShow.TmdbId > 0)
        //                            {
        //                                string episodeName = _tmdbService.GetEpisodeName((int)tvShow.TmdbId, seasonNumber, episodeNumber);
        //                                if (episodeName != null)
        //                                {
        //                                    var newVideo = new VideoDto();
        //                                    newVideo.PhysicalPath = physicalPath;
        //                                    newVideo.Title = tvShow.Name;
        //                                    newVideo.Subtitle = "T" + seasonNumber + ":E" + episodeNumber + " " + episodeName;
        //                                    newVideo.LibraryId = libraryId;
        //                                    newVideo.TmdbId = tvShow.TmdbId;
        //                                    newVideo.EpisodeNumber = episodeNumber;
        //                                    newVideo.SeasonNumber = seasonNumber;
        //                                    newVideo.EpisodeName = episodeName;
        //                                    AddMessage(LogType.Information, "Episode finded in TMDb Data Base " + newVideo.Title + " " + newVideo.Subtitle);
        //                                    listVideosTmp.Add(newVideo);
        //                                }
        //                                else
        //                                {
        //                                    AddMessage(LogType.Information, "Episode not found in any database");
        //                                }
        //                            }
        //                            else
        //                            {
        //                                AddMessage(LogType.Information, "Episode not found in any database");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        AddMessage(LogType.Information, "The video path does not comply with the format of an episode");
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    AddMessage(LogType.Error, e.Message);
        //                }
        //            }
        //            RegisterTvSeriesFromExternalDataBaseAndVideos(listTvSeriesTMDFinded, listVideosTmp);
        //        }
        //        else
        //        {
        //            AddMessage(LogType.Information, "New videos not found");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        AddMessage(LogType.Error, e.Message);
        //    }
        //    AddMessage(LogType.Information, "Ending search tv series");
        //}

        //private void RegisterTvSeriesFromExternalDataBaseAndVideos(List<TvSerieDto> listTvSeriesTMDFinded, List<VideoDto> listVideosDto)
        //{
        //    List<int> listIdsTvSeriesTMDFinded = listTvSeriesTMDFinded.Select(it => (int)it.TmdbId).ToList();
        //    if (listTvSeriesTMDFinded.Count > 0)
        //    {
        //        List<Genre> listGenres = (from d in _unitOfWork.Genres.GetAllAsync().Result select d).ToList();
        //        List<int> listTmdbIdsAvoidToCreate = _unitOfWork.TvSeries.GetListTmdbIdsExistingInOwnDataBase(listIdsTvSeriesTMDFinded);
        //        List<TvSerieDto> listTvSeriesToCreate = listTvSeriesTMDFinded.Where(item => !listTmdbIdsAvoidToCreate.Contains((int)item.TmdbId)).ToList();
        //        if (listTvSeriesToCreate.Count > 0)
        //        {
        //            foreach (var tvSerieGenreDto in listTvSeriesToCreate)
        //            {
        //                tvSerieGenreDto.TvSerieGenres = SetTvSerieGenres(tvSerieGenreDto, listGenres);
        //            }
        //            var listTvSeries = listTvSeriesToCreate.MapTo<List<TvSerie>>();
        //            _unitOfWork.TvSeries.AddList(listTvSeries);
        //            _unitOfWork.Save();
        //        }
        //    }
        //    if (listVideosDto.Count > 0)
        //    {
        //        var listRelatedTvSeries = _unitOfWork.TvSeries.GetAllByExpression(item => item.TmdbId != null && listIdsTvSeriesTMDFinded.Contains((int)item.TmdbId));
        //        foreach (var videoDto in listVideosDto)
        //        {
        //            var relatedTvSerie = listRelatedTvSeries.FirstOrDefault(it => it.TmdbId == videoDto.TmdbId);
        //            if (relatedTvSerie != null)
        //            {
        //                videoDto.TvSerieId = relatedTvSerie.Id;
        //            }
        //        }
        //        var listVideos = listVideosDto.MapTo<List<Video>>();
        //        AddMessage(LogType.Information, "Saving all new movie videos");
        //        _unitOfWork.Videos.AddList(listVideos);
        //        _unitOfWork.Save();
        //    }
        //}

        //private List<TvSerieGenreDto> SetTvSerieGenres(TvSerieDto movie, List<Genre> listGenres)
        //{
        //    List<TvSerieGenreDto> listTvSerieGenreToAdd = new List<TvSerieGenreDto>();
        //    if (movie.ListGenreNames != null && movie.ListGenreNames.Count > 0)
        //    {
        //        for (int j = 0; j < movie.ListGenreNames.Count; j++)
        //        {
        //            var genreName = movie.ListGenreNames[j];
        //            var newTvSerieGenre = new TvSerieGenreDto();
        //            newTvSerieGenre.GenreId = listGenres.Find(genre => genre.Name.ToLower().Trim() == genreName.ToLower().Trim()).Id;
        //            listTvSerieGenreToAdd.Add(newTvSerieGenre);
        //        }
        //    }
        //    return listTvSerieGenreToAdd;
        //}


        private List<string> GetFilesToAdd(int libraryId, string paths)
        {
            List<string> filesToAdd = new List<string>();
            try
            {
                AddMessage(LogType.Information, "Prepare video paths");
                List<string> listAllFilesTmp = new List<string>();
                if (!string.IsNullOrEmpty(paths))
                {
                    listAllFilesTmp = paths.Split(",").Select(x => x.ToString()).ToList();
                }
                List<Video> listVideosToAvoidAdd = new List<Video>();
                if (listAllFilesTmp.Count > 0)
                {
                    AddMessage(LogType.Information, "Check if videos were already registered");
                    Expression<Func<Video, bool>> expressionVideosToAvoidAdd = x => listAllFilesTmp.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                    listVideosToAvoidAdd = _unitOfWork.Videos.GetAllByExpressionAsync(expressionVideosToAvoidAdd).Result.ToList();
                }
                if (listVideosToAvoidAdd.Count > 0)
                {
                    List<string> listVideoPathsToAvoidAdd = listVideosToAvoidAdd.Select(o => o.PhysicalPath).ToList();
                    filesToAdd = listAllFilesTmp.FindAll(filePath => listVideoPathsToAvoidAdd.IndexOf(filePath) == -1).ToList();
                }
                else
                {
                    filesToAdd = listAllFilesTmp;
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
            return filesToAdd;
        }

        private void DeleteVideosNotFoundInLibrary(int libraryId, string paths)
        {
            try
            {
                AddMessage(LogType.Information, "Check if there are videos to delete");
                List<string> listAllFilesTmp = new List<string>();
                if (!string.IsNullOrEmpty(paths))
                {
                    listAllFilesTmp = paths.Split(",").Select(x => x.ToString()).ToList();
                }
                List<Video> listVideosToDelete = new List<Video>();
                Expression<Func<Video, bool>> expressionVideosToDelete = x => !listAllFilesTmp.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                listVideosToDelete = _unitOfWork.Videos.GetAllByExpressionAsync(expressionVideosToDelete).Result.ToList();
                if (listVideosToDelete.Count > 0)
                {
                    AddMessage(LogType.Information, "There are videos ready to delete");
                    List<string> listIdsVideosDelete = listVideosToDelete.Select(o => o.Id.ToString()).ToList();
                    Expression<Func<VideoProfile, bool>> expressionVideoProfilesToDelete = x => listIdsVideosDelete.Contains(x.VideoId.ToString());
                    _unitOfWork.VideoProfiles.DeleteByExpression(expressionVideoProfilesToDelete);
                    _unitOfWork.Videos.DeleteList(listVideosToDelete);
                    _unitOfWork.Save();
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
        }




    }
}
