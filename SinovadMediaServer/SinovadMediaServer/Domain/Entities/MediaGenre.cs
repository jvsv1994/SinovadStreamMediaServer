namespace SinovadMediaServer.Domain.Entities;

public partial class MediaGenre : BaseEntity
{
    public string Name { get; set; }
    public virtual ICollection<MediaItemGenre> MediaItemGenres { get; set; } = new List<MediaItemGenre>();

}
