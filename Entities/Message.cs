namespace MessengerServer.Entities;

public class Message
{
    public int Id { get; set; }
    public string Text { get; set; } = default!;
    public DateTime SentAt { get; set; }
    
    public string SenderId { get; set; } = default!;
    public User Sender { get; set; } = default!;

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = default!;
}