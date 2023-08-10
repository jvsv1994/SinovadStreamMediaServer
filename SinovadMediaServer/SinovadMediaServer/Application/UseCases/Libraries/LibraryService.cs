using SinovadMediaServer.Application.Builder;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Infrastructure;
using SinovadMediaServer.Infrastructure.SinovadApi;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System.Linq.Expressions;
using TMDbLib.Objects.Movies;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class LibraryService : ILibraryService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;

        private SharedData _sharedData;

        private readonly SinovadApiService _sinovadApiService;
        private SearchMediaLog? _searchMediaLog { get; set; }

        private readonly SearchMediaLogBuilder _searchMediaBuilder;

        private readonly ITmdbService _tmdbService;

        private readonly IImdbService _imdbService;

        public LibraryService(IUnitOfWork unitOfWork, SinovadApiService sinovadApiService, SharedData sharedData, SharedService sharedService, ITmdbService tmdbService, IImdbService imdbService, SearchMediaLogBuilder searchMediaBuilder)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
            _searchMediaBuilder = searchMediaBuilder;
            _tmdbService = tmdbService;
            _imdbService = imdbService;
            _sinovadApiService = sinovadApiService;
            _sharedData = sharedData;
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
                        RegisterTvSeriesAndVideos(library.Id, listPaths);
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


        private MediaItem CreateMediaItemByTvSerie(string searchQuery,TvSerieDto tvSerie,MetadataAgents metadataAgents)
        {
            var mediaItem = new MediaItem();
            mediaItem.SearchQuery = searchQuery;
            mediaItem.Title = tvSerie.Name;
            mediaItem.Overview = tvSerie.Overview;
            mediaItem.SourceId = tvSerie.Id.ToString();
            mediaItem.MediaTypeId = MediaType.TvSerie;
            mediaItem.MetadataAgentsId = metadataAgents;
            mediaItem.PosterPath = tvSerie.PosterPath;
            mediaItem.FirstAirDate = tvSerie.FirstAirDate;
            mediaItem.LastAirDate = tvSerie.LastAirDate;

            var listMediaItemGenres = new List<MediaItemGenre>();

            foreach (var genre in tvSerie.ListGenres)
            {
                var mediaGenre = _unitOfWork.MediaGenres.GetByExpression(x => x.Name != null && x.Name.ToLower().Trim() == genre.Name.ToLower().Trim());
                if (mediaGenre == null)
                {
                    mediaGenre = new MediaGenre();
                    mediaGenre.Name = genre.Name;
                    mediaGenre = _unitOfWork.MediaGenres.Add(mediaGenre);
                    _unitOfWork.Save();
                }
                var mediaItemGenre = new MediaItemGenre();
                mediaItemGenre.MediaGenreId = mediaGenre.Id;
                listMediaItemGenres.Add(mediaItemGenre);
            }
            mediaItem.MediaItemGenres = listMediaItemGenres;
            mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
            _unitOfWork.Save();
            return mediaItem;
        }

        private void RegisterTvSeriesAndVideos(int libraryId, List<string> paths)
        {
            var tvSerieService = new TvSerieService(_sinovadApiService);
            var episodeService = new EpisodeService(_sinovadApiService);
            AddMessage(LogType.Information, "Starting search tv series");
            try
            {
                var filesToAdd = GetFilesToAdd(libraryId, paths);
                DeleteVideosNotFoundInLibrary(libraryId, paths);
                var listTvSeriesTMDFinded = new List<TvSerieDto>();
                var listVideosTmp = new List<VideoDto>();
                var listVideosFindedInAnyDataBase = new List<VideoDto>();
                if (filesToAdd != null && filesToAdd.Count > 0)
                {
                    for (int i = 0; i < filesToAdd.Count; i++)
                    {
                        try
                        {
                            var path = filesToAdd[i];
                            AddMessage(LogType.Information, "Processing new file with path " + path);
                            var splitted = path.Split("\\");
                            var fileName = splitted[splitted.Length - 1];
                            var physicalPath = path;
                            var tvSerieNameTmp = fileName.Split(".")[0];
                            var tvSerieNameTmp2 = tvSerieNameTmp.Split(" ");
                            var seasonEpisodeText = tvSerieNameTmp2[tvSerieNameTmp2.Length - 1];
                            var tvSerieName = tvSerieNameTmp.Replace(seasonEpisodeText, "").Trim();
                            if (seasonEpisodeText.IndexOf("E") != -1)
                            {
                                var seasonepisodeArray = seasonEpisodeText.Split("E");
                                var seasonText = seasonepisodeArray[0].Replace("S", "");
                                var episodeText = seasonepisodeArray[1];
                                var seasonNumber = int.Parse(seasonText);
                                var episodeNumber = int.Parse(episodeText);
                                var mediaItem = _unitOfWork.MediaItems.GetByExpression(x => x.SearchQuery!=null && x.SearchQuery.ToLower().Trim() == tvSerieName.ToLower().Trim() && x.MediaTypeId == MediaType.TvSerie);
                                if(mediaItem==null)
                                {        
                                    //Search Media in SinovadDb
                                    var result = tvSerieService.SearchAsync(tvSerieName).Result;
                                    if (result.Data != null)
                                    {
                                        var tvSerie = result.Data;
                                        mediaItem = CreateMediaItemByTvSerie(tvSerieName, tvSerie,MetadataAgents.SinovadDb);
                                    }
                                    if(mediaItem==null)
                                    {
                                        //Search Media in Tmdb
                                        var tvSerie = _tmdbService.SearchTvShow(tvSerieName);
                                        mediaItem = CreateMediaItemByTvSerie(tvSerieName, tvSerie, MetadataAgents.TMDb);
                                    }
                                }
                                if(mediaItem==null)
                                {
                                    mediaItem = new MediaItem();
                                    mediaItem.Title = tvSerieName;
                                    mediaItem.SearchQuery = tvSerieName;
                                    mediaItem.MediaTypeId = MediaType.TvSerie;
                                    mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
                                    _unitOfWork.Save();
                                }
                                var mediaFile = new MediaFile();
                                mediaFile.LibraryId = libraryId;
                                mediaFile.EpisodeNumber = episodeNumber;
                                mediaFile.SeasonNumber = seasonNumber;
                                mediaFile.PhysicalPath = physicalPath;               
                                mediaFile.MediaItemId = mediaItem.Id;
                                mediaFile.Title = mediaItem.Title;
                                mediaFile.EpisodeName = "Episodio " + episodeNumber;
                                if (mediaItem.MetadataAgentsId == MetadataAgents.SinovadDb)
                                {
                                    var res = episodeService.GetTvEpisodeAsync(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber).Result;
                                    if (res.Data != null)
                                    {
                                        var episode = res.Data;
                                        mediaFile.Subtitle = "T" + seasonNumber + ":E" + episodeNumber + " " + episode.Title;
                                        mediaFile.EpisodeName = episode.Title;
                                        mediaFile.Overview = episode.Summary;
                                    }
                                }
                                if (mediaItem.MetadataAgentsId == MetadataAgents.TMDb)
                                {
                                    EpisodeDto episode = _tmdbService.GetTvEpisode(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber);
                                    if (episode != null)
                                    {
                                        mediaFile.Subtitle = "T" + seasonNumber + ":E" + episodeNumber + " " + episode.Title;
                                        mediaFile.EpisodeName = episode.Title;
                                        mediaFile.Overview = episode.Summary;
                                        mediaFile.PosterPath = episode.StillPath;
                                    }
                                }
                                _unitOfWork.MediaFiles.Add(mediaFile);
                                _unitOfWork.Save();
                            }
                            else
                            {
                                AddMessage(LogType.Information, "The file path does not comply with the format of an episode");
                            }
                        }
                        catch (Exception e)
                        {
                            AddMessage(LogType.Error, e.Message);
                        }
                    }
                }else
                {
                    AddMessage(LogType.Information, "New files not found");
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
            AddMessage(LogType.Information, "Ending search tv series");
        }


        private MediaItem CreateMediaItemByMovie(string searchQuery, MovieDto movie)
        {
            var mediaItem = new MediaItem();
            mediaItem.SearchQuery = searchQuery;
            mediaItem.Title = movie.Title;
            mediaItem.Overview = movie.Overview;
            mediaItem.MediaTypeId = MediaType.Movie;
            mediaItem.PosterPath = movie.PosterPath;
            mediaItem.ReleaseDate = movie.ReleaseDate;
            if (movie.TmdbId != null && movie.TmdbId > 0)
            {
                mediaItem.SourceId = movie.TmdbId.ToString();
                mediaItem.MetadataAgentsId = MetadataAgents.TMDb;
            }else if (movie.Imdbid != null && movie.Imdbid != "")
            {
                mediaItem.SourceId = movie.Imdbid.ToString();
                mediaItem.MetadataAgentsId = MetadataAgents.IMDb;
            }

            var listMediaItemGenres = new List<MediaItemGenre>();
            foreach (var genreName in movie.ListGenreNames)
            {
                var mediaGenre = _unitOfWork.MediaGenres.GetByExpression(x => x.Name != null && x.Name.ToLower().Trim() == genreName.ToLower().Trim());
                if (mediaGenre == null)
                {
                    mediaGenre = new MediaGenre();
                    mediaGenre.Name = genreName;
                    mediaGenre = _unitOfWork.MediaGenres.Add(mediaGenre);
                    _unitOfWork.Save();
                }
                var mediaItemGenre = new MediaItemGenre();
                mediaItemGenre.MediaGenreId = mediaGenre.Id;
                listMediaItemGenres.Add(mediaItemGenre);
            }
            mediaItem.MediaItemGenres = listMediaItemGenres;
            mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
            _unitOfWork.Save();
            return mediaItem;
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
                            var mediaItem = _unitOfWork.MediaItems.GetByExpression(x => x.SearchQuery != null && x.SearchQuery.ToLower().Trim() == movieName.ToLower().Trim() && x.MediaTypeId==MediaType.Movie);
                            if(mediaItem==null)
                            {
                                MovieDto movie = GetMovieFromExternalDataBase(movieName, year);
                                if (movie != null)
                                {
                                    mediaItem = CreateMediaItemByMovie(movieName, movie);
                                }
                            }
                            if(mediaItem == null) {
                                mediaItem = new MediaItem();
                                mediaItem.Title = movieName;
                                mediaItem.SearchQuery = movieName;
                                mediaItem.MediaTypeId = MediaType.Movie;
                                mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
                                _unitOfWork.Save();
                            }
                            var mediaFile = new MediaFile();
                            mediaFile.LibraryId = libraryId;
                            mediaFile.PhysicalPath = physicalPath;
                            mediaFile.MediaItemId = mediaItem.Id;
                            mediaFile.Title = mediaItem.Title;
                            mediaFile.Overview=mediaItem.Overview;
                            mediaFile.PosterPath = mediaItem.PosterPath;
                            _unitOfWork.MediaFiles.Add(mediaFile);
                            _unitOfWork.Save();
                        }catch (Exception e)
                        {
                            AddMessage(LogType.Error, e.Message);
                        }
                    }
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

        private void RegisterVideos(List<VideoDto> listVideosDto)
        {
            if (listVideosDto.Count > 0)
            {
                var listVideos = listVideosDto.MapTo<List<Video>>();
                AddMessage(LogType.Information, "Saving all new videos");
                _unitOfWork.Videos.AddList(listVideos);
                _unitOfWork.Save();  
            }
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
