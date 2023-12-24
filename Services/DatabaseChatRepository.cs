using AutoMapper;
using MessengerServer.Contexts;
using MessengerServer.Entities;
using MessengerServer.Models.Dto.Chat;
using MessengerServer.Models.Dto.User;
using MessengerServer.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Services;

public class DatabaseChatRepository(SqlServerDbContext context, IMapper mapper) : IChatRepository
{
    public async Task<Chat?> GetChatByIdAsync(int id)
    {
        var chat = await context.Chats.AsNoTracking()
            .FirstAsync(chat => chat.Id == id);
        return chat;
    }

    public async Task<ICollection<ChatInfoDto>> GetChatsByUserIdAsync(string userId)
    {
        var chats = await context.Chats
            .Where(chat => chat.Users.Any(user => user.Id.Equals(userId)))
            .Select(chat => new ChatInfoDto
            {
                Id = chat.Id,
                Users = chat.Users.Select(user => new UserInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    AvatarImagePath = user.AvatarImagePath
                }).ToList(),
                LastMessage = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
            })
            .AsNoTracking()
            .ToListAsync();

        return chats;
    }


    public async Task<Chat?> GetChatByUserIdsAsync(ICollection<string> userIds)
    {
        var chat = await context.Chats.Include(chat => chat.Users)
            .Where(chat => chat.Users!.All(user => userIds.Contains(user.Id)))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return chat;
    }
    
    public async Task<ICollection<User>> GetChatUsersAsync(int chatId)
    {
        var chat = await context.Chats.Include(chat => chat.Users)
            .AsNoTracking()
            .SingleOrDefaultAsync(chat => chat.Id == chatId);

        if (chat is null)
        {
            throw new InvalidOperationException($"Chat with id: {chatId} not found");
        }
        
        return chat.Users!;
    }

    public async Task<Chat> CreateChatAsync(ChatCreationDto chat)
    {
        var users = await context.Users.Where(user => chat.UserUsernames.Contains(user.UserName!))
            .ToListAsync();

        var currentUser = await context.Users.FindAsync(chat.CreatorId);
        users.Add(currentUser!);
        var chatEntity = new Chat
        {
            Users = users
        };
        await context.Chats.AddAsync(chatEntity);
        await context.SaveChangesAsync();

        return chatEntity;
    }

    public async Task DeleteChatAsync(int chatId)
    {
        var chat = await context.Chats.FindAsync(chatId);
        if (chat is not null)
            context.Chats.Remove(chat);

        await context.SaveChangesAsync();
    }
}