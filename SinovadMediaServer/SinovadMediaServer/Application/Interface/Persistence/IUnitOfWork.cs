using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IUnitOfWork : IDisposable
    {

        public IGenericRepository<Library> Libraries { get; }
        public IGenericRepository<TranscoderSettings> TranscoderSettings { get; }
        public IGenericRepository<MediaItem> MediaItems { get; }
        public IGenericRepository<MediaGenre> MediaGenres { get; }
        public IGenericRepository<MediaItemGenre> MediaItemGenres { get; }
        public IGenericRepository<MediaFile> MediaFiles { get; }
        public IGenericRepository<MediaFileProfile> MediaFileProfiles { get; }
        public IGenericRepository<TranscodingProcess> TranscodingProcesses { get; }
        public IVideoRepository Videos { get; }
        public IGenericRepository<VideoProfile> VideoProfiles { get; }
        public void Save();
        public Task SaveAsync();
    }
}
