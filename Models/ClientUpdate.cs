using MessengerServer.Models.Enums;

namespace MessengerServer.Models;

public class ClientUpdate<T>
{
    public string ActionId { get; set; } = default!;
    public ActionType ActionType { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public T? ActionData { get; set; }
}