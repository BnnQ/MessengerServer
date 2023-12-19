namespace MessengerServer.Configuration;

public class Jwt
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Key { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new InvalidOperationException("Jwt:Secret is not set");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Jwt:Issuer is not set");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Audience is not set");
    }
}