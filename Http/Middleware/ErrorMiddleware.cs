using System.Net;
using DevMate.Shared.Constants;

namespace DevMate.Http.Middleware;

public static class ErrorMiddleware
{
    public static async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ObjectDisposedException exception)
            when (exception.ObjectName == typeof(HttpListenerResponse).FullName)
        {
            Console.Error.WriteLine(ErrorMessages.ConnectionClosed);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"{ErrorMessages.RequestProcessingFailed}: {exception}");

            if (context.HasStarted)
            {
                context.Response.Abort();
                return;
            }

            await context.WriteErrorAsync(
                HttpStatusCode.InternalServerError,
                ErrorMessages.RequestFailed
            );
        }
    }
}
