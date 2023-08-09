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
        private IGenericRepository<TranscodingProcess> _transcodingProcesses;
        private IVideoRepository _videos;
        private IGenericRepository<VideoProfile> _videoProfiles;


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

        public IGenericRepository<TranscodingProcess> TranscodingProcesses
        {
            get
            {
                return _transcodingProcesses == null ?
                _transcodingProcesses = new GenericRepository<TranscodingProcess>(_context) :
                _transcodingProcesses;
            }
        }

        public IVideoRepository Videos
        {
            get
            {
                return _videos == null ?
                _videos = new VideoRepository(_context) :
                _videos;
            }
        }

        public IGenericRepository<VideoProfile> VideoProfiles
        {
            get
            {
                return _videoProfiles == null ?
                _videoProfiles = new GenericRepository<VideoProfile>(_context) :
                _videoProfiles;
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
