using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using MessengerServer.Contexts;
using MessengerServer.Extensions;
using MessengerServer.Models;
using MessengerServer.Services.Abstractions;

namespace MessengerServer.Services;

public class FirebaseClientUpdateSender(FirebaseApp firebaseApp, SqlServerDbContext context)
    : IClientUpdateSender
{
    public async Task RefreshTokenAsync(string userId, string userToken)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            throw new ArgumentException($"User with id {userId} not found");

        user.LastFcmToken = userToken;
        await context.SaveChangesAsync();
    }

    public async Task SendUpdateAsync<T>(string userId, ClientUpdate<T> clientUpdate)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            throw new ArgumentException($"User with id {userId} not found");

        var userToken = user.LastFcmToken;
        var message = new Message
        {
            Data = clientUpdate.SerializeToDictionary(),
            Token = userToken
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task SendUpdateWithTokenAsync<T>(string userToken, ClientUpdate<T> clientUpdate)
    {
        var message = new Message
        {
            Data = clientUpdate.SerializeToDictionary(),
            Token = userToken
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
}