using DevMate.Services;

namespace DevMate.Http.Middleware;

public sealed class AuthenticationMiddleware(IAuthService authService)
{
    public async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        string? token = context.Request.Cookies[SessionCookie.Name]?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            var (Session, Renewed) = await authService.FindSessionAsync(
                token,
                context.CancellationToken
            );
            context.Session = Session;

            if (Renewed && Session is not null)
            {
                SessionCookie.Set(context, token, Session.ExpiresAt);
            }
        }

        await next(context);
    }
}
