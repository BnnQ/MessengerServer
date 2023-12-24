using AutoMapper;
using MessengerServer.Contexts;
using MessengerServer.Entities;
using MessengerServer.Models.Dto.Message;
using MessengerServer.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Services;

public class DatabaseMessageRepository(SqlServerDbContext context, IMapper mapper) : IMessageRepository
{
    public async Task<Message?> GetMessageByIdAsync(int id)
    {
        var message = await context.Messages.AsNoTracking()
            .FirstOrDefaultAsync(message => message.Id == id);
        return message;
    }

    public async Task<ICollection<Message>> GetMessagesByChatIdAsync(int chatId)
    {
        var messages = context.Messages.Where(message => message.ChatId == chatId)
            .AsNoTracking();
        return await messages.ToListAsync();
    }

    public async Task<Message> CreateMessageAsync(MessageCreationDto message)
    {
        var messageEntity = mapper.Map<Message>(message);

        await context.Messages.AddAsync(messageEntity);
        await context.SaveChangesAsync();

        return messageEntity;
    }

    public async Task DeleteMessageAsync(int messageId)
    {
        var message = await context.Messages.FirstAsync(message => message.Id == messageId);
        context.Messages.Remove(message);
        await context.SaveChangesAsync();
    }
}