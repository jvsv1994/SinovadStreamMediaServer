namespace SinovadMediaServer.Domain.Entities;

public partial class Library : BaseEntity
{
    public int MediaServerId { get; set; }
    public string PhysicalPath { get; set; }
    public int MediaTypeCatalogId { get; set; }
    public int MediaTypeCatalogDetailId { get; set; }
    public string Name { get; set; }

}
