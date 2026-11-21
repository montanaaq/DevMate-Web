using SkillIssue.Http;
using SkillIssue.Http.Routing;
using SkillIssue.Templates;

namespace SkillIssue.Controllers;

public sealed class PagesController
{
    public static Task HomeAsync(RequestContext context)
    {
        return WritePageAsync(context, "Home.html");
    }

    public static Task ProjectsAsync(RequestContext context)
    {
        return WritePageAsync(context, "Projects.html");
    }

    public static Task SignInAsync(RequestContext context)
    {
        return WritePageAsync(context, "SignIn.html");
    }

    public static Task ProfileAsync(RequestContext context)
    {
        return WritePageAsync(context, "Profile.html");
    }

    public static Task MyProjectsAsync(RequestContext context)
    {
        return WritePageAsync(context, "MyProjects.html");
    }

    public static Task SignUpAsync(RequestContext context)
    {
        return WritePageAsync(context, "SignUp.html");
    }

    private static async Task WritePageAsync(RequestContext context, string fileName)
    {
        var user = context.User;
        var templateData = new Dictionary<string, object?>
        {
            ["currentPath"] = "/" + string.Join('/', Router.GetPathSegments(context.Path)),
            ["isSignedIn"] = user is not null,
            ["accountName"] = user?.FullName ?? string.Empty,
        };

        string html = await TemplateEngine.RenderAsync(
            fileName,
            templateData,
            context.CancellationToken
        );
        await context.WriteTextAsync(html, "text/html; charset=utf-8");
    }
}
