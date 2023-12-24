namespace MessengerServer.Middlewares;

public class ClientCommunicationProcessingMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Items["ActionIdentifier"] = context.Request.Headers["ActionIdentifier"]
            .ToString();

        return next(context);
    }
}