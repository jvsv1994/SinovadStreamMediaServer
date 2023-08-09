namespace SinovadMediaServer.Domain.Entities;

public partial class TranscoderSettings : BaseEntity
{
    public int MediaServerId { get; set; }
    public int VideoTransmissionTypeCatalogId { get; set; }
    public int VideoTransmissionTypeCatalogDetailId { get; set; }
    public int PresetCatalogId { get; set; }
    public int PresetCatalogDetailId { get; set; }
    public string TemporaryFolder { get; set; }
    public int ConstantRateFactor { get; set; }
}
