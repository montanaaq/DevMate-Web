using System.Net;
using DevMate.Configuration;
using DevMate.Controllers;
using DevMate.Http;
using DevMate.Http.Middleware;
using DevMate.Shared.Constants;

namespace DevMate;

internal static class Program
{
    private static async Task Main()
    {
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };

        var settings = AppSettings.FromEnvironment();
        var pages = new PagesController(Path.Combine(AppContext.BaseDirectory, "Views"));

        var router = new Router();
        router.Map("GET", "/", pages.HomeAsync);

        var staticFiles = new StaticFilesMiddleware(
            Path.Combine(AppContext.BaseDirectory, "static")
        );

        var routing = new RoutingMiddleware(router);

        var pipeline = new MiddlewarePipeline()
            .Use(ErrorMiddleware.InvokeAsync)
            .Use(staticFiles.InvokeAsync)
            .Use(routing.InvokeAsync)
            .Build(context =>
                context.WriteErrorAsync(HttpStatusCode.NotFound, ErrorMessages.PageNotFound)
            );

        var server = new HttpServer(settings.Url, pipeline);
        await server.RunAsync(cancellation.Token);
    }
}
