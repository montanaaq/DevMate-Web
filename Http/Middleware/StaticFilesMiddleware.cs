using System.Net;
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
            await context.WriteErrorAsync(
                HttpStatusCode.MethodNotAllowed,
                ErrorMessages.MethodNotAllowed
            );
            return;
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
                await context.WriteErrorAsync(HttpStatusCode.NotFound, ErrorMessages.FileNotFound);
                return;
            }

            await context.WriteFileAsync(path, contentType);
        }
        catch (Exception exception)
            when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            await context.WriteErrorAsync(HttpStatusCode.NotFound, ErrorMessages.FileNotFound);
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

        // Include the static root itself in the symbolic-link check.
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
