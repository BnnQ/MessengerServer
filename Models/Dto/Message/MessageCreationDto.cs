namespace MessengerServer.Models.Dto.Message;

public class MessageCreationDto
{
    public string Text { get; set; } = default!;
    public string SenderId { get; set; } = default!;
    public int ChatId { get; set; }
    public DateTime SentAt { get; set; }
}