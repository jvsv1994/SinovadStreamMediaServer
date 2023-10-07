using SinovadMediaServer.Application.DTOs.MediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class AuthenticateMediaServerResponseDto
    {
        public string ApiToken { get; set; }
        public MediaServerDto MediaServer { get; set; }
        public UserDto User { get; set; }

    }
}
