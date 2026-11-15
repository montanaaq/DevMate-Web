using System.Net;
using DevMate.Shared.Constants;

namespace DevMate.Http.Middleware;

public sealed class RoutingMiddleware(Router router)
{
    public async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        var match = router.Match(context.Request.HttpMethod, context.Path);
        if (match is null)
        {
            await next(context);
            return;
        }

        if (match.Handler is null)
        {
            context.Response.Headers["Allow"] = string.Join(", ", match.AllowedMethods);
            await context.WriteErrorAsync(
                HttpStatusCode.MethodNotAllowed,
                ErrorMessages.MethodNotAllowed
            );
            return;
        }

        context.RouteValues = match.Values;
        await match.Handler(context);
    }
}
