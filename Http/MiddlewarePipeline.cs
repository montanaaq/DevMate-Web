namespace DevMate.Http;

public sealed class MiddlewarePipeline
{
    private readonly List<RequestMiddleware> _middlewares = [];

    public MiddlewarePipeline Use(RequestMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public RequestDelegate Build(RequestDelegate endpoint)
    {
        var pipeline = endpoint;

        for (var index = _middlewares.Count - 1; index >= 0; index--)
        {
            var middleware = _middlewares[index];
            var next = pipeline;
            pipeline = context => middleware(context, next);
        }

        return pipeline;
    }
}
