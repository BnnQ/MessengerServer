using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MessengerServer.Configuration;
using MessengerServer.Entities;
using MessengerServer.Extensions;
using MessengerServer.Models;
using MessengerServer.Models.Dto.Authentication;
using MessengerServer.Models.Dto.User;
using MessengerServer.Models.Enums;
using MessengerServer.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MessengerServer.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IClientUpdateSender clientUpdateSender;
    private readonly Jwt jwtOptions;
    private readonly ILogger<AuthenticationController> logger;
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;

    public AuthenticationController(
        UserManager<User> userManager,
        IOptions<Jwt> jwtOptions,
        ILoggerFactory loggerFactory,
        IMapper mapper,
        IClientUpdateSender clientUpdateSender)
    {
        this.jwtOptions = jwtOptions.Value;
        this.jwtOptions.Validate();

        this.userManager = userManager;
        logger = loggerFactory.CreateLogger<AuthenticationController>();
        this.mapper = mapper;
        this.clientUpdateSender = clientUpdateSender;
    }

    [HttpPost(nameof(Register))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] AuthenticationDto authenticationDto)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(Register),
            "Called with username: {username}", authenticationDto.UserName);
        var result = await userManager.CreateAsync(new User { UserName = authenticationDto.UserName },
            authenticationDto.Password);
        if (result.Succeeded)
        {
            logger.LogActionInformation(HttpMethods.Post, nameof(Register),
                "User registration successful for username: {username}", authenticationDto.UserName);

            await clientUpdateSender.SendUpdateWithTokenAsync(authenticationDto.FcmToken,
                new ClientUpdate<UserInfoDto>
                {
                    ActionId = HttpContext.GetActionIdentifier(),
                    ActionType = ActionType.UserRegistered,
                    ActionData =
                        mapper.Map<UserInfoDto>(
                            await userManager.FindByNameAsync(authenticationDto.UserName)),
                    IsSuccess = true
                });
            return Ok();
        }

        logger.LogActionWarning(HttpMethods.Post, nameof(Register),
            "User registration failed for username: {username}", authenticationDto.UserName);
        await clientUpdateSender.SendUpdateWithTokenAsync(authenticationDto.FcmToken,
            new ClientUpdate<UserInfoDto>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.UserRegistered,
                ActionData = null,
                IsSuccess = false
            });
        return BadRequest(result.Errors);
    }

    [HttpPost(nameof(Login))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] AuthenticationDto authenticationDto)
    {
        logger.LogActionInformation(HttpMethods.Post, nameof(Login), "Called with username: {username}",
            authenticationDto.UserName);
        var user = await userManager.FindByNameAsync(authenticationDto.UserName);
        if (user is null || !await userManager.CheckPasswordAsync(user, authenticationDto.Password))
        {
            logger.LogActionWarning(HttpMethods.Post, nameof(Login),
                "Invalid login attempt for username: {username}", authenticationDto.UserName);

            await clientUpdateSender.SendUpdateWithTokenAsync(authenticationDto.FcmToken,
                new ClientUpdate<UserInfoDto>
                {
                    ActionId = HttpContext.GetActionIdentifier(),
                    ActionType = ActionType.UserLoggedIn,
                    ActionData = null,
                    IsSuccess = false
                });

            return BadRequest("Invalid login attempt");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid()
                .ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(jwtOptions.Issuer, jwtOptions.Audience, claims,
            expires: DateTime.Now.AddMinutes(10), signingCredentials: credentials);

        logger.LogActionInformation(HttpMethods.Post, nameof(Login),
            "User logged in successfully with username: {username}", authenticationDto.UserName);
        await clientUpdateSender.SendUpdateWithTokenAsync(authenticationDto.FcmToken,
            new ClientUpdate<dynamic>
            {
                ActionId = HttpContext.GetActionIdentifier(),
                ActionType = ActionType.UserLoggedIn,
                ActionData = new
                    { Token = new JwtSecurityTokenHandler().WriteToken(token),
                        CurrentUser = user },
                IsSuccess = true
            });
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpGet(nameof(GetCurrentUser))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(GetCurrentUser), "Called");
        var user = await userManager.GetUserAsync(User);
        if (user is not null)
        {
            logger.LogActionInformation(HttpMethods.Get, nameof(GetCurrentUser),
                "Returned user with username: {username}", user.UserName!);

            var userResponse = mapper.Map<UserInfoDto>(user);
            await clientUpdateSender.SendUpdateAsync(User.GetId(),
                new ClientUpdate<UserInfoDto>
                {
                    ActionId = HttpContext.GetActionIdentifier(), ActionData = userResponse,
                    ActionType = ActionType.UserUpdated, IsSuccess = true
                });

            return Ok();
        }

        logger.LogActionWarning(HttpMethods.Get, nameof(GetCurrentUser), "Failed to find user");
        return Unauthorized();
    }

    [HttpGet(nameof(IsAuthenticated))]
    [Authorize("Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult IsAuthenticated()
    {
        logger.LogActionInformation(HttpMethods.Get, nameof(IsAuthenticated), "Called");
        return Ok();
    }
}