using SinovadMediaServer.Domain.Enums;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class VideoDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string PhysicalPath { get; set; }
        public int LibraryId { get; set; }
        public MediaType MediaTypeId { get; set; }
        public string MovieId { get; set; }
        public string TvSerieId { get; set; }
        public string EpisodeId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string EpisodeName { get; set; }
        public string MediaServerUrl { get; set; }
        public string? PosterPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? FirstAirDate { get; set; }
        public DateTime? LastAirDate { get; set; }
        public MetadataAgents? MetadataAgentsId { get; set; }

    }
}
