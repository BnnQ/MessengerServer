using MessengerServer.Models;

namespace MessengerServer.Services.Abstractions;

public interface IClientUpdateSender
{
    public Task RefreshTokenAsync(string userId, string userToken);
    public Task SendUpdateAsync<T>(string userId, ClientUpdate<T> clientUpdate);
    public Task SendUpdateWithTokenAsync<T>(string userToken, ClientUpdate<T> clientUpdate);
}