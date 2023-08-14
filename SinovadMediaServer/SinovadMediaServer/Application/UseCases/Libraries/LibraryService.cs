using Microsoft.AspNetCore.SignalR;
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
using SinovadMediaServer.SignailIR;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class LibraryService : ILibraryService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;

        private SharedData _sharedData;

        private readonly SinovadApiService _sinovadApiService;
        private SearchMediaLog? _searchMediaLog { get; set; }


        private readonly ITmdbService _tmdbService;

        private readonly IImdbService _imdbService;

        private readonly IHubContext<CustomHub> _hubContext;

        public LibraryService(IUnitOfWork unitOfWork, SinovadApiService sinovadApiService, SharedData sharedData, SharedService sharedService, ITmdbService tmdbService, IImdbService imdbService, SearchMediaLogBuilder searchMediaBuilder,IHubContext<CustomHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
            _tmdbService = tmdbService;
            _imdbService = imdbService;
            _sinovadApiService = sinovadApiService;
            _sharedData = sharedData;
            _hubContext = hubContext;
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
                _hubContext.Clients.All.SendAsync("RefreshLibraries");
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
                _hubContext.Clients.All.SendAsync("RefreshLibraries");
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
                _hubContext.Clients.All.SendAsync("RefreshLibraries");
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
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => x.LibraryId==id;
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
                _unitOfWork.Libraries.Delete(id);
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
                _hubContext.Clients.All.SendAsync("RefreshLibraries");
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
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => listIds.Contains((int)x.LibraryId);
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
                _unitOfWork.Libraries.DeleteByExpression(x=> listIds.Contains(x.Id));
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
                _hubContext.Clients.All.SendAsync("RefreshLibraries");
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
                    var listPaths = new List<string>();

                    var exists=Directory.Exists(library.PhysicalPath);
                    if(exists)
                    {
                        listPaths = Directory.GetFiles(library.PhysicalPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mkv") || s.EndsWith(".mp4") || s.EndsWith(".avi")).ToList();
                        if (library.MediaTypeCatalogDetailId == (int)MediaType.Movie)
                        {
                            RegisterMovieFiles(library.Id, listPaths);
                        }
                        if (library.MediaTypeCatalogDetailId == (int)MediaType.TvSerie)
                        {
                            RegisterTvSeriesFiles(library.Id, listPaths);
                        }
                    }else{
                        Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => x.LibraryId == library.Id;
                        DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
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

        private void DeleteMediaFilesByExpresion(Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete)
        {
            var listMediaFilesToDelete = _unitOfWork.MediaFiles.GetAllByExpression(expresionMediaFilesToDelete).ToList();
            if (listMediaFilesToDelete.Count > 0)
            {
                AddMessage(LogType.Information, "There are files ready to delete");
                List<string> listIdsVideosDelete = listMediaFilesToDelete.Select(o => o.Id.ToString()).ToList();
                Expression<Func<MediaFilePlayback, bool>> expressionVideoProfilesToDelete = x => listIdsVideosDelete.Contains(x.MediaFileId.ToString());
                _unitOfWork.MediaFilePlaybacks.DeleteByExpression(expressionVideoProfilesToDelete);
                _unitOfWork.MediaFiles.DeleteList(listMediaFilesToDelete);
                _unitOfWork.Save();
                _hubContext.Clients.All.SendAsync("RefreshMediaItems");
            }
        }


        private void RegisterTvSeriesFiles(int libraryId, List<string> paths)
        {
            var sinovadMediaDataBaseService = new SinovadMediaDatabaseService(_sinovadApiService);
            AddMessage(LogType.Information, "Starting search tv series");
            try
            {
                var filesToAdd = GetFilesToAdd(libraryId, paths);
                DeleteVideosNotFoundInLibrary(libraryId, paths);
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
                                var mediaItemList = _unitOfWork.MediaItems.GetAllByExpression(x => x.SearchQuery!=null && x.SearchQuery.ToLower().Trim() == tvSerieName.ToLower().Trim() && x.MediaTypeId == MediaType.TvSerie);
                                MediaItem mediaItem = null;
                                if(mediaItemList!=null && mediaItemList.Count()>0)
                                {
                                    mediaItem = mediaItemList.FirstOrDefault();
                                }
                                if(mediaItem==null)
                                {
                                    //Search Media in SinovadDb
                                    AddMessage(LogType.Information, "Searching " + tvSerieName+" in Sinovad Media DataBase");
                                    var result = sinovadMediaDataBaseService.SearchTvSerieAsync(tvSerieName).Result;
                                    if (result.Data != null)
                                    {
                                        AddMessage(LogType.Information,"Found "+ tvSerieName + " in Sinovad Media DataBase");
                                        var sinovadTvSerie = result.Data;
                                        var mediaItemDto = new MediaItemDto();
                                        mediaItemDto.SourceId = sinovadTvSerie.Id.ToString();
                                        mediaItemDto.FirstAirDate = sinovadTvSerie.FirstAirDate;
                                        mediaItemDto.LastAirDate = sinovadTvSerie.LastAirDate;
                                        mediaItemDto.Overview = sinovadTvSerie.Overview;
                                        mediaItemDto.Actors=sinovadTvSerie.Actors;
                                        mediaItemDto.Directors=sinovadTvSerie.Directors;
                                        if(sinovadTvSerie.ListGenres!=null)
                                        {
                                            mediaItemDto.Genres = string.Join(", ", sinovadTvSerie.ListGenres.Select(x => x.Name));
                                        }
                                        mediaItemDto.PosterPath = sinovadTvSerie.PosterPath;
                                        mediaItemDto.Title = sinovadTvSerie.Name;
                                        mediaItemDto.ExtendedTitle = sinovadTvSerie.Name + (sinovadTvSerie.LastAirDate.Value.Year > sinovadTvSerie.FirstAirDate.Value.Year ? " (" + sinovadTvSerie.FirstAirDate.Value.Year + "-" + sinovadTvSerie.LastAirDate.Value.Year + ")" : " (" + sinovadTvSerie.FirstAirDate.Value.Year + ")");
                                        mediaItemDto.MediaTypeId = MediaType.TvSerie;
                                        mediaItemDto.MetadataAgentsId = MetadataAgents.SinovadDb;
                                        mediaItemDto.ListGenres = sinovadTvSerie.ListGenres.MapTo<List<MediaGenreDto>>();
                                        mediaItemDto.SearchQuery = tvSerieName;
                                        mediaItem = CreateMediaItem(mediaItemDto);
                                    }else{
                                        AddMessage(LogType.Information,"Not found "+tvSerieName + " in Sinovad Media DataBase");
                                    }
                                    if (mediaItem==null)
                                    {
                                        AddMessage(LogType.Information, "Searching " + tvSerieName + " in Tmdb DataBase");
                                        //Search Media in Tmdb
                                        var mediaItemDto = _tmdbService.SearchTvShow(tvSerieName);
                                        if(mediaItemDto!=null)
                                        {
                                            AddMessage(LogType.Information, "Found " + tvSerieName + " in Tmdb DataBase");
                                            mediaItemDto.SearchQuery = tvSerieName;
                                            mediaItem = CreateMediaItem(mediaItemDto);
                                        }else
                                        {
                                            AddMessage(LogType.Information, "Not found " + tvSerieName + " in Tmdb DataBase");
                                        }
                                    }
                                }else
                                {
                                    AddMessage(LogType.Information, "Found " + tvSerieName + " in Local Storage");
                                }
                                if (mediaItem==null)
                                {
                                    AddMessage(LogType.Information, "Not found " + tvSerieName + " in any external DataBase");
                                    mediaItem = new MediaItem();
                                    mediaItem.Title = tvSerieName;
                                    mediaItem.SearchQuery = tvSerieName;
                                    mediaItem.MediaTypeId = MediaType.TvSerie;
                                    mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
                                    _unitOfWork.Save();
                                }
                                MediaSeason season = null;
                                var seasonList = _unitOfWork.MediaSeasons.GetAllByExpression(x => x.MediaItemId == mediaItem.Id && x.SeasonNumber == seasonNumber);
                                if(seasonList!=null && seasonList.Count()>0)
                                {
                                    season = seasonList.FirstOrDefault();
                                }
                                if (season == null)
                                {
                                    if (mediaItem.MetadataAgentsId == MetadataAgents.TMDb)
                                    {
                                        var seasonDto = _tmdbService.GetTvSeason(int.Parse(mediaItem.SourceId), seasonNumber);
                                        if (seasonDto != null)
                                        {
                                            season = seasonDto.MapTo<MediaSeason>();
                                            season.MediaItemId = mediaItem.Id;
                                        }
                                    }
                                    if (mediaItem.MetadataAgentsId == MetadataAgents.SinovadDb)
                                    {
                                        var res = sinovadMediaDataBaseService.GetTvSeasonAsync(int.Parse(mediaItem.SourceId), seasonNumber).Result;
                                        if (res.Data != null)
                                        {
                                            var sinovadSeason= res.Data;
                                            season = new MediaSeason();
                                            season.MediaItemId=mediaItem.Id;
                                            season.SourceId = sinovadSeason.Id.ToString();
                                            season.Name = sinovadSeason.Name;
                                            season.SeasonNumber= sinovadSeason.SeasonNumber;
                                            season.Overview = sinovadSeason.Summary;
                                        }
                                    }
                                    if (season == null)
                                    {
                                        season = new MediaSeason();
                                        season.MediaItemId = mediaItem.Id;
                                        season.SeasonNumber = seasonNumber;
                                        season.Name = "Temporada " + seasonNumber;
                                    }
                                    season = _unitOfWork.MediaSeasons.Add(season);
                                    _unitOfWork.Save();
                                }
                                var episodeList = _unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItem.Id && x.SeasonNumber == seasonNumber && x.EpisodeNumber == episodeNumber);
                                MediaEpisode episode = null;
                                if(episodeList!=null && episodeList.Count()>0)
                                {
                                    episode= episodeList.FirstOrDefault();
                                }
                                if(episode == null)
                                {
                                    if (mediaItem.MetadataAgentsId == MetadataAgents.TMDb)
                                    {         
                                        MediaEpisodeDto episodeDto = _tmdbService.GetTvEpisode(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber);
                                        if (episodeDto != null)
                                        {
                                            episode = episodeDto.MapTo<MediaEpisode>();
                                            episode.MediaItemId = mediaItem.Id;
                                        }                          
                                    }
                                    if (mediaItem.MetadataAgentsId == MetadataAgents.SinovadDb)
                                    {                     
                                        var res = sinovadMediaDataBaseService.GetTvEpisodeAsync(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber).Result;
                                        if (res.Data != null)
                                        {
                                            var sinovadEpisode = res.Data;
                                            episode = new MediaEpisode();
                                            episode.MediaItemId = mediaItem.Id;
                                            episode.SeasonNumber = seasonNumber;
                                            episode.EpisodeNumber = episodeNumber;
                                            episode.Name = sinovadEpisode.Title;
                                            episode.PosterPath = sinovadEpisode.ImageUrl;
                                            episode.Overview = sinovadEpisode.Summary;
                                            episode.SourceId = sinovadEpisode.Id.ToString();
                                        }          
                                    }
                                    if (episode == null)
                                    {
                                        episode = new MediaEpisode();
                                        episode.MediaItemId = mediaItem.Id;
                                        episode.SeasonNumber = seasonNumber;
                                        episode.EpisodeNumber = episodeNumber;
                                        episode.Name = "Episodio " + episodeNumber;
                                    }
                                    episode = _unitOfWork.MediaEpisodes.Add(episode);
                                    _unitOfWork.Save();
                                }

                                var mediaFile = new MediaFile();
                                mediaFile.LibraryId = libraryId;
                                mediaFile.PhysicalPath = physicalPath;
                                mediaFile.MediaItemId = mediaItem.Id;
                                mediaFile.MediaEpisodeId = episode.Id;                
                                _unitOfWork.MediaFiles.Add(mediaFile);
                                _unitOfWork.Save();
                                if(IsMultipleOf(i,50))
                                {
                                    _hubContext.Clients.All.SendAsync("RefreshMediaItems");
                                }
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
            _hubContext.Clients.All.SendAsync("RefreshMediaItems");
        }

        private bool IsMultipleOf(int multiple, int dividend)
        {
            return (multiple % dividend) == 0;
        }


        private MediaItem CreateMediaItem(MediaItemDto mediaItemDto)
        {
            var mediaItem = mediaItemDto.MapTo<MediaItem>();
            var listMediaItemGenres = new List<MediaItemGenre>();
            foreach (var genre in mediaItemDto.ListGenres)
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

        private void RegisterMovieFiles(int libraryId, List<string> paths)
        {
            AddMessage(LogType.Information, "Starting search movies");
            try
            {
                var filesToAdd = GetFilesToAdd(libraryId, paths);
                DeleteVideosNotFoundInLibrary(libraryId, paths);
                if (filesToAdd != null && filesToAdd.Count > 0)
                {
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
                            var mediaItemList = _unitOfWork.MediaItems.GetAllByExpression(x => x.SearchQuery != null && x.SearchQuery.ToLower().Trim() == movieName.ToLower().Trim() && x.MediaTypeId==MediaType.Movie);
                            MediaItem mediaItem=null;
                            if(mediaItemList!=null && mediaItemList.Count()>0)
                            {
                                mediaItem= mediaItemList.FirstOrDefault();
                            }
                            if(mediaItem==null)
                            {
                                MediaItemDto mediaItemDto = GetMovieFromExternalDataBase(movieName, year);
                                if (mediaItemDto != null)
                                {
                                    mediaItem = CreateMediaItem(mediaItemDto);
                                }
                            }
                            if(mediaItem == null) {
                                mediaItem = new MediaItem();
                                mediaItem.Title = movieName;
                                mediaItem.ExtendedTitle = movieName+ "("+year+")";
                                mediaItem.SearchQuery = movieName;
                                mediaItem.MediaTypeId = MediaType.Movie;
                                mediaItem = _unitOfWork.MediaItems.Add(mediaItem);
                                _unitOfWork.Save();
                            }
                            var mediaFile = new MediaFile();
                            mediaFile.LibraryId = libraryId;
                            mediaFile.PhysicalPath = physicalPath;
                            mediaFile.MediaItemId = mediaItem.Id;
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
            _hubContext.Clients.All.SendAsync("RefreshMediaItems");
        }

        private void DeleteVideosNotFoundInLibrary(int libraryId, List<string> paths)
        {
            try
            {
                AddMessage(LogType.Information, "Check if there are videos to delete");
                List<MediaFile> listVideosToDelete = new List<MediaFile>();
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => !paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
        }

        private MediaItemDto GetMovieFromExternalDataBase(string movieName, string year)
        {
            MediaItemDto movie = _tmdbService.SearchMovie(movieName, year);
            if (movie != null && movie.ListGenres != null && movie.ListGenres.Count > 0)
            {
                AddMessage(LogType.Information, "Movie finded in TMDb Data Base " + movie.Title + " " + year);
                return movie;
            }
            else
            {
                movie = _imdbService.SearchMovie(movieName, year);
                if (movie != null && movie.ListGenres != null && movie.ListGenres.Count > 0)
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
   
                List<MediaFile> listVideosToAvoidAdd = new List<MediaFile>();
                if (paths.Count > 0)
                {
                    AddMessage(LogType.Information, "Check if videos were already registered");
                    Expression<Func<MediaFile, bool>> expressionVideosToAvoidAdd = x => paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                    listVideosToAvoidAdd = _unitOfWork.MediaFiles.GetAllByExpressionAsync(expressionVideosToAvoidAdd).Result.ToList();
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


        private List<ItemsGroupDto> BuildListItemsGroup(List<ItemDto> listItems, List<ItemDto> listLastAddedItems, List<ItemDto> listItemsWatched)
        {
            listLastAddedItems = listLastAddedItems.OrderByDescending(c => c.Created).ToList();
            if (listItemsWatched != null && listItemsWatched.Count > 0)
            {
                listItemsWatched = listItemsWatched.OrderByDescending(c => c.LastModified).ToList();
            }
            var genres = _unitOfWork.MediaGenres.GetAllAsync().Result;

            var listItemsGroup = new List<ItemsGroupDto>();
            if (listItemsWatched != null && listItemsWatched.Count > 0)
            {
                var itemsGroup = new ItemsGroupDto();
                itemsGroup.Id = -1;
                itemsGroup.Name = "Continuar viendo";
                itemsGroup.ListItems = listItemsWatched;
                itemsGroup.MediaServerId = this._sharedData.MediaServerData.Id;
                listItemsGroup.Add(itemsGroup);
            }
            if (listLastAddedItems != null && listLastAddedItems.Count > 0)
            {
                var itemsGroup = new ItemsGroupDto();
                itemsGroup.Id = 0;
                itemsGroup.Name = "Agregados recientemente";
                itemsGroup.ListItems = listLastAddedItems;
                itemsGroup.MediaServerId = this._sharedData.MediaServerData.Id;
                listItemsGroup.Add(itemsGroup);
            }
            if (genres != null && genres.Count() > 0)
            {
                foreach (var genre in genres)
                {
                    var list = listItems.Where(ele => ele.GenreId == genre.Id).ToList();
                    if (list != null && list.Count > 0)
                    {
                        var itemsGroup = new ItemsGroupDto();
                        itemsGroup.Id = genre.Id;
                        itemsGroup.Name = genre.Name;
                        itemsGroup.ListItems = list;
                        itemsGroup.MediaServerId = this._sharedData.MediaServerData.Id;
                        listItemsGroup.Add(itemsGroup);
                    }
                }
            }
            return listItemsGroup;
        }

        public Response<List<ItemsGroupDto>> GetMediaItemsByLibrary(int libraryId, int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = _unitOfWork.MediaItems.GetItemsByLibrary(libraryId);
                List<ItemDto> listLastAddedItems = _unitOfWork.MediaItems.GetItemsRecentlyAddedByLibrary(libraryId);
                List<ItemDto> listItemsWatched = _unitOfWork.MediaItems.GetItemsByLibraryAndProfile(libraryId,profileId);
                response.Data = BuildListItemsGroup(listItems, listLastAddedItems, listItemsWatched);
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

        public Response<List<ItemsGroupDto>> GetMediaItemsByMediaType(MediaType mediaTypeId, int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = _unitOfWork.MediaItems.GetAllItemsByMediaType(mediaTypeId);
                List<ItemDto> listLastAddedItems = _unitOfWork.MediaItems.GetAllItemsRecentlyAddedByMediaType(mediaTypeId);
                List<ItemDto> listItemsWatched = _unitOfWork.MediaItems.GetAllItemsByMediaTypeAndProfile(mediaTypeId, profileId);
                response.Data = BuildListItemsGroup(listItems, listLastAddedItems, listItemsWatched);
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

        public Response<List<ItemsGroupDto>> GetAllMediaItems(int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = _unitOfWork.MediaItems.GetAllItems();
                List<ItemDto> listLastAddedItems = _unitOfWork.MediaItems.GetAllItemsRecentlyAdded();
                List<ItemDto> listItemsWatched = _unitOfWork.MediaItems.GetAllItemsByProfile(profileId);
                response.Data = BuildListItemsGroup(listItems, listLastAddedItems, listItemsWatched);
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

        public Response<List<ItemDto>> GetAllMediaItemsBySearchQuery(string searchQuery)
        {
            var response = new Response<List<ItemDto>>();
            try
            {
                List<ItemDto> listItems = _unitOfWork.MediaItems.GetAllItemsBySearchQuery(searchQuery);
                response.Data = listItems;
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

        public Response<ItemDetailDto> GetMediaItemDetail(int mediaItemId)
        {

            var response = new Response<ItemDetailDto>();
            try
            {
                ItemDetailDto itemDetail = new ItemDetailDto();
                var mediaItem = _unitOfWork.MediaItems.Get(mediaItemId);
                itemDetail.MediaItem = mediaItem.MapTo<MediaItemDto>();
                var mediaFiles = _unitOfWork.MediaFiles.GetAllByExpression(x=>x.MediaItemId == mediaItemId);
                itemDetail.ListMediaFiles = mediaFiles.MapTo<List<MediaFileDto>>();
                var listSeasonsToAdd = new List<MediaSeasonDto>();
                if(mediaItem.MediaTypeId==MediaType.TvSerie)
                {
                    var listSeasons=_unitOfWork.MediaSeasons.GetAllByExpression(x=>x.MediaItemId==mediaItemId).DistinctBy(x=>x.SeasonNumber).MapTo<List<MediaSeasonDto>>();
                    var listEpisodes= _unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItemId).MapTo<List<MediaEpisodeDto>>();
                    foreach (var season in listSeasons)
                    {
                        var existMediaFiles = false;
                        season.ListEpisodes = listEpisodes.Where(x => x.SeasonNumber == season.SeasonNumber).DistinctBy(x => x.EpisodeNumber).OrderBy(x=>x.EpisodeNumber).ToList();
                        foreach (var episodeDto in season.ListEpisodes)
                        {
                           var listMediaFiles=itemDetail.ListMediaFiles.Where(x => x.MediaEpisodeId == episodeDto.Id).ToList();
                           episodeDto.ListMediaFiles=listMediaFiles;
                            if(episodeDto.ListMediaFiles.Count>0)
                            {
                                existMediaFiles = true;
                            }
                        }
                        if(existMediaFiles)
                        {
                            listSeasonsToAdd.Add(season);
                        }
                    }
                }
                itemDetail.ListSeasons= listSeasonsToAdd;
                response.Data = itemDetail;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<ItemDetailDto> GetMediaItemDetailByMediaFileAndProfile(int mediaFileId,int profileId)
        {
            var response = new Response<ItemDetailDto>();
            try
            {
                ItemDetailDto itemDetail = new ItemDetailDto();
                var mediaFile=_unitOfWork.MediaFiles.Get(mediaFileId);
                if(mediaFile!=null)
                {
                    if (mediaFile.MediaItemId != null)
                    {
                        var mediaItemId = (int)mediaFile.MediaItemId;
                        var mediaItem = _unitOfWork.MediaItems.Get(mediaItemId);
                        itemDetail.MediaItem = mediaItem.MapTo<MediaItemDto>();
                        var mediaFiles = _unitOfWork.MediaFiles.GetAllByExpression(x => x.MediaItemId == mediaItemId);
                        itemDetail.ListMediaFiles = mediaFiles.MapTo<List<MediaFileDto>>();
                        var mediaFilePlayback= _unitOfWork.MediaFilePlaybacks.GetByExpression(x=>x.MediaFileId==mediaFileId && x.ProfileId==profileId);
                        if (mediaFilePlayback!=null)
                        {
                            mediaFilePlayback.MediaFile = null;
                            itemDetail.LastMediaFilePlayback = mediaFilePlayback.MapTo<MediaFilePlaybackDto>();
                        }
                        if (mediaItem.MediaTypeId == MediaType.TvSerie)
                        {
                            var listSeasonsToAdd = new List<MediaSeasonDto>();
                            var listSeasons = _unitOfWork.MediaSeasons.GetAllByExpression(x => x.MediaItemId == mediaItemId).DistinctBy(x => x.SeasonNumber).MapTo<List<MediaSeasonDto>>();
                            var listEpisodes = _unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItemId).MapTo<List<MediaEpisodeDto>>();
                            foreach (var season in listSeasons)
                            {
                                var existMediaFiles = false;
                                season.ListEpisodes = listEpisodes.Where(x => x.SeasonNumber == season.SeasonNumber).DistinctBy(x => x.EpisodeNumber).OrderBy(x => x.EpisodeNumber).ToList();
                                foreach (var episodeDto in season.ListEpisodes)
                                {
                                    var listMediaFiles = itemDetail.ListMediaFiles.Where(x => x.MediaEpisodeId == episodeDto.Id).ToList();
                                    episodeDto.ListMediaFiles = listMediaFiles;
                                    if (episodeDto.ListMediaFiles.Count > 0)
                                    {
                                        existMediaFiles = true;
                                    }
                                    if(mediaFile.MediaEpisodeId != null && episodeDto.Id== mediaFile.MediaEpisodeId)
                                    {
                                        itemDetail.CurrentEpisode = episodeDto;
                                    }
                                }
                                if (existMediaFiles)
                                {
                                    listSeasonsToAdd.Add(season);
                                }
                                if (itemDetail.CurrentEpisode != null && itemDetail.CurrentEpisode.SeasonNumber==season.SeasonNumber)
                                {
                                    itemDetail.CurrentSeason = season;
                                }
                            }
                            itemDetail.ListSeasons = listSeasonsToAdd;
                        }
                    }
                }
                response.Data = itemDetail;
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

        public Response<object> UpdateMediaFilePlayback(MediaFilePlaybackDto mediaFilePlaybackDto)
        {
            var response = new Response<object>();
            try
            {
                MediaFilePlayback mediaFilePlayback = _unitOfWork.MediaFilePlaybacks.GetByExpression(a => a.MediaFileId == mediaFilePlaybackDto.MediaFileId && a.ProfileId == mediaFilePlaybackDto.ProfileId);
                if (mediaFilePlayback != null)
                {
                    mediaFilePlayback.CurrentTime = mediaFilePlaybackDto.CurrentTime;
                    mediaFilePlayback.DurationTime = mediaFilePlaybackDto.DurationTime;
                    _unitOfWork.MediaFilePlaybacks.Update(mediaFilePlayback);
                }else
                {
                    mediaFilePlayback = mediaFilePlaybackDto.MapTo<MediaFilePlayback>();
                    _unitOfWork.MediaFilePlaybacks.Add(mediaFilePlayback);
                }
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

    }
}
