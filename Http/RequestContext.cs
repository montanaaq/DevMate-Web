using System.Net;
using System.Text;
using System.Text.Json;
using DevMate.Http.Errors;
using DevMate.Models.Users;

namespace DevMate.Http;

public sealed class RequestContext(HttpListenerContext context, CancellationToken cancellationToken)
{
    public HttpListenerRequest Request => context.Request;
    public HttpListenerResponse Response => context.Response;
    public CancellationToken CancellationToken { get; } = cancellationToken;
    public Session? Session { get; internal set; }
    public string Path => Request.Url?.AbsolutePath ?? "/";
    public bool HasStarted { get; private set; }
    public IReadOnlyDictionary<string, string> RouteValues { get; internal set; } =
        new Dictionary<string, string>();

    public Task WriteJsonAsync<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return WriteTextAsync(
            JsonSerializer.Serialize(value),
            "application/json; charset=utf-8",
            statusCode
        );
    }

    public async Task WriteTextAsync(
        string content,
        string contentType,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        StartResponse(statusCode, contentType, bytes.Length);

        if (Request.HttpMethod != "HEAD")
        {
            await Response.OutputStream.WriteAsync(bytes, CancellationToken);
        }
    }

    public async Task WriteFileAsync(string path, string contentType)
    {
        await using var file = File.OpenRead(path);
        StartResponse(HttpStatusCode.OK, contentType, file.Length);

        if (Request.HttpMethod != "HEAD")
        {
            await file.CopyToAsync(Response.OutputStream, CancellationToken);
        }
    }

    public Task WriteErrorAsync(ApiError error, string? message = null)
    {
        HttpStatusCode statusCode = error.StatusCode;
        message ??= error.Message;

        if (Path.StartsWith("/api/", StringComparison.Ordinal))
        {
            return WriteJsonAsync(new { code = error.Code, message }, statusCode);
        }

        string html = $$"""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{(int)statusCode}} — DevMate</title>
                <link rel="stylesheet" href="/static/vendor/bootstrap/bootstrap.min.css">
            </head>
            <body>
                <main class="container py-5">
                    <h1>{{(int)statusCode}}</h1>
                    <p>{{WebUtility.HtmlEncode(message)}}</p>
                    <a href="/">На главную</a>
                </main>
            </body>
            </html>
            """;

        return WriteTextAsync(html, "text/html; charset=utf-8", statusCode);
    }

    private void StartResponse(HttpStatusCode statusCode, string contentType, long contentLength)
    {
        Response.StatusCode = (int)statusCode;
        Response.ContentType = contentType;
        Response.ContentLength64 = contentLength;
        HasStarted = true;
    }
}
