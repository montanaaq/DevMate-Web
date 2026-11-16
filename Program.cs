using System.Globalization;
using DevMate.Configuration;
using DevMate.Controllers;
using DevMate.Http;
using DevMate.Http.Errors;
using DevMate.Http.Middleware;
using DevMate.Http.Routing;
using DevMate.Repositories;
using DevMate.Services;
using FluentValidation;

namespace DevMate;

internal static class Program
{
    private static async Task Main()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("ru");

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };

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
        router.Map("GET", "/", pages.HomeAsync);
        router.Map("POST", "/api/user/signin", users.SignInAsync);
        router.Map("POST", "/api/user/signout", users.SignOutAsync);
        router.Map("GET", "/api/user/me", users.MeAsync);

        var staticFiles = new StaticFilesMiddleware(
            Path.Combine(AppContext.BaseDirectory, "static")
        );

        var routing = new RoutingMiddleware(router);
        var authentication = new AuthenticationMiddleware(authService);

        var pipeline = new MiddlewarePipeline()
            .Use(ErrorMiddleware.InvokeAsync)
            .Use(staticFiles.InvokeAsync)
            .Use(authentication.InvokeAsync)
            .Use(routing.InvokeAsync)
            .Build(_ => throw new HttpException(ApiError.NotFound, ErrorMessages.PageNotFound));

        var server = new HttpServer(settings.Url, pipeline);
        await server.RunAsync(cancellation.Token);
    }
}
