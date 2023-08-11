namespace SinovadMediaServer.Domain.Entities
{
    public class MediaEpisode:BaseEntity
    {
        public int MediaItemId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public string? SourceId { get; set; }

    }
}
