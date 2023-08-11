using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Domain.Entities;

public partial class MediaItem : BaseEntity
{
    public string Title { get; set; }
    public string? SourceId { get; set; }
    public string? PosterPath { get; set; }
    public string? Overview { get; set; }
    public string? Actors { get; set; }
    public string? Directors { get; set; }
    public string? Genres { get; set; }
    public MediaType? MediaTypeId { get; set; }
    public MetadataAgents? MetadataAgentsId { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? FirstAirDate { get; set; }
    public DateTime? LastAirDate { get; set; }
    public string? SearchQuery { get; set; }
    public virtual ICollection<MediaItemGenre> MediaItemGenres { get; set; } = new List<MediaItemGenre>();

}
