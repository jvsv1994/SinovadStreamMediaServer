
#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class MediaServerDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public int UserId { get; set; }
        public int StateCatalogId { get; set; }
        public int StateCatalogDetailId { get; set; }
        public string Url { get; set; }
    }
}
