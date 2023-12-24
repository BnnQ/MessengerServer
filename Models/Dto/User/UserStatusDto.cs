using MessengerServer.Models.Enums;

namespace MessengerServer.Models.Dto.User;

public class UserStatusDto
{
    public string UserId { get; set; } = null!;
    public StatusType Status { get; set; }
}