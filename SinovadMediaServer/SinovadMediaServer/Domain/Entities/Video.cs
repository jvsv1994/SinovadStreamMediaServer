using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Domain.Entities;

public partial class Video:BaseEntity
{
    public string Title { get; set; }
    public string PhysicalPath { get; set; }
    public string? MediaServerUrl { get; set; }
    public int? LibraryId { get; set; }
    public MediaType? MediaTypeId { get; set; }
    public string? MovieId { get; set; }
    public string? EpisodeId { get; set; }
    public string? TvSerieId { get; set; }
    public int? EpisodeNumber { get; set; }
    public int? SeasonNumber { get; set; }
    public string? Subtitle { get; set; }
    public string? PosterPath { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? FirstAirDate { get; set; }
    public DateTime? LastAirDate { get; set; }
    public MetadataAgents? MetadataAgentsId { get; set; }
    public virtual ICollection<VideoProfile> VideoProfiles { get; set; } = new List<VideoProfile>();
}
