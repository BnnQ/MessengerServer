using MessengerServer.Entities;
using MessengerServer.Extensions;
using MessengerServer.Models;
using MessengerServer.Models.Dto.Chat;
using MessengerServer.Models.Enums;
using MessengerServer.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Exception = System.Exception;

namespace MessengerServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController(
    IChatRepository chatRepository,
    IClientUpdateSender clientUpdateSender,
    ILoggerFactory loggerFactory)
    : ControllerBase
{
    private readonly ILogger<ChatController> logger = loggerFactory.CreateLogger<ChatController>();

    [HttpGet("GetChatById/{id:int}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatById([FromRoute] int id)
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(GetChatById), "Called with id: {id}", id);

        var chat = await chatRepository.GetChatByIdAsync(id);
        if (chat is null)
        {
            logger.LogActionWarning(HttpMethods.Get, nameof(GetChatById), "Chat with id: {id} not found",
                id);

            return NotFound();
        }

        logger.LogActionInformation(HttpMethods.Get, nameof(GetChatById), "Returned chat with id: {id}",
            id);
        return Ok(chat);
    }

    [HttpGet("GetChatsByUserId/{userId}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatsByUserId([FromRoute] string userId)
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(GetChatsByUserId),
            "Called with userId: {userId}", userId);
        var chats = await chatRepository.GetChatsByUserIdAsync(userId);

        logger.LogActionInformation(HttpMethods.Get, nameof(GetChatsByUserId),
            "Returned chats for userId: {userId}", userId);

        await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<ICollection<ChatInfoDto>>
        {
            ActionId = HttpContext.GetActionIdentifier(),
            ActionType = ActionType.ChatListUpdated,
            IsSuccess = true,
            ActionData = chats
        });

        return Ok(chats);
    }

    [HttpPost(nameof(GetChatByUserIds))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatByUserIds([FromBody] ICollection<string> userIds)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(GetChatByUserIds),
            "Called with userIds: {userIds}", userIds);
        var chat = await chatRepository.GetChatByUserIdsAsync(userIds);
        if (chat is null)
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(GetChatByUserIds),
                "Chat with userIds: {userIds} not found", userIds);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<Chat?>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatUpdated,
                IsSuccess = false,
                ActionData = null
            });
            return NotFound();
        }

        logger.LogActionInformation(HttpMethods.Post, nameof(GetChatByUserIds),
            "Returned chat with userIds: {userIds}", userIds);
        return Ok(chat);
    }

    [HttpGet("GetChatUsers/{chatId:int}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatUsers([FromRoute] int chatId)
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(GetChatUsers),
            "Called with chatId: {chatId}", chatId);

        try
        {
            var chatUsers = await chatRepository.GetChatUsersAsync(chatId);
            logger.LogActionInformation(HttpMethods.Get, nameof(GetChatUsers),
                "Returned chat users for chatId: {chatId}", chatId);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<ICollection<User>>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatUsersUpdated,
                IsSuccess = true,
                ActionData = chatUsers
            });
            return Ok(chatUsers);
        }
        catch (Exception e)
        {
            logger.LogActionWarning(HttpMethods.Get, nameof(GetChatUsers),
                "Chat users for chatId: {chatId} not found", chatId);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<ICollection<User>>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatUsersUpdated,
                IsSuccess = false,
                ActionData = null
            });
            return NotFound(e.Message);
        }
    }

    [HttpPost(nameof(CreateChat))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChat([FromBody] ChatCreationDto chat)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(CreateChat), "Called with chat: {chat}",
            chat);

        try
        {
            var createdChat = await chatRepository.CreateChatAsync(chat);
            logger.LogActionInformation(HttpMethods.Post, nameof(CreateChat),
                "Chat creation successful with chat: {chat}", chat);

            foreach (var user in createdChat.Users.Where(user => !string.IsNullOrWhiteSpace(user.LastFcmToken)))
            {
                await clientUpdateSender.SendUpdateAsync(user.Id, new ClientUpdate<Chat>
                {
                    ActionId = HttpContext.GetActionIdentifier(),
                    ActionType = ActionType.ChatCreated,
                    IsSuccess = true,
                    ActionData = createdChat
                });    
            }
            
            return CreatedAtAction(nameof(GetChatById), new { id = createdChat.Id }, createdChat);
        }
        catch (Exception exception)
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(CreateChat),
                "Chat creation failed with chat: {chat}", chat);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<Chat>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatCreated,
                IsSuccess = false,
                ActionData = null
            });
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("DeleteChat/{chatId:int}")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteChat([FromRoute] int chatId)
    {
        logger.LogActionInformation(HttpMethods.Delete, nameof(DeleteChat),
            "Called with chatId: {chatId}", chatId);
        try
        {
            await chatRepository.DeleteChatAsync(chatId);
            logger.LogActionInformation(HttpMethods.Delete, nameof(DeleteChat),
                "Chat deletion successful with chatId: {chatId}", chatId);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<int>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatDeleted,
                IsSuccess = true,
                ActionData = chatId
            });
            return NoContent();
        }
        catch (Exception exception)
        {
            logger.LogActionWarning(HttpMethods.Delete, nameof(DeleteChat),
                "Chat deletion failed with chatId: {chatId}", chatId);

            await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<int>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.ChatDeleted,
                IsSuccess = false,
                ActionData = chatId
            });
            return BadRequest(exception.Message);
        }
    }
}