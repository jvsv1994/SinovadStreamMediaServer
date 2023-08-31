#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackClientDto
    {
        public string Platform { get; set; }
        public string LocalIpAddress { get; set; }
        public bool IsPlaying { get; set; }
        public double DurationTime { get; set; }
        public double CurrentTime { get; set; }

    }
}
