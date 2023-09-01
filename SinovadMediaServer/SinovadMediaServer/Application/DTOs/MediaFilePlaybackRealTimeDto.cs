#nullable disable
namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackRealTimeDto
    {
        public string Guid { get; set; }
        public MediaFilePlaybackClientDto ClientData { get; set; }
        public MediaFilePlaybackItemDto ItemData { get; set; }
        public MediaFilePlaybackCurrentStreamsDto StreamsData { get; set; }
        public MediaFilePlaybackProfileDto ProfileData { get; set; }

    }
}
