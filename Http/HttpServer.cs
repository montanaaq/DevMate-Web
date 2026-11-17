using System.Net;
using DevMate.Http.Errors;

namespace DevMate.Http;

public sealed class HttpServer(string url, RequestDelegate pipeline)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var listener = new HttpListener();
        using var slots = new SemaphoreSlim(64, 64);
        var requests = new List<Task>();

        listener.Prefixes.Add(url);
        listener.Start();
        using var registration = cancellationToken.Register(listener.Stop);
        Console.WriteLine($"skill_issue is running: {url}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await slots.WaitAsync(cancellationToken);
                HttpListenerContext context;

                try
                {
                    context = await listener.GetContextAsync();
                }
                catch
                {
                    slots.Release();
                    throw;
                }

                requests.RemoveAll(request => request.IsCompleted);
                requests.Add(HandleRequestAsync(context, slots, cancellationToken));
            }
        }
        catch (Exception exception)
            when (cancellationToken.IsCancellationRequested
                && exception
                    is OperationCanceledException
                        or HttpListenerException
                        or ObjectDisposedException
            ) { }
        finally
        {
            listener.Stop();
            await Task.WhenAll(requests);
            Console.WriteLine("skill_issue stopped.");
        }
    }

    private async Task HandleRequestAsync(
        HttpListenerContext listenerContext,
        SemaphoreSlim slots,
        CancellationToken cancellationToken
    )
    {
        var context = new RequestContext(listenerContext, cancellationToken);

        try
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            await pipeline(context);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            context.Response.Abort();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"{ErrorMessages.ResponseWriteFailed}: {exception}");
            context.Response.Abort();
        }
        finally
        {
            try
            {
                context.Response.Close();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"{ErrorMessages.ResponseCloseFailed}: {exception}");
            }
            finally
            {
                slots.Release();
            }
        }
    }
}
