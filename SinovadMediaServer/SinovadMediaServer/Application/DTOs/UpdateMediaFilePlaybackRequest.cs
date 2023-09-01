#nullable disable
namespace SinovadMediaServer.Application.DTOs
{
    public class UpdateMediaFilePlaybackRequestDto
    {
        public string Guid { get; set; }
        public double CurrentTime { get; set; }
        public double DurationTime { get; set; }
        public bool IsPlaying { get; set; }
        public int MediaFileId { get; set; }
        public int ProfileId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }

    }
}
