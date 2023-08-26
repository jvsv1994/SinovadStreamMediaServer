#nullable disable


namespace SinovadMediaServer.Application.DTOs
{
    public class StreamDto
    {
        public int index { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public int processID { get; set; }
        public bool isDefault { get; set; }
        public bool isForced { get; set; }
        public string routePath { get; set; }
        public string lineTextPlayList { get; set; }

    }
}
