namespace SinovadMediaServer.Domain.Entities;

public partial class VideoProfile : BaseAuditableEntity
{
    public int VideoId { get; set; }

    public int ProfileId { get; set; }

    public double DurationTime { get; set; }

    public double CurrentTime { get; set; }

    public virtual Video Video { get; set; } = null!;
}
