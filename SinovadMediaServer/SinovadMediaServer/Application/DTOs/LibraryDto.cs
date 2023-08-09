#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class LibraryDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int MediaServerId { get; set; }
        public string PhysicalPath { get; set; }
        public int MediaTypeCatalogId { get; set; }
        public int MediaTypeCatalogDetailId { get; set; }
        public List<string> ListPaths { get; set; }

    }
}
