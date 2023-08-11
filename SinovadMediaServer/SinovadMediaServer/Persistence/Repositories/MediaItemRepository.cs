using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Persistence.Contexts;
using TMDbLib.Objects.Movies;

namespace SinovadMediaServer.Persistence.Repositories
{
    public class MediaItemRepository : GenericRepository<MediaItem>, IMediaItemRepository
    {
        private ApplicationDbContext _context;
        public MediaItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
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
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }


        // all items by media type

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
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).GroupBy(a => a.GenreId).SelectMany(x => x.DistinctBy(x => x.MediaItemId)).ToList();

            return listMediaItems;
        }

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
                                  join mediaFilePlayback in _context.MediaFilePlaybacks on mediaFile.Id equals mediaFilePlayback.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFilePlayback.ProfileId == profileId && (int)mediaItem.MediaTypeId == (int)mediaTypeId
                                  orderby mediaFilePlayback.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFilePlayback.Title,
                                      Subtitle=mediaFilePlayback.Subtitle,
                                      CurrentTime=mediaFilePlayback.CurrentTime,
                                      DurationTime=mediaFilePlayback.DurationTime,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId = mediaFile.MediaEpisodeId,
                                      EpisodeNumber = mediaFilePlayback.EpisodeNumber,
                                      SeasonNumber = mediaFilePlayback.SeasonNumber,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }

        // all items

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
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId= mediaItem.Id,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.GenreId).SelectMany(x=>x.DistinctBy(x=>x.MediaItemId)).ToList();
            return listMediaItems;
        }

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
                                  join mediaFilePlayback in _context.MediaFilePlaybacks on mediaFile.Id equals mediaFilePlayback.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFilePlayback.ProfileId == profileId
                                  orderby mediaFilePlayback.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFilePlayback.Title,
                                      Subtitle=mediaFilePlayback.Subtitle,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      CurrentTime = mediaFilePlayback.CurrentTime,
                                      DurationTime=mediaFilePlayback.DurationTime,
                                      FileId = mediaFile.Id,
                                      LibraryId = library.Id,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId=mediaFile.MediaEpisodeId,
                                      EpisodeNumber=mediaFilePlayback.EpisodeNumber,
                                      SeasonNumber=mediaFilePlayback.SeasonNumber,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }


        //get items by library

        public List<ItemDto> GetItemsByLibrary(int libraryId)
        {
            var listMediaItems = (from mediaItem in _context.MediaItems
                                  join mediaFile in _context.MediaFiles on mediaItem.Id equals mediaFile.MediaItemId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFile.LibraryId==libraryId
            select new ItemDto
            {
                          Title = mediaItem.ExtendedTitle,
                          Overview = mediaItem.Overview,
                          SearchQuery = mediaItem.SearchQuery,
                          SourceId = mediaItem.SourceId,
                          MetadataAgentsId = mediaItem.MetadataAgentsId,
                          MediaTypeId   = mediaItem.MediaTypeId,
                          PosterPath = mediaItem.PosterPath,
                          GenreId = mediaGenre.Id,
                          GenreName = mediaGenre.Name,
                          FileId=mediaFile.Id,
                          LibraryId = libraryId,
                          PhysicalPath = mediaFile.PhysicalPath,
                          Created = (DateTime)mediaFile.Created,
                          MediaItemId = mediaItem.Id,
                          MediaServerId = library.MediaServerId

            }).GroupBy(a => a.GenreId).SelectMany(x => x.DistinctBy(x => x.MediaItemId)).ToList();

            return listMediaItems;
        }

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
                                  join mediaFilePlayback in _context.MediaFilePlaybacks on mediaFile.Id equals mediaFilePlayback.MediaFileId
                                  join mediaGenreItem in _context.MediaItemGenres on mediaItem.Id equals mediaGenreItem.MediaItemId
                                  join mediaGenre in _context.MediaGenres on mediaGenreItem.MediaGenreId equals mediaGenre.Id
                                  join library in _context.Libraries on mediaFile.LibraryId equals library.Id
                                  where mediaFile.LibraryId == libraryId && mediaFilePlayback.ProfileId == profileId
                                  orderby mediaFilePlayback.LastModified descending
                                  select new ItemDto
                                  {
                                      Title = mediaFilePlayback.Title,
                                      Subtitle = mediaFilePlayback.Subtitle,
                                      CurrentTime = mediaFilePlayback.CurrentTime,
                                      DurationTime = mediaFilePlayback.DurationTime,
                                      Overview = mediaItem.Overview,
                                      SearchQuery = mediaItem.SearchQuery,
                                      SourceId = mediaItem.SourceId,
                                      MetadataAgentsId = mediaItem.MetadataAgentsId,
                                      MediaTypeId = mediaItem.MediaTypeId,
                                      PosterPath = mediaItem.PosterPath,
                                      GenreId = mediaGenre.Id,
                                      GenreName = mediaGenre.Name,
                                      FileId = mediaFile.Id,
                                      LibraryId = libraryId,
                                      PhysicalPath = mediaFile.PhysicalPath,
                                      Created = (DateTime)mediaFile.Created,
                                      MediaItemId = mediaItem.Id,
                                      MediaEpisodeId = mediaFile.MediaEpisodeId,
                                      EpisodeNumber = mediaFilePlayback.EpisodeNumber,
                                      SeasonNumber = mediaFilePlayback.SeasonNumber,
                                      MediaServerId = library.MediaServerId
                                  }).AsEnumerable().GroupBy(a => a.MediaItemId).Select(x => x.First()).ToList();

            return listMediaItems;
        }

    }
}
