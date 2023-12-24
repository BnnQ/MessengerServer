namespace MessengerServer.Extensions;

public static class HttpContextExtensions
{
    public static T GetRequiredItem<T>(this HttpContext context, string key)
    {
        if (context.Items.TryGetValue(key, out var value))
            return (T)value!;

        throw new ArgumentException($"Item with key {key} not found in HttpContext");
    }

    public static string GetActionIdentifier(this HttpContext context)
    {
        return context.GetRequiredItem<string>("ActionIdentifier");
    }
}