using DevMate.Shared.Constants;

namespace DevMate.Http.Middleware;

public sealed class StaticFilesMiddleware(string root)
{
    private static readonly Dictionary<string, string> ContentTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "text/javascript; charset=utf-8",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
        [".woff2"] = "font/woff2",
    };

    public async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        const string prefix = "/static/";
        if (!context.Path.StartsWith(prefix, StringComparison.Ordinal))
        {
            await next(context);
            return;
        }

        if (context.Request.HttpMethod is not ("GET" or "HEAD"))
        {
            context.Response.Headers["Allow"] = "GET, HEAD";
            throw new HttpException(ApiError.MethodNotAllowed);
        }

        try
        {
            string relativePath = Uri.UnescapeDataString(context.Path[prefix.Length..]);
            string? path = ResolveFilePath(relativePath);
            if (
                path is null
                || !ContentTypes.TryGetValue(Path.GetExtension(path), out var contentType)
            )
            {
                throw new HttpException(ApiError.NotFound, ErrorMessages.FileNotFound);
            }

            await context.WriteFileAsync(path, contentType);
        }
        catch (Exception exception)
            when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new HttpException(ApiError.NotFound, ErrorMessages.FileNotFound, exception);
        }
    }

    private string? ResolveFilePath(string relativePath)
    {
        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (
            relativePath.IndexOfAny(['\\', '\0', ':']) >= 0
            || segments.Any(segment => segment.StartsWith('.'))
        )
        {
            return null;
        }

        string path = Path.GetFullPath(root);

        foreach (string segment in segments.Prepend(string.Empty))
        {
            path = Path.Combine(path, segment);
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            {
                return null;
            }
        }

        return File.Exists(path) ? path : null;
    }
}
