using System.Security.Claims;
using MessengerServer.Contexts;
using MessengerServer.Extensions;
using MessengerServer.Models.Dto.ClientCommunication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Controllers;

[Route("api/client")]
[ApiController]
public class ClientCommunicationController(SqlServerDbContext context, ILoggerFactory loggerFactory)
    : ControllerBase
{
    private readonly ILogger<ClientCommunicationController> logger =
        loggerFactory.CreateLogger<ClientCommunicationController>();

    [HttpPost("RefreshFcmToken")]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshFcmToken([FromBody] UpdatedFcmTokenDto updatedFcmTokenDto)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(RefreshFcmToken),
            "Called with fcmToken: {fcmToken}", updatedFcmTokenDto.FcmToken);

        var user = await context.Users
            .Where(user => user.Id.Equals(User.GetId()))
            .FirstOrDefaultAsync();
        if (user is null)
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(RefreshFcmToken),
                "User not found");
            return Unauthorized();
        }

        user.LastFcmToken = updatedFcmTokenDto.FcmToken;
        await context.SaveChangesAsync();
        logger.LogActionInformation(HttpMethods.Post, nameof(RefreshFcmToken),
            "Refreshed fcmToken for user: {user}", user.UserName!);
        return Ok();
    }
}