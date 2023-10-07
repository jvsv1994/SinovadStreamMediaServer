using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IMediaItemService
    {
        Task<Response<List<ItemsGroupDto>>> GetMediaItemsByLibrary(int libraryId, int profileId);
        Task<Response<List<ItemsGroupDto>>> GetMediaItemsByMediaType(MediaType mediaTypeId, int profileId);
        Task<Response<List<ItemsGroupDto>>> GetAllMediaItems(int profileId);
        Task<Response<List<ItemDto>>> GetAllMediaItemsBySearchQuery(string searchQuery);
        Task<Response<ItemDetailDto>> GetMediaItemDetail(int mediaItemId);
        Task<Response<ItemDetailDto>> GetMediaItemDetailByMediaFileAndProfile(int mediaFileId, int profileId);
    }
}
