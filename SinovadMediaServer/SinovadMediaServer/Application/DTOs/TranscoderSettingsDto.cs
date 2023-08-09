#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class TranscoderSettingsDto
    {
        public int Id { get; set; }
        public int MediaServerId { get; set; }
        public int VideoTransmissionTypeCatalogId { get; set; }
        public int VideoTransmissionTypeCatalogDetailId { get; set; }
        public int PresetCatalogId { get; set; }
        public int PresetCatalogDetailId { get; set; }
        public string TemporaryFolder { get; set; }
        public int ConstantRateFactor { get; set; }
    }
}
