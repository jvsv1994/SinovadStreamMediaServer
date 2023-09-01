using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IUnitOfWork : IDisposable
    {

        public IGenericRepository<Library> Libraries { get; }
        public IGenericRepository<TranscoderSettings> TranscoderSettings { get; }
        public IMediaItemRepository MediaItems { get; }
        public IGenericRepository<MediaGenre> MediaGenres { get; }
        public IGenericRepository<MediaItemGenre> MediaItemGenres { get; }
        public IGenericRepository<MediaSeason> MediaSeasons { get; }
        public IGenericRepository<MediaEpisode> MediaEpisodes { get; }
        public IGenericRepository<MediaFile> MediaFiles { get; }
        public IGenericRepository<MediaFilePlayback> MediaFilePlaybacks { get; }
        public IGenericRepository<Alert> Alerts { get; }
        public void Save();
        public Task SaveAsync();
    }
}
