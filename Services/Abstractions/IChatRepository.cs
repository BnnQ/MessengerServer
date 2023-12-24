using MessengerServer.Entities;
using MessengerServer.Models.Dto.Chat;

namespace MessengerServer.Services.Abstractions;

public interface IChatRepository
{
    Task<Chat?> GetChatByIdAsync(int id);
    Task<ICollection<ChatInfoDto>> GetChatsByUserIdAsync(string userId);
    Task<Chat?> GetChatByUserIdsAsync(ICollection<string> userIds);
    Task<ICollection<User>> GetChatUsersAsync(int chatId);
    Task<Chat> CreateChatAsync(ChatCreationDto chat);
    Task DeleteChatAsync(int chatId);
}