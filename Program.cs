using System.Net;
using DevMate.Configuration;
using DevMate.Controllers;
using DevMate.Http;
using DevMate.Http.Middleware;
using DevMate.Shared.Constants;
using Npgsql;
using Superpower;

namespace DevMate;

internal static class Program
{
    private static async Task Main()
    {
        try
        {
            DotNetEnv.Env.NoClobber().Load(".env");
        }
        catch (ParseException exception)
        {
            // Parser diagnostics can contain values from the .env file.
            throw new FormatException(
                string.Format(ErrorMessages.InvalidEnvironmentFile, exception.ErrorPosition.Line)
            );
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };

        var settings = AppSettings.FromEnvironment();
        await using var dataSource = NpgsqlDataSource.Create(settings.DatabaseConnectionString);

        try
        {
            await using var command = dataSource.CreateCommand("SELECT 1");
            await command.ExecuteScalarAsync(cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return;
        }
        catch (NpgsqlException exception)
        {
            throw new InvalidOperationException(ErrorMessages.DatabaseConnectionFailed, exception);
        }

        Console.WriteLine("Подключение к PostgreSQL установлено.");

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
