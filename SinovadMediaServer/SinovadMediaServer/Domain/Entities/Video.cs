namespace SinovadMediaServer.Domain.Entities;

public partial class Video:BaseEntity
{
    public string Title { get; set; }

    public string PhysicalPath { get; set; }

    public int? LibraryId { get; set; }

    public int? MovieId { get; set; }

    public int? EpisodeId { get; set; }

    public int? TvSerieId { get; set; }

    public int? EpisodeNumber { get; set; }

    public int? SeasonNumber { get; set; }

    public string? Subtitle { get; set; }

    public virtual ICollection<VideoProfile> VideoProfiles { get; set; } = new List<VideoProfile>();
}
