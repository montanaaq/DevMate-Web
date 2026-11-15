namespace DevMate.Http;

public delegate Task RequestDelegate(RequestContext context);

public delegate Task RequestMiddleware(RequestContext context, RequestDelegate next);
