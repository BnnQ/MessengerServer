namespace MessengerServer.Models.Dto.Authentication;

public class AuthenticationDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}