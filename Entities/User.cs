using Microsoft.AspNetCore.Identity;

namespace MessengerServer.Entities;

public class User : IdentityUser
{
    public string LastFcmToken { get; set; } = null!;
    public string AvatarImagePath { get; set; } = null!;
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}