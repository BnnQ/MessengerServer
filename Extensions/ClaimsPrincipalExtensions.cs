using System.Security.Claims;

namespace MessengerServer.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User is not authorized");
    }
}