#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class ItemsGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ItemDto> ListItems { get; set; }
        public int MediaServerId { get; set; }

    }
}
