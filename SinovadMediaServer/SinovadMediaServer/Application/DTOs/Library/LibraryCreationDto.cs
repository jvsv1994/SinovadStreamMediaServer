using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.DTOs.Library
{
    public class LibraryCreationDto
    {
        public int MediaServerId { get; set; }
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
        public MediaType MediaTypeCatalogDetailId { get; set; }
    }
}
