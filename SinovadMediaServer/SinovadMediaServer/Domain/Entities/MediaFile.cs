namespace SinovadMediaServer.Domain.Entities;

public partial class MediaFile : BaseEntity
{
    public int? MediaItemId { get; set; }
    public int? MediaEpisodeId { get; set; }
    public int? LibraryId { get; set; }
    public string? PhysicalPath { get; set; }
    public virtual ICollection<MediaFileProfile> MediaFilePlaybacks { get; set; } = new List<MediaFileProfile>();
}
