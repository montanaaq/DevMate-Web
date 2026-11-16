using DevMate.Http.Errors;

namespace DevMate.Http.Middleware;

public sealed class CsrfMiddleware(Uri applicationUrl)
{
    private readonly string _allowedOrigin = applicationUrl.GetLeftPart(UriPartial.Authority);

    public async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        string method = context.Request.HttpMethod;

        if (method is "GET" or "HEAD" or "OPTIONS")
        {
            await next(context);
            return;
        }

        string? origin = context.Request.Headers["Origin"];

        if (!string.Equals(origin, _allowedOrigin, StringComparison.OrdinalIgnoreCase))
        {
            throw new HttpException(ApiError.Forbidden, ErrorMessages.CsrfValidationFailed);
        }

        await next(context);
    }
}
