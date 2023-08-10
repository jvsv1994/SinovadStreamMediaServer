namespace SinovadMediaServer.Domain.Entities;

public partial class MediaFile : BaseEntity
{
    public string Title { get; set; }
    public string? Subtitle { get; set; }
    public int? MediaItemId { get; set; }
    public string? PosterPath { get; set; }
    public int? LibraryId { get; set; }
    public string? PhysicalPath { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? EpisodeName { get; set; }
    public int? SeasonNumber { get; set; }
    public string? Overview { get; set; }
    public virtual ICollection<MediaFileProfile> MediaFileProfiles { get; set; } = new List<MediaFileProfile>();
}
