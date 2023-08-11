using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IMediaItemRepository : IGenericRepository<MediaItem>
    {
        List<ItemDto> GetAllItemsByMediaType(MediaType mediaTypeId);
        List<ItemDto> GetAllItemsRecentlyAddedByMediaType(MediaType mediaTypeId);
        List<ItemDto> GetAllItemsByMediaTypeAndProfile(MediaType mediaTypeId, int profileId);
        List<ItemDto> GetAllItems();
        List<ItemDto> GetAllItemsRecentlyAdded();
        List<ItemDto> GetAllItemsByProfile(int profileId);
        List<ItemDto> GetItemsByLibrary(int libraryId);
        List<ItemDto> GetItemsRecentlyAddedByLibrary(int libraryId);
        List<ItemDto> GetItemsByLibraryAndProfile(int libraryId, int profileId);

    }
}
