namespace SinovadMediaServer.Domain.Entities;

public partial class MediaFile : BaseEntity
{
    public int? MediaItemId { get; set; }
    public int? MediaEpisodeId { get; set; }
    public int? LibraryId { get; set; }
    public string? PhysicalPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public virtual ICollection<MediaFilePlayback> MediaFilePlaybacks { get; set; } = new List<MediaFilePlayback>();
}
