using DevMate.Http.Errors;
using DevMate.Http.Routing;

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
            throw new HttpException(ApiError.MethodNotAllowed);
        }

        context.RouteValues = match.Values;
        await match.Handler(context);
    }
}
