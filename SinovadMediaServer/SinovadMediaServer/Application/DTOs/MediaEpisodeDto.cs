#nullable disable


namespace SinovadMediaServer.Application.DTOs
{
    public class MediaEpisodeDto
    {
        public int Id { get; set; } 
        public int MediaItemId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public string? SourceId { get; set; }
        public List<MediaFileDto> MediaFiles { get; set; }   

    }
}
