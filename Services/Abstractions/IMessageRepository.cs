using MessengerServer.Entities;
using MessengerServer.Models.Dto.Message;

namespace MessengerServer.Services.Abstractions;

public interface IMessageRepository
{
    Task<Message?> GetMessageByIdAsync(int id);
    Task<ICollection<Message>> GetMessagesByChatIdAsync(int chatId);
    Task<Message> CreateMessageAsync(MessageCreationDto message);
    Task DeleteMessageAsync(int messageId);
}