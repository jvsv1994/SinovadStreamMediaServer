using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Persistence.Contexts;

namespace SinovadMediaServer.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
      
        private IGenericRepository<Library> _accountLibraries;
        private IGenericRepository<TranscoderSettings> _transcoderSettings;
        private IMediaItemRepository _mediaItems;
        private IGenericRepository<MediaGenre> _mediaGenres;
        private IGenericRepository<MediaItemGenre> _mediaItemGenres;
        private IGenericRepository<MediaSeason> _mediaSeasons;
        private IGenericRepository<MediaEpisode> _mediaEpisodes;
        private IGenericRepository<MediaFile> _mediaFiles;
        private IGenericRepository<MediaFilePlayback> _mediaFileProfiles;
        private IGenericRepository<Alert> _alerts;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Library> Libraries
        {
            get
            {
                return _accountLibraries == null ?
                _accountLibraries = new GenericRepository<Library>(_context) :
                _accountLibraries;
            }
        }

        public IGenericRepository<TranscoderSettings> TranscoderSettings
        {
            get
            {
                return _transcoderSettings == null ?
                _transcoderSettings = new GenericRepository<TranscoderSettings>(_context) :
                _transcoderSettings;
            }
        }

        public IMediaItemRepository MediaItems
        {
            get
            {
                return _mediaItems == null ?
                _mediaItems = new MediaItemRepository(_context) :
                _mediaItems;
            }
        }
        public IGenericRepository<MediaGenre> MediaGenres
        {
            get
            {
                return _mediaGenres == null ?
                _mediaGenres = new GenericRepository<MediaGenre>(_context) :
                _mediaGenres;
            }
        }
        public IGenericRepository<MediaItemGenre> MediaItemGenres
        {
            get
            {
                return _mediaItemGenres == null ?
                _mediaItemGenres = new GenericRepository<MediaItemGenre>(_context) :
                _mediaItemGenres;
            }
        }

        public IGenericRepository<MediaSeason> MediaSeasons
        {
            get
            {
                return _mediaSeasons == null ?
                _mediaSeasons = new GenericRepository<MediaSeason>(_context) :
                _mediaSeasons;
            }
        }

        public IGenericRepository<MediaEpisode> MediaEpisodes
        {
            get
            {
                return _mediaEpisodes == null ?
                _mediaEpisodes = new GenericRepository<MediaEpisode>(_context) :
                _mediaEpisodes;
            }
        }

        public IGenericRepository<MediaFile> MediaFiles
        {
            get
            {
                return _mediaFiles == null ?
                _mediaFiles = new GenericRepository<MediaFile>(_context) :
                _mediaFiles;
            }
        }

        public IGenericRepository<MediaFilePlayback> MediaFilePlaybacks
        {
            get
            {
                return _mediaFileProfiles == null ?
                _mediaFileProfiles = new GenericRepository<MediaFilePlayback>(_context) :
                _mediaFileProfiles;
            }
        }

        public IGenericRepository<Alert> Alerts
        {
            get
            {
                return _alerts == null ?
                _alerts = new GenericRepository<Alert>(_context) :
                _alerts;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
