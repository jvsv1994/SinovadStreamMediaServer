using SinovadMediaServer.Application.Builder;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class LibraryService : ILibraryService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;
        private SearchMediaLog? _searchMediaLog { get; set; }

        private readonly SearchMediaLogBuilder _searchMediaBuilder;

        private readonly ITmdbService _tmdbService;

        private readonly IImdbService _imdbService;

        public LibraryService(IUnitOfWork unitOfWork, SharedService sharedService, ITmdbService tmdbService, IImdbService imdbService, SearchMediaLogBuilder searchMediaBuilder)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
            _searchMediaBuilder = searchMediaBuilder;
            _tmdbService = tmdbService;
            _imdbService = imdbService;
        }

        public async Task<Response<LibraryDto>> GetAsync(int id)
        {
            var response = new Response<LibraryDto>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAsync(id);
                response.Data = result.MapTo<LibraryDto>();
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

        public async Task<Response<List<LibraryDto>>> GetAllLibraries()
        {
            var response = new Response<List<LibraryDto>>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAllAsync();
                response.Data = result.MapTo<List<LibraryDto>>();
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

        public Response<object> Create(LibraryDto libraryDto)
        {
            var response = new Response<object>();
            try
            {
                var library = libraryDto.MapTo<Library>();
                _unitOfWork.Libraries.Add(library);
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

        public Response<object> CreateList(List<LibraryDto> list)
        {
            var response = new Response<object>();
            try
            {
                var mediaServers = list.MapTo<List<Library>>();
                _unitOfWork.Libraries.AddList(mediaServers);
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

        public Response<object> Update(LibraryDto libraryDto)
        {
            var response = new Response<object>();
            try
            {
                var library = libraryDto.MapTo<Library>();
                _unitOfWork.Libraries.Update(library);
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

        public Response<object> Delete(int id)
        {
            var response = new Response<object>();
            try
            {
                _unitOfWork.VideoProfiles.DeleteByExpression(it => it.Video.LibraryId == id);
                _unitOfWork.Videos.DeleteByExpression(it => it.LibraryId == id);
                _unitOfWork.Libraries.Delete(id);
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

        public Response<object> DeleteList(string ids)
        {
            var response = new Response<object>();
            try
            {
                List<int> listIds = new List<int>();
                if (!string.IsNullOrEmpty(ids))
                {
                    listIds = ids.Split(",").Select(x => Convert.ToInt32(x)).ToList();
                }
                _unitOfWork.Libraries.DeleteByExpression(x => listIds.Contains(x.Id));
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

        public Response<object> SearchFiles(SearchFilesDto searchFilesDto)
        {
            var response = new Response<object>();
            try
            {
                if (searchFilesDto.LogIdentifier == null)
                {
                    searchFilesDto.LogIdentifier = System.Guid.NewGuid().ToString();
                }
                IEnumerable<string> filesToAdd = new List<string>();

                foreach (var library in searchFilesDto.ListLibraries)
                {
                    var listPaths = Directory.GetFiles(library.PhysicalPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mkv") || s.EndsWith(".mp4") || s.EndsWith(".avi")).ToList();

                    if (library.MediaTypeCatalogDetailId == (int)MediaType.Movie)
                    {
                        RegisterMoviesAndVideos(library.Id, listPaths);
                    }
                    if (library.MediaTypeCatalogDetailId == (int)MediaType.TvSerie)
                    {
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



        private void RegisterMoviesAndVideos(int libraryId, List<string> paths)
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

        private void DeleteVideosNotFoundInLibrary(int libraryId, List<string> paths)
        {
            try
            {
                AddMessage(LogType.Information, "Check if there are videos to delete");
                List<Video> listVideosToDelete = new List<Video>();
                Expression<Func<Video, bool>> expressionVideosToDelete = x => !paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
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

        private List<string> GetFilesToAdd(int libraryId, List<string> paths)
        {
            List<string> filesToAdd = new List<string>();
            try
            {
                AddMessage(LogType.Information, "Prepare video paths");
   
                List<Video> listVideosToAvoidAdd = new List<Video>();
                if (paths.Count > 0)
                {
                    AddMessage(LogType.Information, "Check if videos were already registered");
                    Expression<Func<Video, bool>> expressionVideosToAvoidAdd = x => paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                    listVideosToAvoidAdd = _unitOfWork.Videos.GetAllByExpressionAsync(expressionVideosToAvoidAdd).Result.ToList();
                }
                if (listVideosToAvoidAdd.Count > 0)
                {
                    List<string> listVideoPathsToAvoidAdd = listVideosToAvoidAdd.Select(o => o.PhysicalPath).ToList();
                    filesToAdd = paths.FindAll(filePath => listVideoPathsToAvoidAdd.IndexOf(filePath) == -1).ToList();
                }
                else
                {
                    filesToAdd = paths;
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
            return filesToAdd;
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








    }
}
