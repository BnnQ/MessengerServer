using MessengerServer.Entities;
using MessengerServer.Extensions;
using MessengerServer.Models;
using MessengerServer.Models.Dto.Message;
using MessengerServer.Models.Dto.User;
using MessengerServer.Models.Enums;
using MessengerServer.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessengerServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController(
    IMessageRepository messageRepository,
    IClientUpdateSender clientUpdateSender,
    IChatRepository chatRepository,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<MessageController> logger = loggerFactory.CreateLogger<MessageController>();

    [HttpGet("GetMessagesByChatId/{chatId:int}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesByChatId([FromRoute] int chatId)
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(GetMessagesByChatId),
            "Called with chatId: {chatId}", chatId);
        var messages = await messageRepository.GetMessagesByChatIdAsync(chatId);

        logger.LogActionInformation(HttpMethods.Get, nameof(GetMessagesByChatId),
            "Returned messages for chatId: {chatId}", chatId);
        await clientUpdateSender.SendUpdateAsync(User.GetId(),
            new ClientUpdate<ICollection<Message>>
            {
                ActionId = HttpContext.GetActionIdentifier(), ActionData = messages,
                ActionType = ActionType.ChatMessagesUpdated, IsSuccess = true
            });

        return Ok();
    }

    [HttpPost(nameof(SendMessage))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage([FromBody] MessageCreationDto messageCreationDto)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(SendMessage),
            "Called with message: {message}", messageCreationDto.Text);
        try
        {
            var message = await messageRepository.CreateMessageAsync(messageCreationDto);
            logger.LogActionInformation(HttpMethods.Post, nameof(SendMessage),
                "Message creation successful for message: {message}", messageCreationDto.Text);

            await clientUpdateSender.SendUpdateAsync(User.GetId(),
                new ClientUpdate<Message>
                {
                    ActionId = HttpContext.GetActionIdentifier(), ActionData = message,
                    ActionType = ActionType.MessageSent, IsSuccess = true
                });

            var users = await chatRepository.GetChatUsersAsync(message.ChatId);
            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.LastFcmToken) || user.Id.Equals(User.GetId()))
                    continue;

                await clientUpdateSender.SendUpdateAsync(user.Id,
                    new ClientUpdate<Message>
                    {
                        ActionId = HttpContext.GetActionIdentifier(), ActionData = message,
                        ActionType = ActionType.MessageReceived, IsSuccess = true
                    });

                // TODO: Separate all status-related updates to a separate "controller" service
                await clientUpdateSender.SendUpdateAsync(user.Id,
                    new ClientUpdate<UserStatusDto>
                    {
                        ActionId = HttpContext.GetActionIdentifier(), ActionData = new UserStatusDto
                        {
                            UserId = messageCreationDto.SenderId,
                            Status = StatusType.Online
                        },
                        ActionType = ActionType.UserStatusChanged,
                        IsSuccess = true
                    });
            }

            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(SendMessage),
                "Message creation failed for message: {message}. Details: {exception}",
                messageCreationDto.Text, exception.Message);

            await clientUpdateSender.SendUpdateAsync(User.GetId(),
                new ClientUpdate<string>
                {
                    ActionId = HttpContext.GetActionIdentifier(), ActionData = "Message creation failed",
                    ActionType = ActionType.MessageSent, IsSuccess = false
                });
            return BadRequest();
        }
    }

    [HttpDelete("DeleteMessage/{messageId:int}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMessage([FromRoute] int messageId)
    {
        logger.LogActionInformation(HttpMethods.Delete, nameof(DeleteMessage),
            "Called with messageId: {messageId}", messageId);
        try
        {
            await messageRepository.DeleteMessageAsync(messageId);
            logger.LogActionInformation(HttpMethods.Delete, nameof(DeleteMessage),
                "Message deletion successful for messageId: {messageId}", messageId);

            await clientUpdateSender.SendUpdateAsync(User.GetId(),
                new ClientUpdate<int>
                {
                    ActionId = HttpContext.GetActionIdentifier(), ActionData = messageId,
                    ActionType = ActionType.MessageDeleted, IsSuccess = true
                });
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogActionWarning(HttpMethods.Delete, nameof(DeleteMessage),
                "Message deletion failed for messageId: {messageId}. Details: {exception}", messageId,
                exception.Message);

            await clientUpdateSender.SendUpdateAsync(User.GetId(),
                new ClientUpdate<string>
                {
                    ActionId = HttpContext.GetActionIdentifier(), ActionData = "Message deletion failed",
                    ActionType = ActionType.MessageDeleted, IsSuccess = false
                });
            return BadRequest();
        }
    }
}