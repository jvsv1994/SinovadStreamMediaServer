using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.DTOs.Library;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class LibraryService : ILibraryService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly IAppLogger<LibraryService> _logger;

        private readonly IAlertService _alertService;

        private readonly FfmpegStrategy _ffmpegStrategy;

        private readonly SharedService _sharedService;

        public LibraryService(IUnitOfWork unitOfWork, SharedData sharedData,IMapper mapper, IAppLogger<LibraryService> logger, IAlertService alertService, FfmpegStrategy ffmpegStrategy, SharedService sharedService)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _mapper = mapper;
            _logger = logger;
            _alertService = alertService;
            _ffmpegStrategy = ffmpegStrategy;
            _sharedService = sharedService;
        }

        public async Task<Response<LibraryDto>> GetAsync(int id)
        {
            var response = new Response<LibraryDto>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAsync(id);
                response.Data = _mapper.Map<LibraryDto>(result);
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<List<LibraryDto>>> GetAllAsync()
        {
            var response = new Response<List<LibraryDto>>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAllAsync();
                response.Data = _mapper.Map<List<LibraryDto>>(result);
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<LibraryDto>> CreateAsync(LibraryCreationDto libraryCreationDto)
        {
            var response = new Response<LibraryDto>();
            try
            {
               var library = _mapper.Map<Library>(libraryCreationDto);
               library.MediaTypeCatalogId = (int)Catalog.MediaType;
               await _unitOfWork.Libraries.AddAsync(library);
               await _unitOfWork.SaveAsync();
               response.IsSuccess = true;
               response.Message = "Successful";
               await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
               response.Message = ex.Message;
               _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<object>> UpdateAsync(int id,LibraryCreationDto libraryCreationDto)
        {
            var response = new Response<object>();
            try
            {
               var library=await _unitOfWork.Libraries.GetAsync(id);
               library = _mapper.Map(libraryCreationDto, library);
               _unitOfWork.Libraries.Update(library);
               await _unitOfWork.SaveAsync();
               response.IsSuccess = true;
               response.Message = "Successful";
               await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
               response.Message = ex.Message;
               _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<object>> DeleteAsync(int id)
        {
            var response = new Response<object>();
            try
            {
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => x.LibraryId==id;
                _sharedData.ListMediaFiles.RemoveAll(x => x.LibraryId == id);
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
                _unitOfWork.Libraries.Delete(id);
                await _unitOfWork.SaveAsync();
                response.IsSuccess = true;
                response.Message = "Successful";
                await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
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
                _logger.LogError(ex.StackTrace);
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
                _logger.LogError(ex.StackTrace);
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
                _logger.LogError(ex.StackTrace);
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
                _logger.LogError(ex.StackTrace);
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
                itemDetail.MediaItem = _mapper.Map<MediaItemDto>(mediaItem);
                var mediaFiles = _unitOfWork.MediaFiles.GetAllByExpression(x=>x.MediaItemId == mediaItemId);
                itemDetail.ListMediaFiles = _mapper.Map<List<MediaFileDto>>(mediaFiles);
                var listSeasonsToAdd = new List<MediaSeasonDto>();
                if(mediaItem.MediaTypeId==MediaType.TvSerie)
                {
                    var listSeasons=_mapper.Map<List<MediaSeasonDto>>(_unitOfWork.MediaSeasons.GetAllByExpression(x => x.MediaItemId == mediaItemId).DistinctBy(x => x.SeasonNumber));
                    var listEpisodes= _mapper.Map<List<MediaEpisodeDto>>(_unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItemId));
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
                _logger.LogError(ex.StackTrace);
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
                        itemDetail.MediaItem = _mapper.Map<MediaItemDto>(mediaItem);
                        var mediaFiles = _unitOfWork.MediaFiles.GetAllByExpression(x => x.MediaItemId == mediaItemId);
                        itemDetail.ListMediaFiles = _mapper.Map<List<MediaFileDto>>(mediaFiles);
                        var mediaFileProfile= _unitOfWork.MediaFileProfiles.GetByExpression(x=>x.MediaFileId==mediaFileId && x.ProfileId==profileId);
                        if (mediaFileProfile != null)
                        {
                            itemDetail.MediaFileProfile = _mapper.Map<MediaFileProfileDto>(mediaFileProfile);
                        }
                        if (mediaItem.MediaTypeId == MediaType.TvSerie)
                        {
                            var listSeasonsToAdd = new List<MediaSeasonDto>();
                            var listSeasons =  _mapper.Map<List<MediaSeasonDto>>(_unitOfWork.MediaSeasons.GetAllByExpression(x => x.MediaItemId == mediaItemId).DistinctBy(x => x.SeasonNumber));
                            var listEpisodes = _mapper.Map<List<MediaEpisodeDto>>(_unitOfWork.MediaEpisodes.GetAllByExpression(x => x.MediaItemId == mediaItemId));
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
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

    }
}
