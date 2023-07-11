#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class AccountStorageDto
    {
        public int Id { get; set; }
        public int AccountServerId { get; set; }
        public string PhisicalPath { get; set; }
        public int AccountStorageTypeId { get; set; }
        public List<string> ListPaths { get; set; }

    }
}
