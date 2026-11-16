using DevMate.Http;

namespace DevMate.Controllers;

public sealed class PagesController(string viewsDirectory)
{
    public async Task HomeAsync(RequestContext context)
    {
        string html = await File.ReadAllTextAsync(
            Path.Combine(viewsDirectory, "Home.html"),
            context.CancellationToken
        );
        await context.WriteTextAsync(html, "text/html; charset=utf-8");
    }
}
