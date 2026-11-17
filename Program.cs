using System.Globalization;
using SkillIssue.Configuration;
using SkillIssue.Controllers;
using SkillIssue.Http;
using SkillIssue.Http.Errors;
using SkillIssue.Http.Middleware;
using SkillIssue.Http.Routing;
using SkillIssue.Models.Users;
using SkillIssue.Repositories;
using SkillIssue.Services;
using FluentValidation;

namespace SkillIssue;

internal static class Program
{
    private static async Task Main()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };

        DotNetEnv.Env.NoClobber().TraversePath().Load();
        var settings = AppSettings.FromEnvironment();
        await using var dataSource = DatabaseSettings.CreateDataSource(
            settings.DatabaseConnectionString
        );

        var userRepository = new UserRepository(dataSource);
        var sessionRepository = new SessionRepository(dataSource);
        var authService = new AuthService(userRepository, sessionRepository);
        var users = new UserController(authService);

        var pages = new PagesController(Path.Combine(AppContext.BaseDirectory, "Views"));

        var router = new Router();
        UserRole[] publicRoles = [UserRole.Guest, UserRole.User, UserRole.Admin];
        router.Map("GET", "/", pages.HomeAsync, publicRoles);
        router.Map("GET", "/signin", pages.SignInAsync, publicRoles);
        router.Map("GET", "/signup", pages.SignUpAsync, publicRoles);
        router.Map("POST", "/api/user/signin", users.SignInAsync, publicRoles);
        router.Map("POST", "/api/user/signup", users.SignUpAsync, publicRoles);
        router.Map("POST", "/api/user/signout", users.SignOutAsync, publicRoles);
        router.Map("GET", "/api/user/me", users.MeAsync);

        var staticFilesMiddleware = new StaticFilesMiddleware(
            Path.Combine(AppContext.BaseDirectory, "static")
        );

        var routingMiddleware = new RoutingMiddleware(router);
        var authenticationMiddleware = new AuthenticationMiddleware(authService);
        var csrfMiddleware = new CsrfMiddleware(new Uri(settings.Url));

        var pipeline = new MiddlewarePipeline()
            .Use(ErrorMiddleware.InvokeAsync)
            .Use(staticFilesMiddleware.InvokeAsync)
            .Use(csrfMiddleware.InvokeAsync)
            .Use(authenticationMiddleware.InvokeAsync)
            .Use(routingMiddleware.InvokeAsync)
            .Build(_ => throw new HttpException(ApiError.NotFound));

        var server = new HttpServer(settings.Url, pipeline);
        await server.RunAsync(cancellation.Token);
    }
}
