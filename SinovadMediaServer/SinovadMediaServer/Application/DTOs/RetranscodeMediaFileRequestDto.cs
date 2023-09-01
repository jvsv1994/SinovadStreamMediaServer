#nullable disable
namespace SinovadMediaServer.Application.DTOs
{
    public class RetranscodeMediaFileRequestDto
    {
        public string Guid { get; set; }
        public double NewTime { get; set; }
    }
}
