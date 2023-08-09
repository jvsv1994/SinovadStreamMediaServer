using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.Interface.Persistence
{
    public interface IUnitOfWork : IDisposable
    {

        public IGenericRepository<Library> Libraries { get; }
        public IGenericRepository<TranscoderSettings> TranscoderSettings { get; }
        public IGenericRepository<TranscodingProcess> TranscodingProcesses { get; }
        public IVideoRepository Videos { get; }
        public IGenericRepository<VideoProfile> VideoProfiles { get; }
        public void Save();
        public Task SaveAsync();
    }
}
