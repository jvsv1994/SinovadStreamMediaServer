#nullable disable


namespace SinovadMediaServer.Application.DTOs
{
    public class MediaSeasonDto
    {

        public int Id { get; set; }
        public int MediaItemId { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public int? SeasonNumber { get; set; }
        public string? SourceId { get; set; }
        public List<MediaEpisodeDto> ListEpisodes { get; set; }

    }
}
