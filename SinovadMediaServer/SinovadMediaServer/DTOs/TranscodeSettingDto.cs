#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class TranscodeSettingDto
    {
        public int Id { get; set; }
        public int AccountServerId { get; set; }
        public int TransmissionMethodCatalogId { get; set; }
        public int TransmissionMethodCatalogDetailId { get; set; }
        public int PresetCatalogId { get; set; }
        public int PresetCatalogDetailId { get; set; }
        public string DirectoryPhysicalPath { get; set; }
        public int ConstantRateFactor { get; set; }
    }
}
