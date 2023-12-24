namespace MessengerServer.Models.Enums;

public enum ActionType : byte
{
    MessageReceived,
    MessageSent,
    MessageDeleted,
    ChatMessagesUpdated,
    ChatListUpdated,
    ChatUpdated,
    ChatUsersUpdated,
    ChatCreated,
    ChatDeleted,
    UserRegistered,
    UserLoggedIn,
    UserUpdated,
    UserStatusChanged
}