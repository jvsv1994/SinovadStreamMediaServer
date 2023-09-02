using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Persistence.Contexts;

namespace SinovadMediaServer.Persistence.Repositories
{
    public class MediaItemRepository : GenericRepository<MediaItem>, IMediaItemRepository
    {
        private ApplicationDbContext _context;
        public MediaItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }



        public List<ItemDto> GetAllItems()
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid=mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.GenreId).SelectMany(x => x.DistinctBy(x => x.MediaItemId)).ToList();
            return listMediaItems;
        }


        public List<ItemDto> GetItemsByLibrary(int libraryId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where library.Id == libraryId
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId

                                  }).AsEnumerable().GroupBy(a => a.GenreId).SelectMany(x=>x.DistinctBy(x=>x.MediaItemId)).ToList();

            return listMediaItems;
        }

        public List<ItemDto> GetAllItemsByMediaType(MediaType mediaTypeId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where (int)mediaItem.MediaTypeId == (int)mediaTypeId
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.GenreId).SelectMany(x => x.DistinctBy(x => x.MediaItemId)).ToList();

            return listMediaItems;
        }






        //Search All Items

        public List<ItemDto> GetAllItemsBySearchQuery(string searchQuery)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaItem.SearchQuery.ToLower().Trim().Contains(searchQuery.ToLower().Trim()) || mediaItem.Title.ToLower().Trim().Contains(searchQuery.ToLower().Trim())
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }


        // all items by media type


        public List<ItemDto> GetAllItemsRecentlyAddedByMediaType(MediaType mediaTypeId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where (int)mediaItem.MediaTypeId == (int)mediaTypeId
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Take(10).Select(x => x.First()).ToList();

            return listMediaItems;
        }

        public List<ItemDto> GetAllItemsByMediaTypeAndProfile(MediaType mediaTypeId,int profileId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaFileProfile in _context.MediaFileProfiles on mediaFile.Id equals mediaFileProfile.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFileProfile.ProfileId == profileId && (int)mediaItem.MediaTypeId == (int)mediaTypeId
                                  orderby mediaFileProfile.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFileProfile.Title,
                                      Subtitle=mediaFileProfile.Subtitle,
                                      CurrentTime=mediaFileProfile.CurrentTime,
                                      DurationTime=mediaFileProfile.DurationTime,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = mediaFileProfile.LastModified != null ? (DateTime)mediaFileProfile.LastModified : (DateTime)mediaFileProfile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId = mediaFile.MediaEpisodeId,
                                      MediaServerId = library.MediaServerId,
                                      ContinueVideo = true
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }

        // all items


        public List<ItemDto> GetAllItemsRecentlyAdded()
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Take(10).Select(x => x.First()).ToList();

            return listMediaItems;
        }

        public List<ItemDto> GetAllItemsByProfile(int profileId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaFileProfile in _context.MediaFileProfiles on mediaFile.Id equals mediaFileProfile.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFileProfile.ProfileId == profileId
                                  orderby mediaFileProfile.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFileProfile.Title,
                                      Subtitle=mediaFileProfile.Subtitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      CurrentTime = mediaFileProfile.CurrentTime,
                                      DurationTime=mediaFileProfile.DurationTime,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = mediaFileProfile.LastModified!=null?(DateTime)mediaFileProfile.LastModified: (DateTime)mediaFileProfile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId=mediaFile.MediaEpisodeId,
                                      MediaServerId = library.MediaServerId,
                                      ContinueVideo=true
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }


        //get items by library

        public List<ItemDto> GetItemsRecentlyAddedByLibrary(int libraryId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFile.LibraryId == libraryId
                                  orderby mediaFile.Created descending
                                  select new ItemDto
                                  {
                                      Title = mediaItem.ExtendedTitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = libraryId,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Take(10).Select(x => x.First()).ToList();

            return listMediaItems;
        }

        public List<ItemDto> GetItemsByLibraryAndProfile(int libraryId,int profileId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaFileProfile in _context.MediaFileProfiles on mediaFile.Id equals mediaFileProfile.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFile.LibraryId == libraryId && mediaFileProfile.ProfileId == profileId
                                  orderby mediaFileProfile.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFileProfile.Title,
                                      Subtitle = mediaFileProfile.Subtitle,
                                      CurrentTime = mediaFileProfile.CurrentTime,
                                      DurationTime = mediaFileProfile.DurationTime,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      MediaFileGuid = mediaFile.Guid,
                                      LibraryId = libraryId,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = mediaFileProfile.LastModified != null ? (DateTime)mediaFileProfile.LastModified : (DateTime)mediaFileProfile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId = mediaFile.MediaEpisodeId,
                                      MediaServerId = library.MediaServerId,
                                      ContinueVideo = true
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }

    }
}
