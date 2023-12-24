using AutoMapper;
using MessengerServer.Configuration;
using MessengerServer.Contexts;
using MessengerServer.Entities;
using MessengerServer.Extensions;
using MessengerServer.Models;
using MessengerServer.Models.Dto.User;
using MessengerServer.Models.Enums;
using MessengerServer.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MessengerServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IClientUpdateSender clientUpdateSender;
    private readonly Jwt jwtOptions;
    private readonly ILogger<UserController> logger;
    private readonly SqlServerDbContext context;
    private readonly IMapper mapper;
    
    public UserController(
        UserManager<User> userManager,
        IOptions<Jwt> jwtOptions,
        IClientUpdateSender clientUpdateSender,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        SqlServerDbContext context)
    {
        this.jwtOptions = jwtOptions.Value;
        this.jwtOptions.Validate();

        logger = loggerFactory.CreateLogger<UserController>();
        this.clientUpdateSender = clientUpdateSender;
        this.mapper = mapper;
        this.context = context;
    }

    [HttpPost(nameof(UpdateUser))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(IFormFile avatar, [FromServices] IFormFileManager fileManager)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(UpdateUser),
            "Called with avatar for user: {id}", User.GetId());
        
        var userEntity = await context.Users.Where(user => user.Id.Equals(User.GetId())).FirstOrDefaultAsync();
        if (userEntity is null)
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(UpdateUser),
                "User not found");
            return BadRequest();
        }
        
        var imagePath = await fileManager.SaveFileAsync(avatar); 
        userEntity.AvatarImagePath = imagePath.ToString();
        
        await context.SaveChangesAsync();

        logger.LogActionInformation(HttpMethods.Post, nameof(UpdateUser),
            "User updated successful for username: {username}", userEntity.UserName!);
        await clientUpdateSender.SendUpdateAsync(User.GetId(), new ClientUpdate<UserInfoDto>
        {
            ActionId = HttpContext.GetActionIdentifier(),
            ActionType = ActionType.UserUpdated,
            ActionData = mapper.Map<UserInfoDto>(userEntity),
            IsSuccess = true
        });
        
        return Ok();
    }
}