namespace SinovadMediaServer.Application.DTOs
{
    public partial class MediaFileProfileDto
    {
        public int MediaFileId { get; set; }
        public int ProfileId { get; set; }
        public double DurationTime { get; set; }
        public double CurrentTime { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }

    }

}
