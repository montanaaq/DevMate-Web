namespace DevMate.Http;

public sealed record RouteMatch(
    RequestDelegate? Handler,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyCollection<string> AllowedMethods
);
