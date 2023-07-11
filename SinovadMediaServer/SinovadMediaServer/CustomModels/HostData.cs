using SinovadMediaServer.DTOs;

#nullable disable

namespace SinovadMediaServer.CustomModels
{
    public class HostData
    {
        public string apiKey { get; set; }
        public int accountServerId { get; set; }
        public AccountServerDto accountServer { get; set; }

    }
}
