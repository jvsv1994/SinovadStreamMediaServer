namespace SinovadMediaServer.Domain.Entities;

public class BaseEntity : BaseAuditableEntity
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public bool? Deleted { get; set; }

}
