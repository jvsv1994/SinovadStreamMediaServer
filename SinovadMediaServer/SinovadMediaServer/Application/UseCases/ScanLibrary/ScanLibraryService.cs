﻿using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
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
using SinovadMediaServer.Strategies;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class ScanLibraryService : IScanLibraryService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly SinovadApiService _sinovadApiService;

        private readonly ITmdbService _tmdbService;

        private readonly IImdbService _imdbService;

        private readonly AutoMapper.IMapper _mapper;

        private readonly IAppLogger<ScanLibraryService> _logger;

        private readonly IAlertService _alertService;

        private readonly FfmpegStrategy _ffmpegStrategy;

        private readonly SharedService _sharedService;

        public ScanLibraryService(IUnitOfWork unitOfWork, SharedData sharedData, SinovadApiService sinovadApiService, ITmdbService tmdbService, IImdbService imdbService, IMapper mapper, IAppLogger<ScanLibraryService> logger, IAlertService alertService, FfmpegStrategy ffmpegStrategy, SharedService sharedService)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _sinovadApiService = sinovadApiService;
            _tmdbService = tmdbService;
            _imdbService = imdbService;
            _mapper = mapper;
            _logger = logger;
            _alertService = alertService;
            _ffmpegStrategy = ffmpegStrategy;
            _sharedService = sharedService;
        }

        public async Task<Response<object>> SearchFilesAsync(SearchFilesDto searchFilesDto)
        {
            var response = new Response<object>();
            try
            {
                IEnumerable<string> filesToAdd = new List<string>();

                foreach (var library in searchFilesDto.ListLibraries)
                {
                    await _alertService.Create("Comenzando a escanear la librería " + library.Name, AlertType.Bullhorn);
                    var listPaths = new List<string>();

                    var exists = Directory.Exists(library.PhysicalPath);
                    if (exists)
                    {
                        listPaths = Directory.GetFiles(library.PhysicalPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mkv") || s.EndsWith(".mp4") || s.EndsWith(".avi")).ToList();
                        var listMediaFilesToAdd = GetListMediaFilesToAdd(library.Id, listPaths);
                        if (library.MediaTypeCatalogDetailId == (int)MediaType.Movie)
                        {
                            RegisterMovieFiles(listMediaFilesToAdd);
                        }
                        if (library.MediaTypeCatalogDetailId == (int)MediaType.TvSerie)
                        {
                            RegisterTvSeriesFiles(listMediaFilesToAdd);
                        }
                    }
                    else
                    {
                        _sharedData.ListMediaFiles.RemoveAll(x => x.LibraryId == library.Id);
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
                AddMessage(LogType.Error, ex.StackTrace);
            }
            return response;
        }

        private void DeleteMediaFilesByExpresion(Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete)
        {
            var listMediaFilesToDelete = _unitOfWork.MediaFiles.GetAllByExpression(expresionMediaFilesToDelete).ToList();
            if (listMediaFilesToDelete.Count > 0)
            {
                _alertService.Create("Eliminando archivos en las rutas " + string.Join(",", listMediaFilesToDelete.Select(x => x.PhysicalPath)), AlertType.Bin);
                List<string> listIdsVideosDelete = listMediaFilesToDelete.Select(o => o.Id.ToString()).ToList();
                List<MediaFilePlaybackDto> listMediaFilePlaybackForDelete = _sharedData.ListMediaFilePlayback.Where(x => listIdsVideosDelete.Contains(x.ItemData.MediaFileId.ToString())).ToList();
                foreach (var mediaFilePlayback in listMediaFilePlaybackForDelete)
                {
                    _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                    _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                }
                Expression<Func<MediaFileProfile, bool>> expressionVideoProfilesToDelete = x => listIdsVideosDelete.Contains(x.MediaFileId.ToString());
                _unitOfWork.MediaFileProfiles.DeleteByExpression(expressionVideoProfilesToDelete);
                _unitOfWork.MediaFiles.DeleteList(listMediaFilesToDelete);
                _unitOfWork.Save();
            }
        }

        private List<MediaFile> GetListMediaFilesToAdd(int libraryId, List<string> paths)
        {
            var listMediaFilesAdded = new List<MediaFile>();
            foreach (var path in paths)
            {
                var mediaFileFinded = _sharedData.ListMediaFiles.Where(x => x.PhysicalPath == path).FirstOrDefault();
                if (mediaFileFinded == null)
                {
                    var mediaFile = new MediaFile();
                    mediaFile.LibraryId = libraryId;
                    mediaFile.PhysicalPath = path;
                    mediaFile = _unitOfWork.MediaFiles.Add(mediaFile);
                    listMediaFilesAdded.Add(mediaFile);
                    _sharedData.ListMediaFiles.Add(_mapper.Map<MediaFileDto>(mediaFile));
                    _unitOfWork.Save();
                    _alertService.Create("Guardando nuevo archivo localizado en " + mediaFile.PhysicalPath, AlertType.Plus);
                }
            }
            DeleteVideosNotFoundInLibrary(libraryId, paths);
            return listMediaFilesAdded;
        }

        private void RegisterTvSeriesFiles(List<MediaFile> listMediaFilesAdded)
        {
            var sinovadMediaDataBaseService = new SinovadMediaDatabaseService(_sinovadApiService);
            try
            {
                if (listMediaFilesAdded != null && listMediaFilesAdded.Count > 0)
                {
                    for (int i = 0; i < listMediaFilesAdded.Count; i++)
                    {
                        try
                        {
                            var mediaFile = listMediaFilesAdded[i];
                            var path = mediaFile.PhysicalPath;
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
                                var mediaItemList = _unitOfWork.MediaItems.GetAllByExpression(x => x.SearchQuery != null && x.SearchQuery.ToLower().Trim() == tvSerieName.ToLower().Trim() && x.MediaTypeId == MediaType.TvSerie);
                                MediaItem mediaItem = null;
                                if (mediaItemList != null && mediaItemList.Count() > 0)
                                {
                                    mediaItem = mediaItemList.FirstOrDefault();
                                }
                                if (mediaItem == null)
                                {
                                    mediaItem = GenerateMediaItemFromTvSerie(tvSerieName);
                                }
                                MediaSeason season = null;
                                var seasonList = _unitOfWork.MediaSeasons.GetAllByExpression(x => x.MediaItemId == mediaItem.Id && x.SeasonNumber == seasonNumber);
                                if (seasonList != null && seasonList.Count() > 0)
                                {
                                    season = seasonList.FirstOrDefault();
                                }
                                if (season == null)
                                {
                                    season = GenerateMediaSeason(tvSerieName, seasonNumber, mediaItem);
                                }
                                var episodeList = _unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItem.Id && x.SeasonNumber == seasonNumber && x.EpisodeNumber == episodeNumber);
                                MediaEpisode episode = null;
                                if (episodeList != null && episodeList.Count() > 0)
                                {
                                    episode = episodeList.FirstOrDefault();
                                }
                                if (episode == null)
                                {
                                    episode = GenerateMediaEpisode(tvSerieName, seasonNumber, episodeNumber, mediaItem);
                                }
                                mediaFile.MediaItemId = mediaItem.Id;
                                mediaFile.MediaEpisodeId = episode.Id;
                                _unitOfWork.MediaFiles.Update(mediaFile);
                                _unitOfWork.Save();
                                _alertService.Create("Actualizando nuevo archivo para " + tvSerieName + " S" + seasonNumber + "E" + episodeNumber + " localizado en " + physicalPath, AlertType.Plus);
                                _ffmpegStrategy.GenerateThumbnailByPhysicalPath(mediaFile.Guid.ToString(), mediaFile.PhysicalPath);
                                if (IsMultipleOf(i, 20))
                                {
                                    _sharedData.HubConnection.InvokeAsync("UpdateItemsByMediaServer", _sharedData.MediaServerData.Guid);
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
                }
                else
                {
                    AddMessage(LogType.Information, "New files not found");
                }
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
            AddMessage(LogType.Information, "Ending search tv series");
            _sharedData.HubConnection.InvokeAsync("UpdateItemsByMediaServer", _sharedData.MediaServerData.Guid);
        }

        private MediaEpisode GenerateMediaEpisode(string tvSerieName, int seasonNumber, int episodeNumber, MediaItem mediaItem)
        {
            _alertService.Create("Buscando metadatos para " + tvSerieName + " S" + seasonNumber + " E" + episodeNumber, AlertType.Tags);
            MediaEpisodeDto episodeDto = null;
            var sinovadMediaDataBaseService = new SinovadMediaDatabaseService(_sinovadApiService);
            if (mediaItem.MetadataAgentsId == MetadataAgents.TMDb)
            {
                episodeDto = _tmdbService.GetTvEpisode(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber);
                if (episodeDto != null)
                {
                    episodeDto.MediaItemId = mediaItem.Id;
                    _alertService.Create("Metadatos encontrados en TMDB Data Base para " + tvSerieName + " S" + seasonNumber + " E" + episodeNumber, AlertType.Tags);
                }
            }
            if (mediaItem.MetadataAgentsId == MetadataAgents.SinovadDb)
            {
                var res = sinovadMediaDataBaseService.SearchEpisodeAsync(int.Parse(mediaItem.SourceId), seasonNumber, episodeNumber).Result;
                if (res.Data != null)
                {
                    var sinovadEpisode = res.Data;
                    episodeDto = new MediaEpisodeDto();
                    episodeDto.MediaItemId = mediaItem.Id;
                    episodeDto.SeasonNumber = seasonNumber;
                    episodeDto.EpisodeNumber = episodeNumber;
                    episodeDto.Name = sinovadEpisode.Title;
                    episodeDto.PosterPath = sinovadEpisode.ImageUrl;
                    episodeDto.Overview = sinovadEpisode.Summary;
                    episodeDto.SourceId = sinovadEpisode.Id.ToString();
                    _alertService.Create("Metadatos encontrados en Sinovad Media Data Base para " + tvSerieName + " S" + seasonNumber + " E" + episodeNumber, AlertType.Tags);
                }
            }
            if (episodeDto == null)
            {
                episodeDto = new MediaEpisodeDto();
                episodeDto.MediaItemId = mediaItem.Id;
                episodeDto.SeasonNumber = seasonNumber;
                episodeDto.EpisodeNumber = episodeNumber;
                episodeDto.Name = "Episodio " + episodeNumber;
                _alertService.Create("Generando metadatos para " + tvSerieName + " S0" + seasonNumber + " E" + episodeNumber, AlertType.Tags);
            }

            var episode = _unitOfWork.MediaEpisodes.Add(_mapper.Map<MediaEpisode>(episodeDto));
            _unitOfWork.Save();
            _alertService.Create("Guardando metadatos para " + tvSerieName + " S" + seasonNumber + " E" + episodeNumber, AlertType.Plus);
            return episode;
        }

        private MediaSeason GenerateMediaSeason(string tvSerieName, int seasonNumber, MediaItem mediaItem)
        {
            _alertService.Create("Buscando metadatos para " + tvSerieName + " S" + seasonNumber, AlertType.Tags);
            var sinovadMediaDataBaseService = new SinovadMediaDatabaseService(_sinovadApiService);
            MediaSeasonDto seasonDto = null;
            if (mediaItem.MetadataAgentsId == MetadataAgents.TMDb)
            {
                seasonDto = _tmdbService.GetTvSeason(int.Parse(mediaItem.SourceId), seasonNumber);
                if (seasonDto != null)
                {
                    seasonDto.MediaItemId = mediaItem.Id;
                    _alertService.Create("Metadatos encontrados en TMDB Data Base para " + tvSerieName + " S" + seasonNumber, AlertType.Tags);
                }
            }
            if (mediaItem.MetadataAgentsId == MetadataAgents.SinovadDb)
            {
                var res = sinovadMediaDataBaseService.SearchSeasonAsync(int.Parse(mediaItem.SourceId), seasonNumber).Result;
                if (res.Data != null)
                {
                    var sinovadSeason = res.Data;
                    seasonDto = new MediaSeasonDto();
                    seasonDto.MediaItemId = mediaItem.Id;
                    seasonDto.SourceId = sinovadSeason.Id.ToString();
                    seasonDto.Name = sinovadSeason.Name;
                    seasonDto.SeasonNumber = sinovadSeason.SeasonNumber;
                    seasonDto.Overview = sinovadSeason.Summary;
                    _alertService.Create("Metadatos encontrados en Sinovad Media Data Base para " + tvSerieName + " S" + seasonNumber, AlertType.Tags);
                }
            }
            if (seasonDto == null)
            {
                seasonDto = new MediaSeasonDto();
                seasonDto.MediaItemId = mediaItem.Id;
                seasonDto.SeasonNumber = seasonNumber;
                seasonDto.Name = "Temporada " + seasonNumber;
                _alertService.Create("Generando metadatos para " + tvSerieName + " S" + seasonNumber, AlertType.Tags);
            }

            var season = _unitOfWork.MediaSeasons.Add(_mapper.Map<MediaSeason>(seasonDto));
            _unitOfWork.Save();
            _alertService.Create("Guardando metadatos para " + tvSerieName + " S" + seasonNumber, AlertType.Tags);
            return season;
        }


        private MediaItem GenerateMediaItemFromTvSerie(string tvSerieName)
        {
            _alertService.Create("Buscando metadatos para " + tvSerieName, AlertType.Tags);
            MediaItemDto mediaItemDto = null;
            var sinovadMediaDataBaseService = new SinovadMediaDatabaseService(_sinovadApiService);
            var result = sinovadMediaDataBaseService.SearchTvSerieAsync(tvSerieName).Result;
            if (result.Data != null)
            {
                var sinovadTvSerie = result.Data;
                mediaItemDto = new MediaItemDto();
                mediaItemDto.SourceId = sinovadTvSerie.Id.ToString();
                mediaItemDto.FirstAirDate = sinovadTvSerie.FirstAirDate;
                mediaItemDto.LastAirDate = sinovadTvSerie.LastAirDate;
                mediaItemDto.Overview = sinovadTvSerie.Overview;
                mediaItemDto.Actors = sinovadTvSerie.Actors;
                mediaItemDto.Directors = sinovadTvSerie.Directors;
                if (sinovadTvSerie.TvSerieGenres != null)
                {
                    mediaItemDto.Genres = string.Join(", ", sinovadTvSerie.TvSerieGenres.Select(x => x.GenreName));
                }
                mediaItemDto.PosterPath = sinovadTvSerie.PosterPath;
                mediaItemDto.Title = sinovadTvSerie.Name;
                mediaItemDto.ExtendedTitle = sinovadTvSerie.Name + (sinovadTvSerie.LastAirDate.Value.Year > sinovadTvSerie.FirstAirDate.Value.Year ? " (" + sinovadTvSerie.FirstAirDate.Value.Year + "-" + sinovadTvSerie.LastAirDate.Value.Year + ")" : " (" + sinovadTvSerie.FirstAirDate.Value.Year + ")");
                mediaItemDto.MediaTypeId = MediaType.TvSerie;
                mediaItemDto.MetadataAgentsId = MetadataAgents.SinovadDb;
                mediaItemDto.ListGenres = sinovadTvSerie.TvSerieGenres.Select(x => new MediaGenreDto() { Id = x.GenreId, Name = x.GenreName }).ToList();
                mediaItemDto.SearchQuery = tvSerieName;
                _alertService.Create("Metadatos encontrados en Sinovad Media Data Base para " + tvSerieName, AlertType.Tags);
            }
            else
            {
                mediaItemDto = _tmdbService.SearchTvShow(tvSerieName);
                if (mediaItemDto != null)
                {
                    mediaItemDto.SearchQuery = tvSerieName;
                    _alertService.Create("Metadatos encontrados en TMDB Data Base para " + tvSerieName, AlertType.Tags);
                }
                else
                {
                    mediaItemDto = new MediaItemDto();
                    mediaItemDto.Title = tvSerieName;
                    mediaItemDto.ExtendedTitle = tvSerieName;
                    mediaItemDto.SearchQuery = tvSerieName;
                    mediaItemDto.MediaTypeId = MediaType.TvSerie;
                    var listMediaGenres = new List<MediaGenreDto>();
                    var mediaGenre = new MediaGenreDto();
                    mediaGenre.Name = "Otros";
                    listMediaGenres.Add(mediaGenre);
                    mediaItemDto.ListGenres = listMediaGenres;
                    _alertService.Create("Generando metadatos para " + tvSerieName, AlertType.Tags);
                }
            }
            var mediaItem = CreateMediaItem(mediaItemDto);
            _alertService.Create("Guardando metadatos para " + tvSerieName, AlertType.Plus);
            return mediaItem;
        }

        private bool IsMultipleOf(int multiple, int dividend)
        {
            return (multiple % dividend) == 0;
        }


        private MediaItem CreateMediaItem(MediaItemDto mediaItemDto)
        {
            var mediaItem = _mapper.Map<MediaItem>(mediaItemDto);
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

        private void RegisterMovieFiles(List<MediaFile> listMediaFilesAdded)
        {
            try
            {
                if (listMediaFilesAdded != null && listMediaFilesAdded.Count > 0)
                {
                    for (int i = 0; i < listMediaFilesAdded.Count; i++)
                    {
                        try
                        {
                            var mediaFile = listMediaFilesAdded[i];
                            var splitted = mediaFile.PhysicalPath.Split("\\");
                            var fileName = splitted[splitted.Length - 1];
                            var physicalPath = mediaFile.PhysicalPath;
                            var fileNameWithoutExtension = fileName;
                            if (fileName.ToLower().EndsWith(".mp4") || fileName.ToLower().EndsWith(".mkv") || fileName.ToLower().EndsWith(".avi"))
                            {
                                fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
                            }
                            var partsMovieNameWithoutExtension = fileNameWithoutExtension.Split(" ");
                            var year = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 4);
                            bool detectYear = int.TryParse(year, out int age);
                            MediaItem mediaItem = null;
                            var movieName = "";
                            if (detectYear)
                            {
                                movieName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 5);
                                var mediaItemList = _unitOfWork.MediaItems.GetAllByExpression(x => x.SearchQuery != null && x.SearchQuery.ToLower().Trim() == movieName.ToLower().Trim() && x.MediaTypeId == MediaType.Movie);
                                if (mediaItemList != null && mediaItemList.Count() > 0)
                                {
                                    mediaItem = mediaItemList.FirstOrDefault();
                                }
                                if (mediaItem == null)
                                {
                                    mediaItem = GenerateMediaItemFromMovie(movieName, year);
                                }
                            }
                            else
                            {
                                movieName = fileNameWithoutExtension;
                                mediaItem = GenerateMediaItemFromMovieWithoutYear(movieName);
                            }
                            mediaFile.MediaItemId = mediaItem.Id;
                            _unitOfWork.MediaFiles.Update(mediaFile);
                            _unitOfWork.Save();
                            _alertService.Create("Actualizando nuevo archivo para " + movieName + " (" + year + ") localizado en " + physicalPath, AlertType.Plus);
                            _ffmpegStrategy.GenerateThumbnailByPhysicalPath(mediaFile.Guid.ToString(), mediaFile.PhysicalPath);
                            if (IsMultipleOf(i, 20))
                            {
                                _sharedData.HubConnection.InvokeAsync("UpdateItemsByMediaServer", _sharedData.MediaServerData.Guid);
                            }
                        }
                        catch (Exception e)
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
            _sharedData.HubConnection.InvokeAsync("UpdateItemsByMediaServer", _sharedData.MediaServerData.Guid);
        }

        private MediaItem GenerateMediaItemFromMovie(string movieName, string year)
        {
            _alertService.Create("Buscando metadatos para " + movieName + " (" + year + ")", AlertType.Tags);
            MediaItemDto mediaItemDto = _tmdbService.SearchMovie(movieName, year);
            if (mediaItemDto != null && mediaItemDto.ListGenres != null && mediaItemDto.ListGenres.Count > 0)
            {
                _alertService.Create("Metadatos encontrados en TMDb Data Base para " + movieName + " (" + year + ")", AlertType.Tags);
            }
            else
            {
                mediaItemDto = _imdbService.SearchMovie(movieName, year);
                if (mediaItemDto != null && mediaItemDto.ListGenres != null && mediaItemDto.ListGenres.Count > 0)
                {
                    _alertService.Create("Metadatos encontrados en IMDB Data Base para " + movieName + " (" + year + ")", AlertType.Tags);
                }
                else
                {
                    mediaItemDto = new MediaItemDto();
                    mediaItemDto.Title = movieName;
                    mediaItemDto.ExtendedTitle = movieName + " (" + year + ")";
                    mediaItemDto.SearchQuery = movieName;
                    mediaItemDto.MediaTypeId = MediaType.Movie;
                    var listMediaGenres = new List<MediaGenreDto>();
                    var mediaGenre = new MediaGenreDto();
                    mediaGenre.Name = "Otros";
                    listMediaGenres.Add(mediaGenre);
                    mediaItemDto.ListGenres = listMediaGenres;
                    _alertService.Create("Generando metadatos para " + movieName + " (" + year + ")", AlertType.Tags);
                }
            }
            var mediaItem = CreateMediaItem(mediaItemDto);
            _alertService.Create("Guardando metadatos para " + movieName + " (" + year + ")", AlertType.Plus);
            return mediaItem;
        }

        private MediaItem GenerateMediaItemFromMovieWithoutYear(string movieName)
        {
            _alertService.Create("Generando metadatos para " + movieName, AlertType.Tags);
            var mediaItemDto = new MediaItemDto();
            mediaItemDto.Title = movieName;
            mediaItemDto.ExtendedTitle = movieName;
            mediaItemDto.SearchQuery = movieName;
            mediaItemDto.MediaTypeId = MediaType.Movie;
            var listMediaGenres = new List<MediaGenreDto>();
            var mediaGenre = new MediaGenreDto();
            mediaGenre.Name = "Otros";
            listMediaGenres.Add(mediaGenre);
            mediaItemDto.ListGenres = listMediaGenres;
            var mediaItem = CreateMediaItem(mediaItemDto);
            _alertService.Create("Guardando metadatos para " + movieName, AlertType.Plus);
            return mediaItem;
        }

        private void DeleteVideosNotFoundInLibrary(int libraryId, List<string> paths)
        {
            try
            {
                AddMessage(LogType.Information, "Check if there are videos to delete");
                List<MediaFile> listVideosToDelete = new List<MediaFile>();
                _sharedData.ListMediaFiles.RemoveAll(x => !paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId);
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => !paths.Contains(x.PhysicalPath) && x.LibraryId == libraryId;
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
            }
            catch (Exception e)
            {
                AddMessage(LogType.Error, e.Message);
            }
        }

        private void AddMessage(LogType logType, string message)
        {
            if (logType == LogType.Information)
            {
                _logger.LogInformation(message); ;
            }
            if (logType == LogType.Error)
            {
                _logger.LogError(message);
            }
        }

    }
}
