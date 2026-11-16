namespace DevMate.Http.Routing;

public sealed record Entry(
    string Method,
    string[] Segments,
    string Pattern,
    RequestDelegate Handler
);
