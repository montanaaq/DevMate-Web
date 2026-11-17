using SkillIssue.Http;

namespace SkillIssue.Controllers;

public sealed class PagesController(string viewsDirectory)
{
    public Task HomeAsync(RequestContext context)
    {
        return WritePageAsync(context, "Home.html");
    }

    public Task SignInAsync(RequestContext context)
    {
        return WritePageAsync(context, "SignIn.html");
    }

    public Task SignUpAsync(RequestContext context)
    {
        return WritePageAsync(context, "SignUp.html");
    }

    private async Task WritePageAsync(RequestContext context, string fileName)
    {
        string html = await File.ReadAllTextAsync(
            Path.Combine(viewsDirectory, fileName),
            context.CancellationToken
        );
        await context.WriteTextAsync(html, "text/html; charset=utf-8");
    }
}
