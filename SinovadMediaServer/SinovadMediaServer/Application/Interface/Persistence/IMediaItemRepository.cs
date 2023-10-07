using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IMediaItemRepository : IGenericRepository<MediaItem>
    {
        Task<List<ItemDto>> GetAllItemsBySearchQuery(string searchQuery);
        Task<List<ItemDto>> GetAllItemsByMediaType(MediaType mediaTypeId);
        Task<List<ItemDto>> GetAllItemsRecentlyAddedByMediaType(MediaType mediaTypeId);
        Task<List<ItemDto>> GetAllItemsByMediaTypeAndProfile(MediaType mediaTypeId, int profileId);
        Task<List<ItemDto>> GetAllItems();
        Task<List<ItemDto>> GetAllItemsRecentlyAdded();
        Task<List<ItemDto>> GetAllItemsByProfile(int profileId);
        Task<List<ItemDto>> GetItemsByLibrary(int libraryId);
        Task<List<ItemDto>> GetItemsRecentlyAddedByLibrary(int libraryId);
        Task<List<ItemDto>> GetItemsByLibraryAndProfile(int libraryId, int profileId);

    }
}
