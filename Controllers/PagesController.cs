using System.Net;
using SkillIssue.Http;

namespace SkillIssue.Controllers;

public sealed class PagesController(string viewsDirectory)
{
    private const string CurrentPage = " aria-current=\"page\"";

    public Task HomeAsync(RequestContext context)
    {
        return WritePageAsync(context, "Home.html");
    }

    public Task SignInAsync(RequestContext context)
    {
        return WritePageAsync(context, "SignIn.html");
    }

    public Task ProfileAsync(RequestContext context)
    {
        return WritePageAsync(context, "Profile.html");
    }

    public Task MyProjectsAsync(RequestContext context)
    {
        return WritePageAsync(context, "MyProjects.html");
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
        string header = await File.ReadAllTextAsync(
            Path.Combine(viewsDirectory, "SiteHeader.html"),
            context.CancellationToken
        );
        bool isHome = fileName == "Home.html";
        var user = context.User;
        header = header
            .Replace("{{home-current}}", isHome ? CurrentPage : string.Empty)
            .Replace(
                "{{projects-current}}",
                fileName == "MyProjects.html" ? CurrentPage : string.Empty
            )
            .Replace("{{signin-current}}", fileName == "SignIn.html" ? CurrentPage : string.Empty)
            .Replace("{{profile-current}}", fileName == "Profile.html" ? CurrentPage : string.Empty)
            .Replace("{{guest-hidden}}", user is not null ? " hidden" : string.Empty)
            .Replace("{{signed-in-hidden}}", user is null ? " hidden" : string.Empty)
            .Replace("{{account-name}}", WebUtility.HtmlEncode(user?.FullName ?? string.Empty));

        await context.WriteTextAsync(
            html.Replace("        <!-- site-header -->", header.TrimEnd()),
            "text/html; charset=utf-8"
        );
    }
}
