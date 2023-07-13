#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class StorageDto
    {
        public int Id { get; set; }
        public int MediaServerId { get; set; }
        public string PhysicalPath { get; set; }
        public int MediaTypeCatalogId { get; set; }
        public int MediaTypeCatalogDetailId { get; set; }
        public List<string> ListPaths { get; set; }

    }
}
