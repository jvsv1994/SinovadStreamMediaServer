#nullable disable


namespace SinovadMediaServer.Application.DTOs
{
    public class DirectoryDto
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsMainDirectory { get; set; }
        public List<DirectoryDto> ListSubdirectories { get; set; }

    }
}
