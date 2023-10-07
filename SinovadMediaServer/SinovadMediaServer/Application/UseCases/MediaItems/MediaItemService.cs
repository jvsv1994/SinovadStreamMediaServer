using AutoMapper;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;

namespace SinovadMediaServer.Application.UseCases.MediaItems
{
    public class MediaItemService:IMediaItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly IAppLogger<MediaItemService> _logger;

        public MediaItemService(IUnitOfWork unitOfWork, SharedData sharedData, IMapper mapper, IAppLogger<MediaItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<List<ItemsGroupDto>>> GetMediaItemsByLibrary(int libraryId, int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = await _unitOfWork.MediaItems.GetItemsByLibrary(libraryId);
                List<ItemDto> listLastAddedItems = await _unitOfWork.MediaItems.GetItemsRecentlyAddedByLibrary(libraryId);
                List<ItemDto> listItemsWatched = await _unitOfWork.MediaItems.GetItemsByLibraryAndProfile(libraryId, profileId);
                response.Data = BuildListItemsGroup(listItems, listLastAddedItems, listItemsWatched);
                response.IsSuccess = true;
                response.Message = "Successful";
            } catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<List<ItemsGroupDto>>> GetMediaItemsByMediaType(MediaType mediaTypeId, int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = await _unitOfWork.MediaItems.GetAllItemsByMediaType(mediaTypeId);
                List<ItemDto> listLastAddedItems = await _unitOfWork.MediaItems.GetAllItemsRecentlyAddedByMediaType(mediaTypeId);
                List<ItemDto> listItemsWatched = await _unitOfWork.MediaItems.GetAllItemsByMediaTypeAndProfile(mediaTypeId, profileId);
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

        public async Task<Response<List<ItemsGroupDto>>> GetAllMediaItems(int profileId)
        {
            var response = new Response<List<ItemsGroupDto>>();
            try
            {
                List<ItemDto> listItems = await _unitOfWork.MediaItems.GetAllItems();
                List<ItemDto> listLastAddedItems = await _unitOfWork.MediaItems.GetAllItemsRecentlyAdded();
                List<ItemDto> listItemsWatched = await _unitOfWork.MediaItems.GetAllItemsByProfile(profileId);
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

        public async Task<Response<List<ItemDto>>> GetAllMediaItemsBySearchQuery(string searchQuery)
        {
            var response = new Response<List<ItemDto>>();
            try
            {
                List<ItemDto> listItems = await  _unitOfWork.MediaItems.GetAllItemsBySearchQuery(searchQuery);
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

        public async Task<Response<ItemDetailDto>> GetMediaItemDetail(int mediaItemId)
        {
            var response = new Response<ItemDetailDto>();
            try
            {
                ItemDetailDto itemDetail = new ItemDetailDto();
                var mediaItem = await _unitOfWork.MediaItems.GetAsync(mediaItemId);
                itemDetail.MediaItem = _mapper.Map<MediaItemDto>(mediaItem);
                var mediaFiles = await _unitOfWork.MediaFiles.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId);
                itemDetail.ListMediaFiles = _mapper.Map<List<MediaFileDto>>(mediaFiles);
                var listSeasonsToAdd = new List<MediaSeasonDto>();
                if (mediaItem.MediaTypeId == MediaType.TvSerie)
                {
                    var listSeasons = _mapper.Map<List<MediaSeasonDto>>((await _unitOfWork.MediaSeasons.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId)).DistinctBy(x => x.SeasonNumber));
                    var listEpisodes = _mapper.Map<List<MediaEpisodeDto>>(await _unitOfWork.MediaEpisodes.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId));
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
                        }
                        if (existMediaFiles)
                        {
                            listSeasonsToAdd.Add(season);
                        }
                    }
                }
                itemDetail.ListSeasons = listSeasonsToAdd;
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

        public async Task<Response<ItemDetailDto>> GetMediaItemDetailByMediaFileAndProfile(int mediaFileId, int profileId)
        {
            var response = new Response<ItemDetailDto>();
            try
            {
                ItemDetailDto itemDetail = new ItemDetailDto();
                var mediaFile = _unitOfWork.MediaFiles.Get(mediaFileId);
                if (mediaFile != null)
                {
                    if (mediaFile.MediaItemId != null)
                    {
                        var mediaItemId = (int)mediaFile.MediaItemId;
                        var mediaItem = await _unitOfWork.MediaItems.GetAsync(mediaItemId);
                        itemDetail.MediaItem = _mapper.Map<MediaItemDto>(mediaItem);
                        var mediaFiles = await _unitOfWork.MediaFiles.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId);
                        itemDetail.ListMediaFiles = _mapper.Map<List<MediaFileDto>>(mediaFiles);
                        var mediaFileProfile = await _unitOfWork.MediaFileProfiles.GetByExpressionAsync(x => x.MediaFileId == mediaFileId && x.ProfileId == profileId);
                        if (mediaFileProfile != null)
                        {
                            itemDetail.MediaFileProfile = _mapper.Map<MediaFileProfileDto>(mediaFileProfile);
                        }
                        if (mediaItem.MediaTypeId == MediaType.TvSerie)
                        {
                            var listSeasonsToAdd = new List<MediaSeasonDto>();
                            var listSeasons = _mapper.Map<List<MediaSeasonDto>>((await _unitOfWork.MediaSeasons.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId)).DistinctBy(x => x.SeasonNumber));
                            var listEpisodes = _mapper.Map<List<MediaEpisodeDto>>(await _unitOfWork.MediaEpisodes.GetAllByExpressionAsync(x => x.MediaItemId == mediaItemId));
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
                                    if (mediaFile.MediaEpisodeId != null && episodeDto.Id == mediaFile.MediaEpisodeId)
                                    {
                                        itemDetail.CurrentEpisode = episodeDto;
                                    }
                                }
                                if (existMediaFiles)
                                {
                                    listSeasonsToAdd.Add(season);
                                }
                                if (itemDetail.CurrentEpisode != null && itemDetail.CurrentEpisode.SeasonNumber == season.SeasonNumber)
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
    }
}
