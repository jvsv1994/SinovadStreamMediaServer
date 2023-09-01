#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackClientDto
    {
        public DeviceDto DeviceData { get; set; }
        public string LocalIpAddress { get; set; }
        public bool IsPlaying { get; set; }
        public double Duration { get; set; }
        public double CurrentTime { get; set; }

    }
}
