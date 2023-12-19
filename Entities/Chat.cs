namespace MessengerServer.Entities;

public class Chat
{
    public int Id { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<User> Users { get; set; } = new List<User>();
}