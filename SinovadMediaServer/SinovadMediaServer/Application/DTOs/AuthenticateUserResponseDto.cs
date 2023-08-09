namespace SinovadMediaServer.Application.DTOs
{
    public class AuthenticateUserResponseDto
    {
        public string ApiToken { get; set; }
        public UserDto User { get; set; }

    }
}
