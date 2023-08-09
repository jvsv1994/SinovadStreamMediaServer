#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class VideoProfileDto
    {
        public int VideoId { get; set; }
        public int ProfileId { get; set; }
        public double DurationTime { get; set; }
        public double CurrentTime { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
