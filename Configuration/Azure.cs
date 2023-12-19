using Microsoft.IdentityModel.Tokens;

namespace MessengerServer.Configuration;

public class Azure
{
    public string? AvatarImageContainerName { get; init; }

    public void Validate()
    {
        if (AvatarImageContainerName.IsNullOrEmpty())
            throw new InvalidOperationException("Azure:AvatarImageContainerName is not set");
    }
}