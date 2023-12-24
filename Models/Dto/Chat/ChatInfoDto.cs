using MessengerServer.Models.Dto.User;

namespace MessengerServer.Models.Dto.Chat;

public class ChatInfoDto
{
    public int Id { get; set; }
    public ICollection<UserInfoDto> Users { get; set; } = new List<UserInfoDto>();
    public Entities.Message? LastMessage { get; set; } = default;
}