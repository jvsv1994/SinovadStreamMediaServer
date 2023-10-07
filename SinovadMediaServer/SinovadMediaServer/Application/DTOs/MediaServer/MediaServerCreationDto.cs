namespace SinovadMediaServer.Application.DTOs.MediaServer
{
    public class MediaServerCreationDto
    {
        public string IpAddress { get; set; }
        public string PublicIpAddress { get; set; }
        public string SecurityIdentifier { get; set; }
        public int? UserId { get; set; }
        public string Url { get; set; }
        public string FamilyName { get; set; }
        public string DeviceName { get; set; }
        public int Port { get; set; }
    }
}
