#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class CatalogDetailDto
    {
        public int CatalogId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string TextValue { get; set; }
        public int? NumberValue { get; set; }
    }
}
