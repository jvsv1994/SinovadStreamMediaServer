namespace SinovadMediaServer.Domain.Entities;

public partial class MediaItemGenre : BaseEntity
{
    public int MediaItemId { get; set; }
    public int MediaGenreId { get; set; }
    public virtual MediaGenre MediaGenre { get; set; } = null!;
    public virtual MediaItem MediaItem { get; set; } = null!;

}
