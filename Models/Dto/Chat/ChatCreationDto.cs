namespace MessengerServer.Models.Dto.Chat;

public class ChatCreationDto
{
    public string CreatorId { get; set; } = default!;
    public ICollection<string> UserUsernames { get; set; } = new List<string>();
}